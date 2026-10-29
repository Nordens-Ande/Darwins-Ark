using UnityEngine;
using System.Collections.Generic;
using System.Resources;
using UnityEngine.Assemblies;
using System;
using System.Runtime.CompilerServices;

public class RainDeityManager : SingletonManager<RainDeityManager>
{
    private CurrencyManager currencyManager;

    private int baseCost;

    private bool isRaining;
    private float rainDuration;
    private float rainTimer;

    public event Action OnRainStarted;
    public event Action OnRainStopped;

    public string GetCostString => baseCost.ToString(); //for ui?

    private void Start()
    {
        currencyManager = CurrencyManager.Instance;
        baseCost = 10;
        isRaining = false;
        rainDuration = 5;
        rainTimer = 0f;
    }

    public void TriggerRain() // on button press
    {
        if (isRaining) return;
        if (!currencyManager.SpendDeityPoints(baseCost)) return;

        OnRainStarted?.Invoke();
        isRaining = true;
    }

    private void Update()
    {
        if (!isRaining) return;

        if(rainTimer < rainDuration)
        {
            isRaining = false;
            OnRainStopped?.Invoke();
        }
    }
}
