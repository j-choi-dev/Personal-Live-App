using UnityEngine;
using TMPro;
using UniRx;
using UnityEditor.PackageManager.Requests;

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
                _valueText.text = value.ToString(_format);
            }
        }

        private void UpdateRangeLabels()
        {
            if (_minText != null)
            {
                _minText.text = MinValue.ToString(_format);
            }

            if (_maxText != null)
            {
                _maxText.text = MaxValue.ToString(_format);
            }
        }

        private void ResetToDefault()
        {
            Value = Mathf.Clamp(_defaultValue, MinValue, MaxValue);
        }

    }
}
