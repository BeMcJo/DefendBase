using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tnt : Trap {
    public GameObject explosionPrefab;
	// Use this for initialization
	protected override void Start () {
        base.Start();
        hp = 1;
	}

    // Helps determine who shot this
    public override void Activate(string sourceType, int sid)
    {
        NetworkManager.nm.debugLog.Add("Activate " + sid + " " + id);
        Debug.Log("Activate " + sid + " " + id);
        base.Activate(sourceType, sid);
        GameObject explosion = Instantiate(explosionPrefab);
        explosion.transform.GetComponent<Explosion>().ownerID = sid;
        explosion.transform.GetComponent<Explosion>().ownerType = sourceType;
        explosion.transform.position = transform.position;
        Destroy(gameObject);
    }

    public override void Activate()
    {
        base.Activate();
        GameObject explosion = Instantiate(explosionPrefab);
        explosion.transform.position = transform.position;
        Destroy(gameObject);
    }

    

    public override void TakeDamage(int dmg, int sid)
    {
        base.TakeDamage(dmg, sid);
        if (hp > 0)
            return;
        Activate(ownerType, sid);

    }

    public override void TakeDamage(int dmg)
    {
        Debug.Log("omg");
        if (hp <= 0)
            return;
        //Activate();
        //return;
        base.TakeDamage(dmg);
        if(hp <= 0)
        {
            Activate();
        }
    }
    // Update is called once per frame
    protected override void Update () {
        base.Update();
	}
}
