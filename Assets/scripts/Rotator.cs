using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Rotator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,IDragHandler {
    Vector2 initialPos;
    float incrementor;
    public float initialAngle, prevAngle,
          dragSpd = 2,
          rotateAcceleration,
          rotateVelocity,
          rotateSpd = 1,
          rotateFriction = .8f;
    bool reached45deg =false;
    public bool isTouched = false,
                isDragging;
    public bool isInteractable;
    public int[] itemOrder;
    public float holdTimer;
    public int curItem,
               nextItem,
               prevItem,
               awaitingItem,
               potentialAttribute,
               containerID,
               itemSwapIndex;
    Transform itemUIContainer;
    Text currentItemTxt;

    private void OnDisable()
    {
        currentItemTxt.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        currentItemTxt.gameObject.SetActive(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isInteractable)
            return;
        if (!isTouched)
            return;
        if (holdTimer < 0)
            return;
        //print("DRTAGGIN");
        isDragging = true;
        Transform wheel = transform;
        Vector2 touchPos = eventData.position;
        Vector2 C = new Vector2(wheel.position.x, wheel.position.y);
        float distCA = Vector2.Distance(C, initialPos), distCB = Vector2.Distance(C, touchPos), distAB = Vector2.Distance(initialPos, touchPos);
        float angle = GameManager.CosineFormula(distCA, distCB, distAB);
        int dir = 1;
        //print(t.position);
        //print(">> " + wheel.transform.position);
        //print("<<" + wheel.transform.localPosition);
        //angle *= 360 / Mathf.PI;
        int quadrant = 0;
        if (initialPos.y > wheel.position.y)
        {
            if (initialPos.x < wheel.position.x)
                quadrant = 1;
        }
        else
        {
            if (initialPos.x < wheel.position.x)
                quadrant = 2;
            else
                quadrant = 3;
        }
        Vector2 dif = initialPos - touchPos;
        float magX = Mathf.Abs(dif.x), magY = Mathf.Abs(dif.y);
        // Get direction to rotate wheel (clockwise = -1 or counter = 1)
        if (magX > magY)
        {
            if (dif.x < 0)
            {
                if (quadrant < 2)
                {
                    dir = -1;
                }
                else
                {
                    dir = 1;
                }
            }
            else
            {
                if (quadrant < 2)
                {
                    dir = 1;
                }
                else
                {
                    dir = -1;
                }
            }
        }
        else
        {
            if (dif.y < 0)
            {
                if (quadrant < 3 && quadrant > 0)
                {
                    dir = -1;
                }
                else
                {
                    dir = 1;
                }
            }
            else
            {
                if (quadrant < 3 && quadrant > 0)
                {
                    dir = 1;
                }
                else
                {
                    dir = -1;
                }
            }

        }

        float dist = Vector2.Distance(initialPos, touchPos);
        float offset = dir * angle * dragSpd;
        wheel.localEulerAngles = new Vector3(0, 0, offset + initialAngle);

        initialAngle = wheel.localEulerAngles.z;
        initialPos = touchPos;

        incrementor = (incrementor + offset + 360) % 360;

        CheckToSwapItem();
      
        rotateVelocity = dir * angle *rotateSpd;
    }

    public int GetSwapIndex()
    {
        //print("getswap");
        int curItemSwapIndex = 0;
        if (incrementor < 45 || incrementor >= 315)
        {
            curItemSwapIndex = 2;
        }
        else if (incrementor >= 45 && incrementor < 135)
        {
            curItemSwapIndex = 1;
        }
        else if (incrementor >= 135 && incrementor < 225)
        {
            curItemSwapIndex = 0;
        }
        else if (incrementor >= 225 && incrementor < 315)
        {
            curItemSwapIndex = 3;
        }
        return curItemSwapIndex;
    }

    public void CheckToSwapItem()
    {
        //print("chckswap");
        int curItemSwapIndex = GetSwapIndex();
        
        //print(itemSwapIndex + "..." + curItemSwapIndex + "......" + incrementor);
        int nextIndex = (itemSwapIndex + 1) % 4, prevIndex = (itemSwapIndex + 3) % 4;
        
        if (curItemSwapIndex != itemSwapIndex)
        {

            if (curItemSwapIndex == prevIndex)
            {
                print("rotate clockwise");
                ShiftItems(-1);
            }
            else if (curItemSwapIndex == nextIndex)
            {
                print("rotate counter");
                ShiftItems(1);
            }
        }
        //ShiftItems((int) Mathf.Sign(curItemSwapIndex - itemSwapIndex));
        itemSwapIndex = curItemSwapIndex;
        EditSelectableAttributes();
    }

    

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isInteractable)
            return;
        
        //print(eventData.hovered[0]);
        //print("ATTRIBUTE 111".Contains("ATTRIBUTE"));
        isTouched = true;
        initialPos = eventData.position;
        initialAngle = transform.localEulerAngles.z;
        foreach (GameObject go in eventData.hovered)
        {
            if (go.name.Contains("Attribute"))
            {
                if (isDragging)
                    return;
                string[] splitData = go.name.Split(' ');
                containerID = int.Parse(splitData[1]);
                
                potentialAttribute = itemOrder[containerID];
                print(containerID +"potential<<<<<<<<<<<<<<<<< " + potentialAttribute);
                holdTimer = .5f;
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isInteractable)
            return;
        foreach (GameObject go in eventData.hovered)
        {
            if (go.name.Contains("Attribute"))
            {
                string[] splitData = go.name.Split(' ');
                int cid = int.Parse(splitData[1]);
                if(cid == containerID && !isDragging)
                {
                    GameManager.gm.ChangeSelectedAttribute(itemOrder[cid]);//int.Parse(go.transform.GetChild(0).GetComponent<Text>().text.Split(' ')[1]));
                    //GameManager.gm.selectedAttribute = ;//potentialAttribute;
                    
                    //print("SELECTED ATTR " + potentialAttribute);
                    //print(GameManager.gm.selectedAttribute);
                    EditSelectableAttributes();
                    break;
                }
            }
        }
        potentialAttribute = -1;
        holdTimer = 0;
        isDragging = false;
        isTouched = false;
    }

    public void SetInteractable(bool b)
    {
        isInteractable = b;
        if (!b)
        {
            rotateVelocity = 0;
            isTouched = false;
        }
    }

    // Use this for initialization
    void Awake () {
        print("start");
        itemOrder = new int[4];
        currentItemTxt = transform.parent.Find("CurrentItemTxt").GetComponent<Text>();
        itemUIContainer = transform.Find("ItemUIContainer");
        for (int i = 0; i < 4; i++)//attributePrefabs.Length; i++)
        {
            GameObject icon = Instantiate(GameManager.gm.iconPrefab);
            icon.transform.SetParent(itemUIContainer);
            float x = Mathf.Cos(90 * Mathf.Deg2Rad * (1 + i)), y = Mathf.Sin(90 * Mathf.Deg2Rad * (1 + i));
            icon.transform.localPosition = new Vector3(x, y, 0) * 250;
            icon.transform.localEulerAngles += new Vector3(0, 0, 90 * i);
            icon.name = "Attribute " + i;
            icon.transform.localScale = new Vector3(1, 1, 1);
            //icon.AddComponent<Selector>();
            //icon.transform.GetChild(0).GetComponent<Text>().text = "Attribute " + i; ;
        }
        potentialAttribute = -1;
        itemSwapIndex = 2;
    }
    private void Start()
    {

        ResetItemWheel(0);
    }

    public void ResetItemWheel(int selectedAttribute)
    {
        //print("RESET WHEEL");
        initialAngle = 0;
        transform.localEulerAngles = Vector3.zero;
        incrementor = 0;
        itemSwapIndex = 2;
        curItem = itemOrder[0] = selectedAttribute;
        nextItem = itemOrder[1] = GameManager.gm.GetNextItem(curItem);//1 % GameManager.gm.attributePrefabs.Length;
        prevItem = itemOrder[3] = GameManager.gm.GetPrevItem(curItem);//(GameManager.gm.attributePrefabs.Length - 1) % GameManager.gm.attributePrefabs.Length;
        awaitingItem = itemOrder[2] = GameManager.gm.GetNextItem(nextItem);//(nextItem + 1) % GameManager.gm.attributePrefabs.Length;
        /*
        for (int i = 0; i < 4; i++)//attributePrefabs.Length; i++)
        {
            Transform icon = itemUIContainer.GetChild(i);// + itemSwapIndex - 2 + 4)%4);
            print("ITEM:"+i+".." + itemOrder[i]);
            icon.GetChild(0).GetComponent<Text>().text = "Attribute " + itemOrder[i] + " \n" + GameManager.gm.myAttributes[itemOrder[i]];
        }
        */
        //print("CURIS:"+curItem);
        //UpdateItemUI();
        EditSelectableAttributes();
    }

    public void UpdateItemUI()
    {
        
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
        }
        /*
        Transform icon = itemUIContainer.GetChild(itemSwapIndex);
        int attributeID = itemOrder[itemSwapIndex];
        icon.GetChild(0).GetComponent<Text>().text = "Attribute " + attributeID + " \n" + GameManager.gm.myAttributes[attributeID];

        icon = itemUIContainer.GetChild(GameManager.gm.GetPrevItem(itemSwapIndex));
        attributeID = itemOrder[itemSwapIndex];
        icon.GetChild(0).GetComponent<Text>().text = "Attribute " + attributeID + " \n" + GameManager.gm.myAttributes[attributeID];
        */
    }

    public void EditSelectableAttributes()
    {
        //print("editselec");
        for (int i = 0; i < 4; i++)
        {
            Transform icon = itemUIContainer.GetChild(i);
            //string data = icon.GetChild(0).GetComponent<Text>().text;// = "Attribute " + itemOrder[i];
            //string[] splitData = data.Split(' ');
            int attributeID = itemOrder[i];//int.Parse(splitData[1]);

            icon.Find("ItemIcon").GetComponent<Image>().sprite = GameManager.gm.itemIcons[attributeID];
            icon.Find("Selected BG").gameObject.SetActive(attributeID == GameManager.gm.selectedAttribute);
            /*
            icon.GetChild(0).GetComponent<Text>().text = "Attribute " + attributeID + " \n" + GameManager.gm.myAttributes[attributeID];
            if (attributeID == GameManager.gm.selectedAttribute)
            {
                icon.GetChild(0).GetComponent<Text>().text += " \n*";
            }
            */
            icon.Find("QtyTxt").GetComponent<Text>().text = "x";
            if (attributeID == 0)
                icon.Find("QtyTxt").GetComponent<Text>().text += "---";
            else
                icon.Find("QtyTxt").GetComponent<Text>().text += GameManager.gm.myAttributes[itemOrder[i]];
        }
        //print("INSIDE EDIT CUR :" + curItem);
        currentItemTxt.text = Attribute.names[curItem] + '\n' + Projectile.names[1];
    }
	
    // clockwise = -1, counter = 1
    public void ShiftItems(int dir)
    {
        //itemSwapIndex = 2;
        //int tmp = itemOrder[itemSwapIndex];
        int attributeCt = Attribute.names.Length;
        //int nextCurrentItem;
        //int nextItemSwapIndex;

        nextItem = GameManager.gm.GetNextItem(curItem);//(curItem + 1) % attributeCt;
        prevItem = GameManager.gm.GetPrevItem(curItem);//(curItem - 1 + attributeCt) % attributeCt;
        //print("NEXT ITEM" + nextItem);
        //print("PREV ITEM" + prevItem);
        //print(curItem + ".....");
        if (dir == 1)
        {
            awaitingItem = GameManager.gm.GetNextItem(nextItem);
            curItem = nextItem;
            //nextCurrentItem = nextItem;
            //nextItemSwapIndex = (itemSwapIndex + 1) % 4;
             //transform.GetChild(nextItemSwapIndex).GetChild(0).GetComponent<Text>().text = "Attribute " + (nextItem + 2) % attributeCt;

        }
        else
        {
            curItem = prevItem;
            //nextCurrentItem = prevItem;
            //nextItemSwapIndex = (itemSwapIndex + 3) % 4;
            awaitingItem = GameManager.gm.GetPrevItem(prevItem);
            //transform.GetChild(itemSwapIndex).GetChild(0).GetComponent<Text>().text = "Attribute " + (prevItem - 1 + attributeCt) % attributeCt;

        }
        //print(curItem);
        //int curItemIndex = (itemSwapIndex +)
        //itemUIContainer.GetChild(itemSwapIndex).GetChild(0).GetComponent<Text>().text = "Attribute " + awaitingItem + " \n" + GameManager.gm.myAttributes[awaitingItem];
        itemOrder[itemSwapIndex] = awaitingItem;
        GameManager.gm.ChangeSelectedAttribute(curItem);
        //itemSwapIndex = nextItemSwapIndex;
        /*
        if (dir == 1)
        {
            //int prevItem = (GameManager.gm.attributePrefabs.Length - 1 + curItem) % GameManager.gm.attributePrefabs.Length;
            transform.GetChild((itemSwapIndex + 1) % 4).GetChild(0).GetComponent<Text>().text = "Attribute " + (curItem + 3) % GameManager.gm.attributePrefabs.Length;
            transform.GetChild(itemSwapIndex).GetChild(0).GetComponent<Text>().text = "Attribute " + (curItem + 2) % GameManager.gm.attributePrefabs.Length;
            curItem = (curItem+1) % GameManager.gm.attributePrefabs.Length;
            itemSwapIndex = (itemSwapIndex + 1) % 4;
        }else if(dir == -1)
        {
            //int prevItem = (GameManager.gm.attributePrefabs.Length - 1 + curItem) % GameManager.gm.attributePrefabs.Length;
            transform.GetChild((itemSwapIndex + 3) % 4).GetChild(0).GetComponent<Text>().text = "Attribute " + (curItem + GameManager.gm.attributePrefabs.Length - 1) % GameManager.gm.attributePrefabs.Length;
            transform.GetChild(itemSwapIndex).GetChild(0).GetComponent<Text>().text = "Attribute " + (curItem -2 + GameManager.gm.attributePrefabs.Length) % GameManager.gm.attributePrefabs.Length;
            curItem = (curItem - 1 + GameManager.gm.attributePrefabs.Length) % GameManager.gm.attributePrefabs.Length;
            itemSwapIndex = (itemSwapIndex + 3) % 4;

        }*/
        //transform.GetChild((itemSwapIndex + 2) % 4).GetChild(0).GetComponent<Text>().text += "\n*";

    }

    // Update is called once per frame
    void Update () {
        if (!isInteractable)
            return;
        //return;
        if (!isTouched)
        {
            //print("nottouching");
            transform.localEulerAngles += new Vector3(0, 0, rotateVelocity);
            prevAngle = incrementor;
            incrementor = (incrementor + rotateVelocity + 360) % 360;
            CheckToSwapItem();
            /*
            if(incrementor != prevAngle)
                print(incrementor + "..." + prevAngle);
            if (Mathf.Abs(prevAngle) < 45 && Mathf.Abs(incrementor) >= 45)
            {
                print("shift items");
                ShiftItems(1);
            }
            else if (Mathf.Abs(prevAngle) >= 45 && Mathf.Abs(incrementor) < 45)
            {
                print("shift item2 ");
                ShiftItems(-1);
            }
            if (Mathf.Abs(incrementor) >= 90)
            {
                print("move to next item");
                int sign = (int)Mathf.Sign(incrementor);
                incrementor = (Mathf.Abs(incrementor) - 90) * sign;
                prevAngle = incrementor;
                curItem += sign;
            }
            */
            rotateVelocity *= rotateFriction;
        }
        if (potentialAttribute != -1 && !isDragging)
        {
            holdTimer -= Time.deltaTime;
            //print(holdTimer);
        }
    }
}
