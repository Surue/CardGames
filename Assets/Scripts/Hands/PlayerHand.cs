using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    [SerializeField] protected float _spaceBetweenCards = 2.0f;
    [SerializeField] protected CardOrientation _defaultCardOrientation = CardOrientation.Face;
    [SerializeField] protected Transform _playPosition;
    [SerializeField] protected Transform _winHandPosition;
    [SerializeField] protected Transform _blindPosition;
    [SerializeField] protected Vector3 _offsetReadySwitchBlind;
    
    protected List<CardController> _availableCards;
    protected List<CardController> _playedCards;
    protected List<CardController> _blindCards;
    
    protected bool _hasPlayed = false;
    private bool _hasFinishedFirstTurn;
    public bool HasPlayed => _hasPlayed;

    protected bool _isPlaying = false;
    
    protected CardController _lastCardPlayed;
    public CardController PlayedCard => _lastCardPlayed;
    protected bool _hasPlayedACard;

    protected bool _hasSwitchBlind;
    protected bool _isSwitchingWithBlind;
    protected List<CardController> _cardToSwitchWithBlind;

    private int _score = 0;
    public int Score => _score;

    protected CardController _firstCardPlayed;
    protected bool _playFirst;

    protected bool _hasSwitchTrumpCard;
    public bool HasSwitchTrumpCard => _hasSwitchTrumpCard;

    private void Awake()
    {
        _availableCards = new List<CardController>();
        _playedCards = new List<CardController>();
        _blindCards = new List<CardController>();
        _cardToSwitchWithBlind = new List<CardController>();
    }

    private void LateUpdate()
    {
        if (_hasPlayedACard && !_lastCardPlayed.IsMoving)
        {
            _hasPlayedACard = false;
            _hasPlayed = true;
        }

        if (_isSwitchingWithBlind)
        {
            if (_cardToSwitchWithBlind.Count == 3)
            {
                _isSwitchingWithBlind = false;
                _hasSwitchBlind = true;

                foreach (var cardController in _blindCards)
                {
                    cardController.AddToHand(_defaultCardOrientation);
                    _availableCards.Add(cardController);
                }
                
                _blindCards.Clear();

                foreach (var cardController in _cardToSwitchWithBlind)
                {
                    cardController.AddToBlind(_blindPosition.position);
                    _availableCards.Remove(cardController);
                    _blindCards.Add(cardController);
                }
                
                ResetCardsPosition();
            }
        }
    }

    public void AddCardToHand(CardController newCard)
    {
        _availableCards.Add(newCard);
        newCard.transform.parent = transform;
        newCard.AddToHand(_defaultCardOrientation);
        
        ResetCardsPosition();
    }

    public void AddCardToBlind(CardController newCard)
    {
        _blindCards.Add(newCard);
        newCard.transform.parent = transform;
        newCard.AddToBlind(_blindPosition.position);
    }
    
    public void RemoveCard(CardController cardToRemove)
    {
        _availableCards.Remove(cardToRemove);
        ResetCardsPosition();
    }

    public void StartTurn()
    {
        _isPlaying = true;
        _playFirst = true;
    }
    
    public void StartTurn(CardController firstCardPlayed)
    {
        _isPlaying = true;
        _firstCardPlayed = firstCardPlayed;
        _playFirst = false;
    }

    public void EndTurn()
    {
        _hasPlayed = false;
        _hasFinishedFirstTurn = true;
    }

    public void Win(CardController otherCard, int score)
    {
        _score += score;
        
        _lastCardPlayed.EndPlay(_winHandPosition.position);
        otherCard.EndPlay(_winHandPosition.position);
    }

    protected void ResetCardsPosition()
    {
        var cardCount = _availableCards.Count;
        var halfLength = cardCount * 0.5f * _spaceBetweenCards;

        var currentPosition = transform.position;
        
        for (var i = 0; i < _availableCards.Count; i++)
        {
            var cardController = _availableCards[i];
            if (cardCount % 2 == 0)
            {
                Vector3 newPosition;
                newPosition.x = currentPosition.x - halfLength + i * _spaceBetweenCards + _spaceBetweenCards * 0.5f;
                newPosition.y = currentPosition.y;
                newPosition.z = currentPosition.z;
                
                cardController.UpdatePosition(newPosition);
            }
            else
            {
                Vector3 newPosition;
                newPosition.x = currentPosition.x - halfLength + i * _spaceBetweenCards;
                newPosition.y = currentPosition.y;
                newPosition.z = currentPosition.z;
                
                cardController.UpdatePosition(newPosition);
            }
        }
    }

    protected bool CanSwitchTrumpCard()
    {
        if (!GameManager.Instance.CanSwitchTrumpCard) return false;
        if (_hasFinishedFirstTurn) return false;
        
        var trumpCard = GameManager.Instance.TrumpCard;
        foreach (var cardController in _availableCards)
        {
            if (cardController.CardSuits == trumpCard.CardSuits && cardController.CardNumber == CardNumber.Six)
            {
                return true;
            }
        }

        return false;
    }

    protected CardController GetCardToSwitchWithTrumpCard()
    {
        var trumpCard = GameManager.Instance.TrumpCard;
        foreach (var cardController in _availableCards)
        {
            if (cardController.CardSuits == trumpCard.CardSuits && cardController.CardNumber == CardNumber.Six)
            {
                return cardController;
            }
        }

        return null;
    }

    protected List<CardController> GetListOfPossibleCardToPlay(CardController firstCardPlayed)
    {
        var result = new List<CardController>();
        bool hasCardOfSameSuits = false;

        var trumpCardSuits = GameManager.Instance.TrumpCard.CardSuits;
        
        foreach (var availableCard in _availableCards)       
        {
            if (availableCard.CardSuits == trumpCardSuits)
            {
                result.Add(availableCard);
            }

            if (availableCard.CardSuits == firstCardPlayed.CardSuits)
            {
                hasCardOfSameSuits = true;
                result.Add(availableCard);
            }
        }

        // Doesn't force the player to play the "buur"
        if (result.Count == 1 && result[0].CardSuits == trumpCardSuits && result[0].CardNumber == CardNumber.Jack)
        {
            hasCardOfSameSuits = false;
        }

        if (hasCardOfSameSuits)
        {
            return result;
        }
        else
        {
            return _availableCards;
        }
    }
    
    protected List<CardController> GetListOfPossibleCardToPlay(List<CardController> availableCards, CardController firstCardPlayed)
    {
        var result = new List<CardController>();
        bool hasCardOfSameSuits = false;

        var trumpCardSuits = GameManager.Instance.TrumpCard.CardSuits;
        
        foreach (var availableCard in availableCards)       
        {
            if (availableCard.CardSuits == trumpCardSuits)
            {
                result.Add(availableCard);
            }

            if (availableCard.CardSuits == firstCardPlayed.CardSuits)
            {
                hasCardOfSameSuits = true;
                result.Add(availableCard);
            }
        }

        // Doesn't force the player to play the "buur"
        if (result.Count == 1 && result[0].CardSuits == trumpCardSuits && result[0].CardNumber == CardNumber.Jack)
        {
            hasCardOfSameSuits = false;
        }

        if (hasCardOfSameSuits)
        {
            return result;
        }
        else
        {
            return availableCards;
        }
    }

    protected bool CanSwitchBlind()
    {
        return !_firstCardPlayed && !_hasSwitchBlind;
    }

    public List<CardController> CardsPlayed()
    {
        return _playedCards;
    }
    
    public List<CardController> CardsInHand()
    {
        return _availableCards;
    }
    
    public List<CardController> CardsInBlind()
    {
        return _blindCards;
    }
}
