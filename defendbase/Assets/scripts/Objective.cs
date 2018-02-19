using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objective : MonoBehaviour {
    public static int ObjectiveCount = 0; // Counts number of objectives in game
    public int id; // Unique ID
    public int HP = 10, // Current health 
               maxHP = 10; // Max health
    public string objectiveType = "defend"; // Type of objective for different game modes
	// Use this for initialization
	void Start () {
        id = ObjectiveCount;
        ObjectiveCount++;
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
        // Indicate gameover if losing objective
		if(HP <= 0)
        {
            GameManager.gm.resultNotification.SetActive(true);
            GameManager.gm.gameOver = false;
        }
	}
}
