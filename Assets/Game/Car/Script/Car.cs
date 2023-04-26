using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames.Core;
using UnityEngine.Events;
using DG.Tweening;
using PathCreation;
using System.Linq;

[RequireComponent(typeof(RoadFollower))]
[RequireComponent(typeof(CarUIPlacer))]
public class Car : FateMonoBehaviour, ICarRaycastBlock, IShakeable
{
    [SerializeField] private EffectPool tossEffectPool, disappearEffectPool;
    [SerializeField] private MeshRenderer bodyMeshRenderer;
    [SerializeField] private EffectPool[] madEffectPools;
    [SerializeField] private Color color = Color.white;
    [SerializeField] private SoundEntity carHitSound;
    [SerializeField] private SoundEntity carDriveSound;
    [SerializeField] private SoundEntity hornSound;
    [SerializeField] private SoundEntity goToPlatformSound;
    [SerializeField] private SoundEntity disappearSound;
    private SoundWorker carDriveSoundWorker;
    [SerializeField] private Animator shakeableAnimator;
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
    private float lastGoMadTime = float.MinValue;
    private float goMadCooldown = 1.5f;
    private bool reached = false;
    private List<Car> sortedCarsOnRoad = new();
    public int Length { get => length; }
    public string CarTag { get => carTag; }
    public float Speed { get => speed; }
    public Transform FrontPoint { get => frontPoint; }
    public Transform BackPoint { get => backPoint; }
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
        bodyMeshRenderer.material.color = color;
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


    public void StopCar()
    {
        if (!reached)
        {
            Debug.Log("StopCar", this);
            DOTween.Pause(transform);
            DOTween.Pause(meshTransform);
            roadFollower.StopFollowing();
            movement.CancelMovement();
        }
    }

    private void OnReachedEndOfRoad()
    {
        DOTween.To(() => carDriveSoundWorker.AudioSource.volume, (float x) => carDriveSoundWorker.AudioSource.volume = x, 0, 0.1f);
        GameManager.Instance.PlaySound(goToPlatformSound, transform.position);
        carsOnRoadRuntimeSet.Remove(this);
        reached = true;
        CarPlatformController.Instance.Place(this);
    }

    public void Place(CarPlatform carPlatform)
    {
        carUIPlacer.Place(carPlatform);
    }
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
            GameManager.Instance.PlaySound(disappearSound, transform.position);
            PooledEffect disappearEffect = disappearEffectPool.Get();
            disappearEffect.transform.position = transform.position + Vector3.up * 2;
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
            shakeableAnimator.enabled = false;
            Transform hitTransform = hit.transform;
            Moving = true;
            ICarRaycastBlock raycastBlock = hitTransform.GetComponent<ICarRaycastBlock>();
            float hitObjectWidth = raycastBlock.GetWidth(point.forward);
            Vector3 hitObjectOrigin = raycastBlock.GetOriginPosition();
            Vector3 differenceWithHitObjectOrigin = hitObjectOrigin - point.position;
            Vector3 differenceWithHitPoint = Vector3.Project(differenceWithHitObjectOrigin, point.forward);
            Vector3 hitPoint = point.position + point.forward * (differenceWithHitPoint.magnitude - hitObjectWidth / 2f);
            Vector3 currentMeshPosition = meshTransform.position;
            float distance = (hitPoint - point.position).magnitude;
            transform.position += point.forward * distance;
            meshTransform.position = currentMeshPosition;
            if (hitTransform.CompareTag("Road"))
            {
                goingToRoad = true;
                DOVirtual.DelayedCall(CalculateWaitingTime(direction, distance), () =>
                {
                    carDriveSoundWorker = GameManager.Instance.PlaySound(carDriveSound, point.position);
                    carDriveSoundWorker.transform.SetParent(transform);
                });
            }
            else if (hitTransform.CompareTag("Car"))
            {
                hit.transform.GetComponent<ICarRaycastBlock>().GetWidth(point.forward);
            }
            meshTransform.DOLocalMove(Vector3.zero, distance / speed).OnComplete(() =>
            {
                if (hitTransform.CompareTag("Road"))
                {
                    GetOnRoad(direction);

                }
                else
                {
                    if (hitTransform.CompareTag("Car"))
                    {
                        Car otherCar = hitTransform.GetComponent<Car>();
                        otherCar.GoMad();
                    }
                    PooledEffect tossEffect = tossEffectPool.Get();
                    tossEffect.transform.position = point.position;
                    tossEffect.transform.forward = point.forward;
                    GameManager.Instance.PlaySound(carHitSound, point.position);
                    shakeableAnimator.enabled = true;
                    shakeableAnimator.SetTrigger(direction == DragDirection.Forward ? "HitForward" : "HitBack");
                    IShakeable shakeable = hitTransform.GetComponent<IShakeable>();
                    if (shakeable != null)
                    {
                        shakeable.Shake(transform);
                    }
                }
                Moving = false;
            });
        }
        gameObject.layer = layerMask;
    }

    private List<Car> SortCarsOnRoad()
    {
        sortedCarsOnRoad = carsOnRoadRuntimeSet.Items.OrderBy((car) => -Road.VertexPath.GetClosestDistanceAlongPath(car.backPoint.position)).ToList();
        return sortedCarsOnRoad;
    }

    private float CalculateWaitingTime(DragDirection direction, float distanceToRoad)
    {
        SortCarsOnRoad();
        Transform point = direction == DragDirection.Forward ? frontPoint : backPoint;
        float positionOnRoad = Road.VertexPath.GetClosestDistanceAlongPath(point.position + point.forward * (distanceToRoad + 1));
        if (sortedCarsOnRoad.Count == 0) return 0;

        Car closestCarOnRoad = sortedCarsOnRoad[0];
        float closestCarOnRoadPositionOnRoad = Road.VertexPath.GetClosestDistanceAlongPath(closestCarOnRoad.backPoint.position);

        float difference = positionOnRoad - closestCarOnRoadPositionOnRoad;
        float waitingTime = difference / closestCarOnRoad.speed - distanceToRoad / speed;
        if (waitingTime > 1) return 0;
        if (sortedCarsOnRoad.Count == 1)
            return waitingTime;
        for (int i = 0; i < sortedCarsOnRoad.Count - 1; i++)
        {
            Car carOnRoad = sortedCarsOnRoad[i];
            Car carBehindOnRoad = sortedCarsOnRoad[i + 1];
            float carOnRoadPositionOnRoad = Road.VertexPath.GetClosestDistanceAlongPath(carOnRoad.backPoint.position);
            float carBehindOnRoadPositionOnRoad = Road.VertexPath.GetClosestDistanceAlongPath(carBehindOnRoad.frontPoint.position);
            float space = carOnRoadPositionOnRoad - carBehindOnRoadPositionOnRoad;
            if (space > length + 5)
            {
                difference = positionOnRoad - carOnRoadPositionOnRoad;
                waitingTime = difference / carOnRoad.speed - distanceToRoad / speed;
                return waitingTime;
            }
        }
        Car furthestCarOnRoad = sortedCarsOnRoad[^1];
        float furthestCarOnRoadPositionOnRoad = Road.VertexPath.GetClosestDistanceAlongPath(furthestCarOnRoad.backPoint.position);
        difference = positionOnRoad - furthestCarOnRoadPositionOnRoad;
        waitingTime = difference / furthestCarOnRoad.speed - distanceToRoad / speed;
        return waitingTime;

    }

    public void StopDriveSound()
    {
        if (carDriveSoundWorker != null)
            carDriveSoundWorker.Stop();
    }

    public void GoMad()
    {
        if (lastGoMadTime + goMadCooldown > Time.time) return;
        lastGoMadTime = Time.time;
        PooledEffect madEffect = madEffectPools[Random.Range(0, madEffectPools.Length)].Get();
        madEffect.transform.position = transform.position + Vector3.up * 2;
        madEffect.transform.SetParent(meshTransform);
    }

    public void Horn()
    {
        GameManager.Instance.PlaySound(hornSound);
    }

    private void GetOnRoad(DragDirection direction)
    {
        float waitingTime = CalculateWaitingTime(direction, 0);
        Transform point = direction == DragDirection.Forward ? frontPoint : backPoint;

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
        {
            GoMad();
            Horn();
            DOVirtual.DelayedCall(waitingTime, getOnRoad);
        }
        else getOnRoad();
    }
    #region ICarRaycastBlock
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
    #endregion
    #region IShakeable
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
                //Debug.Log("object2 is on the relative right of object1");
            }
            else if (dotProductRight < 0)
            {
                //Debug.Log("object2 is on the relative left of object1");
                shakeableAnimator.SetTrigger("Left");
            }
            else
            {
                //Debug.Log("object1 and object2 are in the same position relative to each other on the x-axis");
            }
        }
        else
        {
            if (dotProductForward > 0)
            {
                //Debug.Log("object2 is on the relative forward of object1");
                shakeableAnimator.SetTrigger("Forward");
            }
            else if (dotProductForward < 0)
            {
                //Debug.Log("object2 is on the relative back of object1");
                shakeableAnimator.SetTrigger("Back");
            }
            else
            {
                //Debug.Log("object1 and object2 are in the same position relative to each other on the z-axis");
            }
        }

    }
    #endregion
    public enum DragDirection { Forward, Back }

}
