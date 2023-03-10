using System;
using System.Collections;
using UnityEngine;

public enum CardOrientation
{
    Face,
    Back
}

public enum CardState
{
    InDeck,
    InBlind,
    ReadyToSwitchWithBlind,
    InHand,
    BeingPlayed,
    Played,
}

public class CardController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private BoxCollider _boxCollider;
    [SerializeField] private Sprite _spriteBack;
    [SerializeField] private float _movementSpeed;
    
    private CardOrientation _cardOrientation; 
    private SO_Card _cardData;

    public CardNumber CardNumber => _cardData.CardNumber;
    public CardSuits CardSuits => _cardData.CardSuits;

    private CardState _cardState;
    public CardState CardState => _cardState;

    private Vector3 _movementTarget;
    private Coroutine _movementCoroutine;
    private bool _isMoving;
    public bool IsMoving => _isMoving;

    public void Setup(SO_Card data)
    {
        _cardData = data;
        _isMoving = false;
        _boxCollider.enabled = false;

        SetOrientation(CardOrientation.Back);
    }

    private void SetOrientation(CardOrientation newCardOrientation)
    {
        _cardOrientation = newCardOrientation;

        switch (_cardOrientation)
        {
            case CardOrientation.Face:
                _spriteRenderer.sprite = _cardData.Sprite;
                _spriteRenderer.sortingOrder = 1;
                break;
            case CardOrientation.Back:
                _spriteRenderer.sprite = _spriteBack;
                _spriteRenderer.sortingOrder = 0;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void UpdatePosition(Vector3 targetPosition)
    {
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f) return; 
        
        if (_isMoving)
        {
            StopCoroutine(_movementCoroutine);
        }
        
        _movementTarget = targetPosition;
        _movementCoroutine = StartCoroutine(MoveTo());
    }

    public void SetAsTrumpCard(Vector3 deckPosition)
    {
        _boxCollider.enabled = true;
        
        UpdatePosition(deckPosition);
        SetOrientation(CardOrientation.Face);
    }

    public void AddToHand(CardOrientation cardOrientation)
    {
        _cardState = CardState.InHand;
        _boxCollider.enabled = true;
        SetOrientation(cardOrientation);
    }
    
    public void AddToBlind(Vector3 targetPosition)
    {
        _cardState = CardState.InBlind;
        _boxCollider.enabled = true;
        SetOrientation(CardOrientation.Back);
        
        _movementTarget = targetPosition;
        _movementCoroutine = StartCoroutine(MoveTo());
    }

    public void SetReadyToSwitchWithBlind(Vector3 offsetReadyToSwitch)
    {
        _cardState = CardState.ReadyToSwitchWithBlind;
        
        _movementTarget += offsetReadyToSwitch;
        _movementCoroutine = StartCoroutine(MoveTo());
    }
    
    public void UnsetReadyToSwitchWithBlind(Vector3 offsetReadyToSwitch)
    {
        _cardState = CardState.InHand;
        
        _movementTarget -= offsetReadyToSwitch;
        _movementCoroutine = StartCoroutine(MoveTo());
    }

    public void Play(Vector3 targetPosition)
    {
        _cardState = CardState.BeingPlayed;
        
        _movementTarget = targetPosition;
        _movementCoroutine = StartCoroutine(MoveTo());
        _boxCollider.enabled = false;
    }

    public void EndPlay(Vector3 targetPosition)
    {
        _cardState = CardState.Played;
        
        _movementTarget = targetPosition;
        _movementCoroutine = StartCoroutine(MoveTo());
    }

    private IEnumerator MoveTo()
    {
        _isMoving = true;

        while (Vector3.Distance(transform.position, _movementTarget) > 0.01f)
        {
            var direction = _movementTarget - transform.position;
            if (direction.magnitude > 1.0f)
            {
                direction.Normalize();
            }
            transform.position += direction * _movementSpeed * Time.deltaTime;
            yield return null;
        }
        
        _isMoving = false;
    }

    public int GetScore(CardController trumpCard)
    {
        if (CardSuits == trumpCard.CardSuits && (CardNumber == CardNumber.Jack || CardNumber == CardNumber.Nine))
        {
            if (CardNumber == CardNumber.Jack)
            {
                return 20;
            }
            else
            {
                return 14;
            }
        }
        else
        {
            switch (CardNumber)
            {
                case CardNumber.Six:
                case CardNumber.Seven:
                case CardNumber.Eight:
                case CardNumber.Nine:
                    return 0;
                case CardNumber.Ten:
                    return 10;
                case CardNumber.Jack:
                    return 2;
                case CardNumber.Queen:
                    return 3;
                case CardNumber.King:
                    return 4;
                case CardNumber.Ace:
                    return 11;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
