using System.Collections;
using System.Collections.Generic;
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
        
        // Play card
        if (!_isPlaying) return;

        var card = _availableCards[Random.Range(0, _availableCards.Count)];
        
        card.Play(_playPosition.position);
        
        _lastCardPlayed = card;
        _availableCards.Remove(_lastCardPlayed);
        _playedCards.Add(_lastCardPlayed);

        ResetCardsPosition();

        _isPlaying = false;
        _hasPlayedACard = true;
    }
}
