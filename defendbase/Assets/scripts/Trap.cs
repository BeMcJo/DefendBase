using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Trap : MonoBehaviour {
    public static int TrapCount = 0; // Keep track of traps created in game
    public int hp, // Amount of damage before activating trap
               trapID, // Distinguishes type of trap this is
               id; // Unique ID
	// Use this for initialization
	protected virtual void Awake () {
        id = TrapCount;
        TrapCount++;
        name = "Trap " + id;
	}

    protected virtual void Start()
    {

    }

    // Format:
    // TRAP|Trap ID|Relative Position to First Objective|
    public virtual string NetworkInformation()
    {
        Transform parent = transform.parent;
        Vector3 scale = transform.localScale;
        transform.SetParent(GameManager.gm.objective.transform);
        Vector3 pos = transform.localPosition;
        transform.SetParent(parent);
        transform.localScale = scale;
        return "TRAP|" + trapID + "|" + pos.x +"," + pos.y +"," +pos.z+"|";
    }

    // Extract network information
    public virtual void SetNetworkInformation(string[] data)
    {
        string[] pos = data[2].Split(',');
        Transform parent = transform.parent;
        Vector3 scale = transform.localScale;
        transform.SetParent(GameManager.gm.objective.transform);
        transform.localPosition = new Vector3(float.Parse(pos[0]),float.Parse(pos[1]),float.Parse(pos[2]));
        transform.SetParent(parent);
        transform.localScale = scale;
        Debug.Log("SETUP TRAP" + transform.localScale);

        //float chargePower = float.Parse(data[2]);
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
