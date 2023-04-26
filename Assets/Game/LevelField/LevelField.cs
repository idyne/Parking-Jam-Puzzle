using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames.Core;
using TMPro;
public class LevelField : FateMonoBehaviour
{
    [SerializeField] private SaveDataVariable saveData;
    [SerializeField] private TextMeshProUGUI levelText;

    private void Start()
    {
        levelText.text = "Level " + saveData.Value.Level;
    }
}
