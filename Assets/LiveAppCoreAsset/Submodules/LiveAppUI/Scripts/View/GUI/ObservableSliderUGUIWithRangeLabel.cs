using TMPro;
using UniRx;
using UnityEngine;

namespace LiveAppUI.View
{
    public class ObservableSliderUGUIWithRangeLabel : ObservableSliderUGUI
    {
        [SerializeField] private TMP_Text _valueText = null;
        [SerializeField] private TMP_Text _minText = null;
        [SerializeField] private TMP_Text _maxText = null;
        [SerializeField] private ObservableButton _resetButton = null;
        [SerializeField] private float _defaultValue = 0.0f;
        [SerializeField] private string _format = "0.0";

        private const string IndeterminateText = "---";

        public override float Value
        {
            get => base.Value;
            set
            {
                base.Value = value;
                UpdateValueText(base.Value);
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
            }
        }

        public override void SetValueWithoutNotify(float value)
        {
            base.SetValueWithoutNotify(value);
            UpdateValueText(base.Value); //clamp된 실제 값을 표시
        }

        public override void SetRangeWithoutNotify(float minValue, float maxValue)
        {
            //범위 라벨은 MinValue, MaxValue 프로퍼티로 갱신됨
            base.SetRangeWithoutNotify(minValue, maxValue);
            UpdateValueText(base.Value);
        }

        public override void SetIndeterminateValue()
        {
            base.SetIndeterminateValue();

            if (_valueText != null)
            {
                _valueText.text = IndeterminateText;
            }
        }

        /// <summary>
        /// 값을 라벨에 표시할 문자열로 변경
        /// virtual, proteced로 자식 슬라이더에서 표시 방식을 재정의할 수 있도록 함
        /// 우선은 scale 배율쪽 표시만 달라서 이렇게 했지만 추후 더 많은 방식의 표시 방법이 생기면
        /// Formatter를 분리해서 만든 후 slider에 comp를 붙이는 방식으로 전환 검토 @OH 26.10.29
        /// </summary>
        protected virtual string FormatValue(float value)
        {
            return value.ToString(_format);
        }

        private void Awake()
        {
            OnValueChanged
                .Subscribe(UpdateValueText)
                .AddTo(this);

            if (_resetButton != null)
            {
                _resetButton.OnClick
                    .Subscribe(_ => ResetToDefault())
                    .AddTo(this);
            }

            UpdateRangeLabels();
            UpdateValueText(base.Value);
        }

        private void UpdateValueText(float value)
        {
            if(_valueText != null)
            {
                _valueText.text = FormatValue(value);
            }
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
