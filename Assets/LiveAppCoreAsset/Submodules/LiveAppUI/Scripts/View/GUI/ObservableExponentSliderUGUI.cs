using UnityEngine;

namespace LiveAppUI.View
{
    public class ObservableExponentSliderUGUI : ObservableSliderUGUIWithRangeLabel
    {
        /// <summary>
        /// <see cref="StudioResourceSDK.Application.TransformInspectorContext"/>의 ScaleExponentBase와 같은 값이어야 함
        /// </summary>
        [SerializeField] private float _exponentBase = 2.0f;
        [SerializeField] private string _multiplierFormat = "0.00";

        protected override string FormatValue(float value)
        {
            return $"x{FormatMultiplier(value)}";
        }

        protected override string FormatInput(float value)
        {
            return FormatMultiplier(value);
        }

        protected override bool TryParseInput(string text, out float value)
        {
            value = 0.0f;

            if(base.TryParseInput(text, out float multiplier) == false)
            {
                return false;
            }

            if(multiplier <= 0.0f)
            {
                return false;
            }

            value = Mathf.Log(multiplier, _exponentBase);
            return true;
        }

        private string FormatMultiplier(float exponent)
        {
            return Mathf.Pow(_exponentBase, exponent).ToString(_multiplierFormat);
        }

    }
}
