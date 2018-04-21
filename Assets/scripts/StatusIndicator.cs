using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusIndicator : MonoBehaviour {
    public GameObject target; // Follow this object
    public Transform healthBar, // Visual indicator of health 
                     healthBarGauge; // Visual indicator of max health
    public GameObject damageIndicatorPrefab;
    public Text healthTxt;
    public int prevHP;
    // Use this for initialization
    void Start()
    {
        healthBar = transform.Find("Health Bar Gauge").Find("Health Bar");
        healthTxt = transform.Find("HealthTxt").GetComponent<Text>();
        healthBarGauge = healthBar.parent;
        prevHP = -1;
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
            //transform.LookAt(GameManager.gm.player.transform.GetComponent<PlayerController>().playerCam.transform);
            healthBarGauge.LookAt(GameManager.gm.player.transform.GetComponent<PlayerController>().playerCam.transform);
            healthTxt.transform.LookAt(GameManager.gm.player.transform.GetComponent<PlayerController>().playerCam.transform);
            healthBarGauge.transform.localEulerAngles += new Vector3(0, 180, 0);
            healthTxt.transform.localEulerAngles += new Vector3(0, 180, 0);
        }

        // Update health bar based on target's health
        healthBarGauge.position = target.transform.position +
            new Vector3(
                0,
                target.transform.localScale.y +
                healthBarGauge.transform.GetComponent<RectTransform>().rect.height,
                0);

        int hp = 0, maxHP = 1;
        // fetch target's current hp
        if(target.tag == "Enemy")
        {
            Enemy e = target.transform.GetComponent<Enemy>();
            healthBarGauge.position += e.go.transform.localPosition + new Vector3(0, 2, 0) ;
            hp = e.health;
            maxHP = e.maxHP;
        } else if(target.tag == "Objective")
        {
            Objective o = target.transform.GetComponent<Objective>();
            hp = o.HP;
            maxHP = o.maxHP;
        }

        // check if hp differed from previous known hp
        if(prevHP != -1 && prevHP != hp)
        {
            //Debug.Log("DIFFER");
            int dif = hp - prevHP;
            GameObject damageIndicator = Instantiate(damageIndicatorPrefab);
            damageIndicator.transform.position = healthBarGauge.position + new Vector3(0,healthBarGauge.GetComponent<RectTransform>().rect.height,0);
            damageIndicator.transform.LookAt(GameManager.gm.player.transform.GetComponent<PlayerController>().playerCam.transform);
            damageIndicator.transform.GetChild(0).GetComponent<Text>().text = "" + dif;
        }

        // make sure hp is valid
        if (hp <= 0)
            hp = 0;
        healthBar.localScale = new Vector3((float)hp / (float)maxHP, 1, 1);

        prevHP = hp;
        healthTxt.text = hp + "/" + maxHP;
        healthTxt.transform.position = healthBarGauge.position - new Vector3(0, .1f, 0);
    }
}
