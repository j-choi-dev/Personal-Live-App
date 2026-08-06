using System;
using StudioRendererSDK.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace StudioRendererSDK.Infrastructure
{
    [DisallowMultipleComponent]
    public sealed class ObsRenderOutput : MonoBehaviour, IObsVideoSource
    {
        [Header("Required References")]
        [SerializeField] private Camera outputCamera;
        [SerializeField] private RawImage localPreview;

        [Header("OBS Output")]
        [SerializeField] private Vector2Int outputResolution = new Vector2Int(1280, 720);
        [SerializeField] private LayerMask outputLayerMask;

        [Header("RenderTexture")]
        [SerializeField] private RenderTextureFormat colorFormat = RenderTextureFormat.ARGB32;
        [SerializeField] private int depthBufferBits = 24;
        [SerializeField] private FilterMode filterMode = FilterMode.Bilinear;

        private RenderTexture _outputTexture;

        private RenderTexture _previousTargetTexture;
        private int _previousCullingMask;
        private float _previousCameraAspect;

        private bool _initialized;

        public RenderTexture OutputTexture => _outputTexture;

        public Vector2Int OutputResolution => outputResolution;

        public bool IsReady => _outputTexture != null && _outputTexture.IsCreated();

        public event Action<RenderTexture> OutputTextureChanged;

        private void Awake()
        {
            if( !ValidateReferences() )
            {
                enabled = false;
                return;
            }

            _previousTargetTexture = outputCamera.targetTexture;
            _previousCullingMask = outputCamera.cullingMask;
            _previousCameraAspect = outputCamera.aspect;

            CreateOutputTexture();

            _initialized = true;
        }

        /// <summary>
        /// 방송 중 해상도를 바꾸면 WebRTC 트랙 재생성과 재협상이 필요할 수 있다.
        /// 가능하면 영상 세션이 정지된 상태에서 호출한다.
        /// </summary>
        public void SetOutputResolution( int width, int height )
        {
            width = Mathf.Max( 16, width );
            height = Mathf.Max( 16, height );

            if( outputResolution.x == width && outputResolution.y == height )
            {
                return;
            }

            outputResolution = new Vector2Int( width, height );
            CreateOutputTexture();
        }

        public void SetOutputLayerMask( LayerMask layerMask )
        {
            outputLayerMask = layerMask;
            if( outputCamera != null )
            {
                outputCamera.cullingMask = outputLayerMask.value;
            }
        }

        private void CreateOutputTexture()
        {
            int width = Mathf.Max(16, outputResolution.x);
            int height = Mathf.Max(16, outputResolution.y);

            var newTexture = new RenderTexture(
                width,
                height,
                depthBufferBits,
                colorFormat,
                RenderTextureReadWrite.Default)
            {
                name = $"OBS_Output_{width}x{height}",
                filterMode = filterMode,
                wrapMode = TextureWrapMode.Clamp,

                // 인코더로 전달할 RenderTexture에는 우선 MSAA를 사용하지 않는다.
                antiAliasing = 1,

                useMipMap = false,
                autoGenerateMips = false,
                anisoLevel = 0,
                hideFlags = HideFlags.DontSave
            };

            if( !newTexture.Create() )
            {
                Debug.LogError(
                    $"OBS RenderTexture 생성 실패: {width}x{height}" );

                UnityEngine.Object.Destroy( newTexture );
                return;
            }

            RenderTexture oldTexture = _outputTexture;
            _outputTexture = newTexture;

            outputCamera.cullingMask = outputLayerMask.value;
            outputCamera.targetTexture = _outputTexture;
            outputCamera.aspect = width / ( float )height;

            localPreview.texture = _outputTexture;
            localPreview.raycastTarget = false;
            //localPreview.preserveAspect = true;

            DestroyRenderTexture( oldTexture );
            OutputTextureChanged?.Invoke( _outputTexture );

            Debug.Log( $"OBS RenderTexture 생성 완료: {width}x{height}, LayerMask={outputLayerMask.value}" );
        }

        private bool ValidateReferences()
        {
            bool valid = true;

            if( outputCamera == null )
            {
                Debug.LogError( $"{nameof( ObsRenderOutput )}: Output Camera가 없습니다." );
                valid = false;
            }

            if( localPreview == null )
            {
                Debug.LogError( $"{nameof( ObsRenderOutput )}: Local Preview RawImage가 없습니다." );
                valid = false;
            }

            return valid;
        }

        private static void DestroyRenderTexture( RenderTexture renderTexture )
        {
            if( renderTexture == null )
            {
                return;
            }

            if( renderTexture.IsCreated() )
            {
                renderTexture.Release();
            }

            UnityEngine.Object.Destroy( renderTexture );
        }

        private void OnDestroy()
        {
            if( !_initialized )
            {
                return;
            }

            if( localPreview != null && localPreview.texture == _outputTexture )
            {
                localPreview.texture = null;
            }

            if( outputCamera != null )
            {
                if( outputCamera.targetTexture == _outputTexture )
                {
                    outputCamera.targetTexture = _previousTargetTexture;
                }

                outputCamera.cullingMask = _previousCullingMask;

                outputCamera.aspect = _previousCameraAspect;
            }

            DestroyRenderTexture( _outputTexture );
            _outputTexture = null;
            _initialized = false;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            outputResolution.x = Mathf.Max( 16, outputResolution.x );

            outputResolution.y = Mathf.Max( 16, outputResolution.y );

            if( depthBufferBits != 0 && depthBufferBits != 16 && depthBufferBits != 24 )
            {
                depthBufferBits = 24;
            }
        }
#endif
    }
}
