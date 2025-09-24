using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/CardDataBase")]
public class CardDataBase : ScriptableObject
{
    public List<CardData> allCards = new List<CardData>();

    public CardData GetCardById(int id)
    {
        return allCards.Find(card => card.id == id);
    }
}
