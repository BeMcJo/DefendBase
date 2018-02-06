﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : Weapon {
    public Transform arrow;
    public Transform bow;
    private LineRenderer lr;
    private int touchID;
    private Vector2 originalTouchPos;
    private float drawRange, drawLimit, drawOffset;
    private float maxScreenDrawRange;
    public float drawElasticity = 15;
	// Use this for initialization
	protected override void Start () {
        base.Start();
        distance = 2000;
        arrow = transform.Find("Arrow");
        bow = transform.Find("Bow");
        lr = bow.GetComponent<LineRenderer>();
        drawOffset = -3.65f;
        drawRange = drawOffset;
        drawLimit = -1.7f;
        reloadTime = 0;
        timeToReload = 1f;
        maxScreenDrawRange = Vector2.Distance(new Vector2(0, 0), new Vector2(Screen.width, Screen.height) / 2);
	}
	
    public override string NetworkInformation()
    {
        string s = base.NetworkInformation();
        //s += chargePower + "|";
        return s;
    }

    public override void SetNetworkInformation(string[] data)
    {
        base.SetNetworkInformation(data);
        float chargePower = float.Parse(data[2]);
        Charge(chargePower);
    }

	// Update is called once per frame
	protected override void Update () {
        if (reloading)
        {
            Reload();
        }
        if (charging)
        {
            Charge(chargePower);
        }
        if(arrow == null)
        {
            Reload();
            return;
        }
        List<Vector3> positions = new List<Vector3>();
        positions.Add(new Vector3(0, .5f, -.475f));
        if (reloading)
            positions.Add(new Vector3(0, 0, drawRange));
        else
        {
            //Transform tail = arrow.transform.Find("Tail");
            positions.Add(new Vector3(0, 0, drawRange + arrow.localPosition.z - 2.35f));
        }
        positions.Add(new Vector3(0, -.5f, -.475f));
        lr.SetPositions(positions.ToArray());
        //Debug.Log(positions.Count);
        //foreach(Vector3 v in positions){
        //    Debug.Log(">" + v.x + " " + v.y + " " + v.z);
        //}
        //Debug.Log("zzz");
        
	}

    public void Reload()
    {
        reloadTime -= Time.deltaTime;
        if (reloadTime > 0)
            return;
        reloading = false;
        arrow = Instantiate(bulletPrefab).transform;
        arrow.transform.rotation = bulletSpawn.transform.rotation;
        arrow.transform.SetParent(transform);
        arrow.transform.localPosition = bulletSpawn.transform.localPosition;
    }

    public override bool StartUse(Touch t)
    {
        if (NetworkManager.nm.isStarted && !user.IsMyPlayer())
        {
            base.StartUse(t);
            charging = true;
            return true;
        }
        //Debug.Log("check");
        RaycastHit2D hitInfo = Physics2D.Raycast(t.position, user.playerCam.transform.forward);
        if(hitInfo.collider != null)
        {
            Debug.Log(hitInfo.collider.name);
        }

        RaycastHit hit;
        Ray ray = user.playerCam.ScreenPointToRay(t.position);
        if(Physics.Raycast(ray, out hit))
        {
            //Debug.Log("SOMTHING");
            //Debug.Log(hit.collider.name);
            if(hit.collider.name == "Tail")
            {
                base.StartUse(t);
                charging = true;
                touchID = t.fingerId;
                originalTouchPos = t.position;
                return true;
            }
        }

        return false;
    }

    public override void EndUse()
    {
        touchID = -1;
        if(arrow)
            arrow.localPosition = bulletSpawn.localPosition;
        drawRange = drawOffset;
        Shoot(chargePower);
        charging = false;
        chargePower = 0;
        base.EndUse();
    }

    public override void Charge(float chargePower)
    {
        if (NetworkManager.nm.isStarted && !user.IsMyPlayer())
        {
            this.chargePower = chargePower;
            drawRange = drawOffset + chargePower * drawLimit * drawElasticity;
            if(arrow)
                arrow.localPosition = bulletSpawn.localPosition + new Vector3(0, 0, chargePower * drawLimit);
            return;
        }
        Touch t;
        for (int i = 0; i < Input.touchCount; i++)
        {
            if (Input.GetTouch(i).fingerId == touchID)
            {
                t = Input.GetTouch(i);
                if (t.position.x < originalTouchPos.x)
                {
                    chargePower = 0;
                    //    break;
                }
                else
                {
                    float dist = Vector2.Distance(t.position, originalTouchPos);
                    //Debug.Log(dist + " " + Screen.width / 2);
                    chargePower = dist / maxScreenDrawRange;
                }
                drawRange = drawOffset + chargePower * drawLimit * drawElasticity;
                arrow.localPosition = bulletSpawn.localPosition + new Vector3(0, 0, chargePower * drawLimit);
                break;
            }
        }
        this.chargePower = chargePower;
        //Debug.Log(chargePower);
    }

    public override void Shoot(float chargePower)
    {
        if (chargePower > .05f && arrow)
        {
            //chargeBarGuage.SetActive(false);
            //GameObject bullet = Instantiate(bulletPrefab);
            arrow.SetParent(GameManager.gm.projectilesContainer.transform);
            //bullet.transform.position = bulletSpawn.transform.position;
            Rigidbody rb = arrow.transform.GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.None;
            rb.AddForce(user.playerCam.transform.forward * chargePower * distance);
            rb.useGravity = true;
            arrow.GetComponent<Arrow>().isShot = true;
            arrow.GetComponent<Arrow>().id = user.id;
            //arrow.transform.Find("Arrow Tip").GetComponent<BoxCollider>().isTrigger = false;
            arrow = null;
            reloading = true;
            reloadTime = timeToReload;
            List<Vector3> positions = new List<Vector3>();
            positions.Add(new Vector3(0, .5f, -.475f));
            positions.Add(new Vector3(0, 0, drawRange));
            positions.Add(new Vector3(0, -.5f, -.475f));
            lr.SetPositions(positions.ToArray());
        }
    }
}
