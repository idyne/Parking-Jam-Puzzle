using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames.Core;

public class Block : FateMonoBehaviour, ICarRaycastBlock
{
    [SerializeField] private int width = 2, length = 1;
    [SerializeField] private LayerMask gridLayerMask;

    public Vector3 GetOriginPosition()
    {
        return transform.position;
    }

    public float GetWidth(Vector3 forwardVector)
    {
        Vector3 vector1 = forwardVector;
        Vector3 vector2 = transform.forward;
        float dotProduct = Vector3.Dot(vector1, vector2);
        float magnitude1 = vector1.magnitude;
        float magnitude2 = vector2.magnitude;
        float angle = Mathf.Acos(dotProduct / (magnitude1 * magnitude2)) * Mathf.Rad2Deg;
        Debug.Log(angle);
        if (Mathf.Abs(angle - 0f) <= 5 || Mathf.Abs(angle - 180f) <= 5)
        {
            //Debug.Log("Vectors are parallel");
            return length;
        }
        else if (Mathf.Abs(angle - 90f) <= 5)
        {
            //Debug.Log("Vectors are perpendicular");
            return width;
        }
        else
        {

            Debug.LogError("Vectors are neither parallel nor perpendicular");
        }
        return 2;
    }
}
