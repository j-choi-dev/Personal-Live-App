using LiveApp.UI;
using System;
using TMPro;
using UniRx;
using UnityEngine;

namespace LiveAppUI.View
{
    public class GimmickButtonItemView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _displayName = null;
        [SerializeField] private ObservableButton _button = null;

        public IObservable<Unit> OnClick => _button.OnClick;

        public void Initialize(string displayName)
        {
            _displayName.text = displayName;
        }
    }
}