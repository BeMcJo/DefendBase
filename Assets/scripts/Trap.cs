using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Trap : MonoBehaviour {
    public static int TrapCount = 0; // Keep track of traps created in game
    public static int[] costs = new int[] { 20 ,1};
    public static string[] names = new string[] { "TNT","txt2" };
    public string ownerType; // unused?
    public int hp, // Amount of damage before activating trap
               ownerID = -1, // Which player owns this trap? (-1 = No one)
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
    // TRAP|Trap ID|Relative Position to First Objective|owner ID|
    public virtual string NetworkInformation()
    {
        Transform parent = transform.parent;
        Vector3 scale = transform.localScale;
        transform.SetParent(GameManager.gm.objective.transform);
        Vector3 pos = transform.localPosition;
        transform.SetParent(parent);
        transform.localScale = scale;
        return "TRAP|" + trapID + "|" + pos.x +"," + pos.y +"," +pos.z+"|" + ownerID + "|";
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
        ownerID = int.Parse(data[3]);
        Debug.Log("SETUP TRAP" + transform.localScale);
        GetComponent<ObjectPlacement>().description.transform.Find("Trap " + id).gameObject.SetActive(ownerID == GameManager.gm.player.GetComponent<PlayerController>().id);
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

    public virtual void Activate(string sourceType,int sid)
    {

    }


    // Update is called once per frame
    protected virtual void Update () {
        

    }
}
