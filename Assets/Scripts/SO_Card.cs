using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardSuits
{
    Heart, 
    Club,
    Spade,
    Diamond
}

public enum CardNumber
{
    Six, 
    Seven,
    Eight,
    Nine,
    Ten,
    Jack,
    Queen,
    King,
    Ace
}

[CreateAssetMenu(menuName = "SO/Card")]
public class SO_Card : ScriptableObject
{
    [SerializeField] private CardSuits _cardSuits;
    public CardSuits CardSuits => _cardSuits;
    
    [SerializeField] private CardNumber _cardNumber;
    public CardNumber CardNumber => _cardNumber;
    
    [SerializeField] private Sprite _sprite;
    public Sprite Sprite => _sprite;
}
