using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Random = UnityEngine.Random;

public class CPUHand : PlayerHand
{
    private bool _isEvaluating;
    private bool _hasChooseACard;

    private CardController _cardToPlay;
    
    [SerializeField] private float _maxComputationTime = 10;
    
    private void Update()
    {
        // Check can switch trump card
        if (CanSwitchTrumpCard())
        {
            GameManager.Instance.SwitchTrumpCard(this, GetCardToSwitchWithTrumpCard());
            _hasSwitchTrumpCard = true;
        }
        
        // S$witching blind
        if (CanSwitchBlind())
        {
            var trumpCard = GameManager.Instance.TrumpCard;
            var sortedCards = _availableCards.Where(x => x.CardSuits != trumpCard.CardSuits).OrderBy(x => x.CardNumber).ToList();

            if (sortedCards.Count >= 3)
            {
                _isSwitchingWithBlind = true;
                foreach (var cardController in sortedCards)
                {
                    if (_cardToSwitchWithBlind.Count == 3) continue;

                    cardController.SetReadyToSwitchWithBlind(_offsetReadySwitchBlind);
                    _cardToSwitchWithBlind.Add(cardController);

                }
            }

        }
        
        // Play card
        if (!_isPlaying) return;

        if (!_isEvaluating && !_hasChooseACard)
        {
            StartCoroutine(MonteCarlo());
        }

        if (_isEvaluating && !_hasChooseACard) return;
        
        
        _cardToPlay.Play(_playPosition.position);
        
        _lastCardPlayed = _cardToPlay;
        _availableCards.Remove(_lastCardPlayed);
        _playedCards.Add(_lastCardPlayed);

        ResetCardsPosition();

        _isPlaying = false;
        _hasPlayedACard = true;

        _hasChooseACard = false;
    }

    private List<CardController> _humanCards = new List<CardController>();
    private List<CardController> _selfCards = new List<CardController>();
    private List<CardController> _usedCards = new List<CardController>();
    private List<CardController> _remainingCards = new List<CardController>();
    private int _selfScore;
    private int _humanScore;

    private IEnumerator MonteCarlo()
    {
        var startTime = Time.time;
        _isEvaluating = true;
        _hasChooseACard = false;
        
        // reset nodes
        _firstLevelNodes = new HashSet<MC_Node>();
        _nodes = new HashSet<MC_Node>();
        _leavesLevelNodes = new HashSet<MC_Node>();
        
        // Get infos
        _selfScore = Score;
        _humanScore = GameManager.Instance.GetScore(PlayerType.Human);
        
        _humanCards = new List<CardController>(); // Human cards, which are unkonw
        _selfCards = new List<CardController>(); // self hand
        _usedCards = new List<CardController>();
        _remainingCards = new List<CardController>();
        
        _selfCards.AddRange(_availableCards);

        _usedCards.AddRange(_blindCards); // self blind cards
        _usedCards.AddRange(_playedCards); // self played cards
        _usedCards.AddRange(GameManager.Instance.GetCardsPlayed(PlayerType.Human)); // human played cards
        _usedCards.Add(GameManager.Instance.TrumpCard); // The current trump card cannot be played by any player
        if (GameManager.Instance.HasSwitchWithTrumpCard(PlayerType.Human))
        {
            var originalTrumpCard = GameManager.Instance.OriginalTrumpCard;

            if (!_usedCards.Contains(originalTrumpCard))
            {
                _humanCards.Add(originalTrumpCard);
            }
        }

        _remainingCards.AddRange(GameManager.Instance.GetCardsInDeck());
        if (GameManager.Instance.HasSwitchWithTrumpCard(PlayerType.Human))
        {
            foreach (var cardController in GameManager.Instance.GetCardsInHand(PlayerType.Human))
            {
                if (_humanCards.Contains(cardController)) continue;
                
                _remainingCards.Add(cardController);
            }
        }
        else
        {
            _remainingCards.AddRange(GameManager.Instance.GetCardsInHand(PlayerType.Human));
        }
        _remainingCards.AddRange(GameManager.Instance.GetCardsInBlind(PlayerType.Human));
        
        Debug.Log("human cards (" + _humanCards.Count + ")");
        var s = "";
        foreach (var cardController in _humanCards)
        {
            s += cardController.CardNumber + " of " + cardController.CardSuits + " | ";
        }
        Debug.Log(s);
        s = "";
        
        Debug.Log("self cards (" + _selfCards.Count + ")");
        foreach (var cardController in _selfCards)
        {
            s += cardController.CardNumber + " of " + cardController.CardSuits + " | ";
        }
        Debug.Log(s);
        s = "";
        
        Debug.Log("used cards (" + _usedCards.Count + ")");
        foreach (var cardController in _usedCards)
        {
            s += cardController.CardNumber + " of " + cardController.CardSuits + " | ";
        }
        Debug.Log(s);
        s = "";
        
        Debug.Log("remaining cards (" + _remainingCards.Count + ")");
        foreach (var cardController in _remainingCards)
        {
            s += cardController.CardNumber + " of " + cardController.CardSuits + " | ";
        }

        Debug.Log(s);
        
        Debug.Log("Total = " + (_remainingCards.Count + _humanCards.Count + _selfCards.Count + _usedCards.Count));

        while (Time.time - startTime < _maxComputationTime)
        {
            Debug.Log("Wait end timer " + (Time.time - startTime));
            
            // Check player cards
            var possibleCardsForHuman = new List<CardController>();
            possibleCardsForHuman.AddRange(_remainingCards);
            possibleCardsForHuman.AddRange(_humanCards);
            
            var possibleCardsForCPU = new List<CardController>();

            CardController cardPlayedHuman;
            CardController cardPlayedCPU;
            if (_playFirst)
            {
                possibleCardsForCPU = _availableCards;
                cardPlayedCPU = possibleCardsForCPU[Random.Range(0, possibleCardsForCPU.Count)];
                var subPossibleCardsForHuman = GetListOfPossibleCardToPlay(possibleCardsForHuman, cardPlayedCPU);
                cardPlayedHuman = subPossibleCardsForHuman[Random.Range(0, subPossibleCardsForHuman.Count)];
            }
            else
            {
                cardPlayedHuman = GameManager.Instance.GetCardPlayed(PlayerType.Human);
                possibleCardsForCPU = GetListOfPossibleCardToPlay(cardPlayedHuman);
                cardPlayedCPU = possibleCardsForCPU[Random.Range(0, possibleCardsForCPU.Count)];
            }
            
            // Select first card

            var nodeAlreadyExists = false;
            MC_Node nodeTopLevel = null;
            foreach (var firstLevelNode in _firstLevelNodes)
            {
                if (firstLevelNode.firstCardPlayed == cardPlayedCPU)
                {
                    nodeAlreadyExists = true;
                    nodeTopLevel = firstLevelNode;
                    break;
                }
            }

            if (!nodeAlreadyExists)
            {
                nodeTopLevel = new MC_Node
                {
                    scoreHuman = _humanScore,
                    scoreCPU = _selfScore,
                    cardPlayedCPU = cardPlayedCPU,
                    cardPlayedHuman = cardPlayedHuman,
                    childNodes = new List<MC_Node>(),
                    firstCardPlayed = cardPlayedCPU,
                    humanCards = new List<CardController>(_humanCards),
                    selfCards = new List<CardController>(_selfCards),
                    usedCard = new List<CardController>(_usedCards),
                    parentNode = null,
                    hasParentNode = false,
                    playerToPlayFirst = _playFirst ? PlayerType.CPU : PlayerType.Human,
                    remainingCards = new List<CardController>(_remainingCards),
                };

                _firstLevelNodes.Add(nodeTopLevel);
                _nodes.Add(nodeTopLevel);
                
                nodeTopLevel.EvaluateResults();
                nodeTopLevel.UpdateCardContainers();
            }
            
            // Security for last card
            if (!nodeTopLevel.CanStillPlay)
            {
                _leavesLevelNodes.Add(nodeTopLevel);
                break;
            }
            

            // Loop until end result reached
            var currentNode = nodeTopLevel;

            while (currentNode.CanStillPlay)
            {
                possibleCardsForHuman = new List<CardController>();
                possibleCardsForHuman.AddRange(currentNode.remainingCards);
                possibleCardsForHuman.AddRange(currentNode.humanCards);
                
               // Select first card
               if (currentNode.turnWinner == PlayerType.CPU)
               {
                   possibleCardsForCPU = currentNode.selfCards;
                   cardPlayedCPU = possibleCardsForCPU[Random.Range(0, possibleCardsForCPU.Count)];
                   var subPossibleCardsForHuman = GetListOfPossibleCardToPlay(possibleCardsForHuman, cardPlayedCPU);
                   cardPlayedHuman = subPossibleCardsForHuman[Random.Range(0, subPossibleCardsForHuman.Count)];
               }
               else
               {
                   cardPlayedHuman = possibleCardsForHuman[Random.Range(0, possibleCardsForHuman.Count)];
                   possibleCardsForCPU = GetListOfPossibleCardToPlay(cardPlayedHuman);
                   cardPlayedCPU = possibleCardsForCPU[Random.Range(0, possibleCardsForCPU.Count)];
               }
               
               // Check node already exsit
                nodeAlreadyExists = false;
                MC_Node nextNode = null;
                foreach (var childNode in currentNode.childNodes)
                {
                    if (childNode.cardPlayedHuman == cardPlayedHuman && childNode.cardPlayedCPU == cardPlayedCPU)
                    {
                        nodeAlreadyExists = true;
                        nextNode = childNode;
                        break;
                    }
                }

                if (!nodeAlreadyExists)
                {
                    nextNode = new MC_Node
                    {
                        scoreHuman = currentNode.scoreHuman,
                        scoreCPU = currentNode.scoreCPU,
                        cardPlayedCPU = cardPlayedCPU,
                        cardPlayedHuman = cardPlayedHuman,
                        childNodes = new List<MC_Node>(),
                        firstCardPlayed = currentNode.firstCardPlayed,
                        humanCards = new List<CardController>(currentNode.humanCards),
                        selfCards = new List<CardController>(currentNode.selfCards),
                        usedCard = new List<CardController>(currentNode.usedCard),
                        parentNode = currentNode,
                        hasParentNode = true,
                        playerToPlayFirst = currentNode.turnWinner,
                        remainingCards = new List<CardController>(currentNode.remainingCards),
                    };

                    currentNode.childNodes.Add(nextNode);
                    _nodes.Add(nextNode);
                    
                    nextNode.EvaluateResults();
                    nextNode.UpdateCardContainers();
                }

                currentNode = nextNode;
                if (!nextNode.CanStillPlay)
                {
                    _leavesLevelNodes.Add(nextNode);
                }
                
                yield return null;
            }
            
            yield return null;
        }
        
        // Loop through all end node to choose best cards
        var maxScore = 0;
        foreach (var leavesLevelNode in _leavesLevelNodes)
        {
            if (leavesLevelNode.scoreCPU > leavesLevelNode.scoreHuman && leavesLevelNode.scoreCPU > maxScore)
            {
                maxScore = leavesLevelNode.scoreCPU;
                _cardToPlay = leavesLevelNode.firstCardPlayed;
            }
        }

        _isEvaluating = false;
        _hasChooseACard = true;
    }

    private HashSet<MC_Node> _firstLevelNodes = new();
    private HashSet<MC_Node> _nodes = new();
    private HashSet<MC_Node> _leavesLevelNodes = new();

    [Serializable]
    private class MC_Node
    {
        public CardController firstCardPlayed;
        public CardController cardPlayedCPU;
        public CardController cardPlayedHuman;
        
        public List<CardController> humanCards;
        public List<CardController> selfCards;
        public List<CardController> usedCard;
        public List<CardController> remainingCards;
        
        public int scoreCPU;
        public int scoreHuman;
        public PlayerType turnWinner;

        public bool hasParentNode;
        public MC_Node parentNode;
        public List<MC_Node> childNodes;

        public PlayerType playerToPlayFirst;

        public bool CanStillPlay => selfCards.Count > 0;

        public void UpdateCardContainers()
        {
            if (remainingCards.Contains(cardPlayedHuman))
            {
                remainingCards.Remove(cardPlayedHuman);
            } else if (humanCards.Contains(cardPlayedHuman))
            {
                humanCards.Remove(cardPlayedHuman);
            }

            selfCards.Remove(cardPlayedCPU);
            
            usedCard.Add(cardPlayedHuman);
            usedCard.Add(cardPlayedCPU);
        }
        
        public void EvaluateResults()
        {
            PlayerType firstPlayer;
            PlayerType secondPlayer;

            CardController firstCard;
            CardController secondCard;
            if (playerToPlayFirst == PlayerType.Human)
            {
                firstPlayer = PlayerType.Human;
                secondPlayer = PlayerType.CPU;

                firstCard = cardPlayedHuman;
                secondCard = cardPlayedCPU;
            }
            else
            {
                firstPlayer = PlayerType.CPU;
                secondPlayer = PlayerType.Human;
                    
                firstCard = cardPlayedCPU;
                secondCard = cardPlayedHuman;
            }

            turnWinner = GameManager.Instance.GetTurnWinner(firstPlayer, secondPlayer, firstCard, secondCard);
            var turnScore = GameManager.Instance.GetTurnScore(firstCard, secondCard);

            if (turnWinner == PlayerType.CPU)
            {
                scoreCPU += turnScore;
            }
            else
            {
                scoreHuman += turnScore;
            }
        }
    }
    
    public void EvaluateBranch()
    {
        
    }
}
