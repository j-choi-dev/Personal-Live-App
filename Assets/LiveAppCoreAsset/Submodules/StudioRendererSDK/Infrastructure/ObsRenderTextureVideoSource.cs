using StudioRendererSDK.Domain;
using UnityEngine;

namespace StudioRendererSDK.Infrastructure
{
    // @Use
    public sealed class ObsRenderTextureVideoSource : MonoBehaviour, IObsVideoSource
    {
        [SerializeField] private Camera sourceCamera;
        [SerializeField] private RenderTexture outputTexture;

        public Texture OutputTexture => outputTexture;

        public bool IsReady =>
            isActiveAndEnabled &&
            sourceCamera != null &&
            outputTexture != null &&
            sourceCamera.targetTexture == outputTexture;

        private void Awake()
        {
            ValidateReferences();
        }

        private void ValidateReferences()
        {
            if( sourceCamera == null )
            {
                Debug.LogError( $"{nameof( ObsRenderTextureVideoSource )}: Source Camera가 연결되지 않았습니다.", this );
                return;
            }

            if( outputTexture == null )
            {
                Debug.LogError( $"{nameof( ObsRenderTextureVideoSource )}: Output RenderTexture가 연결되지 않았습니다.", this );
                return;
            }

            if( sourceCamera.targetTexture != outputTexture )
            {
                Debug.LogError( $"{nameof( ObsRenderTextureVideoSource )}: Source Camera의 Target Texture와 Output Texture가 서로 다릅니다.", this );
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // 자동 할당하거나 오브젝트를 만들지 않고
            // Inspector 연결 상태만 검사한다.
            if( sourceCamera != null &&
                outputTexture != null &&
                sourceCamera.targetTexture != null &&
                sourceCamera.targetTexture != outputTexture )
            {
                Debug.LogWarning( "Source Camera와 Video Source가 서로 다른 RenderTexture를 사용하고 있습니다.", this );
            }
        }
#endif
    }
}