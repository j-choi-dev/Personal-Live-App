using LiveApp.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace LiveApp.UI
{
    public class ListView : MonoBehaviour, IListView
    {
        [SerializeField] private CellView _prefab = null;
        [SerializeField] private List<CellView> _staticCells = null;
        [SerializeField] private Transform _contents = null;

        private List<ICellView> _cells = null;
        private HashSet<ICellView> _staticCellSet = null;

        private ReactiveCollection<ICellView> _onCellChanged;
        public IReactiveCollection<ICellView> OnCellChanged => _onCellChanged;

        public IReadOnlyList<ICellView> Cells => _cells;
        public IObservable<int> OnSelectedIndex => throw new NotImplementedException();

        public IObservable<string> OnSelectedId => throw new NotImplementedException();

        public IObservable<string> OnSelectedDisplayName => throw new NotImplementedException();

        public IReadOnlyList<string> CurrentSelectedItemList => _cells.Where(arg => arg.IsSelected).Select(arg => arg.ID).ToList();

        private void Awake()
        {
            _cells = new List<ICellView>();
            for ( var i = 0 ; i < _staticCells.Count ; i++ )
            {
                _staticCells[ i ].SetIdIfNull( $"default_{i}" );
            }
            _cells.AddRange( _staticCells );
            _staticCellSet = new HashSet<ICellView>( _staticCells );
            _onCellChanged = new ReactiveCollection<ICellView>( _cells );
        }

        public void AddItem( string id, string displayName )
        {
            var obj = Instantiate( _prefab, _contents );
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;

            var tmpList = _cells.Select( arg => arg.ID ).ToList();
            var newID = IDUtil.GenerateNewId( id, tmpList, data => obj.ID );
            obj.SetItem( newID, displayName );
            obj.name = newID;
            _cells.Add( obj );
            _onCellChanged.Add( obj );
        }

        public void AddItem( string displayName )
        {
            var obj = Instantiate( _prefab, _contents );
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;

            var tmpList = _cells.Select( arg => arg.ID ).ToList();
            var newID = IDUtil.GenerateNewId( ( _cells.Count + 1 ).ToString(), tmpList, data => obj.ID );
            obj.SetItem( displayName, newID );
            obj.name = newID;
            _cells.Add( obj );
            _onCellChanged.Add( obj );
        }

        public void RemoveItem( string id )
        {
            var item = _cells.FirstOrDefault( arg => arg.ID == id );
            if(item == null)
            {
                return;
            }
            Destroy( item.Object );
            _cells.Remove( item );
            _onCellChanged.Remove( item );
        }

        public void Clear()
        {
            for( var i = _cells.Count - 1; i >= 0; i-- )
            {
                var cell = _cells[ i ];
                if( _staticCellSet.Contains( cell ) )
                {
                    continue;
                }
                Destroy( cell.Object );
                _cells.RemoveAt( i );
                _onCellChanged.Clear();
            }
        }
    }
}
