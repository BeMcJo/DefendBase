using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContentSizeFitter : MonoBehaviour {
    public Vector2 dimension;
    public int childCount;
    RectTransform rt;
	// Use this for initialization
	void Awake () {
        childCount = 0;
        rt = transform.GetComponent<RectTransform>();
	}

    public void AddItem(GameObject item)
    {
        item.transform.SetParent(transform);
        childCount++;
        Resize();
    }
	
    public void Resize()
    {
        rt.sizeDelta = new Vector3(rt.rect.width, dimension.y * childCount);
    }

    public void SetItemActive(int i, bool b)
    {
        if (i < 0 || i >= childCount)
            return;

        Transform item = transform.GetChild(i);
        if (item.gameObject.activeSelf != b)
        {
            item.gameObject.SetActive(b);
            if (b)
                childCount++;
            else
                childCount--;
            Resize();
        }
    }

	// Update is called once per frame
	void Update () {
        /*
		if(childCount != transform.childCount)
        {
            childCount = transform.childCount;
            rt.sizeDelta = new Vector3(rt.rect.width, dimension.y * childCount);//, transform.localScale.z);
        }
        */
	}
}
