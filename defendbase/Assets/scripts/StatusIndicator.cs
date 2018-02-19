using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusIndicator : MonoBehaviour {
    public GameObject target; // Follow this object
    public Transform healthBar, // Visual indicator of health 
                     healthBarGauge; // Visual indicator of max health
    // Use this for initialization
    void Start()
    {
        healthBar = transform.Find("Health Bar Gauge").Find("Health Bar");
        healthBarGauge = healthBar.parent;
    }

    // Update is called once per frame
    void Update()
    {
        // No target? Remove self
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }
        // Have this face the player to player can see
        if (GameManager.gm.player)
        {
            healthBarGauge.LookAt(GameManager.gm.player.transform.GetComponent<PlayerController>().playerCam.transform);
        }

        // Update health bar based on target's health
        healthBarGauge.position = target.transform.position +
            new Vector3(
                0,
                target.transform.localScale.y +
                healthBarGauge.transform.GetComponent<RectTransform>().rect.height,
                0);
        int hp = 0, maxHP = 1;
        if(target.tag == "Enemy")
        {
            Enemy e = target.transform.GetComponent<Enemy>();
            hp = e.health;
            maxHP = e.maxHP;
        } else if(target.tag == "Objective")
        {
            Objective o = target.transform.GetComponent<Objective>();
            hp = o.HP;
            maxHP = o.maxHP;
        }
        if (hp < 0)
            hp = 0;
        healthBar.localScale = new Vector3((float)hp / (float)maxHP, 1, 1);
    }
}
