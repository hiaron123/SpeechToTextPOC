using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonConfigScript : MonoBehaviour,IPointerDownHandler,IPointerUpHandler
{
    public UnityEvent onStartSpeech;
    public UnityEvent onStopSpeech;

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("OnPointerDown");
        onStartSpeech.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
       Debug.Log("OnPointerUp");
         onStopSpeech.Invoke();
    }
}
