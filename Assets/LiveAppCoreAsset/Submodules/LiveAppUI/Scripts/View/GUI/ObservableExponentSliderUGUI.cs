using System;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

namespace LiveAppUI.View
{
    public class ObservableExponentSliderUGUI : ObservableSliderUGUIWithRangeLabel
    {
        /// <summary>
        /// Infra(SceneResourceTransfomer)의 _scaleExponentBase와 같은 값이어야 함
        /// </summary>
        [SerializeField] private float _exponentBase = 2.0f;
        [SerializeField] private string _multiplierFormat = "0.00";

        protected override string FormatValue(float value)
        {
            return $"x{Mathf.Pow(_exponentBase, value).ToString(_multiplierFormat)}";
        }
    }
}
