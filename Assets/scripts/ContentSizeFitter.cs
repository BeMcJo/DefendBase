using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContentSizeFitter : MonoBehaviour {
    public Vector2 dimension, offset;
    public int activeChildCount, columnsPerRow=1;
    RectTransform rt;
    private List<GameObject> items;
	// Use this for initialization
	void Awake () {
        activeChildCount = 0;
        items = new List<GameObject>();
        rt = transform.GetComponent<RectTransform>();
	}

    public void AddItem(GameObject item)
    {
        item.transform.SetParent(transform);
        items.Add(item);
        activeChildCount++;
        Resize();
    }
	
    public void Resize()
    {
        rt.sizeDelta = new Vector3(rt.rect.width, dimension.y * (1 + activeChildCount / columnsPerRow));
        transform.localPosition = new Vector3(transform.localPosition.x + offset.x, -dimension.y / 2 + offset.y, transform.localPosition.z);
        if(activeChildCount == 1)
        {
            transform.localPosition += new Vector3(0, dimension.y, 0);
        }
    }

    public void SetItemActive(int i, bool b)
    {
        if (i < 0 || i >= items.Count)
        {
            Debug.LogError("Out of bounds");
            return;
        }
        
        if (items[i].activeSelf != b)
        {
            print("??");
            items[i].SetActive(b);
            if (b)
                activeChildCount++;
            else
                activeChildCount--;
            Resize();
        }
    }

	// Update is called once per frame
	void Update ()
    {
        if(activeChildCount == 1)        
            transform.localPosition = new Vector3(transform.localPosition.x + offset.x, -dimension.y / 2 + (2*offset.y), transform.localPosition.z);
        /*
		if(childCount != transform.childCount)
        {
            childCount = transform.childCount;
            rt.sizeDelta = new Vector3(rt.rect.width, dimension.y * childCount);//, transform.localScale.z);
        }
        */
    }
}
