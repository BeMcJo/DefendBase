using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapViewCamera : MonoBehaviour {
    Camera mapCam;
    int[] touchIDs, zoomIDs;
    int moveID;
    Vector2 prevPos;
    float prevDist;
    public float moveSpd = .01f, zoomSpd = .1f;
    public float maxZoomIn = 4f, maxZoomOut = 75, defaultZoom = 50;
	// Use this for initialization
	void Start () {
        mapCam = transform.GetComponent<Camera>();
        touchIDs = new int[] { -1, -1 };
        zoomIDs = new int[] { -1, -1 };
        moveID = -1;
        prevDist = 0;
    }

    public void Reset()
    {
        if(touchIDs != null)
            touchIDs[0] = touchIDs[1] = -1;
    }

    // Update is called once per frame
    void Update() {
        for (int i = 0; i < Input.touchCount; i++)
        {
            //Debug.Log(1);
            Touch t = Input.GetTouch(i);
            if (t.phase == TouchPhase.Began)
            {
                // Is there an object we are touching?
                if (EventSystem.current.IsPointerOverGameObject(t.fingerId))
                {
                    if (EventSystem.current.currentSelectedGameObject)
                        Debug.Log(EventSystem.current.currentSelectedGameObject.tag + "," + EventSystem.current.currentSelectedGameObject.name + "..." + LayerMask.LayerToName(EventSystem.current.currentSelectedGameObject.layer));
                    // Is that object a PlaceDefense object?
                    if (EventSystem.current.currentSelectedGameObject && EventSystem.current.currentSelectedGameObject.layer == LayerMask.NameToLayer("UI"))
                    {
                        Debug.Log(EventSystem.current.currentSelectedGameObject.tag + "," + EventSystem.current.currentSelectedGameObject.name + "..." + LayerMask.LayerToName(EventSystem.current.currentSelectedGameObject.layer));

                        continue;
                    }
                }
                else
                {
                    Debug.Log(1);
                }
                for (int j = 0; j < touchIDs.Length; j++)
                {
                    if (touchIDs[j] == -1)
                    {
                        //Debug.Log(2);
                        touchIDs[j] = t.fingerId;
                        break;
                    }
                }
            }
            else if (t.phase == TouchPhase.Ended)
            {
                for (int j = 0; j < touchIDs.Length; j++)
                {
                    if (touchIDs[j] == t.fingerId)
                    {
                        if(touchIDs[j] == moveID)
                            moveID = -1;
                        touchIDs[j] = -1;
                        break;
                    }
                }
            }
        }
        if (GameManager.gm.selectedDefense)
        {
            return;
        }
        int touchCt = 0;
        List<int> touches = new List<int>();
        for (int i = 0; i < touchIDs.Length; i++)
        {
            if (touchIDs[i] != -1)
                touches.Add(touchIDs[i]);
        }
        if(touches.Count == 1)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                //Debug.Log(1);
                Touch t = Input.GetTouch(i);
                if (touches[0] == t.fingerId)
                {
                    if (moveID == -1)
                    {
                        moveID = touches[0];
                        prevPos = Vector2.zero;
                    }
                    if (t.phase == TouchPhase.Moved)
                    {
                        if (prevPos != Vector2.zero)
                        {
                            //Debug.Log(4);
                            Vector2 dif = t.position - prevPos;
                            transform.position += new Vector3(-dif.x, 0, -dif.y) * moveSpd * (mapCam.orthographicSize / defaultZoom);
                        }
                        prevPos = t.position;
                    }
                }
            }
        }
        else if (touches.Count == 2)
        {
            List<Vector2> positions = new List<Vector2>();
            for(int i = 0;i <Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                for(int j = 0; j < touches.Count; j++)
                {
                    if(touches[j] == t.fingerId)
                    {
                        positions.Add(t.position);
                        break;
                    }
                }
            }
            float newDist = Vector2.Distance(positions[0], positions[1]);
            float dif = newDist - prevDist;
            //Debug.Log(dif);
            if (prevDist > 0 || prevDist < 0)
            {
                mapCam.orthographicSize -= dif * zoomSpd;
                if (mapCam.orthographicSize <= maxZoomIn)
                    mapCam.orthographicSize = maxZoomIn;
                if (mapCam.orthographicSize >= maxZoomOut)
                    mapCam.orthographicSize = maxZoomOut;
            }
            prevDist = newDist;
        }
        else if (touches.Count == 0)
        {
            prevDist = 0;
        }
	}
}
