using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrecisionSplit : MonoBehaviour
{
    public int Value
    {
        get { return int.Parse(textInput.text); }
        set
        {
            value = Mathf.Clamp(value, 1, (int)slider.maxValue);
            textInput.text = value.ToString();
            slider.value = value;
        }
    }

    [Header("Object References")]
    public Slider slider;
    public TMP_InputField textInput;

    public Action<int> SplitCalled;

    public void Split()
    {
        if (SplitCalled != null)
            SplitCalled.Invoke(Value);
    }

    public void TextInputChanged()
    {
        if (int.Parse(textInput.text) > 0)
            Value = int.Parse(textInput.text);
    }

    public void SliderChaged()
    {
        if (slider.value > 0)
            Value = (int)Mathf.Floor(slider.value);
    }
}