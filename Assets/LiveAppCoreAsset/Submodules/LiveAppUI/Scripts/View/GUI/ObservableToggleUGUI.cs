using UnityEngine;
using UnityEngine.UI;
using System;
using UniRx;

namespace LiveAppUI.View
{
    public class ObservableToggleUGUI : ObservableToggle
    {
        [SerializeField] private Toggle m_Toggle = null;
        [SerializeField] private Image m_IndeterminateImage = null;

        public override bool IsActive
        {
            get => m_Toggle.isOn;
            set
            {
                SetIndeterminate( false );
                m_Toggle.isOn = value;
            }
        }

        public override IObservable<bool> OnActiveChanged => m_Toggle.onValueChanged.AsObservable();
        public override bool Interactable { get => m_Toggle.interactable; set => m_Toggle.interactable = value; }

        public override void SetIsActiveWithoutNotify( bool isActive )
        {
            SetIndeterminate( false );
            m_Toggle.SetIsOnWithoutNotify( isActive );
        }

        private void SetIndeterminate( bool indeterminate )
        {
            if( m_IndeterminateImage )
            {
                m_IndeterminateImage.enabled = indeterminate;
            }
        }

        /// <summary>
        /// Indeterminateを表示する
        /// </summary>
        public override void SetIndeterminateValue()
        {
            SetIndeterminate( true );
            m_Toggle.SetIsOnWithoutNotify( false );
        }
    }
}
