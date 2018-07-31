using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageOverTime : MonoBehaviour {
    public int type, // burn, poison
               dmg,
               life; // how many times effect applies
    public int ownerID;
    public string ownerType;
    public float duration, timeStamp;

    private void Awake()
    {
        if (GetComponents<DamageOverTime>().Length > 1)
            Destroy(this);
    }

    // Use this for initialization
    void Start () {
        timeStamp = Time.time + duration;
        if (type == 0) {
            life = 1;
            GetComponent<Enemy>().statusEffectObjects[1].gameObject.SetActive(true);
        }
    }
	
	// Update is called once per frame
	void Update () {
		if(timeStamp <= Time.time)
        {
            Enemy e = GetComponent<Enemy>();
            if (e)
            {
                //e.ApplyEffect(Effect.burn);//,dmg,ownerType,ownerID);
                if (NetworkManager.nm.isStarted)
                {
                    print("hit4");
                    if (ownerID == GameManager.gm.player.GetComponent<PlayerController>().id)
                    {
                        GameManager.gm.OnHitEnemy();
                        NetworkManager.nm.NotifyObjectDamagedBy("ENEMY", "Enemy", e.id, ownerID, dmg, ownerType);//e.gameObject, gameObject);
                                                                                                                 //return;
                    }
                }
                else
                {
                    e.TakeDamageFrom(dmg, ownerType, ownerID);
                    GameManager.gm.OnHitEnemy();
                    // if enemy dies, update record statistics
                    if (e.health <= 0)
                    {
                        // increment kill count for fire attribute
                        GameManager.gm.data.killsByArrowAttribute[5]++;
                    }
                }
            }

            life--;
            if (life <= 0)
            {
                if (type == 0)
                    GetComponent<Enemy>().statusEffectObjects[1].gameObject.SetActive(false);
                Destroy(this);
            }
            else
            {
                timeStamp = duration + Time.time;
            }
        }
	}
}
