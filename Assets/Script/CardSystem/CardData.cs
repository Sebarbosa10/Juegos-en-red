using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardData
{
    public int id;
    public string cardName;
    public string description;
    public CardEffectType cardEffectType; 

}

public enum CardEffectType
{
    SlipperyFeet,
    RandomSensitivity,
    LightingStop,
    HeavyWeight,
   
}
