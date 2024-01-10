using GModule.Unity.CardHandZone;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

namespace Test
{
    public class HandZoomSample : MonoBehaviour
    {
        [SerializeField] private HandZone handZone;
        [SerializeField] private Card cardSample;
        [SerializeField] private Transform deckPoint;
        [SerializeField] private Transform discardPoint;

        private List<Card> cards = new List<Card>();

        private void Awake()
        {
            handZone.PlayCardEvent += onCardPlay;
            handZone.CardReleaseEvent += onCardRelease;
            handZone.CardInOutPlayableAreaEvent += onCardInOutPlayableArea;
            handZone.CardMoveOutFinishEvent += onCardMoveOutFinish;
        }

        public void DrawCard()
        {
            var card = Instantiate(cardSample, cardSample.transform.parent);
            card.transform.position = deckPoint.position;
            card.gameObject.SetActive(true);
            handZone.PutCard(card, .5f);
            cards.Add(card);
        }

        public void Discard()
        {
            if (cards.Count == 0) { return; }

            var card = cards[Random.Range(0, cards.Count)];
            handZone.MoveOutCard(card, discardPoint.position, .5f);
            cards.Remove(card);
        }

        private void onCardInOutPlayableArea(Card card, bool isIn)
        {
            if (isIn)
            {
                card.GetComponent<Image>().color = Color.yellow;
            }
            else
            {
                card.GetComponent<Image>().color = Color.white;
            }
        }

        private void onCardPlay(Card card)
        {
            handZone.RemoveCard(card);
            cards.Remove(card);
            Destroy(card.gameObject);
        }

        private void onCardRelease(Card card)
        {
            card.GetComponent<Image>().color = Color.white;
        }

        private void onCardMoveOutFinish(Card card)
        {
            Destroy(card.gameObject);
        }

    }
}