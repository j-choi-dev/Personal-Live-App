using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UniRx.Triggers;

namespace LiveAppUI.View
{
    public class ColorBlockApplier : MonoBehaviour
        , ISelectHandler
        , IDeselectHandler
        , IPointerDownHandler
        , IPointerUpHandler
        , IPointerEnterHandler
        , IPointerExitHandler
    {
        [SerializeField] private Image _overrideColor;
        [SerializeField] private ColorBlock _overrideColorBlock = ColorBlock.defaultColorBlock;

        private Selectable _selectable;
        private bool _isPressed;
        private bool _isSelected;
        private bool _isHovered;
        private bool IsInteractable => _selectable != null ? _selectable.interactable : false;

        private void Awake()
        {
            _selectable = GetComponent<Selectable>();
        }

        public void OnSelect( BaseEventData eventData )
        {
            _isSelected = true;
            ApplyColor();
        }

        public void OnDeselect( BaseEventData eventData )
        {
            _isSelected = false;
            ApplyColor();
        }

        public void OnPointerDown( PointerEventData eventData )
        {
            _isPressed = true;
            ApplyColor();
        }

        public void OnPointerUp( PointerEventData eventData )
        {
            _isPressed = false;
            ApplyColor();
        }

        public void OnPointerEnter( PointerEventData eventData )
        {
            _isHovered = true;
            ApplyColor();
        }

        public void OnPointerExit( PointerEventData eventData )
        {
            _isHovered = false;
            ApplyColor();
        }

        private void ApplyColor()
        {
            // NOTE: 복수의 플래그가 동시에 true가 될 경우의 우선순위를 if-else를 통해 관리함.
            // 순서를 바꿀 경우 동작 오류가 발생할 수 있음.
            // ※ _isHovered 과 _isSelected의 우선순위는 Unity 표준 Selectable 클래스와 의도적으로 역순으로 했음. @Choi
            if( IsInteractable == false )
            {
                _overrideColor.color = _overrideColorBlock.disabledColor;
            }
            else if( _isPressed )
            {
                _overrideColor.color = _overrideColorBlock.pressedColor;
            }
            else if( _isHovered )
            {
                _overrideColor.color = _overrideColorBlock.highlightedColor;
            }
            else if( _isSelected )
            {
                _overrideColor.color = _overrideColorBlock.selectedColor;
            }
            else
            {
                _overrideColor.color = _overrideColorBlock.normalColor;
            }
        }
    }
}
