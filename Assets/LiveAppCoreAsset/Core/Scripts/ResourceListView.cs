using LiveApp.UI;
using LiveAppUI.Presenter;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;

namespace LiveAppUI.View
{
    public class ResourceListView : MonoBehaviour, IResourceListView
    {
        [SerializeField] private TMP_Text _title = null;
        [SerializeField] private ObservableButton _cancleButton = null;
        [SerializeField] private ObservableButton _closeButton = null;
        [SerializeField] private ObservableDropdown _server = null;
        [SerializeField] private ListView _listView = null;
        public bool IsActive => gameObject.activeSelf;

        public IObservable<int> OnServerChange => _server.OnValueChanged;

        public IObservable<Unit> OnClickClose => _closeButton.OnClick;

        public IObservable<Unit> OnClickCancle => _cancleButton.OnClick;

        public int CurrentServerIndex => _server.Value;

        private List<(string id, string name)> _currentItemList = new List<(string id, string name)>();

        private void Awake()
        {
            _closeButton.OnClick
                .Merge( _cancleButton.OnClick )
                .Subscribe( x => gameObject.SetActive( false ) )
                .AddTo( this );
        }

        public void SetActive( bool isActive )
        {
            gameObject.SetActive( isActive );
        }

        public void AddListItem( string id, string name )
        {
            _listView.AddItem( id, name );
        }

        public void RemoveListItem( string id )
        {
            var item = _currentItemList.FirstOrDefault( arg => arg.id == id );
            _currentItemList.Remove( item );
            _listView.RemoveItem( id );
        }

        public void SetList( IReadOnlyList<(string id, string name)> list )
        {
            for( var i = 0; i < list.Count; i++ )
            {
                var item = list[i];
                _listView.AddItem( item.id, item.name );

                _currentItemList.Add((item.id, item.name) );
            }
        }

        public void ResetList()
        {
            _currentItemList.Clear();
            _listView.Clear();
        }

        public void SetTitle( string title )
        {
            _title.text = title;
        }

        public void SetServerList( IReadOnlyList<string> servers )
            => _server.SetOptions( servers );

        public void SetServerItem( int index )
            => _server.SetValueWithoutNotify( index );
    }
}
