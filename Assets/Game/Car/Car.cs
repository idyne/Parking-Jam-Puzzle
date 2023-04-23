using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames.Core;
using UnityEngine.Events;
using DG.Tweening;
using PathCreation;

[RequireComponent(typeof(RoadFollower))]
[RequireComponent(typeof(CarUIPlacer))]
public class Car : FateMonoBehaviour, ICarRaycastBlock
{
    [SerializeField] private string carTag = "Car";
    [SerializeField] private float speed = 10;
    private RoadFollower roadFollower;
    [SerializeField] private LayerMask raycastBlockLayerMask;
    [SerializeField] private CarRuntimeSet carRuntimeSet, carsOnRoadRuntimeSet;
    [SerializeField] private int length = 2;
    [SerializeField] private Transform meshTransform;
    public CarMovement movement;
    private CarUIPlacer carUIPlacer;
    [SerializeField] private Transform frontPoint, backPoint;
    [SerializeField] private Collider carCollider;
    public bool Moving = false;
    private bool goingToRoad = false;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool reached = false;
    public int Length { get => length; }
    public string CarTag { get => carTag; }
    public float Speed { get => speed; }
    #region RuntimeSet

    private void OnEnable()
    {
        carRuntimeSet.Add(this);
    }
    private void OnDisable()
    {
        carRuntimeSet.Remove(this);
        carsOnRoadRuntimeSet.Remove(this);
    }
    #endregion

    private void Awake()
    {
        carUIPlacer = GetComponent<CarUIPlacer>();
        roadFollower = GetComponent<RoadFollower>();
        roadFollower.OnReached += OnReachedEndOfRoad;
        movement = new(this, 1 / speed);
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        roadFollower.Speed = speed;
    }

    public void ResetCar()
    {
        roadFollower.StopFollowing();
        StopCar();
        transform.SetPositionAndRotation(initialPosition, initialRotation);
        meshTransform.localPosition = Vector3.zero;
        meshTransform.localRotation = Quaternion.identity;
        Moving = false;
        goingToRoad = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) ResetCar();
    }

    public void StopCar()
    {
        Debug.Log("StopCar", this);
        if (!reached)
        {
            roadFollower.StopFollowing();
            movement.CancelMovement();
        }
    }

    private void OnReachedEndOfRoad()
    {
        carsOnRoadRuntimeSet.Remove(this);
        reached = true;
        CarPlatformController.Instance.Place(this);
    }

    public void Place(CarPlatform carPlatform) => carUIPlacer.Place(carPlatform);
    public void Free() => carUIPlacer.Free();

    public void OnDrag(Vector3 delta)
    {
        Vector3 lookDirection = delta.normalized;
        float dotProduct = Vector3.Dot(lookDirection, transform.forward);
        if (dotProduct < 0)
        {
            Move(DragDirection.Back);
        }
        else
        {
            Move(DragDirection.Forward);
        }
    }

    public void Disappear(System.Action onCompleted)
    {
        //Debug.Log("Disappear", this);
        void onComplete()
        {
            Deactivate();
            onCompleted();
        }
        transform.DOScale(Vector3.zero, 0.1f).OnComplete(onComplete);
    }

    public void DisableCollider()
    {
        carCollider.enabled = false;
    }

    public void Move(DragDirection direction)
    {
        if (Moving || goingToRoad) return;
        LayerMask layerMask = gameObject.layer;
        gameObject.layer = 0;
        Transform point = direction == DragDirection.Forward ? frontPoint : backPoint;
        if (Physics.SphereCast(point.position - point.forward, 0.9f, point.forward, out RaycastHit hit, Mathf.Infinity, raycastBlockLayerMask))
        {
            Moving = true;
            ICarRaycastBlock raycastBlock = hit.transform.GetComponent<ICarRaycastBlock>();
            float hitObjectWidth = raycastBlock.GetWidth(point.forward);
            Vector3 hitObjectOrigin = raycastBlock.GetOriginPosition();
            Vector3 differenceWithHitObjectOrigin = hitObjectOrigin - point.position;
            Vector3 differenceWithHitPoint = Vector3.Project(differenceWithHitObjectOrigin, point.forward);
            Vector3 hitPoint = point.position + point.forward * (differenceWithHitPoint.magnitude - hitObjectWidth / 2f);
            Vector3 currentMeshPosition = meshTransform.position;
            float distance = (hitPoint - point.position).magnitude;
            transform.position += point.forward * distance;
            meshTransform.position = currentMeshPosition;
            if (hit.transform.CompareTag("Road"))
            {
                goingToRoad = true;
                Debug.Log("Hit Road");
            }
            else if (hit.transform.CompareTag("Car"))
            {
                Debug.Log("Hit Car");
                hit.transform.GetComponent<ICarRaycastBlock>().GetWidth(point.forward);
            }
            else if (hit.transform.CompareTag("Block"))
            {
                Debug.Log("Hit Block");
            }
            meshTransform.DOLocalMove(Vector3.zero, distance / speed).OnComplete(() =>
            {
                if (hit.transform.CompareTag("Road"))
                {
                    GetOnRoad(direction);

                }
                else if (hit.transform.CompareTag("Car"))
                {
                    //Debug.Log("Hit Car");
                }
                else if (hit.transform.CompareTag("Block"))
                {
                    //Debug.Log("Hit Block");
                }
                Moving = false;
            });
        }
        gameObject.layer = layerMask;
    }

    private void GetOnRoad(DragDirection direction)
    {
        Transform point = direction == DragDirection.Forward ? frontPoint : backPoint;
        float waitingTime = 0;
        float dist = Road.VertexPath.GetClosestDistanceAlongPath(point.position);
        for (int i = 0; i < carsOnRoadRuntimeSet.Items.Count; i++)
        {
            Car roadCar = carsOnRoadRuntimeSet.Items[i];
            float carDist = Road.VertexPath.GetClosestDistanceAlongPath(roadCar.transform.position);
            float diff = dist - carDist;
            if (diff <= 10)
            {
                waitingTime = (diff + 1) / roadCar.Speed;
            }
        }
        void getOnRoad()
        {
            DisableCollider();
            void onComplete()
            {
                carsOnRoadRuntimeSet.Add(this);
                roadFollower.StartFollowing();
            }
            //Debug.Log("Hit Road");
            if (direction == DragDirection.Back) movement.TurnRightBackAndForward().OnComplete(onComplete);
            else if (direction == DragDirection.Forward) movement.TurnRight().OnComplete(onComplete);
        }
        if (waitingTime > 0)
            DOVirtual.DelayedCall(waitingTime, getOnRoad);
        else getOnRoad();
    }

    public float GetWidth(Vector3 forwardVector)
    {
        Vector3 vector1 = forwardVector;
        Vector3 vector2 = transform.forward;
        float dotProduct = Vector3.Dot(vector1, vector2);
        float magnitude1 = vector1.magnitude;
        float magnitude2 = vector2.magnitude;
        float angle = Mathf.Acos(dotProduct / (magnitude1 * magnitude2)) * Mathf.Rad2Deg;

        if (Mathf.Abs(angle - 0f) <= 5 || Mathf.Abs(angle - 180f) <= 5)
        {
            //Debug.Log("Vectors are parallel");
            return length;
        }
        else if (Mathf.Abs(angle - 90f) <= 5)
        {
            //Debug.Log("Vectors are perpendicular");
            return 2;
        }
        else
        {
            Debug.LogError("Vectors are neither parallel nor perpendicular, angle: " + angle, this);
        }
        return 2;
    }

    public Vector3 GetOriginPosition()
    {
        return transform.position;
    }

    public enum DragDirection { Forward, Back }

}
