using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanHand : PlayerHand
{
    private void Update()
    {
        if (!_isPlaying) return;

        if (!Input.GetMouseButtonDown(0)) return;
        
        var mouseVector = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(mouseVector, out var hit)) return;
        
        if (!hit.transform.TryGetComponent(out CardController card)) return;
            
        if (!_availableCards.Contains(card)) return;
        card.Play(_playPosition.position);
        
        _lastCardPlayed = card;
        _availableCards.Remove(_lastCardPlayed);
        _playedCards.Add(_lastCardPlayed);

        ResetCardsPosition();

        _isPlaying = false;
        _hasPlayedACard = true;
    }
}
