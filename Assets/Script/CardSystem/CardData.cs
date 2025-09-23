using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardData
{
    public int id;
    public string cardName;
    public string description;
    public CardType cardType; // Buff - Debuff

}

public enum CardType
{
    Buff,
    Debuff
}
