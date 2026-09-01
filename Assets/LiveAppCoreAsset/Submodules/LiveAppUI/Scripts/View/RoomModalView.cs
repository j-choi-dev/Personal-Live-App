using LiveAppUI.Presenter;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace LiveAppUI.View
{
    public class RoomModalView : MonoBehaviour, IRoomModalView
    {
        [SerializeField]
        private ObservableDropdownTMPro _dropdown;
        [SerializeField]
        private ObservableInputTMPro _name;
        [SerializeField]
        private ObservableButton _closeButton;
        [SerializeField]
        private ObservableButton _enterButton;
        [SerializeField]
        private ObservableButton _exitButton;

        public bool IsActive => gameObject.activeSelf;

        public IObservable<Unit> OnClose => _closeButton.OnClick;

        public IObservable<Unit> OnClickExit => _exitButton.OnClick;

        public IObservable<Unit> OnClickEnter => _enterButton.OnClick;

        public string CurrentName => _name.Text;

        public int CurrenIndex => _dropdown.Value;

        public void SetActive( bool isActive )
        {
            gameObject.SetActive( isActive );
        }

        public void SetNameWithoutNotify( string name )
        {
            _name.SetTextWithoutNotify( name );
        }

        public void SetRoomList( IReadOnlyList<string> list )
        {
            _dropdown.SetOptions( list );
        }
    }
}
