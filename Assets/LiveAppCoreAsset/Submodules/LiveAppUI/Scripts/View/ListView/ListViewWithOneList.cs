using LiveApp.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;

namespace LiveApp.UI
{
    public class ListViewWithOneList : MonoBehaviour, IListViewWithOneList
    {
        [SerializeField] private CellView _prefab = null;
        [SerializeField] private List<CellView> _staticCells = new List<CellView>();
        [SerializeField] private Transform _content = null;
        [SerializeField] private bool _singleSelect = true;
        private HashSet<ICellView> _staticCellSet = new HashSet<ICellView>();

        private ReactiveCollection<ICellView> _cells = new ReactiveCollection<ICellView>();
        public IReadOnlyReactiveCollection<ICellView> OnCellChanged => _cells;

        public IObservable<int> OnSelectedIndex => throw new NotImplementedException();
        public IObservable<string> OnSelectedId => throw new NotImplementedException();
        public IObservable<string> OnSelectedDiplayName => throw new NotImplementedException();
        public IReadOnlyList<string> CurrentSelectedItemList => _cells.Where(arg => arg.IsSelected).Select(arg => arg.ID).ToList();


        private void Awake()
        {
            if ( _staticCells.Count > 0)
            {
                for (var i = 0; i < _staticCells.Count; i++)
                {
                    _staticCells[i].SetIdIfNull($"default_{i}");
                    BindCell(_staticCells[i]);
                }
                _cells.AddRange(_staticCells);
                _staticCellSet = new HashSet<ICellView>(_staticCells);
            }
        }

        public void AddItem(string id, string displayName)
        {
            AddItemInternal(id, displayName);
        }

        public void AddItem(string displayName)
        {
            AddItemInternal((_cells.Count + 1).ToString(), displayName);
        }

        private void AddItemInternal(string baseId, string displayName)
        {
            // 기존 cellview의 id 목록으로 id중복 여부 판별
            var existingIds = _cells.Select(arg => arg.ID).ToList();
            var newId = IDUtil.GenerateNewId(baseId, existingIds, existing => existing);

            var obj = Instantiate(_prefab, _content);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;

            obj.SetItem(newId, displayName);
            obj.name = newId;

            _cells.Add(obj);
            BindCell(obj);
        }

        public void RemoveItem(string id)
        {
            var item = _cells.FirstOrDefault(arg => arg.ID == id);
            if (item == null)
            {
                return;
            }

            Destroy(item.Object); 
            _cells.Remove(item);
        }
        
        private void BindCell(ICellView cell)
        {
            cell.OnSelected
                .Subscribe(_ => OnCellSelected(cell));
        }

        private void OnCellSelected(ICellView selected)
        {
            if(_singleSelect == false)
            {
                return;
            }

            for(var i = 0; i < _cells.Count; i++)
            {
                if (_cells[i] != selected && _cells[i].IsSelected)
                {
                    _cells[i].SetSelected(false);
                }
            }
        }

        public void Clear()
        {
            for (var i = _cells.Count - 1; i >= 0; i--)
            {
                var cell = _cells[i];
                if (_staticCellSet.Contains(cell))
                {
                    continue;
                }

                Destroy(cell.Object);
                _cells.RemoveAt(i);
            }
        }

        private void OnDestroy()
        {
            _cells.Dispose();
        }

    }
}
