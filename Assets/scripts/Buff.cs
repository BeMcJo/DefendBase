using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Buff : MonoBehaviour {
    public int buffID, // uniquely identifies the buff (ex: different cosmetics or name)
               buffType; // categorizes the buffs (ex: boosters, stuns, ...)
    public string buffName; // name of buff
    public float timeToLive, timer; // duration of buff
    public bool canStackActive, // determines if buff effect can add on top of the current active
                canReApply; // determines if buff can refresh active duration and/or effect
    public List<GameObject> conditions; // if buff requires UI interaction (ex: tap objects for frozen, swipe ink for blindness, ...)

    public PlayerController player; // target player receiving buff
    public GameObject buffDurationUI; // visual indication of buff duration

	// Use this for initialization
	void Start ()
    {
        Activate();
        buffDurationUI = Instantiate(buffDurationUI);
        buffDurationUI.transform.SetParent(GameManager.gm.playerStatusCanvas.transform);
        buffDurationUI.transform.localPosition = new Vector2(0, -200f);
    }

    /* unnecessary?
    private void OnEnable()
    {
    }
    */

    // Remove any lingering conditions when buff expires
    private void OnDestroy()
    {
        foreach(GameObject cond in conditions)
        {
            print("DESTROYING CONDS");
            Destroy(cond);
        }
        Destroy(buffDurationUI);
    }

    // Update is called once per frame
    void Update () {
        // Remove buff when expired
        if (!isActive())
        {
            print("BUFF EXPIRED");
            player.RemoveBuff(this);
            //Destroy(gameObject);
        }
        buffDurationUI.transform.GetChild(0).GetComponent<Image>().fillAmount = (timer - Time.time) / timeToLive;
	}

    public void AddCondition(GameObject cond)
    {
        cond.transform.SetParent(GameManager.gm.playerStatusCanvas.transform);
        cond.transform.localPosition = new Vector2(0, 0);
        cond.GetComponent<InteractiveUI>().owner = this;
        conditions.Add(cond);
    }

    public void RemoveCondition(GameObject cond)
    {
        print("removeing condition");
        conditions.Remove(cond);
        Destroy(cond);
        if (conditions.Count == 0)
        {
            print("CONDITIONS ALL MET REMOVE BUFF " + buffID + " ," + buffType);
            player.RemoveBuff(this);
        }
    }

    public void ReActivate()
    {
        if (!canStackActive)
            return;
        Activate();
    }

    public void Activate()
    {
        timer = timeToLive + Time.time;
        HandleBuffType();
    }

    public void HandleBuffType()
    {
        switch (buffType)
        {
            // can't perform actions
            case 1:
                AddCondition(Instantiate(GameManager.gm.interactiveUIPrefabs[0]));
                break;
        }
    }

    public bool isActive()
    {
        //print(timer + " " + Time.time);
        return timer > Time.time;
    }
}
