using LiveApp.UI;
using LiveAppUI.Presenter;
using System;
using System.Collections.Generic;
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
        [SerializeField] private ListView _listView = null;
        public bool IsActive => gameObject.activeSelf;

        public IObservable<Unit> OnClickClose => _closeButton.OnClick;

        public IObservable<Unit> OnClickCancle => _cancleButton.OnClick;

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

        public void SetItem( string id, string name )
        {
            _listView.AddItem( id, name );
        }

        public void RemoveItem( string id )
        {
            _listView.AddItem( id, name );
        }

        public void SetList( IReadOnlyList<(string id, string name)> list )
        {
            for( var i = 0; i < list.Count; i++ )
            {
                var item = list[i];
                _listView.AddItem( item.id, item.name );
            }
        }

        public void ResetList()
        {
            _listView.Clear();
        }

        public void SetTitle( string title )
        {
            _title.text = title;
        }
    }
}
