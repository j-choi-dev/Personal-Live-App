using System;
using TMPro;
using UniRx;
using UnityEngine;

namespace LiveAppUI.View
{
    public class GimmickInputItemView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _displayName = null;
        [SerializeField] private TMP_InputField _inputField = null;

        public IObservable<string> OnEndEdit => _inputField.onEndEdit.AsObservable();

        public void Initialize(string displayName)
        {
            _displayName.text = displayName;
        }

        public void SetContentType(TMP_InputField.ContentType contentType)
        {
            _inputField.contentType = contentType;
        }
    }
}