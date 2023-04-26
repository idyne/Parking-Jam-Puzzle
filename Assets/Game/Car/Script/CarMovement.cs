using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames.Core;
using DG.Tweening;

public class CarMovement
{
    private float unitMovementDuration;
    private Transform transform;
    private Car car;
    private Tween tween = null;

    public CarMovement(Car car, float unitMovementDuration)
    {
        this.car = car;
        this.unitMovementDuration = unitMovementDuration;
        transform = car.transform;
    }

    public Tween TurnRight(bool forward = true)
    {
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(transform.DOMove(transform.position + transform.forward * 2, unitMovementDuration * 2));
        float angle = 90;
        float currentAngle = 0;
        float previousAngle = 0;
        Vector3 point = transform.position + transform.forward * 2 + ((car.Length - 2) * 0.5f) * transform.right;
        Vector3 axis = forward ? Vector3.up : Vector3.down;
        mySequence.Append(DOTween.To(() => currentAngle, (float x) => currentAngle = x, angle, unitMovementDuration * 1.5f).OnUpdate(() =>
        {
            float delta = currentAngle - previousAngle;
            transform.RotateAround(point, axis, delta);
            previousAngle = currentAngle;
        }));
        mySequence.Play();
        tween = mySequence;
        return mySequence;
    }

    public void CancelMovement()
    {
        Debug.Log(tween);
        if (tween != null)
        {
            Debug.Log(tween);
            Debug.Log(tween.IsComplete());

        }
        if (tween != null && tween.IsPlaying() && !tween.IsComplete())
        {
            Debug.Log(tween);
            tween.Kill();
            tween = null;
        }
    }


    public Tween TurnRightBackAndForward()
    {
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(transform.DOMove(transform.position - transform.forward * 2, unitMovementDuration * 2));
        float angle = 90;
        float currentAngle = 0;
        float previousAngle = 0;
        Vector3 point = transform.position - transform.forward * 2 + ((car.Length - 2) * 0.5f) * transform.right;
        Vector3 axis = Vector3.down;

        // calculate the rotation quaternion
        Quaternion rotation = Quaternion.AngleAxis(angle, axis);
        // define the point you want to rotate
        Vector3 pointToRotate = transform.position;
        // transform the point to be relative to the pivot point
        pointToRotate = pointToRotate - point;
        // apply the rotation to the point
        pointToRotate = rotation * pointToRotate;
        // transform the point back to its original position
        pointToRotate = pointToRotate + point;
        // pointToRotate is now the rotated point
        Vector3 finalPosition = pointToRotate;
        Quaternion finalRotation = rotation * transform.rotation;

        mySequence.Append(DOTween.To(() => currentAngle, (float x) => currentAngle = x, angle, unitMovementDuration * 2.5f).OnUpdate(() =>
        {
            float delta = currentAngle - previousAngle;
            transform.RotateAround(point, axis, delta);
            previousAngle = currentAngle;
        }));

        Vector3 v_forward = finalRotation * Vector3.forward;
        //mySequence.Append(transform.DOMove(finalPosition + (car.Length - 1) * v_forward, unitMovementDuration * car.Length));
        mySequence.Play();
        tween = mySequence;
        return mySequence;
    }


}
