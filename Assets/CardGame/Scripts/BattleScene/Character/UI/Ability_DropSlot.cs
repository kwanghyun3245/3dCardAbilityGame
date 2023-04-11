﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Ability_DropSlot : MonoBehaviour, IDropHandler
{
    [SerializeField] GameObject character;
    [SerializeField] GameObject dropDetecter;
    ICardTargetable target;

    private void Awake()
    {
        target = character.GetComponent<ICardTargetable>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameCard gameCard = eventData.pointerDrag.gameObject.GetComponent<GameCard>();

        if (gameCard != null && target != null)
        {
            gameCard.Active(target);
        }
        else
        {
            CustomDebugger.Debug(LogType.LogWarning, "None Target Or Not Found GameCard");
        }
    }
}