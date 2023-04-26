using PathCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames.Core;
public class RoadFollower : FateMonoBehaviour
{


    public float Speed = 10;
    public float TurningSpeed = 15;
    private bool follow = false;
    private float currentDistance = 0;
    public event System.Action OnReached;



    private void Update()
    {
        if (!follow) return;
        if (currentDistance >= Road.VertexPath.length)
        {
            follow = false;
            OnReached();
            return;
        }
        transform.position = Road.VertexPath.GetPointAtDistance(currentDistance);
        transform.rotation = Quaternion.Lerp(transform.rotation, Road.VertexPath.GetRotationAtDistance(currentDistance), Time.deltaTime * TurningSpeed);
        currentDistance += Time.deltaTime * Speed;
    }

    public void StartFollowing()
    {
        follow = true;
        currentDistance = Road.VertexPath.GetClosestDistanceAlongPath(transform.position);
    }
    public void StopFollowing()
    {
        follow = false;
    }
}
