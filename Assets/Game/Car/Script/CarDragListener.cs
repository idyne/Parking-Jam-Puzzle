using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames.Core;

public class CarDragListener : MonoBehaviour
{
    [SerializeField] private LayerMask carLayerMask;
    [SerializeField] private GameStateVariable gameState;
    private Vector2 mousePositionOnSelect;
    private Camera mainCamera;
    private Car selectedCar;

    private void Awake()
    {
        mainCamera = Camera.main;
    }
    private void Update()
    {
        if (gameState.Value == GameState.IN_GAME)
            CheckDrag();
    }

    private void CheckDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Input.mousePosition;
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, carLayerMask))
            {
                selectedCar = hit.transform.GetComponent<Car>();
                mousePositionOnSelect = mousePosition;
            }
        }
        else if (selectedCar && Input.GetMouseButton(0))
        {
            Vector2 mousePosition = Input.mousePosition;
            Vector2 vector2Delta = mousePosition - mousePositionOnSelect;
            Vector3 vector3Delta = new Vector3(vector2Delta.x, 0, vector2Delta.y);
            if (Vector2.Distance(mousePosition, mousePositionOnSelect) > Screen.width / 20f)
            {
                selectedCar.OnDrag(vector3Delta);
                selectedCar = null;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            selectedCar = null;
        }
    }
}
