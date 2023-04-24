using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarPlatformCameraController : MonoBehaviour
{
    [SerializeField] private Transform platformContainer;
    [SerializeField] private float minHeight, maxHeight;
    [SerializeField] private float minAspectRatio, maxAspectRatio;

    private void Awake()
    {
        UpdateAspectRatio();
    }



    private void UpdateAspectRatio()
    {
        float aspectRatio = Screen.width / (float)Screen.height;
        //print("originalAspectRatio: " + aspectRatio);
        aspectRatio = Mathf.Clamp(aspectRatio, minAspectRatio, maxAspectRatio);
        //print("aspectRatio: " + aspectRatio);
        float ratio = (aspectRatio - minAspectRatio) / (maxAspectRatio - minAspectRatio);
        //print("ratio: " + ratio);
        float height = (maxHeight - minHeight) * ratio + minHeight;
        //print("height: " + ratio);
        Vector3 pos = platformContainer.position;
        pos.y = height;
        platformContainer.position = pos;
    }
}
