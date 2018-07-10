using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ExplosionType
{
    damage,
    freeze
}

public class Explosion : MonoBehaviour {
    public float growthRate = 1f;
    public float growthLimit = 20f;
    public int dmg = 10,
               ownerID;
    public string ownerType;
    public ExplosionType explosionType;
    public List<GameObject> alreadyHit;

    private void Awake()
    {
        alreadyHit = new List<GameObject>();
    }

    // Use this for initialization
    void Start () {
        print((int)explosionType);
        print(GameManager.gm.VFXPrefabs.Length);
        GameObject vfx = Instantiate(GameManager.gm.VFXPrefabs[(int)explosionType+1]);
        GetComponent<MeshRenderer>().enabled = explosionType == ExplosionType.freeze;
        vfx.transform.position = gameObject.transform.position;

	}
    

    // Update is called once per frame
    void Update () {
        transform.localScale += new Vector3(growthRate, growthRate, growthRate);
        if (transform.localScale.x >= growthLimit)
            Destroy(gameObject);
	}

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.tag);
        if (ownerID != GameManager.gm.player.transform.GetComponent<PlayerController>().id)
            return;
        //Debug.Log("aa");
        if(other.tag == "Enemy")
        {
            //Debug.Log("bomer");
            Transform t = other.transform;
            Enemy e = t.GetComponent<Enemy>();
            while(e == null)
            {
                //print("WHY");
                t = t.parent;
                e = t.GetComponent<Enemy>();
            }
            if (alreadyHit.Contains(e.gameObject))
                return;
            alreadyHit.Add(e.gameObject);
            if(explosionType == ExplosionType.damage)
            //e.OnHit();
            {
                GameManager.gm.OnHitEnemy();
            }
            else if(explosionType == ExplosionType.freeze)
            {
                print("FREEze");
                e.ApplyEffect(Effect.freeze);
            }
            if (NetworkManager.nm.isStarted)
            {
                NetworkManager.nm.NotifyObjectDamagedBy(e.gameObject, gameObject);
                return;
            }
            // If not using online feature, inflict damage to enemy
            e.TakeDamageFrom(dmg, ownerType, ownerID);
            //Debug.Log("BOOM");
        }else if (other.tag == "Trap")
        {
            //if (!other.transform.GetComponent<ObjectPlacement>().isSet)
            //    return;
            Debug.Log("hit trp");
            Trap t = other.GetComponent<Trap>();
            if (NetworkManager.nm.isStarted)
            {
                NetworkManager.nm.NotifyObjectDamagedBy(t.gameObject, gameObject);
                return;
            }
            // If not using online feature, inflict damage to enemy
            t.TakeDamage(dmg);
            Debug.Log("BOOM");
        }
    }
}
