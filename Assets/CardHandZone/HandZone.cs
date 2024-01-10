using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GModule.Unity.CardHandZone
{
    [RequireComponent(typeof(RectTransform))]
    public class HandZone : MonoBehaviour
    {
        public event Action<Card> PlayCardEvent;
        public event Action<Card> CardReleaseEvent;
        public event Action<Card, PointerEventData> CardDragEvent;
        //true = in, false = out
        public event Action<Card, bool> CardInOutPlayableAreaEvent;
        public event Action<Card> CardMoveOutFinishEvent;

        [SerializeField] protected RectTransform playZone;
        [SerializeField] protected RectTransform invalidZone;
        [SerializeField] protected CanvasScaler scaler;
        [SerializeField] protected float defaultCardSpace = 16;
        [SerializeField] protected bool insertFromLeft = true;
        [SerializeField] protected float moveSpeed = 100;
        [SerializeField] protected float holdScale = 1.5f;
        [SerializeField] protected float holdHeight = 150;
        [SerializeField] protected float scaleTime = .2f;
        [SerializeField] protected float minCardSpace = 32;

        public float Width => rectTransform.rect.width;
        public bool IsCardInPlayArea { get; protected set; } = false;

        protected List<Card> cards = new();
        protected List<Vector2> cardHandPos = new();
        protected RectTransform rectTransform;

        protected Card selectCard = null;

        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public float PutCard(Card card, float startScale = 1)
        {
            card.PointerEnterEvent += onPointerEnterCard;
            card.PointerDownEvent += onPointerDownCard;
            card.DragEvent += onDragCard;
            card.PointerUpEvent += onPointerUpCard;
            card.PointerExitEvent += onPointerExitCard;

            cards.Add(card);

            cardHandPos = calculateCardSpacing();

            var animTime = Vector2.Distance(card.RectTransform.anchoredPosition, cardHandPos[cardHandPos.Count -1])/moveSpeed;

            if(startScale != 1)
            {
                card.RectTransform.localScale *= startScale;
                card.RectTransform.DOScale(Vector3.one, animTime/2);
            }

            for (int i = 0; i < cardHandPos.Count; i++)
            {
                cards[i].RectTransform.DOAnchorPos(cardHandPos[i], animTime);
            }

            return animTime;
        }

        public virtual void HoldCard(Card card)
        {
            var cardIndex = cards.IndexOf(card); 
            if (cardIndex == -1) { return; }

            var cardTrans = card.RectTransform;
            cardTrans.SetAsLastSibling();
            cardTrans.DOScale(holdScale, scaleTime);
            cardTrans.DOAnchorPosY(cardHandPos[cardIndex].y + holdHeight, holdScale/moveSpeed);
        }

        public virtual void DeholdCard(Card card)
        {
            var cardIndex = cards.IndexOf(card);
            if (cardIndex == -1) { return; }

            var cardTrans = card.RectTransform;
            cardTrans.DOScale(1, scaleTime);
            cardTrans.DOAnchorPosY(cardHandPos[cardIndex].y, holdScale / moveSpeed);
        }

        public virtual void ReleaseCard(Card card)
        {
            var cardIndex = cards.IndexOf(card);
            if (cardIndex == -1) { return; }

            CardReleaseEvent?.Invoke(card);
            selectCard = null;

            var pos = cardHandPos[cardIndex];
            var cardTrans = card.RectTransform;
            var dis = Vector2.Distance(pos, cardTrans.position);

            if (dis < .01f) { return; }

            var animTime = dis / moveSpeed;
            cardTrans.DOAnchorPos(pos, animTime);
        }

        public virtual void MoveOutCard(Card card, Vector3 outPosition, float endScale = 1)
        {
            var cardIndex = cards.IndexOf(card);
            if (cardIndex == -1) { return; }

            var cardTrans = card.RectTransform;
            var dis = Vector2.Distance(outPosition, cardTrans.position);
            var animTime = dis / moveSpeed;
            cardTrans.DOMove(outPosition, animTime).OnComplete(() => { CardMoveOutFinishEvent?.Invoke(card); });
            card.RectTransform.DOScale(Vector3.one * endScale, animTime / 2);

            RemoveCard(card);
        }

        public virtual void RemoveCard(Card card)
        {
            var cardIndex = cards.IndexOf(card);
            if (cardIndex == -1) { return; }

            card.PointerEnterEvent -= onPointerEnterCard;
            card.PointerDownEvent -= onPointerDownCard;
            card.DragEvent -= onDragCard;
            card.PointerUpEvent -= onPointerUpCard;
            card.PointerExitEvent -= onPointerExitCard;

            cards.RemoveAt(cardIndex);
            cardHandPos.RemoveAt(cardIndex);

            cardHandPos = calculateCardSpacing();
            for (int i = 0; i < cardHandPos.Count; i++)
            {
                cards[i].RectTransform.DOAnchorPos(cardHandPos[i], .1f);
            }
        }

        protected List<Vector2> calculateCardSpacing()
        {
            List<Vector2> result = new();

            if(cards.Count == 0) { return result; }

            float cardWidth = cards[0].RectTransform.rect.width;
            float cardSpace = defaultCardSpace;

            float totoalWidth = cardWidth * cards.Count + defaultCardSpace * (cards.Count - 1);
            float fristCardX = 0;

            if (totoalWidth > Width)
            {
                float space = Width / cards.Count;

                if(space < minCardSpace)
                {
                    space = minCardSpace;
                }

                if (insertFromLeft)
                {
                    fristCardX += (cards.Count - 1) * space/2;
                    space = -space;
                }
                else
                {
                    fristCardX -= (cards.Count - 1) * space/2;
                }

                for (int i = 0; i < cards.Count; i++)
                {
                    var pos = rectTransform.anchoredPosition;
                    pos.x = fristCardX + i * space;
                    result.Add(pos);
                }
            }
            else
            {
                float space = cardWidth + cardSpace;

                if (insertFromLeft)
                {
                    fristCardX += (cards.Count - 1) * .5f * space;
                    space = -space;
                }
                else
                {
                    fristCardX -= (cards.Count - 1) * .5f * space;
                }

                for (int i = 0 ; i < cards.Count; i++)
                {
                    var pos = rectTransform.anchoredPosition;
                    pos.x = fristCardX + i * space;
                    result.Add(pos);
                }
            }

            return result;
        }

        protected virtual void onPointerEnterCard(Card card)
        {
            if (selectCard != null) { return; }

            HoldCard(card);
        }

        protected virtual void onPointerDownCard(Card card)
        {
            if(selectCard != null) { return; }

            selectCard = card;
        }

        protected virtual void onPointerUpCard(Card card)
        {
            if (selectCard != card) { return; }

            selectCard = null;
            IsCardInPlayArea = false;

            if (RectTransformUtility.RectangleContainsScreenPoint(playZone, Input.mousePosition))
            {
                PlayCardEvent?.Invoke(card);
                return;
            }
            ReleaseCard(card);
        }

        protected virtual void onPointerExitCard(Card card)
        {
            if (selectCard != null) { return;}

            DeholdCard(card);
        }

        protected virtual void onDragCard(Card card, PointerEventData eventData)
        {
            if (selectCard != card) { return; }

            card.RectTransform.anchoredPosition += eventData.delta / GetComponentInParent<Canvas>().scaleFactor;

            triggerCardDrag(card, eventData);

            bool inPlayable = RectTransformUtility.RectangleContainsScreenPoint(playZone, Input.mousePosition);

            if (IsCardInPlayArea != inPlayable)
            {
                IsCardInPlayArea = inPlayable;

                triggerCardInOutPlayableArea(card, inPlayable);
            }

            if (invalidZone != null 
                && RectTransformUtility.RectangleContainsScreenPoint(invalidZone, Input.mousePosition))
            {
                ReleaseCard(card);
            }
        }

        protected void triggerCardDrag(Card card, PointerEventData eventData)
        {
            CardDragEvent?.Invoke(card, eventData);
        }

        protected void triggerCardInOutPlayableArea(Card card, bool isIn)
        {
            CardInOutPlayableAreaEvent?.Invoke(card, isIn);
        }
    }
}