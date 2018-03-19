using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Selector : MonoBehaviour, IPointerClickHandler,IPointerUpHandler {
    public void OnPointerClick(PointerEventData eventData)
    {
        print("CLICKED");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        print("FINUP");
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
