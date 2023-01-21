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
    InHand,
    BeingPlayed,
    Played,
}

public class CardController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
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

        SetOrientation(CardOrientation.Back);
    }

    public void SetOrientation(CardOrientation newCardOrientation)
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

    public void AddToHand()
    {
        _cardState = CardState.InHand;
    }

    public void Play(Vector3 targetPosition)
    {
        _cardState = CardState.BeingPlayed;
        
        _movementTarget = targetPosition;
        _movementCoroutine = StartCoroutine(MoveTo());
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
