using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AV.AsyncComputeTessellation
{
    public class DebugDropDown : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown _dropDown;
        [SerializeField] private TMP_Text _nameText;

        private int? _changedValue = null;
        
        private void OnEnable()
        {
            _dropDown.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDisable()
        {
            _dropDown.onValueChanged.RemoveListener(OnValueChanged);
        }
        
        public void Setup(string dropDownName, List<string> options)
        {
            _nameText.text = dropDownName;

            _dropDown.ClearOptions();
            _dropDown.AddOptions(options);
            
            _changedValue = null;
        }

        public bool UpdateValue(ref int value)
        {
            bool update = false;

            if (_changedValue != null)
            {
                value = _changedValue.Value;
                _changedValue = null;
                update = true;
            }

            if (_dropDown.value != value)
                _dropDown.value = value;
            
            return update;
        }
        
        private void OnValueChanged(int value)
        {
            _changedValue = value;
        }
    }
}