using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SweetSpot : MonoBehaviour {
    public GameObject owner;
    public GameObject onHitParticleEffectPrefab;
    //public int ssid; // sweet spot id (for determining placement of particle effect)
	// Use this for initialization
	void Start () {
		
	}

	// Update is called once per frame
	void Update () {
		
	}

    public void TakeDamage(GameObject source)
    {
        GameObject particleEffect = Instantiate(onHitParticleEffectPrefab);
        particleEffect.transform.position = transform.position;//owner.transform.Find("ParticleEffectPlaceHolders").GetChild(ssid).position;
        if (source.tag == "Projectile")
        {
            Projectile p = source.GetComponent<Projectile>();
            PlayerController pc = GameManager.gm.player.GetComponent<PlayerController>();
            if (owner.tag == "Enemy")
            {
                
                Enemy e = owner.GetComponent<Enemy>();
                if (p.ownerID == pc.id)
                {
                    print("MY PLAYER");
                    pc.criticalShotCount++;
                    GameManager.gm.data.weakSpotsHitByEnemy[e.enemyID]++;
                }
                GameManager.gm.OnHitEnemy();
                e.OnHit();
                // If using online feature, let Network Manager handle this
                if (NetworkManager.nm.isStarted)
                {
                    NetworkManager.nm.NotifyObjectDamagedBy(owner.tag.ToUpper(),owner.tag,e.id, p.ownerID,p.dmg*2,p.ownerType);
                    return;
                }

                // If not using online feature, inflict damage to enemy
                //e.TakeDamageFrom(dmg, ownerType, ownerID);
                e.TakeDamageFrom(2*p.dmg,p.ownerType, p.ownerID);
            }else if(owner.tag == "Dummy")
            {
                if(p.ownerID == pc.id)
                    GameManager.gm.OnHitEnemy();
            }
            print("SWEETSPOT HAS BEEN HIT");
        }
    }
}
