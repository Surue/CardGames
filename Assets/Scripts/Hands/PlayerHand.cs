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
    
    protected List<CardController> _availableCards;
    protected List<CardController> _playedCards;
    protected bool _hasPlayed = false;
    public bool HasPlayed => _hasPlayed;

    protected bool _isPlaying = false;
    
    protected CardController _lastCardPlayed;
    public CardController PlayedCard => _lastCardPlayed;
    protected bool _hasPlayedACard;

    private int _score = 0;
    public int Score => _score;
    
    private void Start()
    {
        _availableCards = new List<CardController>();
        _playedCards = new List<CardController>();
    }

    private void LateUpdate()
    {
        if (_hasPlayedACard && !_lastCardPlayed.IsMoving)
        {
            _hasPlayedACard = false;
            _hasPlayed = true;
        }
    }

    public void AddCard(CardController newCard)
    {
        _availableCards.Add(newCard);
        newCard.transform.parent = transform;
        newCard.SetOrientation(_defaultCardOrientation);
        newCard.AddToHand();
        
        ResetCardsPosition();
    }

    public void StartTurn()
    {
        _isPlaying = true;
    }

    public void EndTurn()
    {
        _hasPlayed = false;
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
}
