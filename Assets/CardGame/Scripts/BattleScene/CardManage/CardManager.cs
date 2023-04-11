using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(CardPoolManager))]
public class CardManager : MonoBehaviour
{
    [Header("����")]
    [Tooltip("ī�� ���� �� ��Ȱ�� ���� ��ũ��Ʈ")]
    [SerializeField] CardPoolManager cardPoolManager;
    [Tooltip("�ܿ� ���� ǥ�� UI")]
    [SerializeField] TextMeshProUGUI manaTMP;
    [Tooltip("�� ���� ��, ī�� ���� ���� �̹���")]
    [SerializeField] GameObject blockImg;

    [Header("ī�� ���� ����")]
    [Tooltip("���� �� ȹ���� ī�� ����")]
    [SerializeField] int startCard = 5;
    [Tooltip("�� �� ȹ���� ī�� ����")]
    [SerializeField] int turnCard = 1;
    [Tooltip("���� �� �ִ� �ִ� ī�� ����")]
    [SerializeField] int maxCard = 10;
    List<SOCard> cardList;

    [Header("���� ���� ����")]
    [Tooltip("���� �� ȹ���� ����")]
    [SerializeField] int startMana = 5;
    [Tooltip("���� ȹ���� ����")]
    [SerializeField] int turnMana = 1;
    int runtimeMana = 0;
    int runtimeCardCount = 0;

    private void Awake()
    {
        if (cardPoolManager == null) this.GetComponent<CardPoolManager>();

        SetMana(startMana);
        blockImg.SetActive(false);
    }

    private void Start()
    {
        //Static Class�� CardStorage�� �ִ� ��� ī�� �����͸� �ҷ��´�.
        cardList = CardStorage.GetAllCardDatas();

        Debug.Log(cardList.Count);
        foreach (var data in cardList)
        {
            Debug.Log(data.name);
        }
        //cardList�� �����Ϳ� ����Ͽ� ī�� ����
        MakeCard(startCard);
    }

    /// <summary>
    /// ����� Random���� ����Ͽ� ī�带 �����Ѵ�.
    /// </summary>
    /// <param name="num">���� ī�� ��</param>
    public void MakeCard(int num)
    {
        if (runtimeCardCount >= maxCard) return;

        for(int i=0; i < num; i++)
        {
            int randNum = Random.Range(0, cardList.Count);
            var cardData = cardList[randNum];
            var card = cardPoolManager.pool.Get();
            card.Setting(cardData,this);

            runtimeCardCount++;
            if (runtimeCardCount >= maxCard) break;
        }
    }

    public void ReleaseCard()
    {
        runtimeCardCount--;
    }

    /// <summary>
    /// ���� Ȱ��ȭ�Ǿ� �ִ� ī���� ���� ��ȯ
    /// </summary>
    /// <returns></returns>
    public int GetAliveCardCount()
    {
        return runtimeCardCount;
    }

    public void OnGameState(EGameState gameState)
    {
        switch (gameState)
        {
            case EGameState.��������:
                blockImg.SetActive(true);
                break;
            case EGameState.�Ͻ���:
                blockImg.SetActive(true);
                break;
            case EGameState.������:
                NextTurn();
                break;
        }
    }

    public void NextTurn()
    {
        SetMana(turnMana);
        MakeCard(turnCard);
        blockImg.SetActive(false);
    }

    public int GetMana()
    {
        return runtimeMana;
    }

    /// <summary>
    /// ������ �� ������ ��� SetMana�� ���ؼ� �̷������.
    /// </summary>
    /// <param name="value"></param>
    public void SetMana(int value)
    {
        runtimeMana += value;
        manaTMP.text = runtimeMana.ToString();
    }
}