using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class CPUHand : PlayerHand
{
    private void Update()
    {
        // Check can switch trump card
        if (CanSwitchTrumpCard())
        {
            GameManager.Instance.SwitchTrumpCard(this, GetCardToSwitchWithTrumpCard());
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

        var possibleCards = _playFirst ? _availableCards : GetListOfPossibleCardToPlay(_firstCardPlayed);
        var card = possibleCards[Random.Range(0, possibleCards.Count)];
        
        card.Play(_playPosition.position);
        
        _lastCardPlayed = card;
        _availableCards.Remove(_lastCardPlayed);
        _playedCards.Add(_lastCardPlayed);

        ResetCardsPosition();

        _isPlaying = false;
        _hasPlayedACard = true;
    }
}
