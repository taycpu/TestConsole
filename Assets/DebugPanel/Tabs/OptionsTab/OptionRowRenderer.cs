using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DebugPanel.Options;

namespace DebugPanel.Tabs
{
    /// <summary>
    /// Renders a single OptionDefinition row.
    /// Shows a toggle for bool, slider for float/int with NumberRange,
    /// input field for string, dropdown for enum, and a button for void methods.
    /// </summary>
    public class OptionRowRenderer : MonoBehaviour
    {
        [Header("Common")]
        [SerializeField] private TMP_Text labelText;

        [Header("Controls (assign or leave null)")]
        [SerializeField] private Toggle toggleControl;
        [SerializeField] private Slider sliderControl;
        [SerializeField] private TMP_Text sliderValueLabel;
        [SerializeField] private TMP_InputField inputFieldControl;
        [SerializeField] private TMP_Dropdown dropdownControl;
        [SerializeField] private Button buttonControl;
        [SerializeField] private TMP_Text buttonLabel;
        [SerializeField] private TMP_Text readonlyLabel;

        private OptionDefinition _option;
        private bool _ignoreCallbacks;

        public void Bind(OptionDefinition option)
        {
            _option = option;

            if (labelText != null)
                labelText.text = option.Name;

            // Hide all controls first
            SetActive(toggleControl?.gameObject, false);
            SetActive(sliderControl?.gameObject, false);
            SetActive(inputFieldControl?.gameObject, false);
            SetActive(dropdownControl?.gameObject, false);
            SetActive(buttonControl?.gameObject, false);
            SetActive(readonlyLabel?.gameObject, false);

            if (option.IsMethod)
            {
                SetActive(buttonControl?.gameObject, true);
                if (buttonLabel != null) buttonLabel.text = option.Name;
                if (labelText != null) labelText.gameObject.SetActive(false);
                buttonControl?.onClick.AddListener(() => option.MethodAction?.Invoke());
                return;
            }

            if (option.IsReadOnly)
            {
                SetActive(readonlyLabel?.gameObject, true);
                RefreshValue();
                return;
            }

            var t = option.ValueType;

            if (t == typeof(bool))
            {
                SetActive(toggleControl?.gameObject, true);
                if (toggleControl != null)
                {
                    RefreshValue();
                    toggleControl.onValueChanged.AddListener(v =>
                    {
                        if (_ignoreCallbacks) return;
                        option.Setter?.Invoke(v);
                    });
                }
            }
            else if ((t == typeof(float) || t == typeof(double) || t == typeof(int) || t == typeof(uint)
                      || t == typeof(short) || t == typeof(ushort) || t == typeof(byte) || t == typeof(sbyte))
                     && option.RangeMin.HasValue)
            {
                SetActive(sliderControl?.gameObject, true);
                if (sliderControl != null)
                {
                    sliderControl.minValue = option.RangeMin.Value;
                    sliderControl.maxValue = option.RangeMax ?? 1f;
                    sliderControl.wholeNumbers = (t == typeof(int) || t == typeof(uint) || t == typeof(short)
                                                  || t == typeof(ushort) || t == typeof(byte) || t == typeof(sbyte));
                    RefreshValue();
                    sliderControl.onValueChanged.AddListener(v =>
                    {
                        if (_ignoreCallbacks) return;
                        option.Setter?.Invoke(Convert.ChangeType(v, option.ValueType));
                        if (sliderValueLabel != null)
                            sliderValueLabel.text = sliderControl.wholeNumbers ? ((int)v).ToString() : v.ToString("F2");
                    });
                }
            }
            else if (t == typeof(string))
            {
                SetActive(inputFieldControl?.gameObject, true);
                if (inputFieldControl != null)
                {
                    RefreshValue();
                    inputFieldControl.onEndEdit.AddListener(v =>
                    {
                        if (_ignoreCallbacks) return;
                        option.Setter?.Invoke(v);
                    });
                }
            }
            else if (t.IsEnum)
            {
                SetActive(dropdownControl?.gameObject, true);
                if (dropdownControl != null)
                {
                    dropdownControl.ClearOptions();
                    var names = Enum.GetNames(t);
                    dropdownControl.AddOptions(new System.Collections.Generic.List<string>(names));
                    RefreshValue();
                    dropdownControl.onValueChanged.AddListener(idx =>
                    {
                        if (_ignoreCallbacks) return;
                        option.Setter?.Invoke(Enum.GetValues(t).GetValue(idx));
                    });
                }
            }
            else
            {
                // Numeric without range — use input field
                SetActive(inputFieldControl?.gameObject, true);
                if (inputFieldControl != null)
                {
                    inputFieldControl.contentType = TMP_InputField.ContentType.DecimalNumber;
                    RefreshValue();
                    inputFieldControl.onEndEdit.AddListener(v =>
                    {
                        if (_ignoreCallbacks) return;
                        try { option.Setter?.Invoke(Convert.ChangeType(v, option.ValueType)); }
                        catch { /* ignore invalid input */ }
                    });
                }
            }
        }

        public void RefreshValue()
        {
            if (_option == null) return;
            _ignoreCallbacks = true;

            try
            {
                var val = _option.Getter?.Invoke();
                if (val == null) return;

                var t = _option.ValueType;

                if (t == typeof(bool) && toggleControl != null)
                    toggleControl.isOn = (bool)val;
                else if (sliderControl != null && sliderControl.gameObject.activeSelf)
                {
                    sliderControl.value = Convert.ToSingle(val);
                    if (sliderValueLabel != null)
                        sliderValueLabel.text = sliderControl.wholeNumbers
                            ? ((int)sliderControl.value).ToString()
                            : sliderControl.value.ToString("F2");
                }
                else if (inputFieldControl != null && inputFieldControl.gameObject.activeSelf)
                    inputFieldControl.text = val.ToString();
                else if (dropdownControl != null && dropdownControl.gameObject.activeSelf)
                    dropdownControl.value = Array.IndexOf(Enum.GetValues(t), val);
                else if (readonlyLabel != null && readonlyLabel.gameObject.activeSelf)
                    readonlyLabel.text = val.ToString();
            }
            finally
            {
                _ignoreCallbacks = false;
            }
        }

        private static void SetActive(GameObject go, bool active)
        {
            if (go != null) go.SetActive(active);
        }
    }
}
