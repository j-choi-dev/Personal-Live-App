using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using LiveApp.Util;

namespace LiveApp.UI
{
    public class ListView : MonoBehaviour, IListView
    {
        [SerializeField] private CellView _prefab = null;
        [SerializeField] private List<CellView> _staticCells = null;
        private List<ICellView> _cells = null;

        public IReadOnlyList<ICellView> Cells => _cells;
        public IObservable<int> OnSelectedIndex => throw new NotImplementedException();

        public IObservable<string> OnSelectedId => throw new NotImplementedException();

        public IObservable<string> OnSelectedDisplayName => throw new NotImplementedException();

        private void Awake()
        {
            _cells = new List<ICellView>();
            for ( var i = 0 ; i < _staticCells.Count ; i++ )
            {
                _staticCells[ i ].SetIdIfNull( $"default_{i}" );
            }
            _cells.AddRange( _staticCells );
        }

        public void AddItem( string id, string displayName )
        {
            var obj = Instantiate( _prefab );
            var tmpList = _cells.Select( arg => arg.ID ).ToList();
            var newID = IDUtil.GenerateNewId( id, tmpList, data => obj.ID );
            obj.SetItem( newID, displayName );
            obj.name = newID;
            _cells.Add( obj );
        }

        public void AddItem( string displayName )
        {
            var obj = Instantiate( _prefab );
            var tmpList = _cells.Select( arg => arg.ID ).ToList();
            var newID = IDUtil.GenerateNewId( ( _cells.Count + 1 ).ToString(), tmpList, data => obj.ID );
            obj.SetItem( newID, displayName );
            obj.name = newID;
            _cells.Add( obj );
        }
    }
}
