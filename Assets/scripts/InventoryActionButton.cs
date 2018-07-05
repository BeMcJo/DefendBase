using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryActionButton : MonoBehaviour {

    public void PerformInventoryAction()
    {
        print("inventory "+ name);
        string[] details = name.Split(' ');
        print(details[0]);
        print(details[1]);
        // Handle action based on item type
        switch (details[0])
        {
            case "wepUI":
                GameManager.gm.HandleWeaponItemAction(int.Parse(details[1]));
                break;
        }
    }
    



    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
