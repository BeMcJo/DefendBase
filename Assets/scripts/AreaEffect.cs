using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectTimer
{
    public GameObject obj;
    public float expireTime;
    public ObjectTimer(GameObject o, float time)
    {
        obj = o;
        expireTime = time;
    }
}

public class AreaEffect : MonoBehaviour {
    public float duration, timeToLive;
    public int ownerID,
               effectID;
    public string ownerType;
    public Dictionary<int, ObjectTimer> enemies; // list of enemies that will take damage after duration amount of time
                                                 // if still within area effect
    public Stack<ObjectTimer> unusedObjectTimers; // list of unused objectTimer objects
    
	// Use this for initialization
	void Start () {
        enemies = new Dictionary<int, ObjectTimer>();
        unusedObjectTimers = new Stack<ObjectTimer>();
        //timeToLive = duration + Time.time;
	}
	
	// Update is called once per frame
	void Update () {
        //if (timeToLive >= Time.time)
        //{
        //    Destroy(gameObject);
        //}
        /*
        List<int> objectsToRemove = new List<int>(); 
        
        foreach(KeyValuePair<int,ObjectTimer> kvp in enemies)
        {
            if(kvp.Value.expireTime <= Time.time)
            {
                objectsToRemove.Add(kvp.Key);
                print("compare " + kvp.Value.expireTime + " " + Time.time);
            }
        }
        for(int i = 0; i < objectsToRemove.Count; i++)
        {
            print("BURN " + Time.time);
            enemies[objectsToRemove[i]].obj.GetComponent<Enemy>().TakeDamageFrom(1, ownerType, ownerID);
            unusedObjectTimers.Push(enemies[objectsToRemove[i]]);
            enemies.Remove(objectsToRemove[i]);
        }
        */
	}
    /*
    private void OnTriggerEnter(Collider other)
    {
        // fire area
        if (effectID == 0)
        {
            if (other.tag == "Enemy")
            {
                Transform t = other.transform;
                Enemy e = other.transform.parent.GetComponent<Enemy>();
                while (e == null)
                {
                    t = t.parent;
                    e = t.parent.GetComponent<Enemy>();
                }
                
                //    enemies.Add(e.id, ot);
                //print("contact " + Time.time + ", death by " + ot.expireTime);
            }
        }
    }
    */
    private void OnTriggerStay(Collider other)
    {
        // fire area
        if (effectID == 0)
        {
            if (other.tag == "Enemy")
            {
                Transform t = other.transform;
                Enemy e = other.transform.parent.GetComponent<Enemy>();
                while (e == null)
                {
                    t = t.parent;
                    e = t.parent.GetComponent<Enemy>();
                }
                DamageOverTime dot = e.gameObject.GetComponent<DamageOverTime>();
                //print(dot == null);
                if (dot == null)
                {
                    e.gameObject.AddComponent<DamageOverTime>();
                    dot = e.gameObject.GetComponent<DamageOverTime>();
                    dot.dmg = 1;
                    dot.duration = 1.5f;
                    dot.ownerID = ownerID;
                    dot.ownerType = ownerType;
                }
                    /*
                if (enemies.ContainsKey(e.id)) {
                    if (enemies[e.id].obj && enemies[e.id].expireTime <= Time.time)
                    {
                        print("BURN ENEMIE");
                        enemies[e.id].obj.GetComponent<Enemy>().TakeDamageFrom(1, ownerType, ownerID);
                        //enemies[e.id].obj.GetComponent<Enemy>().OnHit();
                        GameManager.gm.OnHitEnemy();
                        enemies[e.id].expireTime = Time.time + duration;
                        //unusedObjectTimers.Push(enemies[e.id]);
                        //enemies.Remove(e.id);
                    }
                }
                else
                {
                    ObjectTimer ot;
                    if (unusedObjectTimers.Count > 0)
                    {
                        ot = unusedObjectTimers.Pop();
                        ot.expireTime = Time.time + duration;
                        ot.obj = e.gameObject;
                    }
                    else
                        ot = new ObjectTimer(e.gameObject, Time.time + duration);
                    if (!enemies.ContainsKey(e.id))
                        enemies[e.id] = ot;
                }*/
            }
        }
    }


    /*
    private void OnTriggerExit(Collider other)
    {
        // fire area
        if (effectID == 0)
        {
            if (other.tag == "Enemy")
            {
                Transform t = other.transform;
                Enemy e = other.transform.parent.GetComponent<Enemy>();
                while (e == null)
                {
                    t = t.parent;
                    e = t.parent.GetComponent<Enemy>();
                }
                unusedObjectTimers.Push(enemies[e.id]);
                //if (enemies.ContainsKey(e.id))
                //{
                enemies.Remove(e.id);//[e.id] = new ObjectTimer(e.gameObject, Time.time + duration);
                //}
            }
        }
    }*/
}
