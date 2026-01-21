using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LiveAppUI.View
{
	public class FloatInputDragger : MonoBehaviour
		, IBeginDragHandler
		, IDragHandler
	{
		[SerializeField] private ObservableFloatInput m_FloatInput;
		[SerializeField] private float m_ValueByPixel = 0.01f;

		private float m_CurrentValue;

		public void OnBeginDrag( PointerEventData eventData )
		{
			m_CurrentValue = m_FloatInput.Value;
		}

		public void OnDrag( PointerEventData eventData )
		{
			if( m_FloatInput.Interactable )
			{
				var diff = eventData.position.x - eventData.pressPosition.x;
				m_FloatInput.Value = m_CurrentValue + diff * m_ValueByPixel;
			}
		}
	}
}
