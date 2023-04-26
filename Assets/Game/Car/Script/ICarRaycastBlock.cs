using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICarRaycastBlock
{
    public float GetWidth(Vector3 forwardVector);
    public Vector3 GetOriginPosition();
}
