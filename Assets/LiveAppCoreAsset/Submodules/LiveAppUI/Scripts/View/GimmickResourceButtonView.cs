using LiveApp.UI;
using System;
using TMPro;
using UniRx;
using UnityEngine;

namespace LiveAppUI.View
{
    public class GimmickResourceButtonView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label = null;
        [SerializeField] private ObservableButton _button = null;
        [SerializeField] private GameObject _selectedObject = null;

        public IObservable<Unit> OnClick => _button.OnClick;

        public void Initialize(string resourceID, int collectionID)
        {
            _label.text = $"{resourceID} / {collectionID}";
        }

        public void SetSelected(bool selected)
        {
            if (_selectedObject != null)
            {
                _selectedObject.SetActive(selected);
            }
        }
    }
}