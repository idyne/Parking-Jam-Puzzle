using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using FateGames.Core;

public class LevelGenerator : FateMonoBehaviour
{
    [SerializeField] private LayerMask occupierLayerMask, blockLayerMask;
    [SerializeField] private int width = 4, length = 6;
    [SerializeField] private CarToGenerate[] carsToGenerate;
    [SerializeField] private GameObject roadBlockPrefab;
    [SerializeField] private GameObject[] blockPrefabs;
    [SerializeField] private GameObject roadPrefab;
    [SerializeField] private int numberOfBlocks = 1;
    private List<Car> cars = new();
    private List<GameObject> gameObjectsToDestroy = new();

    [System.Serializable]
    public class CarToGenerate
    {
        public int width = 2, length = 3;
        public GameObject prefab;
        public int groupCount = 1;
    }



    public void Generate()
    {
        gameObjectsToDestroy.Clear();
        cars.Clear();
        while (transform.childCount > 0) DestroyImmediate(transform.GetChild(0).gameObject);
        GenerateRoad();
        GenerateBlocks();
        for (int i = 0; i < carsToGenerate.Length; i++)
        {
            CarToGenerate carToGenerate = carsToGenerate[i];
            for (int j = 0; j < carToGenerate.groupCount; j++)
            {
                Transform container = transform.Find(carToGenerate.prefab.name + "-" + j);

                if (container)
                {
                    DestroyImmediate(container.gameObject);
                }
                container = new GameObject(carToGenerate.prefab.name + "-" + j).transform;
                container.SetParent(transform);
                int count = 0;
                int numberOfCars = 0;
                while (count++ < 10000 && numberOfCars < 3)
                {
                    Car.DragDirection direction = Random.value < 0.5f ? Car.DragDirection.Forward : Car.DragDirection.Back;
                    if (direction == Car.DragDirection.Forward)
                    {
                        Vector3 position = new(carToGenerate.width / 2f + Random.Range(0, width - carToGenerate.width + 1), 0, -(carToGenerate.length / 2f + Random.Range(0, length - carToGenerate.length + 1)));
                        Collider[] colliders = Physics.OverlapBox(transform.position + position, new Vector3(carToGenerate.width / 2f - 0.1f, 2, carToGenerate.length / 2f - 0.1f), Quaternion.identity, occupierLayerMask, QueryTriggerInteraction.Collide);
                        if (colliders.Length >= 1)
                        {
                            continue;
                        }
                        else
                        {
                            Car car = (PrefabUtility.InstantiatePrefab(carToGenerate.prefab, container) as GameObject).GetComponent<Car>();
                            car.transform.SetPositionAndRotation(position + transform.position, Quaternion.Euler(0, Random.value > 0.5f ? 0 : 180, 0));
                            cars.Add(car);
                            if (AreCarsBlocked())
                            {
                                cars.Remove(car);
                                DestroyImmediate(car.gameObject);
                                continue;
                            }
                            else
                            {
                                gameObjectsToDestroy.Add(Instantiate(carToGenerate.prefab, position + transform.position, Quaternion.Euler(0, Random.value > 0.5f ? 0 : 180, 0), container));
                                numberOfCars++;

                            }
                        }
                    }
                    else
                    {
                        Vector3 position = new(carToGenerate.length / 2f + Random.Range(0, width - carToGenerate.length + 1), 0, -(carToGenerate.width / 2f + Random.Range(0, length - carToGenerate.width + 1)));
                        Collider[] colliders = Physics.OverlapBox(transform.position + position, new Vector3(carToGenerate.length / 2f - 0.1f, 2, carToGenerate.width / 2f - 0.1f), Quaternion.identity, occupierLayerMask, QueryTriggerInteraction.Collide);
                        if (colliders.Length >= 1)
                        {
                            continue;
                        }
                        else
                        {
                            Car car = (PrefabUtility.InstantiatePrefab(carToGenerate.prefab, container) as GameObject).GetComponent<Car>();
                            car.transform.SetPositionAndRotation(position + transform.position, Quaternion.Euler(0, Random.value > 0.5f ? 90 : -90, 0));
                            cars.Add(car);
                            if (AreCarsBlocked())
                            {
                                cars.Remove(car);
                                DestroyImmediate(car.gameObject);
                                continue;
                            }
                            else
                            {
                                gameObjectsToDestroy.Add(Instantiate(carToGenerate.prefab, position + transform.position, Quaternion.Euler(0, Random.value > 0.5f ? 90 : -90, 0), container));
                                numberOfCars++;

                            }
                        }
                    }

                }
                if (count == 10001)
                {
                    Generate();
                    return;
                }
            }


        }

        GenerateRoadBlocks();
        while (gameObjectsToDestroy.Count > 0)
        {
            GameObject gameObjectToDestroy = gameObjectsToDestroy[0];
            gameObjectsToDestroy.RemoveAt(0);
            DestroyImmediate(gameObjectToDestroy);
        }
    }

    private void GenerateBlocks()
    {
        for (int i = 0; i < numberOfBlocks; i++)
        {
            int count = 0;
            while (count++ < 10000)
            {
                Vector3 position = new(0.5f + Random.Range(0, width), 0, -(0.5f + Random.Range(0, length)));
                Collider[] colliders = Physics.OverlapBox(transform.position + position, new Vector3(0.4f, 2, 0.4f), Quaternion.identity, occupierLayerMask, QueryTriggerInteraction.Collide);
                if (colliders.Length == 0)
                {
                    GameObject block = Instantiate(blockPrefabs[Random.Range(0, blockPrefabs.Length)], transform.position + position, Quaternion.identity, transform);
                    if (AreCarsBlocked())
                    {
                        DestroyImmediate(block);
                        continue;
                    }
                    break;
                }
            }
        }
    }


    public void GenerateRoadBlocks()
    {
        for (int i = 0; i < cars.Count; i++)
        {
            Car car = cars[i];
            float dot = Quaternion.Dot(Quaternion.LookRotation(car.transform.forward), Quaternion.LookRotation(Vector3.forward));
            //Vertical car
            if (dot < 0.5f || dot > 0.9f)
            {
                float x = Random.value;
                Vector3 pos = car.transform.position;
                if (x > 0.5f)
                    pos.z = transform.position.z + 0.5f;
                else
                    pos.z = transform.position.z - length - 0.5f;
                Collider[] colliders = Physics.OverlapBox(pos, new Vector3(1 - 0.1f, 2, 0.5f - 0.1f), Quaternion.identity, occupierLayerMask, QueryTriggerInteraction.Collide);
                bool tryOtherSide = false;
                if (colliders.Length == 0)
                {
                    GameObject roadBlock = Instantiate(roadBlockPrefab, pos, Quaternion.identity, transform);
                    if (AreCarsBlocked())
                    {
                        DestroyImmediate(roadBlock);
                        tryOtherSide = true;
                    }
                }
                else
                {
                    tryOtherSide = true;
                }
                if (tryOtherSide)
                {
                    x = 1 - x;
                    pos = car.transform.position;
                    if (x > 0.5f)
                        pos.z = transform.position.z + 0.5f;
                    else
                        pos.z = transform.position.z - length - 0.5f;
                    colliders = Physics.OverlapBox(pos, new Vector3(1 - 0.1f, 2, 0.5f - 0.1f), Quaternion.identity, occupierLayerMask, QueryTriggerInteraction.Collide);
                    if (colliders.Length == 0)
                    {
                        GameObject roadBlock = Instantiate(roadBlockPrefab, pos, Quaternion.identity, transform);
                        if (AreCarsBlocked())
                        {
                            DestroyImmediate(roadBlock);
                        }
                    }
                }

            }
            //Horizontal car
            else
            {
                float x = Random.value;
                Vector3 pos = car.transform.position;
                if (x > 0.5f)
                    pos.x = transform.position.x - 0.5f;
                else
                    pos.x = transform.position.x + width + 0.5f;
                Collider[] colliders = Physics.OverlapBox(pos, new Vector3(0.5f - 0.1f, 2, 1f - 0.1f), Quaternion.identity, occupierLayerMask, QueryTriggerInteraction.Collide);
                bool tryOtherSide = false;
                if (colliders.Length == 0)
                {
                    GameObject roadBlock = Instantiate(roadBlockPrefab, pos, Quaternion.Euler(0, 90, 0), transform);
                    if (AreCarsBlocked())
                    {
                        DestroyImmediate(roadBlock);
                        tryOtherSide = true;
                    }
                }
                else
                {
                    tryOtherSide = true;
                }
                if (tryOtherSide)
                {
                    x = 1 - x;
                    pos = car.transform.position;
                    if (x > 0.5f)
                        pos.x = transform.position.x - 0.5f;
                    else
                        pos.x = transform.position.x + width + 0.5f;
                    colliders = Physics.OverlapBox(pos, new Vector3(0.5f - 0.1f, 2, 1f - 0.1f), Quaternion.identity, occupierLayerMask, QueryTriggerInteraction.Collide);
                    if (colliders.Length == 0)
                    {
                        GameObject roadBlock = Instantiate(roadBlockPrefab, pos, Quaternion.Euler(0, 90, 0), transform);
                        if (AreCarsBlocked())
                        {
                            DestroyImmediate(roadBlock);
                        }
                    }
                }
            }

        }
    }
    private bool AreCarsBlocked()
    {
        for (int i = 0; i < cars.Count; i++)
        {
            Car car = cars[i];
            if (IsAxisBlocked(car))
                return true;
            /*
            float dot = Quaternion.Dot(Quaternion.LookRotation(car.transform.forward), Quaternion.LookRotation(Vector3.forward));
            //Vertical car
            if (dot < 0.5f || dot > 0.9f)
            {
                bool forwardBlocked = IsForwardRoadBlocked(car);
                bool backBlocked = IsBackRoadBlocked(car);
                if (backBlocked && forwardBlocked)
                    return true;
            }
            //Horizontal car
            else
            {
                bool rightBlocked = IsRightRoadBlocked(car);
                bool leftBlocked = IsLeftRoadBlocked(car);
                if (rightBlocked && leftBlocked)
                    return true;
            }*/
        }
        return false;
    }
    private void GenerateRoad()
    {

        Road road = (PrefabUtility.InstantiatePrefab(roadPrefab) as GameObject).GetComponent<Road>();
        road.transform.SetParent(transform);
        road.Build(width + 2, length + 2);
    }

    private bool IsAxisBlocked(Car car)
    {
        int overlapBoxLength = width > length ? width : length;
        Collider[] colliders = Physics.OverlapBox(car.transform.position, new Vector3(0.9f, 2, overlapBoxLength - 0.1f), car.transform.rotation, blockLayerMask, QueryTriggerInteraction.Collide);
        int numberOfBlocksInBehind = 0;
        int numberOfBlocksInFront = 0;
        for (int i = 0; i < colliders.Length; i++)
        {
            // Assume you have two objects: object1 and object2

            Vector3 relativePos = colliders[i].transform.position - car.transform.position;
            float dotProductForward = Vector3.Dot(relativePos, car.transform.forward);

            if (dotProductForward > 0)
            {
                numberOfBlocksInFront++;
            }
            else if (dotProductForward < 0)
            {
                numberOfBlocksInBehind++;
            }
            else
            {
                Debug.Log("object1 and object2 are in the same position relative to each other on the z-axis");
            }

        }
        return numberOfBlocksInBehind > 0 && numberOfBlocksInFront > 0;
    }

    private bool IsLeftRoadBlocked(Car car)
    {
        Vector3 pos = car.transform.position;
        pos.x = transform.position.x - 0.5f;
        Collider[] colliders = Physics.OverlapBox(pos, new Vector3(0.5f - 0.1f, 2, 1f - 0.1f), Quaternion.identity, occupierLayerMask, QueryTriggerInteraction.Collide);
        bool leftBlocked = colliders.Length > 0;
        return leftBlocked;
    }
    private bool IsRightRoadBlocked(Car car)
    {
        Vector3 pos = car.transform.position;
        pos.x = transform.position.x + width + 0.5f;
        Collider[] colliders = Physics.OverlapBox(pos, new Vector3(0.5f - 0.1f, 2, 1f - 0.1f), Quaternion.identity, occupierLayerMask, QueryTriggerInteraction.Collide);
        bool rightBlocked = colliders.Length > 0;
        return rightBlocked;
    }
    private bool IsForwardRoadBlocked(Car car)
    {
        Vector3 pos = car.transform.position;
        pos.z = transform.position.z + 0.5f;
        Collider[] colliders = Physics.OverlapBox(pos, new Vector3(1f - 0.1f, 2, 0.5f - 0.1f), Quaternion.identity, occupierLayerMask, QueryTriggerInteraction.Collide);
        bool forwardBlocked = colliders.Length > 0;
        return forwardBlocked;
    }
    private bool IsBackRoadBlocked(Car car)
    {
        Vector3 pos = car.transform.position;
        pos.z = transform.position.z - length - 0.5f;
        Collider[] colliders = Physics.OverlapBox(pos, new Vector3(1f - 0.1f, 2, 0.5f - 0.1f), Quaternion.identity, occupierLayerMask, QueryTriggerInteraction.Collide);
        bool forwardBlocked = colliders.Length > 0;
        return forwardBlocked;
    }

}
#if UNITY_EDITOR
[CustomEditor(typeof(LevelGenerator))]
public class LevelGeneratorEditor : Editor
{
    private LevelGenerator levelGenerator;
    private void OnEnable()
    {
        levelGenerator = target as LevelGenerator;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space(20);
        if (GUILayout.Button("Generate"))
            levelGenerator.Generate();
    }
}
#endif