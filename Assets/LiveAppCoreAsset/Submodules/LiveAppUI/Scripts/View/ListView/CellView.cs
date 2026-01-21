using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UniRx;
using System;
using LiveApp.Util;

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

        public void SetItem( string text, string id )
        {
            _text.text = text;
            ID = id;
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
    }
}
