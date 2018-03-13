using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUI : MonoBehaviour {
    public Enemy e;
    public Transform healthBar, healthBarGauge;
	// Use this for initialization
	void Start () {
        healthBar = transform.Find("Health Bar Gauge").Find("Health Bar");
        healthBarGauge = healthBar.parent;
	}
	
	// Update is called once per frame
	void Update () {
		if(e == null)
        {
            Destroy(gameObject);
            return;
        }
        healthBarGauge.localPosition = e.transform.position + 
            new Vector3(
                0, 
                e.transform.localScale.y + 
                healthBarGauge.transform.GetComponent<RectTransform>().rect.height,
                0);
        healthBar.localScale = new Vector3((float)e.health / (float)e.maxHP, 1, 1);
	}
}
