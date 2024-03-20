using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GModule.Unity.CardHandZone
{
    [RequireComponent(typeof(RectTransform))]
    public class Card : MonoBehaviour, IDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public event Action<Card> PointerEnterEvent;
        public event Action<Card, PointerEventData> PointerDownEvent;
        public event Action<Card, PointerEventData> DragEvent;
        public event Action<Card> PointerUpEvent;
        public event Action<Card> PointerExitEvent;

        public RectTransform RectTransform { get; protected set; }

        protected virtual void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            DragEvent?.Invoke(this, eventData);
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            PointerEnterEvent?.Invoke(this);
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            PointerExitEvent?.Invoke(this);
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            PointerDownEvent?.Invoke(this, eventData);
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            PointerUpEvent?.Invoke(this);
        }
    }
}