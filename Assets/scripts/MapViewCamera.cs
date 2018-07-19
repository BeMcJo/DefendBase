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
    public float horizExtent, vertExtent, mapX, mapY, minX, minY, maxX, maxY;
    public bool playerCam, canZoom;
	// Use this for initialization
	void Start () {
        mapCam = transform.GetComponent<Camera>();
        touchIDs = new int[] { -1, -1 };
        zoomIDs = new int[] { -1, -1 };
        moveID = -1;
        prevDist = 0;
        mapX = 30;
        mapY = 30;

        minX = -horizExtent + mapX / 2;
        maxX = -mapX / 2 + horizExtent;
        minY = -vertExtent + mapY / 2;
        maxY = -mapY / 2 + vertExtent;
    }

    public void Reset()
    {
        if(touchIDs != null)
            touchIDs[0] = touchIDs[1] = -1;
    }

    private void LateUpdate()
    {
        if (playerCam)
            return;
        Transform bounds = GameManager.gm.playerRotation.transform.Find("Bounds");
        minX = bounds.GetChild(0).localPosition.x;
        maxX = bounds.GetChild(1).localPosition.x;
        minY = bounds.GetChild(2).localPosition.z;
        maxY = bounds.GetChild(3).localPosition.z;
        vertExtent = mapCam.orthographicSize;
        horizExtent = vertExtent * Screen.width / Screen.height;
        Vector3 pos = transform.localPosition;
        if (pos.z + vertExtent >= maxY)
            pos.z = maxY - vertExtent;
        if (pos.z - vertExtent <= minY)
            pos.z = vertExtent + minY;
        if (pos.x + horizExtent >= maxX)
            pos.x = maxX - horizExtent;
        if (pos.x - horizExtent <= minX)
            pos.x = horizExtent + minX;
        //pos.x = Mathf.Clamp(pos.x, minX, maxX);
        //pos.z = Mathf.Clamp(pos.z, minY, maxY);
        transform.localPosition = pos;
    }
    // Update is called once per frame
    void Update() {
        if (playerCam && !canZoom)
            return;
        for (int i = 0; i < Input.touchCount; i++)
        {
            //Debug.Log(1);
            Touch t = Input.GetTouch(i);
            if (t.phase == TouchPhase.Began)
            {
                // Is there an object we are touching?
                if (EventSystem.current.IsPointerOverGameObject(t.fingerId))
                {
                    //if (EventSystem.current.currentSelectedGameObject)
                    //    Debug.Log(EventSystem.current.currentSelectedGameObject.tag + "," + EventSystem.current.currentSelectedGameObject.name + "..." + LayerMask.LayerToName(EventSystem.current.currentSelectedGameObject.layer));
                    // Is that object a PlaceDefense object?
                    if (EventSystem.current.currentSelectedGameObject && EventSystem.current.currentSelectedGameObject.layer == LayerMask.NameToLayer("UI"))
                    {
                        //Debug.Log(EventSystem.current.currentSelectedGameObject.tag + "," + EventSystem.current.currentSelectedGameObject.name + "..." + LayerMask.LayerToName(EventSystem.current.currentSelectedGameObject.layer));

                        continue;
                    }
                }
                else
                {
                   // Debug.Log(1);
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
        if (!playerCam && GameManager.gm.selectedDefense && GameManager.gm.mapFingerID != -1)
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
        if(!playerCam && touches.Count == 1)
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
                            transform.localPosition += new Vector3(-dif.x, 0, -dif.y) * moveSpd * (mapCam.orthographicSize / defaultZoom);
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
                if (!playerCam)
                {
                    mapCam.orthographicSize -= dif * zoomSpd;
                    if (mapCam.orthographicSize <= maxZoomIn)
                        mapCam.orthographicSize = maxZoomIn;
                    if (mapCam.orthographicSize >= maxZoomOut)
                        mapCam.orthographicSize = maxZoomOut;
                }
                else
                {
                    mapCam.fieldOfView -= dif * zoomSpd;
                    if (mapCam.fieldOfView <= maxZoomIn)
                        mapCam.fieldOfView = maxZoomIn;
                    if (mapCam.fieldOfView >= maxZoomOut)
                        mapCam.fieldOfView = maxZoomOut;
                }
            }
            prevDist = newDist;
        }
        else if (touches.Count == 0)
        {
            prevDist = 0;
        }
	}
}
