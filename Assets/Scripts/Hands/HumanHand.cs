using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HumanHand : PlayerHand
{
    private void Update()
    {
        var mouseVector = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        // Switch with trump card
        if (CanSwitchTrumpCard() && Input.GetMouseButtonDown(0) && Physics.Raycast(mouseVector, out var hitTrumpCard))
        {
            if (hitTrumpCard.transform.TryGetComponent(out CardController otherCard))
            {
                if (otherCard == GameManager.Instance.TrumpCard)
                {
                    GameManager.Instance.SwitchTrumpCard(this, GetCardToSwitchWithTrumpCard());
                    return;
                }
            }
        }
        
        // Switch 3 cards with blind
        if (CanSwitchBlind() && Input.GetMouseButtonDown(0) && Physics.Raycast(mouseVector, out var hitBlindCard))
        {
            if (hitBlindCard.transform.TryGetComponent(out CardController blindCard))
            {
                if (_blindCards.Contains(blindCard))
                {
                    _isSwitchingWithBlind = true;
                }
            }
        }

        if (_isSwitchingWithBlind)
        {
            if (Input.GetMouseButtonDown(0) && Physics.Raycast(mouseVector, out var hitCardToSwitchWithBlind))
            {
                if (hitCardToSwitchWithBlind.transform.TryGetComponent(out CardController cardToSwitch))
                {
                    if (_availableCards.Contains(cardToSwitch))
                    {
                        if (cardToSwitch.CardState == CardState.InHand)
                        {
                            cardToSwitch.SetReadyToSwitchWithBlind(_offsetReadySwitchBlind);
                            _cardToSwitchWithBlind.Add(cardToSwitch);
                        }else if (cardToSwitch.CardState == CardState.ReadyToSwitchWithBlind)
                        {
                            cardToSwitch.UnsetReadyToSwitchWithBlind(_offsetReadySwitchBlind);
                            _cardToSwitchWithBlind.Remove(cardToSwitch);
                        }
                    }
                }
            }
            return;
        }
        
        // Play card
        if (!_isPlaying) return;

        if (!Input.GetMouseButtonDown(0)) return;
        
        if (!Physics.Raycast(mouseVector, out var hit)) return;
        
        if (!hit.transform.TryGetComponent(out CardController card)) return;
            
        var possibleCards = _playFirst ? _availableCards : GetListOfPossibleCardToPlay(_firstCardPlayed);
        if (!possibleCards.Contains(card)) return;
        card.Play(_playPosition.position);
        
        _lastCardPlayed = card;
        _availableCards.Remove(_lastCardPlayed);
        _playedCards.Add(_lastCardPlayed);

        ResetCardsPosition();

        _isPlaying = false;
        _hasPlayedACard = true;
    }
}
