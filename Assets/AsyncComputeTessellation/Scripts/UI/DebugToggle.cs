using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AV.AsyncComputeTessellation
{
    public class DebugToggle : MonoBehaviour
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private TMP_Text _name;
        
        private bool? _changedValue;
        
        private void OnEnable()
        {
            _toggle.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDisable()
        {
            _toggle.onValueChanged.RemoveListener(OnValueChanged);
        }
        
        public void Setup(string toggleName)
        {
            _name.text = toggleName;
        }

        public bool UpdateValue(ref bool value)
        {
            bool update = false;

            if (_changedValue != null)
            {
                value = _changedValue.Value;
                _changedValue = null;
                update = true;
            }

            if (_toggle.isOn != value)
                _toggle.isOn = value;
            
            return update;
        }

        private void OnValueChanged(bool value)
        {
            _changedValue = value;
        }
    }
}