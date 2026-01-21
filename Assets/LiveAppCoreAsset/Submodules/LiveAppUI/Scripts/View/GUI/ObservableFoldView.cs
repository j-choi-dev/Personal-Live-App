using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace LiveAppUI.View
{
    public class ObservableFoldView : MonoBehaviour
    {
        [SerializeField] private Toggle _toggle = null;
        [SerializeField] private TMP_Text _text = null;
        [SerializeField] private Image _open = null;
        [SerializeField] private Image _close = null;
        [SerializeField] private GameObject _foldObject = null;

        private Subject<bool> _onShowChanged = new Subject<bool>();
        public IObservable<bool> OnShowChanged => _onShowChanged;

        public bool IsShow => _foldObject.activeSelf;

        public string Name => _text.text;

        private void Awake()
        {
            _toggle.onValueChanged.AsObservable()
                .Subscribe( isOn =>
                 {
                     _foldObject.SetActive( isOn );
                     _onShowChanged.OnNext( isOn );
                     _open.gameObject.SetActive( !isOn );
                     _close.gameObject.SetActive( !isOn );
                 } )
                .AddTo( this );
        }

        public void SetName( string name )
            => _text.text = name;
    }
}
