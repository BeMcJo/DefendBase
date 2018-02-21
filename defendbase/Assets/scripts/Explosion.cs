using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour {
    public float growthRate = 1f;
    public float growthLimit = 20f;
    public int dmg = 50,
               id; 
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.localScale += new Vector3(growthRate, growthRate, growthRate);
        if (transform.localScale.x >= growthLimit)
            Destroy(gameObject);
	}

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);
        if (id != GameManager.gm.player.transform.GetComponent<PlayerController>().id)
            return;
        Debug.Log("aa");
        if(other.tag == "Enemy")
        {
            Debug.Log("bomer");
            if (NetworkManager.nm.isStarted)
            {
                NetworkManager.nm.NotifyObjectDamagedBy(other.gameObject, gameObject);
                return;
            }
            Enemy e = other.transform.GetComponent<Enemy>();
            // If not using online feature, inflict damage to enemy
            e.TakeDamage(dmg);
            Debug.Log("BOOM");
        }
    }
}
