using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Selector : MonoBehaviour, IPointerClickHandler,IPointerUpHandler {
    public string selectionType;
    public int id;
    public void OnPointerClick(PointerEventData eventData)
    {
        print("CLICKED");
        print(id);
        if (selectionType == "attribute")
        {
            GameManager.gm.ChangeSelectedAttribute(id);
        }
        //Select();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //print("FINUP");
    }

    public void SetSelected(bool isSelected)
    {
        //print("SELECTED");
        transform.Find("Selected BG").gameObject.SetActive(isSelected);
    }

    public void UpdateItemUI()
    {
        /*
        for (int i = 0; i < 4; i++)//attributePrefabs.Length; i++)
        {
            Transform icon = itemUIContainer.GetChild(i);
            //string[] splitData = icon.GetChild(0).GetComponent<Text>().text.Split(' ');
            int attributeID = itemOrder[i];//int.Parse(splitData[1]);
            //icon.GetChild(0).GetComponent<Text>().text = "Attribute " + attributeID + " \n" + GameManager.gm.myAttributes[attributeID];
            icon.Find("QtyTxt").GetComponent<Text>().text = "x";
            if (attributeID == 0)
                icon.Find("QtyTxt").GetComponent<Text>().text += "---";
            else
                icon.Find("QtyTxt").GetComponent<Text>().text += GameManager.gm.myAttributes[itemOrder[i]];
            /*
            if (attributeID == 0)
                icon.GetChild(0).GetComponent<Text>().text += " \nINF";//GameManager.gm.myAttributes[attributeID];// "Attribute " + itemOrder[i];
            else
                icon.GetChild(0).GetComponent<Text>().text += " \n" + GameManager.gm.myAttributes[attributeID];// "Attribute " + itemOrder[i];
            */
        //}
        /*
        Transform icon = itemUIContainer.GetChild(itemSwapIndex);
        int attributeID = itemOrder[itemSwapIndex];
        icon.GetChild(0).GetComponent<Text>().text = "Attribute " + attributeID + " \n" + GameManager.gm.myAttributes[attributeID];

        icon = itemUIContainer.GetChild(GameManager.gm.GetPrevItem(itemSwapIndex));
        attributeID = itemOrder[itemSwapIndex];
        icon.GetChild(0).GetComponent<Text>().text = "Attribute " + attributeID + " \n" + GameManager.gm.myAttributes[attributeID];
        */
    }


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
