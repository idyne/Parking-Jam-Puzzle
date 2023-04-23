using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames.Core;
public class CarPlatform : FateMonoBehaviour
{
    [SerializeField] private int order = 0;
    [SerializeField] private CarPlatformRuntimeSet carPlatformRuntimeSet, availableCarPlatformRuntimeSet, occupiedCarPlatformRuntimeSet;
    private bool occupied => occupier != null;

    public int Order { get => order; }
    public Car Occupier { get => occupier; }

    private Car occupier;

    public void GetOccupied(Car by)
    {
        if (occupied) { Debug.LogError("Occupied", this); return; }
        occupier = by;
        availableCarPlatformRuntimeSet.Remove(this);
        occupiedCarPlatformRuntimeSet.Add(this);
    }

    public void Free()
    {
        if (!occupied) { return; }
        Car previousOccupier = occupier;
        occupier = null;
        previousOccupier.Free();
        occupiedCarPlatformRuntimeSet.Remove(this);
        availableCarPlatformRuntimeSet.Add(this);
    }


    private void OnEnable()
    {
        carPlatformRuntimeSet.Add(this);
        availableCarPlatformRuntimeSet.Add(this);
    }
    private void OnDisable()
    {
        carPlatformRuntimeSet.Remove(this);
        availableCarPlatformRuntimeSet.Remove(this);
        occupiedCarPlatformRuntimeSet.Remove(this);
    }
}
