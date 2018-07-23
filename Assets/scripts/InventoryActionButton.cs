using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryActionButton : MonoBehaviour {

    public void PerformInventoryAction()
    {
        // if it's tab button, change tabs
        if(tag == "ChangeInventoryTab")
        {
            GameManager.gm.ChangeSelectedTab(transform.Find("Text").GetComponent<Text>().text);
            return;
        }

        // otherwise it's an action button for an item
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
            case "arrowUI":
                GameManager.gm.HandleArrowItemAction(int.Parse(details[1]));
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
