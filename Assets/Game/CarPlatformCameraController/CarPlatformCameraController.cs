using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarPlatformCameraController : MonoBehaviour
{
    [SerializeField] private Transform platformContainer;
    [SerializeField] private float minDistance, maxDistance;
    [SerializeField] private float minAspectRatio, maxAspectRatio;

    private void Awake()
    {
        UpdateAspectRatio();
    }
    /*
    private void Update()
    {
        Debug.DrawRay(platformContainer.position, platformContainer.up * 50, Color.black);
        UpdateAspectRatio();
    }
    */
    private void UpdateAspectRatio()
    {
        float aspectRatio = Screen.width / (float)Screen.height;
        //print("originalAspectRatio: " + aspectRatio);
        aspectRatio = Mathf.Clamp(aspectRatio, minAspectRatio, maxAspectRatio);
        //print("aspectRatio: " + aspectRatio);
        float ratio = (aspectRatio - minAspectRatio) / (maxAspectRatio - minAspectRatio);
        //print("ratio: " + ratio);
        float distance = (minDistance - maxDistance) * ratio + maxDistance;
        //print("distance: " + distance);
        platformContainer.position = transform.position - platformContainer.up * distance;
    }
}
