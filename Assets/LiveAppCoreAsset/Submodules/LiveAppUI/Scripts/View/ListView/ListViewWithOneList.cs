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
        private readonly Subject<string> _onSelectedId = new Subject<string>();
        public IObservable<string> OnSelectedId => _onSelectedId;
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

            bool wasSelected = item.IsSelected;

            Destroy(item.Object); 
            _cells.Remove(item);

            if(wasSelected)
            {
                _onSelectedId.OnNext(null);
            }

        }
        
        private void BindCell(ICellView cell)
        {
            cell.OnSelected
                .Subscribe(_ => OnCellSelected(cell));
        }

        private void OnCellSelected(ICellView selected)
        {
            if(_singleSelect)
            {
                for( var i = 0; i < _cells.Count; i++)
                {
                    if (_cells[i] != selected && _cells[i].IsSelected)
                    {
                        _cells[i].SetSelected(false);
                    }
                }
            }

            //단일 선택 시 -> 선택한 셀id
            //다중 선택 시 -> 마지막에 선택한 셀 id
            //단일이던 다중이던 셀 선택에 대한 이벤트는 항상 발행하도록 함
            _onSelectedId.OnNext(selected.ID);
        }

        public void Clear()
        {
            bool removedSelected = false;

            for (var i = _cells.Count - 1; i >= 0; i--)
            {
                var cell = _cells[i];
                if (_staticCellSet.Contains(cell))
                {
                    continue;
                }

                if (cell.IsSelected)
                {
                    removedSelected = true;
                }

                Destroy(cell.Object);
                _cells.RemoveAt(i);

            }

            if (removedSelected)
            {
                _onSelectedId.OnNext(null);
            }
        }

        private void OnDestroy()
        {
            _onSelectedId.OnCompleted();
            _onSelectedId.Dispose();
            _cells.Dispose();
        }

    }
}
