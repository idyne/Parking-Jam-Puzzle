using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FateGames.Core;
using DG.Tweening;
using UnityEngine.Events;

public class XPanel : MonoBehaviour
{
    [SerializeField] private SaveDataVariable saveData;
    [SerializeField] private float speed = 1;
    [SerializeField] private RectTransform cursor;
    [SerializeField] private TextMeshProUGUI coinText, claimText;
    [SerializeField] private float baseCoin = 11.25f;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private RectTransform from, to;
    [SerializeField] private UnityEvent onCoinAdded;
    private int coin = 0;
    private float multiplier = 1;
    private int goneCoinCount = 0;
    private bool claimed = false;

    private void Update()
    {
        if (claimed) return;
        float value = Mathf.Sin(Time.time * speed);
        cursor.anchoredPosition = new Vector2(value * 291, cursor.anchoredPosition.y);

        if (value > 0.6f) multiplier = 2;
        else if (value > 0.2f) multiplier = 3;
        else if (value > -0.2f) multiplier = 5;
        else if (value > -0.6f) multiplier = 3;
        else multiplier = 2;
        coin = Mathf.CeilToInt(baseCoin * multiplier * saveData.Value.Level);
        coinText.text = "+" + coin;
        claimText.text = "Claim " + multiplier + "X";
    }

    public void Claim()
    {
        claimed = true;
        int coinCount = 20;
        int singleCoinValue = coin / coinCount;
        for (int i = 0; i < coinCount - 1; i++)
        {
            StartCoroutine(CoinRoutine(i * 0.05f + 0.1f, singleCoinValue));
        }
        int remainder = coin - (singleCoinValue * coinCount);
        StartCoroutine(CoinRoutine(coinCount * 0.05f + 0.1f, singleCoinValue + remainder));
        IEnumerator coinAnimationRoutine()
        {
            yield return new WaitUntil(() => goneCoinCount >= coinCount);
            GameManager.Instance.LoadCurrentLevel();
        }
        StartCoroutine(coinAnimationRoutine());
    }

    public void NoThanks()
    {
        int coin = Mathf.CeilToInt(baseCoin * saveData.Value.Level);
        int coinCount = 10;
        int singleCoinValue = coin / coinCount;
        for (int i = 0; i < coinCount - 1; i++)
        {
            StartCoroutine(CoinRoutine(i * 0.05f + 0.1f, singleCoinValue));
        }
        int remainder = coin - (singleCoinValue * coinCount);
        StartCoroutine(CoinRoutine(coinCount * 0.05f + 0.1f, singleCoinValue + remainder));
        IEnumerator coinAnimationRoutine()
        {
            yield return new WaitUntil(() => goneCoinCount >= coinCount);
            GameManager.Instance.LoadCurrentLevel();
        }
        StartCoroutine(coinAnimationRoutine());
    }

    private IEnumerator CoinRoutine(float delayAfterSpread, int value)
    {
        bool spread = false;
        bool goneToField = false;
        RectTransform coinTransform = Instantiate(coinPrefab, transform).GetComponent<RectTransform>();
        coinTransform.position = from.position;
        coinTransform.DOMove(coinTransform.position + Quaternion.Euler(0, 0, Random.Range(0f, 360f)) * Vector2.up * Random.Range(10f, 100f), 0.3f).SetEase(Ease.InOutCirc).OnComplete(() => { spread = true; });
        yield return new WaitUntil(() => spread);
        yield return new WaitForSeconds(delayAfterSpread);
        coinTransform.DOMove(to.position, 1f).SetEase(Ease.InOutCubic).OnComplete(() => { goneToField = true; });
        yield return new WaitUntil(() => goneToField);
        saveData.Value.Coin += value;
        onCoinAdded.Invoke();
        goneCoinCount++;
    }
}
