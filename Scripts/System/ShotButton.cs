using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class ShotButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool buttondown;

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("SHOOT");
        buttondown = true;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        buttondown = false;
    }
}
