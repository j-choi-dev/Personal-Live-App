using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI; 

namespace LiveApp.UI
{
    public class CellView : MonoBehaviour, ICellView
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private bool _toggleOnClick = true;
        [SerializeField] private GameObject _selectedMark = null;
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

        public void SetSelected(bool isSelected)
        {
            _isSelected = isSelected;
            UpdateSelectedMark();
        }

        private void Awake()
        {
            _button.onClick
                .AsObservable()
                .Subscribe( _ => OnClick() )
                .AddTo( this );

            UpdateSelectedMark();
        }

        private void OnClick()
        {
            if(_toggleOnClick == false && _isSelected)
            {
                return;
            }

            _isSelected = !_isSelected;
            UpdateSelectedMark();
            if(_isSelected)
            {
                _onSelected.OnNext(Unit.Default);
            }
        }

        private void UpdateSelectedMark()
        {
            if(_selectedMark != null)
            {
                _selectedMark.SetActive(_isSelected);
            }
        }

        private void OnDestroy()
        {
            _onSelected.OnCompleted();
            _onSelected.Dispose();
        }
    }
}
