using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames.Core;
using DG.Tweening;
using UnityEngine.Events;

[RequireComponent(typeof(Car))]
public class CarUIPlacer : FateMonoBehaviour
{
    [SerializeField] private string layerName = "UILayout";
    [SerializeField] private Vector3 localPositionOnTile;
    [SerializeField] private Quaternion localRotationOnTile;
    [SerializeField] private CarPlatformRuntimeSet availableCarPlatformRuntimeSet;
    private CarPlatform carPlatform;
    private Car car;
    private Tween moveTween, rotateTween;

    private void Awake()
    {
        car = GetComponent<Car>();
    }

    public void Place(CarPlatform carPlatform)
    {
        if (!this.carPlatform)
        {
            SetLayerRecursively(gameObject);
        }
        else
        {
            Free();
        }
        this.carPlatform = carPlatform;
        carPlatform.GetOccupied(car);
        transform.SetParent(carPlatform.transform);
        CancelTweens();
        moveTween = transform.DOLocalMove(localPositionOnTile, 0.3f).OnComplete(() => { moveTween = null; });
        rotateTween = transform.DOLocalRotateQuaternion(localRotationOnTile, 0.2f).OnComplete(() => { rotateTween = null; });
    }

    private void CancelTweens()
    {
        if (moveTween != null)
        {
            moveTween.Kill();
            moveTween = null;
        }
        if (rotateTween != null)
        {
            rotateTween.Kill();
            rotateTween = null;
        }
    }

    public void Free()
    {
        if (!carPlatform) return;
        CarPlatform previousCarPlatform = carPlatform;
        carPlatform = null;
        previousCarPlatform.Free();
    }

    public void SetLayerRecursively(GameObject obj)
    {
        obj.layer = LayerMask.NameToLayer(layerName);
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject);
        }
    }



}
