using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Trap : MonoBehaviour {
    public static int TrapCount = 0; // Keep track of traps created in game
    public int hp, // Amount of damage before activating trap
               trapID, // Distinguishes type of trap this is
               id; // Unique ID
	// Use this for initialization
	protected virtual void Start () {
        id = TrapCount;
        TrapCount++;
        name = "Trap " + id;
	}

    public virtual void TakeDamage(int dmg, int sid)
    {
        //Debug.Log("netwrok");
        hp -= dmg;
    }

    public virtual void TakeDamage(int dmg)
    {
        //Debug.Log("..");
        hp -= dmg;
    }

    public virtual void Activate()
    {

    }

    public virtual void Activate(int sid)
    {

    }


    // Update is called once per frame
    protected virtual void Update () {
        

    }
}
