using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InteractiveUI : MonoBehaviour, IPointerDownHandler  {
    public int maxHP, hp, // number of times to hit to break free from disability
               interactionType; // indicates what user interaction is expected
    public string interactionName; // describes the interaction (ex: frozen, stunned, blinded)
    public float timeToLive; // duration of interaction if conditions aren't satisfied
    public Buff owner; // buff to inform of any activity regarding this object
    public Sprite[] images; // list of images used for UI
    // Use this for initialization
    void Start()
    {
        hp = maxHP;
        GetComponent<Image>().sprite = images[0];
    }

    // Update is called once per frame
    void Update()
    {

    }


    public virtual void OnTouch()
    {

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        HandlePointerDown();
    }

    // Handles pointer down action based on the interaction type and interaction name
    public void HandlePointerDown()
    {
        switch (interactionType)
        {
            // perform a tapping action
            case 0:
                TapAction();
                break;

        }
    }

    // Handles tap action based on the interaction name
    public void TapAction()
    {
        switch (interactionName)
        {
            case "frozen":
                hp--;
                print(hp);
                if (hp * 2 < maxHP)
                    GetComponent<Image>().sprite = images[1];
                if (hp <= 0)
                {
                    print("DESTROYED");
                    owner.RemoveCondition(gameObject);
                }
                break;
        }
    }

}
