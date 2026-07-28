using LiveApp.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

namespace LiveApp.UI
{
    public class CellView : MonoBehaviour, ICellView
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _text;
        private bool _isSelected = false;

        private Subject<Unit> _onSelected = new Subject<Unit>();
        public IObservable<Unit> OnSelected => _onSelected;

        public bool IsUsable { get; private set; } = false;

        public string ID { get; private set; } = string.Empty;
        public bool IsSelected => _isSelected;

        public GameObject Object => gameObject;


        /// <summary>
        /// 초기화 등 특수한 경우에만 한정해서 ID 적용
        /// </summary>
        /// <param name="id">id</param>
        /// <remarks>ID가 지정된 후에는 호출해도 ID가 적용되지 않음.</remarks>
        public void SetIdIfNull( string id )
        {
            if(string.IsNullOrEmpty( ID ) == false )
            {
                return;
            }
            ID = id;
        }

        public void SetIsUsable( bool isVal )
            => IsUsable = isVal;

        public void SetItem( string id, string displayName )
        {
            ID = id;
            _text.text = displayName;
        }

        private void Awake()
        {
            _button.onClick
                .AsObservable()
                .Subscribe(arg =>
                {
                    _isSelected = !_isSelected;
                    if( _isSelected )
                    {
                        _onSelected.OnNext( Unit.Default );
                    }
                } )
                .AddTo( this );
        }

        private void OnDestroy()
        {
            _onSelected.OnCompleted();
            _onSelected.Dispose();
        }
    }
}
