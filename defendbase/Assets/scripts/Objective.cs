using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objective : MonoBehaviour {
    public int HP = 10, maxHP = 10;
    public string objectiveType = "defend";
	// Use this for initialization
	void Start () {
		
	}

    public void Reset()
    {
        HP = maxHP;
    }

    public void TakeDamage(int dmg)
    {
        HP -= dmg;
    }

	// Update is called once per frame
	void Update () {
		if(HP <= 0)
        {
            //Debug.Log("Lose");
            GameManager.gm.resultNotification.SetActive(true);
            GameManager.gm.inGame = false;
        }
	}
}
