using LiveAppUI.Presenter;
using System;
using UniRx;
using UnityEngine;

namespace LiveAppUI.View
{
    public class SystemMenuView : MonoBehaviour, ISystemMenuView
    {
        [SerializeField] private ObservableButtonTMPro _buttonEmergency = null;
        [SerializeField] private ObservableToggleButton _buttonLock = null;
        [SerializeField] private ObservableToggleButton _buttonRec = null;

        public IObservable<Unit> OnClickEmergency => _buttonEmergency.OnClick;
        public IObservable<bool> OnClickLock => _buttonLock.OnActiveChange;
        public IObservable<bool> OnClickRec => _buttonRec.OnActiveChange;

        private void Awake()
        {
            
        }
    }
}
