using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.Rendering;

namespace StudioRendererSDK.Infrastructure
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder( -10000 )]
    public sealed class WebRtcRuntimePump : MonoBehaviour
    {
        private IEnumerator _webRtcUpdateRoutine;
        private IDisposable _renderSubscription;

        /// <summary>
        /// Unity Editor에서 Scene View와 Game View가 동시에 렌더링될 때
        /// 같은 프레임에 중복 실행되는 것을 방지한다.
        /// </summary>
        private int _lastPumpedFrame = -1;

        private void OnEnable()
        {
            StartPump();
        }

        private void OnDisable()
        {
            StopPump();
        }

        private void StartPump()
        {
            StopPump();
            _lastPumpedFrame = -1;
             // 여기서는 열거자만 생성한 뒤 UniRx가 전달하는 렌더링 종료 이벤트마다 직접 MoveNext()를 호출
            _webRtcUpdateRoutine = WebRTC.Update();

            // WebRTC.Update()의 첫 번째 MoveNext()는 첫 yield return WaitForEndOfFrame 위치까지 진입.
            // 이 선행 호출이 있어야 첫 번째 렌더링 종료 이벤트부터 실제 WebRTC Texture Update가 실행
            if( !AdvanceWebRtcUpdateRoutine() )
            {
                StopPump();
                return;
            }

            _renderSubscription = CreateEndContextRenderingObservable()
                    .Subscribe( _ => PumpCurrentFrame(),
                        exception =>
                        {
                            Debug.LogException( exception, this );
                            StopPump();
                        } );
        }

        private void PumpCurrentFrame()
        {
            int currentFrame = Time.frameCount;
            if( _lastPumpedFrame == currentFrame )
            {
                return;
            }
            _lastPumpedFrame = currentFrame;
            if( !AdvanceWebRtcUpdateRoutine() )
            {
                StopPump();
            }
        }

        private bool AdvanceWebRtcUpdateRoutine()
        {
            if( _webRtcUpdateRoutine == null )
            {
                return false;
            }

            try
            {
                bool hasNext = _webRtcUpdateRoutine.MoveNext();
                if( !hasNext )
                {
                    Debug.LogWarning( "WebRTC Update Routine이 종료되었습니다.", this );
                }
                return hasNext;
            }
            catch( Exception exception )
            {
                Debug.LogException( exception, this );
                return false;
            }
        }

        private void StopPump()
        {
            _renderSubscription?.Dispose();
            _renderSubscription = null;

            if( _webRtcUpdateRoutine is IDisposable disposable )
            {
                disposable.Dispose();
            }

            _webRtcUpdateRoutine = null;
            _lastPumpedFrame = -1;
        }

        private static IObservable<Unit> CreateEndContextRenderingObservable()
        {
            return Observable.FromEvent<Action<ScriptableRenderContext, List<Camera>>, Unit>( convert =>
                        ( context, cameras ) => convert( Unit.Default ),
                        handler => RenderPipelineManager.endContextRendering += handler,
                        handler => RenderPipelineManager.endContextRendering -= handler );
        }
    }
}