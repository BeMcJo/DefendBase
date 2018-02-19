using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Bow : Weapon {
    public Transform arrow;
    public Transform bow;
    private LineRenderer lr;
    private Vector2 originalTouchPos;
    private float drawRange, drawLimit, drawOffset;
    private float maxScreenDrawRange;
    public float drawElasticity = 15;
	// Use this for initialization
	protected override void Start ()
    {
        wepID = 0;
        base.Start();
        //interactiveTouch = false;
        distance = 2000;
        arrow = transform.Find("Arrow");
        bow = transform.Find("Bow");
        lr = bow.GetComponent<LineRenderer>();
        drawOffset = -3.65f;
        drawRange = drawOffset;
        drawLimit = -1.7f;
        chargeLimit = .9f;
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
	protected override bool Update () {
        //Debug.Log(lvl);
        if (reloading)
        {
            Reload();
            return true;
        }

        base.Update();

        if (charging)
        {
            Charge(chargePower);
        }
        if(arrow == null)
        {
            Reload();
            return true;
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
        return true;    
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
        //Debug.Log("bow");
        if (NetworkManager.nm.isStarted && !user.IsMyPlayer())
        {
            base.StartUse(t);
            charging = true;
            return true;
        }
        //if (!GameManager.gm.interactiveTouch)
        //{
        //}
        if (!GameManager.gm.interactiveTouch)
        {
            //Debug.Log("HERE");
            /*RaycastHit2D hitInfo2 = Physics2D.Raycast(t.position, user.playerCam.transform.forward);
            if (hitInfo2.collider != null)
            {
                Debug.Log(hitInfo2.collider.name);
            }
            else
            {
                Debug.Log("NOTHING");
            }*/
            if (EventSystem.current.IsPointerOverGameObject(t.fingerId))
            {
                // ui touched
               // Debug.Log(EventSystem.current.currentSelectedGameObject == null);
                if (EventSystem.current.currentSelectedGameObject && EventSystem.current.currentSelectedGameObject.tag == "Shoot")
                {
                    //Debug.Log(EventSystem.current.currentSelectedGameObject.name);
                    base.StartUse(t);
                    shootTouchID = t.fingerId;
                }
            }
            return charging;
        }
        //Debug.Log("check");
        //Debug.Log(user == null);
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
                shootTouchID = t.fingerId;
                originalTouchPos = t.position;
                return true;
            }
        }

        return false;
    }

    public override void EndUse()
    {
        shootTouchID = -1;
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
        if (!GameManager.gm.interactiveTouch)
        {
            chargePower = chargePower + .03f;
            if (chargePower >= chargeLimit)
                chargePower = chargeLimit;
            chargeBar.transform.localScale = new Vector3(1, chargePower/chargeLimit, 1);
            //this.chargePower = chargePower;
            //chargePower
            /*if (chargeBar.transform.localScale.y >= 1)
            {
                chargeBar.transform.localScale = new Vector3(
                    chargeBar.transform.localScale.x,
                    1,
                    chargeBar.transform.localScale.z);
                chargePower = 1;
                chargeAccelerator *= -1;
                chargeBarAlt = -1;
            }*/
        }
        else
        {
            Touch t;
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).fingerId == shootTouchID)
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

                    break;
                }
            }
        }
        drawRange = drawOffset + chargePower * drawLimit * drawElasticity;
        arrow.localPosition = bulletSpawn.localPosition + new Vector3(0, 0, chargePower * drawLimit);
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
            //rb.AddForce(user.playerCam.transform.forward * chargePower * distance);
            rb.AddForce(user.playerCam.transform.forward * chargePower * statsByLevel[wepID].distance[lvl]);
            rb.useGravity = true;
            arrow.GetComponent<Arrow>().isShot = true;
            arrow.GetComponent<Arrow>().id = user.id;
            arrow.GetComponent<Arrow>().dmg = statsByLevel[wepID].dmg[lvl];
            //arrow.transform.Find("Arrow Tip").GetComponent<BoxCollider>().isTrigger = false;
            arrow = null;
            reloading = true;
            //reloadTime = timeToReload;
            reloadTime = statsByLevel[wepID].timeToReload[lvl];
            List<Vector3> positions = new List<Vector3>();
            positions.Add(new Vector3(0, .5f, -.475f));
            positions.Add(new Vector3(0, 0, drawRange));
            positions.Add(new Vector3(0, -.5f, -.475f));
            lr.SetPositions(positions.ToArray());
        }
    }
}
