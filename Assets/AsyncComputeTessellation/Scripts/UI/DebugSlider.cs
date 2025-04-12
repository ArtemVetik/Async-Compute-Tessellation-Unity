using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AV.AsyncComputeTessellation
{
    public class DebugSlider : MonoBehaviour
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private TMP_Text _valueText;
        [SerializeField] private TMP_Text _nameText;

        private float? _changedValue = null;

        private void OnEnable()
        {
            _slider.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDisable()
        {
            _slider.onValueChanged.RemoveListener(OnValueChanged);
        }

        public void Setup(string sliderName, float min, float max, bool wholeNumbers = false)
        {
            _nameText.text = sliderName;
            _slider.minValue = min;
            _slider.maxValue = max;
            _slider.wholeNumbers = wholeNumbers;

            _changedValue = null;
        }

        public bool UpdateValue(ref float value)
        {
            return UpdateValueInternal(ref value, v => v, v => v);
        }
        
        public bool UpdateValue(ref uint value)
        {
            return UpdateValueInternal(ref value, v => (uint)v, v => v);
        }
        
        public bool UpdateValue(ref int value)
        {
            return UpdateValueInternal(ref value, v => (int)v, v => v);
        }
        
        private void OnValueChanged(float value)
        {
            _changedValue = value;
        }
        
        private bool UpdateValueInternal<T>(ref T value, Func<float, T> fromFloat, Func<T, float> toFloat)
        {
            bool update = false;
            
            if (_changedValue != null)
            {
                value = fromFloat(_changedValue.Value);
                _valueText.text = _changedValue.Value.ToString("F");
                _changedValue = null;
                update = true;
            }

            float floatValue = toFloat(value);

            if (Mathf.Approximately(floatValue, _slider.value) == false)
            {
                _slider.value = floatValue;
                _valueText.text = floatValue.ToString("F");
            }

            return update;
        }
    }
}