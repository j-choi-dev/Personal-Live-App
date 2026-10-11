using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace LiveAppUI.View
{
    public class GimmickToggleItemView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _displayName = null;
        [SerializeField] private Toggle _toggle = null;

        private readonly Subject<bool> _onValueChanged = new Subject<bool>();
        public IObservable<bool> OnValueChanged => _onValueChanged;

        private void Awake()
        {
            _toggle.onValueChanged
                .AsObservable()
                .Subscribe(value => _onValueChanged.OnNext(value))
                .AddTo(this);
        }

        public void Initialize(string displayName, bool currentValue)
        {
            _displayName.text = displayName;
            _toggle.SetIsOnWithoutNotify(currentValue);
        }

        private void OnDestroy()
        {
            _onValueChanged.Dispose();
        }
    }
}