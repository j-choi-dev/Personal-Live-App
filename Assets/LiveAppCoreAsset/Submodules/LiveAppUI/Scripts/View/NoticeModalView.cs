using LiveAppUI.Presenter;
using System;
using TMPro;
using UniRx;
using UnityEngine;

namespace LiveAppUI.View
{
    public class NoticeModalView : MonoBehaviour, INoticeModalView
    {
        [SerializeField] private TMP_Text _header = null;
        [SerializeField] private TMP_Text _mainMessage = null;
        [SerializeField] private TMP_Text _subMessage = null;
        [SerializeField] private ObservableButton _closeButton = null;
        [SerializeField] private ObservableButton _positiveButton = null;
        [SerializeField] private ObservableButton _negativeButton = null;

        public IObservable<Unit> OnClickYes => _positiveButton.OnClick;


        public IObservable<Unit> OnClickClose => _closeButton.OnClick;

        private Subject<Unit> _onClickNo = new Subject<Unit>();
        public IObservable<Unit> OnClickCancle => _onClickNo;

        public bool IsActive => gameObject.activeSelf;

        public void SetActive(bool isActive)
            => gameObject.SetActive(isActive);

        private void Awake()
        {
            if(_negativeButton == null)
            {
                return;
            }
            _negativeButton.OnClick
                .Subscribe(_ => _onClickNo.OnNext(_))
                .AddTo(this);
        }

        public void SetHeaderTitle(string value)
            => _header.text = value;

        public void SetMainMessage(string value)
            => _mainMessage.text = value;

        public void SetSubMessage(string value)
            => _subMessage.text = value;
    }
}
