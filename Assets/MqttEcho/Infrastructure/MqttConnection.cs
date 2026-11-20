using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using UniRx;
using UnityEngine;

namespace MqttEcho.Infrastructure
{
    public readonly struct MqttInbound
    {
        public readonly string Topic;
        public readonly byte[] Payload;

        public MqttInbound(string topic, byte[] payload)
        {
            Topic = topic;
            Payload = payload;
        }
    }

    public sealed class MqttConnectionSettings
    {
        public string Host = "127.0.0.1";
        public int Port = 1883;
        public string ClientId;
        public TimeSpan ReconnectDelay = TimeSpan.FromSeconds(3);
    }

    /// <summary>
    /// MQTTnet(4.3.x) 래퍼.
    /// - MQTTnet 콜백은 스레드풀에서 호출되므로, 모든 이벤트를 메인 스레드로 옮긴 뒤 내보낸다.
    /// - 연결이 끊기면 일정 간격으로 재접속하고, 재접속 시 구독을 복구한다.
    /// </summary>
    public sealed class MqttConnection : IDisposable
    {
        readonly MqttConnectionSettings _settings;
        readonly IMqttClient _client;
        readonly MqttClientOptions _options;
        readonly Subject<MqttInbound> _onMessage = new Subject<MqttInbound>();
        readonly ReactiveProperty<bool> _isConnected = new ReactiveProperty<bool>(false);
        readonly Dictionary<string, MqttQualityOfServiceLevel> _subscriptions = new Dictionary<string, MqttQualityOfServiceLevel>();
        readonly CancellationTokenSource _cts = new CancellationTokenSource();
        bool _started;
        bool _disposed;

        /// <summary>수신 메시지 (메인 스레드에서 발행)</summary>
        public IObservable<MqttInbound> OnMessage => _onMessage;

        /// <summary>연결 상태 (메인 스레드에서 갱신)</summary>
        public IReadOnlyReactiveProperty<bool> IsConnected => _isConnected;

        public MqttConnection(MqttConnectionSettings settings)
        {
            _settings = settings;
            _client = new MqttFactory().CreateMqttClient();
            _options = new MqttClientOptionsBuilder()
                .WithTcpServer(settings.Host, settings.Port)
                .WithClientId(settings.ClientId)
                .WithCleanSession()
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(15))
                .WithTimeout(TimeSpan.FromSeconds(5))
                .Build();

            _client.ConnectedAsync += OnConnectedAsync;
            _client.DisconnectedAsync += OnDisconnectedAsync;
            _client.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
        }

        /// <summary>연결 유지 루프 시작 (끊기면 자동 재접속)</summary>
        public void Start()
        {
            if (_started || _disposed) return;
            _started = true;
            KeepConnectedLoopAsync(_cts.Token).Forget();
        }

        async UniTaskVoid KeepConnectedLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                if (!_client.IsConnected)
                {
                    try
                    {
                        Debug.Log($"[MQTT] 연결 시도 {_settings.Host}:{_settings.Port} ({_settings.ClientId})");
                        await _client.ConnectAsync(_options, ct);
                    }
                    catch (OperationCanceledException) { break; }
                    catch (Exception e) { Debug.LogWarning($"[MQTT] 연결 실패: {e.Message}"); }
                }

                var canceled = await UniTask.Delay(_settings.ReconnectDelay, cancellationToken: ct)
                    .SuppressCancellationThrow();
                if (canceled) break;
            }
        }

        /// <summary>구독 등록. 연결 전이면 연결되는 순간 구독된다.</summary>
        public UniTask SubscribeAsync(string topic, MqttQualityOfServiceLevel qos)
        {
            _subscriptions[topic] = qos;
            return _client.IsConnected ? SubscribeInternalAsync(topic, qos) : UniTask.CompletedTask;
        }

        public async UniTask PublishAsync(string topic, byte[] payload, MqttQualityOfServiceLevel qos, CancellationToken ct)
        {
            if (_disposed || !_client.IsConnected)
            {
                Debug.LogWarning($"[MQTT] 미연결 상태라 전송 생략: {topic}");
                return;
            }

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(qos)
                .Build();

            try
            {
                await _client.PublishAsync(message, ct);
            }
            catch (Exception e) when (!(e is OperationCanceledException))
            {
                Debug.LogWarning($"[MQTT] 전송 실패: {e.Message}");
            }
        }

        async UniTask SubscribeInternalAsync(string topic, MqttQualityOfServiceLevel qos)
        {
            try
            {
                var options = new MqttClientSubscribeOptionsBuilder()
                    .WithTopicFilter(f => f.WithTopic(topic).WithQualityOfServiceLevel(qos))
                    .Build();
                await _client.SubscribeAsync(options, _cts.Token);
                Debug.Log($"[MQTT] 구독: {topic}");
            }
            catch (Exception e) when (!(e is OperationCanceledException))
            {
                Debug.LogWarning($"[MQTT] 구독 실패 {topic}: {e.Message}");
            }
        }

        // ---- MQTTnet 콜백 (스레드풀) → 메인 스레드로 전환 ----

        async Task OnConnectedAsync(MqttClientConnectedEventArgs _)
        {
            await UniTask.SwitchToMainThread();
            if (_disposed) return;

            _isConnected.Value = true;
            Debug.Log("[MQTT] 연결됨");

            // Clean Session이라 재접속하면 구독이 사라지므로 다시 구독한다
            foreach (var kv in _subscriptions) SubscribeInternalAsync(kv.Key, kv.Value).Forget();
        }

        async Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs e)
        {
            await UniTask.SwitchToMainThread();
            if (_disposed) return;

            if (_isConnected.Value) Debug.LogWarning($"[MQTT] 연결 끊김: {e.Reason}");
            _isConnected.Value = false;
        }

        async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = e.ApplicationMessage.PayloadSegment.ToArray(); // 내부 버퍼 재사용 대비 복사

            await UniTask.SwitchToMainThread();
            if (_disposed) return;

            _onMessage.OnNext(new MqttInbound(topic, payload));
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _cts.Cancel();
            DisconnectAndDisposeAsync().Forget();
        }

        async UniTaskVoid DisconnectAndDisposeAsync()
        {
            try
            {
                if (_client.IsConnected) await _client.DisconnectAsync(); // 정상 종료 (best effort)
            }
            catch (Exception) { /* 종료 중 오류는 무시 */ }
            finally
            {
                _client.Dispose();
                _onMessage.Dispose();
                _isConnected.Dispose();
                _cts.Dispose();
            }
        }
    }
}
