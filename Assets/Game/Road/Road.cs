using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FateGames.Core;
using PathCreation;
public class Road : FateMonoBehaviour
{
    private static Road instance = null;
    public static Road Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<Road>();
            return instance;
        }
    }
    [SerializeField] private GameObject cornerLeftForwardRoad2x2Prefab;
    [SerializeField] private GameObject cornerRightForwardRoad2x2Prefab;
    [SerializeField] private GameObject cornerLeftBackRoad2x2Prefab;
    [SerializeField] private GameObject roadEnterExit4x18Prefab;
    [SerializeField] private GameObject horizontalRoad1x2Prefab;
    [SerializeField] private GameObject verticalRoad3x2Prefab;
    [SerializeField] private GameObject parkingLotForward3x1LeftPrefab;
    [SerializeField] private GameObject parkingLotForward3x1RightPrefab;
    [SerializeField] private GameObject parkingLotBack3x1LeftPrefab;
    [SerializeField] private GameObject parkingLotBack3x1RightPrefab;
    [SerializeField] private GameObject verticalRoad2x1Prefab;
    [SerializeField] private GameObject parkingLot1x1Prefab;
    private PathCreator pathCreator;
    public static BezierPath BezierPath => Instance.PathCreator.bezierPath;
    public static VertexPath VertexPath => Instance.PathCreator.path;

    private Transform container;
    [SerializeField] public int lastBuiltRoadParkingLotWidth;
    [SerializeField] public int lastBuiltRoadParkingLotLength;
    public PathCreator PathCreator { get => pathCreator; }
    public Vector3 origin => transform.position + lastBuiltRoadParkingLotWidth / 2f * Vector3.right + lastBuiltRoadParkingLotLength / 2f * Vector3.back;

    private void Awake()
    {

        pathCreator = GetComponent<PathCreator>();
    }

    private void BuildPath(int parkingLotWidth, int parkingLotLength)
    {
        Vector3[] pathPoints = new Vector3[5] {
            new Vector3(parkingLotWidth + 1,0,-parkingLotLength - 3),
            new Vector3(1,0,-parkingLotLength - 3),
            new Vector3(1,0,-1),
            new Vector3(parkingLotWidth + 3,0,-1),
            new Vector3(parkingLotWidth + 3, 0, -parkingLotLength - 5)
        };
        BezierPath bezierPath = new BezierPath(pathPoints, false, PathSpace.xyz);
        bezierPath.ControlPointMode = BezierPath.ControlMode.Automatic;
        bezierPath.AutoControlLength = 0.01f;
        bezierPath.GlobalNormalsAngle = 90;
        PathCreator pathCreator = GetComponent<PathCreator>();
        pathCreator.bezierPath = bezierPath;
    }
    public void Build(int parkingLotWidth, int parkingLotLength)
    {
        if (!container)
        {
            container = transform.Find("Mesh");
        }
        if (container)
        {
            DestroyImmediate(container.gameObject);
            container = null;
        }
        container = new GameObject("Mesh").transform;
        container.SetParent(transform);
        Vector3 cursor = transform.position;
        BuildPath(parkingLotWidth, parkingLotLength);
        Instantiate(cornerLeftForwardRoad2x2Prefab, cursor, Quaternion.identity, container);
        cursor += 2 * Vector3.right;
        for (int i = 0; i < parkingLotWidth; i++)
        {
            Instantiate(horizontalRoad1x2Prefab, cursor, Quaternion.identity, container);
            cursor += Vector3.right;
        }
        Instantiate(cornerRightForwardRoad2x2Prefab, cursor, Quaternion.identity, container);
        cursor = Vector3.back * 2;
        Instantiate(verticalRoad3x2Prefab, cursor, Quaternion.identity, container);
        cursor += Vector3.right * 2;
        for (int i = 0; i < parkingLotWidth; i++)
        {
            if (i % 2 == 0)
                Instantiate(parkingLotForward3x1LeftPrefab, cursor, Quaternion.identity, container);
            else
                Instantiate(parkingLotForward3x1RightPrefab, cursor, Quaternion.identity, container);
            cursor += Vector3.right;
        }
        Instantiate(verticalRoad3x2Prefab, cursor, Quaternion.identity, container);
        cursor = Vector3.back * 5;
        for (int i = 0; i < parkingLotLength - 6; i++)
        {
            Instantiate(verticalRoad2x1Prefab, cursor, Quaternion.identity, container);
            cursor += Vector3.right * 2;
            for (int j = 0; j < parkingLotWidth; j++)
            {
                Instantiate(parkingLot1x1Prefab, cursor, Quaternion.identity, container);
                cursor += Vector3.right;
            }
            Instantiate(verticalRoad2x1Prefab, cursor, Quaternion.identity, container);
            cursor = (6 + i) * Vector3.back;
        }
        Instantiate(verticalRoad3x2Prefab, cursor, Quaternion.identity, container);
        cursor += Vector3.right * 2;
        for (int i = 0; i < parkingLotWidth; i++)
        {
            if (i % 2 == 0)
                Instantiate(parkingLotBack3x1LeftPrefab, cursor, Quaternion.identity, container);
            else
                Instantiate(parkingLotBack3x1RightPrefab, cursor, Quaternion.identity, container);
            cursor += Vector3.right;
        }
        Instantiate(verticalRoad3x2Prefab, cursor, Quaternion.identity, container);
        cursor = new Vector3(0, 0, cursor.z - 3);
        Instantiate(cornerLeftBackRoad2x2Prefab, cursor, Quaternion.identity, container);
        cursor += Vector3.right * 2;
        for (int i = 0; i < parkingLotWidth - 2; i++)
        {
            Instantiate(horizontalRoad1x2Prefab, cursor, Quaternion.identity, container);
            cursor += Vector3.right;
        }
        Instantiate(roadEnterExit4x18Prefab, cursor, Quaternion.identity, container);
        lastBuiltRoadParkingLotWidth = parkingLotWidth;
        lastBuiltRoadParkingLotLength = parkingLotLength;
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(Road))]
public class RoadEditor : Editor
{
    int parkingLotWidth = 5;
    int parkingLotLength = 5;
    private Road road;
    private void OnEnable()
    {
        road = target as Road;
        parkingLotWidth = road.lastBuiltRoadParkingLotWidth;
        parkingLotLength = road.lastBuiltRoadParkingLotLength;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space(20);
        parkingLotWidth = Mathf.Clamp(EditorGUILayout.IntField("Parking Lot Width", parkingLotWidth), 6, int.MaxValue);
        if (parkingLotWidth % 2 == 1)
            parkingLotWidth = Mathf.RoundToInt(parkingLotWidth / 2f) * 2;
        parkingLotLength = Mathf.Clamp(EditorGUILayout.IntField("Parking Lot Length", parkingLotLength), 6, int.MaxValue);
        if (GUILayout.Button("Build"))
            road.Build(parkingLotWidth, parkingLotLength);
    }
}
#endif