using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames.Core;

public class NoInternetPanel : MonoBehaviour
{
    [SerializeField] private Canvas canvas;

    private void Start()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            canvas.enabled = true;
        }
    }
    public void Retry()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            canvas.enabled = false;
        }
    }
}
