using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames.Core;

public class Block : FateMonoBehaviour, ICarRaycastBlock, IShakeable
{
    [SerializeField] private Animator shakeableAnimator;
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

    public void Shake(Transform hitterTransform)
    {
        // Assume you have two objects: object1 and object2

        Vector3 relativePos = hitterTransform.position - transform.position;
        float dotProductRight = Vector3.Dot(relativePos, transform.right);
        float dotProductForward = Vector3.Dot(relativePos, transform.forward);
        if (Mathf.Abs(dotProductRight) > Mathf.Abs(dotProductForward))
        {
            if (dotProductRight > 0)
            {
                shakeableAnimator.SetTrigger("Right");
                Debug.Log("object2 is on the relative right of object1");
            }
            else if (dotProductRight < 0)
            {
                Debug.Log("object2 is on the relative left of object1");
                shakeableAnimator.SetTrigger("Left");
            }
            else
            {
                Debug.Log("object1 and object2 are in the same position relative to each other on the x-axis");
            }
        }
        else
        {
            if (dotProductForward > 0)
            {
                Debug.Log("object2 is on the relative forward of object1");
                shakeableAnimator.SetTrigger("Forward");
            }
            else if (dotProductForward < 0)
            {
                Debug.Log("object2 is on the relative back of object1");
                shakeableAnimator.SetTrigger("Back");
            }
            else
            {
                Debug.Log("object1 and object2 are in the same position relative to each other on the z-axis");
            }
        }
    }
}
