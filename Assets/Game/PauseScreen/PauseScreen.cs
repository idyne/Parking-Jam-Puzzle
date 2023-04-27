using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames.Core;

public class PauseScreen : FateMonoBehaviour
{
    [SerializeField] private Canvas canvas;

    public void Hide() => canvas.enabled = false;
    public void Show() => canvas.enabled = true;

}
