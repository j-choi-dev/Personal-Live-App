using StudioResourceSDK.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace StudioResourceSDK.Infrastructure
{
    public class StudioBackGround : MonoBehaviour, IBackground
    {
        [SerializeField] private RectTransform viewport;
        [SerializeField] private RectTransform imageRect;
        [SerializeField] private Image image;

        public string ID { get; private set; }

        public void SetID( string id )
            => ID = id;

        private void Start()
        {
            Init();
        }

        public void Init()
        {
            if( viewport == null || imageRect == null || image == null || image.sprite == null )
            {
                Debug.LogError( $"{ID} ... Background 설정이 올바르지 않습니다. Object={name}", this );
                return;
            }

            Canvas.ForceUpdateCanvases();

            float viewportWidth = viewport.rect.width;
            float viewportHeight = viewport.rect.height;
            float spriteWidth = image.sprite.rect.width;
            float spriteHeight = image.sprite.rect.height;

            if( viewportWidth <= 0f || viewportHeight <= 0f || spriteWidth <= 0f || spriteHeight <= 0f )
            {
                Debug.LogError( $"{ID} ... Background 크기가 올바르지 않습니다. Viewport={viewportWidth}x{viewportHeight}, Sprite={spriteWidth}x{spriteHeight}", this );
                return;
            }

            float viewportAspect = viewportWidth / viewportHeight;
            float spriteAspect = spriteWidth / spriteHeight;

            float targetWidth;
            float targetHeight;

            if( spriteAspect > viewportAspect )
            {
                targetHeight = viewportHeight;
                targetWidth = targetHeight * spriteAspect;
            }
            else
            {
                targetWidth = viewportWidth;
                targetHeight = targetWidth / spriteAspect;
            }

            imageRect.anchorMin = new Vector2( 0.5f, 0.5f );
            imageRect.anchorMax = new Vector2( 0.5f, 0.5f );
            imageRect.pivot = new Vector2( 0.5f, 0.5f );
            imageRect.localScale = Vector3.one;
            imageRect.sizeDelta = new Vector2( targetWidth, targetHeight );
            imageRect.anchoredPosition = Vector2.zero;

            image.color = Color.white;

            Debug.Log( $"{ID} ... Background Layout :: Viewport={viewportWidth}x{viewportHeight}, Sprite={spriteWidth}x{spriteHeight}, Result={targetWidth}x{targetHeight}" );
        }

        public void RefreshLayout()
        {
        }

        public void SetSprite( Sprite sprite )
        {
            image.sprite = sprite;
            RefreshLayout();
        }
    }
}
