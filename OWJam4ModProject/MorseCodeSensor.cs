using System;
using System.Collections.Generic;
using UnityEngine;

namespace OWJam4ModProject;

/// <summary>
/// implementation of beta dlc mechanic
/// </summary>
public class MorseCodeSensor : MonoBehaviour
{
    public const float MAX_SHORT_INTERVAL = 0.7f;

    public const float MAX_LONG_INTERVAL = 3f;

    private float LastLightTime;

    private float LastDarkTime;

    [SerializeField]
    private LightSensor _lightSensor;

    [Space]
    [SerializeField]
    [Tooltip("true for long, false for short")]
    private bool[] _code;

    private List<bool> CodeInput = new();

    /// <summary>
    /// other things should listen to this
    /// </summary>
    public event Action OnEnterCode;

    public static Action ClearCodeAction;

    private void Awake()
    {
        ClearCodeAction += ClearCode;
        _lightSensor.OnDetectLight += OnDetectLight;
        _lightSensor.OnDetectDarkness += OnDetectDarkness;
    }
    void OnDestroy()
    {
        ClearCodeAction -= ClearCode;
        _lightSensor.OnDetectLight -= OnDetectLight;
        _lightSensor.OnDetectDarkness -= OnDetectDarkness;
    }
    void ClearCode()
    {
        CodeInput.Clear();
    }
    private void OnDetectLight()
    {
        // OWJam4ModProject.instance.ModHelper.Console.WriteLine($"{this} detect light");

        // HACK HACK HACK HACK
        // FindObjectOfType<ShuttleFlightController>().body.SetVelocity(Vector3.zero);

        LastLightTime = Time.time;
        float DarkDiff = Time.time - LastDarkTime;
        //clears code after MAX_LONG_INTERVAL seconds of inaction
        if (DarkDiff > MAX_LONG_INTERVAL)
        {
            OWJam4ModProject.instance.ModHelper.Console.WriteLine("Reset sensor!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            CodeInput.Clear();
            return;
        }
    }

    private void OnDetectDarkness()
    {
        // OWJam4ModProject.instance.ModHelper.Console.WriteLine($"{this} detect dark");

        LastDarkTime = Time.time;

        //gets length between flashes
        float LightDiff = Time.time - LastLightTime;

        //clears code after MAX_LONG_INTERVAL seconds of inaction
        if (LightDiff > MAX_LONG_INTERVAL)
        {
            CodeInput.Clear();
            return;
        }

        //sets the light to check if it is short and makes it a true or false. true = long. false = short
        bool LightInput = LightDiff > MAX_SHORT_INTERVAL;
        OWJam4ModProject.instance.ModHelper.Console.WriteLine($"{this} input = {LightInput}");

        //adds currently new code input
        CodeInput.Add(LightInput);

        //checks if it is longer than the code and lops off the front
        if (CodeInput.Count > _code.Length)
        {
            CodeInput.RemoveAt(0);
        }

        //check if code is right
        if (CodeInput.Count == _code.Length)
        {
            bool CodeMatch = true;
            for (var i = 0; i < _code.Length; i++)
            {
                if (_code[i] != CodeInput[i])
                {
                    CodeMatch = false;
                    break;
                }
            }
            if (CodeMatch)
            {
                //says code is good
                OnEnterCode?.Invoke();
                //clears code after entry
                ClearCodeAction?.Invoke();
            }
        }
    }
}
