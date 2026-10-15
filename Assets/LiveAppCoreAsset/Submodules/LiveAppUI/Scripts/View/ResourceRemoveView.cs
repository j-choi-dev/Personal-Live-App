using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using LiveApp.UI;
using LiveApp.UI.Presenter;
using LiveAppUI.View;

namespace LiveApp.UI
{
    public class ResourceRemoveView : MonoBehaviour, IResourceRemoveView
    {
        //----------------members-------------------------------
        [SerializeField] private TMP_Text _title = null;
        [SerializeField] private ObservableButton _closeButton = null;
        [SerializeField] private ObservableButton _removeButton = null;
        [SerializeField] private ObservableDropdown _filters = null;
        [SerializeField] private ListViewWithOneList _listView = null;

        //---------------implements-----------------------------
        public IObservable<int> OnFilterChanged => _filters.OnValueChanged;
        public IObservable<Unit> OnClickRemove => _removeButton.OnClick;
        public IReadOnlyList<string> CurrSelectedItemList => _listView.CurrentSelectedItemList;
        public int CurrentFilterIndex => _filters.Value;
        public IObservable<Unit> OnClickClose => _closeButton.OnClick;
        public IObservable<Unit> OnClickCancle => throw new NotImplementedException();
        public bool IsActive => gameObject.activeSelf;



        private void Awake()
        {

        }

        //-----------implements---------
        public void SetActive(bool IsActive)
        {
            gameObject.SetActive(IsActive);
        }

        public void SetTitle(string title)
        {
            _title.text = title;
        }

        public void SetFilterList(IReadOnlyList<string> filters)
            => _filters.SetOptions(filters);
        public void SetFilterItem(int index)
            => _filters.SetValueWithoutNotify(index);

        public void SetResourceItemList(IReadOnlyList<(string id, string name)> list)
        {
            _listView.Clear();
           for (var i = 0; i < list.Count; i++)
            {
                _listView.AddItem(list[i].id, list[i].name);
            }

        }

        public void AddListItem(string id, string name)
        {
            _listView.AddItem(id, name);
        }

        public void RemoveListItem(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning($"삭제할 리소스를 선택해주세요.");
                return;
            }

            _listView.RemoveItem( id );
        }

        public void ClearList()
        {
            _listView.Clear();
        }

    }
}
