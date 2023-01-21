using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Deck")]
public class SO_Deck : ScriptableObject
{
    [SerializeField] private List<SO_Card> _cards;
    public List<SO_Card> OrderedCards => _cards;
    public List<SO_Card> ShuffledCards => _cards.OrderBy(x => Guid.NewGuid()).ToList();
}
