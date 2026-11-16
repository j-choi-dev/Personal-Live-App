using TMPro;
using UniRx;
using UnityEngine;

namespace LiveAppUI.View
{
    public class ObservableSliderUGUIWithRangeLabel : ObservableSliderUGUI
    {
        [SerializeField] private TMP_InputField _input = null;
        [SerializeField] private TMP_Text _minText = null;
        [SerializeField] private TMP_Text _maxText = null;
        [SerializeField] private ObservableButton _resetButton = null;
        [SerializeField] private float _defaultValue = 0.0f;
        [SerializeField] private string _format = "0.0";

        private const string IndeterminateText = "---";

        private void Awake()
        {
            OnValueChanged
                .Subscribe(UpdateInputText)
                .AddTo(this);

            if (_resetButton != null)
            {
                _resetButton.OnClick
                    .Subscribe(_ => ResetToDefault())
                    .AddTo(this);
            }

            if (_input != null)
            {
                _input.onEndEdit.AsObservable()
                    .Subscribe(OnInputEndEdit)
                    .AddTo(this);
            }

            UpdateRangeLabels();
            UpdateInputText(base.Value);
        }

        public override float Value
        {
            get => base.Value;
            set
            {
                base.Value = value;
                UpdateInputText(base.Value);
            }
        }

        public override float MinValue
        {
            get => base.MinValue;
            set
            {
                base.MinValue = value;
                UpdateRangeLabels();
            }
        }

        public override float MaxValue 
        { 
            get => base.MaxValue;
            set
            { 
                base.MaxValue = value;
                UpdateRangeLabels();
            } 
        }

        public override bool Interactable 
        { 
            get => base.Interactable; 
            set
            {
                base.Interactable = value;
                if(_resetButton != null)
                {
                    _resetButton.Interactable = value;
                }

                if (_input != null)
                {
                    _input.interactable = value;
                }
            }
        }

        public override void SetValueWithoutNotify(float value)
        {
            base.SetValueWithoutNotify(value);
            UpdateInputText(base.Value); //clamp된 실제 값을 표시
        }

        public override void SetRangeWithoutNotify(float minValue, float maxValue)
        {
            //범위 라벨은 MinValue, MaxValue 프로퍼티로 갱신됨
            base.SetRangeWithoutNotify(minValue, maxValue);
            UpdateInputText(base.Value);
        }

        public override void SetIndeterminateValue()
        {
            base.SetIndeterminateValue();

            if (_input != null)
            {
                _input.SetTextWithoutNotify(string.Empty);
            }
        }

        /// <summary>
        /// 범위 라벨에 표시할 문자열로 변경
        /// virtual, proteced로 자식 슬라이더에서 표시 방식을 재정의할 수 있도록 함
        /// 우선은 scale 배율쪽 표시만 달라서 이렇게 했지만 추후 더 많은 방식의 표시 방법이 생기면
        /// Formatter를 분리해서 만든 후 slider에 comp를 붙이는 방식으로 전환 검토 @OH 26.10.29
        /// </summary>
        protected virtual string FormatValue(float value)
        {
            return value.ToString(_format);
        }

        /// <summary>
        /// input field에 표시할 문자열로 변경
        /// </summary>
       protected virtual string FormatInput(float value)
        {
            return value.ToString(_format);
        }

        protected virtual bool TryParseInput(string text, out float value)
        {
            if( float.TryParse(text, out value) == false)
            {
                return false;
            }

            return float.IsNaN(value) == false 
                && float.IsInfinity(value) == false;
        }


        private void OnInputEndEdit(string text)
        {
            if(TryParseInput(text, out float parsedValue))
            {
                Value = Mathf.Clamp(parsedValue, MinValue, MaxValue);
            }

            SetInputText(base.Value);
        }

        private void UpdateInputText(float value)
        {
            if(_input == null || _input.isFocused)
            {
                return;
            }

            SetInputText(value);
        }

        private void SetInputText(float value)
        {
            if(_input == null)
            {
                return;
            }

            _input.SetTextWithoutNotify(FormatInput(value));
        }

        private void UpdateRangeLabels()
        {
            if (_minText != null)
            {
                _minText.text = FormatValue(MinValue);
            }

            if (_maxText != null)
            {
                _maxText.text = FormatValue(MaxValue);
            }
        }

        private void ResetToDefault()
        {
            Value = Mathf.Clamp(_defaultValue, MinValue, MaxValue);
        }

    }
}
