using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PlayerType
{
    Human,
    CPU
}

public class GameManager : MonoBehaviour
{
    [SerializeField] private SO_Deck _deck;
    [SerializeField] private GameObject _cardPrefab;
    [SerializeField] private Transform _deckSpawnPosition;
    [SerializeField] private PlayerHand _humanHand;
    [SerializeField] private PlayerHand _CPUHand;
    
    private Stack<CardController> _cards;
    private CardController _originalTrumpCard;
    public CardController OriginalTrumpCard => _originalTrumpCard;
    private CardController _trumpCard;
    public CardController TrumpCard => _trumpCard;
    private bool _hasSwitchedTrumpCard = false;
    public bool CanSwitchTrumpCard => !_hasSwitchedTrumpCard;

    private int _nbCardPlayThisTurn = 0;
    private int _turnPlayed = 0;
    private const int c_maxTurn = 9;

    private enum GameState
    {
        Setup,
        HumanTurn,
        CPUTurn,
        AnalyzeResultTurn,
        EndGame
    }

    private GameState _gameState = GameState.Setup;

    private PlayerType _turnFirstPlayer = PlayerType.Human;

    private static GameManager s_gameManager;
    public static GameManager Instance => s_gameManager;
    private void Awake()
    {
        if (s_gameManager == null)
        {
            s_gameManager = this;
        }
    }

    private void Start()
    {
        _cards = new Stack<CardController>(_deck.OrderedCards.Count);
        foreach (var cardData in _deck.ShuffledCards)
        {
            var instance = Instantiate(_cardPrefab, _deckSpawnPosition);

            var cardController = instance.GetComponent<CardController>();
            cardController.Setup(cardData);
            _cards.Push(cardController);
        }

        // Cards to hand
        for (int i = 0; i < 9; i++)
        {
            _humanHand.AddCardToHand(_cards.Pop());
            _CPUHand.AddCardToHand(_cards.Pop());
        }
        
        // Cards to blind
        for (int i = 0; i < 3; i++)
        {
            _humanHand.AddCardToBlind(_cards.Pop());
            _CPUHand.AddCardToBlind(_cards.Pop());
        }

        // Select trump card
        _trumpCard = _cards.Pop();
        _trumpCard.SetAsTrumpCard(_deckSpawnPosition.position);
        _originalTrumpCard = _trumpCard;
        
        _gameState = GameState.HumanTurn;
        _humanHand.StartTurn();
    }

    private void Update()
    {
        switch (_gameState)
        {
            case GameState.Setup:
                break;
            case GameState.HumanTurn:
                if (_humanHand.HasPlayed)
                {
                    _humanHand.EndTurn();
                    _nbCardPlayThisTurn++;

                    if (_nbCardPlayThisTurn == 2)
                    {
                        _gameState = GameState.AnalyzeResultTurn;
                    }
                    else
                    {
                        _CPUHand.StartTurn(_humanHand.PlayedCard);
                        _gameState = GameState.CPUTurn;
                    }
                }
                break;
            case GameState.CPUTurn:
                if (_CPUHand.HasPlayed)
                {
                    _CPUHand.EndTurn();
                    _nbCardPlayThisTurn++;

                    if (_nbCardPlayThisTurn == 2)
                    {
                        _gameState = GameState.AnalyzeResultTurn;
                    }
                    else
                    {
                        _humanHand.StartTurn(_CPUHand.PlayedCard);
                        _gameState = GameState.HumanTurn;
                    }
                }
                break;
            case GameState.AnalyzeResultTurn:
                _nbCardPlayThisTurn = 0;
                _turnPlayed++;
                
                var humanCard = _humanHand.PlayedCard;
                var cpuCard = _CPUHand.PlayedCard;

                PlayerType firstPlayer;
                PlayerType secondPlayer;

                CardController firstCard;
                CardController secondCard;
                if (_turnFirstPlayer == PlayerType.Human)
                {
                    firstPlayer = PlayerType.Human;
                    secondPlayer = PlayerType.CPU;

                    firstCard = humanCard;
                    secondCard = cpuCard;
                }
                else
                {
                    firstPlayer = PlayerType.CPU;
                    secondPlayer = PlayerType.Human;
                    
                    firstCard = cpuCard;
                    secondCard = humanCard;
                }

                var turnWinner = GetTurnWinner(firstPlayer, secondPlayer, firstCard, secondCard);
                var turnScore = GetTurnScore(firstCard, secondCard);

                if (_turnPlayed != c_maxTurn)
                {
                    if (turnWinner == PlayerType.Human)
                    {
                        _humanHand.Win(cpuCard, turnScore);
                        _humanHand.StartTurn();
                        _gameState = GameState.HumanTurn;
                        _turnFirstPlayer = PlayerType.Human;
                    }
                    else
                    {
                        _CPUHand.Win(humanCard, turnScore);
                        _CPUHand.StartTurn();
                        _gameState = GameState.CPUTurn;
                        _turnFirstPlayer = PlayerType.CPU;
                    }
                }
                else
                {
                    if (turnWinner == PlayerType.Human)
                    {
                        _humanHand.Win(cpuCard, turnScore);
                    }
                    else
                    {
                        _CPUHand.Win(humanCard, turnScore);
                    }

                    if (_humanHand.Score > _CPUHand.Score)
                    {
                        Debug.Log("The human win with " + _humanHand.Score);
                        Debug.Log("The CPU lose with " + _CPUHand.Score);
                    }
                    else if(_humanHand.Score < _CPUHand.Score)
                    {
                        Debug.Log("The CPU win with " + _CPUHand.Score);
                        Debug.Log("The human lose with " + _humanHand.Score);
                    }
                    else
                    {
                        Debug.Log("It's a draw, each player has " + _humanHand.Score);
                    }
                    
                    _gameState = GameState.EndGame;
                }
                
                break;
            case GameState.EndGame:
                
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public PlayerType GetTurnWinner(PlayerType firstPlayer, PlayerType secondPlayer, CardController firstCard, CardController secondCard)
    {
        // Trump
        if (firstCard.CardSuits == _trumpCard.CardSuits)
        {
            if (secondCard.CardSuits == _trumpCard.CardSuits)
            {
                if (firstCard.CardNumber == CardNumber.Jack)
                {
                    return firstPlayer;
                }

                if (secondCard.CardNumber == CardNumber.Jack)
                {
                    return secondPlayer;
                }
                
                if (firstCard.CardNumber == CardNumber.Nine)
                {
                    return firstPlayer;
                }

                if (secondCard.CardNumber == CardNumber.Nine)
                {
                    return secondPlayer;
                }
                
                if ((int)firstCard.CardNumber > (int)secondCard.CardNumber)
                {
                    return firstPlayer;
                }
                else
                {
                    return secondPlayer;
                }
            }
            else
            {
                return firstPlayer;
            }
        }
        else if (secondCard.CardSuits == _trumpCard.CardSuits)
        {
            return secondPlayer;
        }
        else
        {
            if (firstCard.CardSuits == secondCard.CardSuits)
            {
                if ((int)firstCard.CardNumber > (int)secondCard.CardNumber)
                {
                    return firstPlayer;
                }
                else
                {
                    return secondPlayer;
                }
            }
            else
            {
                return firstPlayer;
            }
        }
    }

    public int GetTurnScore(CardController firstCard, CardController secondCard)
    {
        return firstCard.GetScore(_trumpCard) + secondCard.GetScore(_trumpCard);
    }

    public void SwitchTrumpCard(PlayerHand playerHand, CardController other)
    {
        playerHand.AddCardToHand(_trumpCard);
        playerHand.RemoveCard(other);
        other.transform.parent = _deckSpawnPosition;
        other.SetAsTrumpCard(_deckSpawnPosition.position);
        _trumpCard = other;
        _hasSwitchedTrumpCard = true;
    }

    public List<CardController> GetCardsPlayed(PlayerType playerType)
    {
        switch (playerType)
        {
            case PlayerType.Human:
                return _humanHand.CardsPlayed();
            case PlayerType.CPU:
                return _CPUHand.CardsPlayed();
            default:
                throw new ArgumentOutOfRangeException(nameof(playerType), playerType, null);
        }
    }
    
    public CardController GetCardPlayed(PlayerType playerType)
    {
        switch (playerType)
        {
            case PlayerType.Human:
                return _humanHand.PlayedCard;
            case PlayerType.CPU:
                return _CPUHand.PlayedCard;
            default:
                throw new ArgumentOutOfRangeException(nameof(playerType), playerType, null);
        }
    }
    
    public List<CardController> GetCardsInHand(PlayerType playerType)
    {
        switch (playerType)
        {
            case PlayerType.Human:
                return _humanHand.CardsInHand();
            case PlayerType.CPU:
                return _CPUHand.CardsInHand();
            default:
                throw new ArgumentOutOfRangeException(nameof(playerType), playerType, null);
        }
    }
    
    public List<CardController> GetCardsInBlind(PlayerType playerType)
    {
        switch (playerType)
        {
            case PlayerType.Human:
                return _humanHand.CardsInBlind();
            case PlayerType.CPU:
                return _CPUHand.CardsInBlind();
            default:
                throw new ArgumentOutOfRangeException(nameof(playerType), playerType, null);
        }
    }

    public bool HasSwitchWithTrumpCard(PlayerType playerType)
    {
        if (!_hasSwitchedTrumpCard) return false;
        
        switch (playerType)
        {
            case PlayerType.Human:
                return _humanHand.HasSwitchTrumpCard;
            case PlayerType.CPU:
                return _CPUHand.HasSwitchTrumpCard;
            default:
                throw new ArgumentOutOfRangeException(nameof(playerType), playerType, null);
        }
    }

    public List<CardController> GetCardsInDeck()
    {
        return _cards.ToList();
    }

    public int GetScore(PlayerType playerType)
    {
        switch (playerType)
        {
            case PlayerType.Human:
                return _humanHand.Score;
            case PlayerType.CPU:
                return _CPUHand.Score;
            default:
                throw new ArgumentOutOfRangeException(nameof(playerType), playerType, null);
        }
    }
}
