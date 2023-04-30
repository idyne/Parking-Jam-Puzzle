using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames.Core;
using DG.Tweening;
public class CarPlatformController : FateMonoBehaviour
{
    private static CarPlatformController instance = null;
    public static CarPlatformController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = instance = FindObjectOfType<CarPlatformController>();
            }
            return instance;
        }
    }
    [SerializeField] private CarPlatformRuntimeSet carPlatformRuntimeSet, occupiedCarPlatformRuntimeSet, availableCarPlatformRuntimeSet;
    [SerializeField] private CarRuntimeSet carRuntimeSet;
    private List<Car> carsOnPlatforms = new();
    private Dictionary<string, List<Car>> carTable = new();
    private List<Car> removeQueue = new();

    public void Reset()
    {
        carsOnPlatforms = new();
        carTable = new();
        removeQueue = new();
    }

    public void Place(Car car)
    {
        if (availableCarPlatformRuntimeSet.Items.Count == 0)
        {
            Debug.LogError("No empty platform");
        }
        bool found = false;
        int index = 0;
        for (int i = 0; i < occupiedCarPlatformRuntimeSet.Items.Count; i++)
        {
            if (occupiedCarPlatformRuntimeSet.Items[i].Occupier.CarTag == car.CarTag)
            {
                found = true;
                index = i;
            }
        }
        if (found) Insert(car, index + 1);
        else Append(car);
        if (!carTable.ContainsKey(car.CarTag))
            carTable.Add(car.CarTag, new List<Car>());
        carTable[car.CarTag].Add(car);
        PlaceCars();
        if (CheckMatch(out string carTag))
        {
            for (int i = 0; i < 3; i++)
            {
                removeQueue.Add(carTable[carTag][i]);
            }
            DOVirtual.DelayedCall(0.4f, () =>
            {
                IEnumerator removeRoutine(int count = 3)
                {
                    if (count == 0)
                    {
                        PlaceCars();
                        yield break;
                    }
                    RemoveFromQueue();
                    yield return new WaitForSeconds(0.05f);
                    yield return removeRoutine(count - 1);
                }
                StartCoroutine(removeRoutine(3));
            });
        }
        else
        {
            DOVirtual.DelayedCall(0.4f, () =>
            {
                CheckGameFinishCondition();
            });

        }
    }

    private void Insert(Car car, int index)
    {
        carsOnPlatforms.Insert(index, car);
    }

    private void Append(Car car)
    {
        carsOnPlatforms.Add(car);
    }

    private void Remove(Car car)
    {
        car.Free();
        carsOnPlatforms.Remove(car);
        carTable[car.CarTag].Remove(car);
        car.Disappear(() => { CheckGameFinishCondition(); });
    }

    private void RemoveFromQueue()
    {
        if (removeQueue.Count == 0) return;
        Car car = removeQueue[0];
        removeQueue.RemoveAt(0);
        Remove(car);
    }

    private bool CheckGameFinishCondition()
    {
        //print("Remaining cars: " + carRuntimeSet.Items.Count);
        //print("Remaining availableCarPlatformRuntimeSet: " + availableCarPlatformRuntimeSet.Items.Count);
        //print("removeQueueCount: " + removeQueue.Count);
        if (carRuntimeSet.Items.Count == 0)
        {
            GameManager.Instance.FinishLevel(true);
            return true;
        }
        else if (occupiedCarPlatformRuntimeSet.Items.Count - removeQueue.Count >= 7)
        {
            GameManager.Instance.FinishLevel(false);
            return true;
        }
        return false;
    }

    public void Stop()
    {
        DOTween.Kill(transform);
    }

    private void PlaceCars()
    {

        //Debug.Log("occupiedCarPlatformRuntimeSet: " + occupiedCarPlatformRuntimeSet.Items.Count);
        while (occupiedCarPlatformRuntimeSet.Items.Count > 0)
        {
            occupiedCarPlatformRuntimeSet.Items[0].Free();
        }
        //Debug.Log("availableCarPlatformRuntimeSet: " + availableCarPlatformRuntimeSet.Items.Count);
        for (int i = 0; i < carsOnPlatforms.Count; i++)
        {
            Car carOnPlatform = carsOnPlatforms[i];
            carOnPlatform.Place(carPlatformRuntimeSet.Items[i]);
        }
    }
    private bool CheckMatch(out string carTag)
    {
        carTag = "";
        foreach (string key in carTable.Keys)
        {
            int count = 0;
            for (int i = 0; i < carTable[key].Count; i++)
            {
                Car car = carTable[key][i];
                if (!removeQueue.Contains(car))
                    count++;
            }
            if (count >= 3)
            {
                carTag = key;
                return true;
            }
        }
        return false;
    }
}
