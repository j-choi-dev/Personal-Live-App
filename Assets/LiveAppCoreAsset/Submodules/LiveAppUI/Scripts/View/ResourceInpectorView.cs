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
    public class ResourceInpectorView : MonoBehaviour, IResourceInspectorView
    {
        //----------------members-------------------------------
        [SerializeField] private TMP_Text _title = null;
        [SerializeField] private ObservableButton _closeButton = null;
        [SerializeField] private ObservableButton _removeButton = null;
        [SerializeField] private ObservableDropdown _filters = null;
        [SerializeField] private ListViewWithOneList _listView = null;

        //선택된 item의 id, name만 list로 따로 저장
        private List<(string id, string name)> _currentItemList = new List<(string id, string name)>();

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
           for (var i = 0; i < list.Count; i++)
            {
                var item = list[i];
                _listView.AddItem(item.id, item.name);
                _currentItemList.Add( (item.id, item.name) );
            }

        }

        public void AddListItem(string id, string name)
        {
            _listView.AddItem(id, name);
        }

        public void RemoveListItem(string id)
        {
            if (id == null || id == "")
            {
                Debug.LogWarning($"삭제할 리소스를 선택해주세요.");
                return;
            }

            var item = _currentItemList.FirstOrDefault(arg => arg.id == id);
            if (item.id == null || item.name == null)
            {
                Debug.LogWarning($"삭제할 리소스를 찾지 못했습니다.");
                return;
            }
            _currentItemList.Remove(item);
            _listView.RemoveItem( id );
        }

        public void ClearList()
        {
            _currentItemList.Clear();
            _listView.Clear();
        }

    }
}
