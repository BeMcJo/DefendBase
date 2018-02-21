﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Objective : MonoBehaviour {
    public static int ObjectiveCount = 0; // Counts number of objectives in game
    public int id; // Unique ID
    public int HP = 10, // Current health 
               maxHP = 10; // Max health
    public string objectiveType = "defend"; // Type of objective for different game modes
    public GameObject[] storeUpgradeUIs;
	// Use this for initialization
	void Start () {
        id = ObjectiveCount;
        ObjectiveCount++;
        Transform upgradeObjectivesDisplay = GameManager.gm.shopCanvas.transform.Find("Displays").Find("UpgradeObjectivesDisplay");
        for(int i = 0; i < storeUpgradeUIs.Length; i++)
        {
            storeUpgradeUIs[i] = Instantiate(storeUpgradeUIs[i]);
            storeUpgradeUIs[i].transform.SetParent(upgradeObjectivesDisplay.GetChild(0));
        }
        if(NetworkManager.nm.isStarted)
        {
            storeUpgradeUIs[0].transform.Find("BuyBtn").GetComponent<Button>().onClick.AddListener(RequestRepairIfValid);
        }
        else
        {
            storeUpgradeUIs[0].transform.Find("BuyBtn").GetComponent<Button>().onClick.AddListener(Repair);
        }
        storeUpgradeUIs[0].transform.Find("BuyBtn").GetComponent<Button>().interactable = HP == maxHP;
    }

    public void Reset()
    {
        HP = maxHP;
    }

    // Used to synchronize repairing objective using online feature
    public void RequestRepairIfValid()
    {
        if (GameManager.gm.inGameCurrency < 50)
            return;
        NetworkManager.nm.ConfirmObjectiveRepair(id, -5);
        storeUpgradeUIs[0].transform.Find("BuyBtn").GetComponent<Button>().interactable = false;
    }

    // Add health to objective and make player pay the price
    public void Repair()
    {
        if (GameManager.gm.inGameCurrency >= 50)
        {
            Debug.Log("Repair gate");
            GameManager.gm.UpdateInGameCurrency(-50);
            TakeDamage(-5);
        }
    }

    // Inflict damage and update uis related to health
    public void TakeDamage(int dmg)
    {
        HP -= dmg;
        Transform healthBarGauge = storeUpgradeUIs[0].transform.Find("Health Bar Gauge");
        
        if (HP > maxHP)
        {
            HP = maxHP;
        }
        if (HP < 0)
        {
            HP = 0;
        }
        storeUpgradeUIs[0].transform.Find("BuyBtn").GetComponent<Button>().interactable = HP != maxHP;
        healthBarGauge.Find("Health Bar").localScale = new Vector3((float)HP / (float)maxHP, 1, 1);
    }

	// Update is called once per frame
	void Update () {
        // Indicate gameover if losing objective
		if(HP <= 0)
        {
            GameManager.gm.resultNotification.SetActive(true);
            GameManager.gm.gameOver = true;
        }

	}
}
