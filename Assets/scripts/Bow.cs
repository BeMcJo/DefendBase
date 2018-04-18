using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Bow : Weapon
{
    public Transform arrow; // The type of projectile shot by this weapon
    public Transform bow; // The actual game object
    private LineRenderer lr; // Used for the bowstring
    private Vector2 originalTouchPos; // Used for Touch Interactive shooting. Determines bow draw distance

    private float drawRange, // How far arrow and bowstring can be pulled
                  drawLimit, // Max draw range
                  drawOffset; // Default position when arrow and bowstring isn't pulled
    private float maxScreenDrawRange; // Distance from middle of screen to corner
    public float drawElasticity = 15; // Determines how much string can stretch upon pulling
	// Use this for initialization
	protected override void Start ()
    {
        wepID = 0;
        base.Start();
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
	
    // Key information to have NetworkManager send to other players
    public override string NetworkInformation()
    {
        string s = base.NetworkInformation();
        return s;
    }
    // Assign values received from NetworkManager
    public override void SetNetworkInformation(string[] data)
    {
        base.SetNetworkInformation(data);
        float chargePower = float.Parse(data[2]);
        Charge(chargePower);
    }

	/*
     * Reload if shot arrow
     * If not, then check if user attempt to shoot
     * Update bowstring based on arrow position
     */
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
        List<Vector3> positions = new List<Vector3>(); // Keeps track of the vertices of the bowstring
        positions.Add(new Vector3(0, .5f, -.475f)); // Vertex at top of bow
        if (reloading)
            positions.Add(new Vector3(0, 0, drawRange)); // Default position of middle vertex
        else
        {
            positions.Add(new Vector3(0, 0, drawRange + arrow.localPosition.z - 2.35f)); // Position of arrow tail
        }
        positions.Add(new Vector3(0, -.5f, -.475f)); // Vertex at bottom of bow
        lr.SetPositions(positions.ToArray()); // Assign vertices positions
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
        //arrow.GetComponent<Projectile>().SetAttribute(GameManager.gm.selectedAttribute);//.attributeID = GameManager.gm.selectedAttribute;
    }

    // Charges up arrow, drawing it back until drawLimit
    public override bool StartUse(Touch t)
    {
        // print(gameObject);
        //print(user);
        //print(user.IsMyPlayer());
        // If playing online and not my weapon, set the arrow and drawstring to information received
        if (NetworkManager.nm.isStarted && !user.IsMyPlayer())
        {
            base.StartUse(t);
            return true;
        }
        // If not using Touch Interactive mode, check if shoot button is being touched
        if (!GameManager.gm.interactiveTouch)
        {
            // Is there an object we are touching?
            if (EventSystem.current.IsPointerOverGameObject(t.fingerId))
            {
                // Is that object our Shoot Button?
                if (EventSystem.current.currentSelectedGameObject && EventSystem.current.currentSelectedGameObject.tag == "Shoot")
                {
                    //print("??");
                    base.StartUse(t);
                    shootTouchID = t.fingerId;
                }
            }
            return charging;
        }
        // Using Touch Interactive. Check if object we touch is arrow tail
        RaycastHit2D hitInfo = Physics2D.Raycast(t.position, user.playerCam.transform.forward);
        RaycastHit hit;
        Ray ray = user.playerCam.ScreenPointToRay(t.position);
        // If we are touching arrow tail
        if (Physics.Raycast(ray, out hit) && hit.collider.name == "Tail")
        {
            base.StartUse(t);
            shootTouchID = t.fingerId;
            originalTouchPos = t.position;
            return true;
        }

        return false;
    }

    public override void ChangeAttribute()
    {
        base.ChangeAttribute();
        if (arrow)
        {
            print("change arrow attribute");
            arrow.GetComponent<Projectile>().SetAttribute(GameManager.gm.selectedAttribute);//.attributeID = GameManager.gm.selectedAttribute;
        }
    }

    // Upon end use, shoot arrow
    public override void EndUse()
    {
        shootTouchID = -1;
        if(arrow)
            arrow.localPosition = bulletSpawn.localPosition;
        drawRange = drawOffset;
        inUse = false;
        Shoot(chargePower);
        //charging = false;
        //chargePower = 0;
        base.EndUse();
    }
    
    public override void Charge(float chargePower)
    {
        // Assign charge to bow if multiplayer and not my weapon
        if (NetworkManager.nm.isStarted && !user.IsMyPlayer())
        {
            this.chargePower = chargePower;
            drawRange = drawOffset + chargePower * drawLimit * drawElasticity;
            if(arrow)
                arrow.localPosition = bulletSpawn.localPosition + new Vector3(0, 0, chargePower * drawLimit);
            return;
        }
        // If not using Touch Interactive mode, progressively increase the chargePower as long as user holds shoot button
        if (!GameManager.gm.interactiveTouch)
        {
            chargePower = chargePower + .03f; // increment charge power
            if (chargePower >= chargeLimit)
                chargePower = chargeLimit;
            chargeBar.transform.localScale = new Vector3(1, chargePower/chargeLimit, 1); // Visual indicator of charge percentage
        }
        // Using Touch Interactive
        else
        {
            Touch t;
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).fingerId == shootTouchID)
                {
                    t = Input.GetTouch(i);
                    // If finger slides to the left half of the screen, arrow won't be drawn
                    if (t.position.x < originalTouchPos.x)
                    {
                        chargePower = 0;
                    }
                    // If finger is on right half of screen, calculate charge power based on distance of original position to finger position
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
        drawRange = drawOffset + chargePower * drawLimit * drawElasticity; // Calculate how far arrow and bowstring is drawn
        arrow.localPosition = bulletSpawn.localPosition + new Vector3(0, 0, chargePower * drawLimit); // Move arrow backwards based on charge power 
        this.chargePower = chargePower;
    }

    // Launch arrow if charge exceeds minimum
    public override void Shoot(float chargePower)
    {
        if (chargePower > .05f && arrow)
        {
            arrow.SetParent(GameManager.gm.projectilesContainer.transform);
            Rigidbody rb = arrow.transform.GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.None; // Remove the fixed position of arrow
            rb.AddForce(user.playerCam.transform.forward * chargePower * statsByLevel[wepID].distance[lvl] * 100); // Launch arrow
            rb.useGravity = true;
            Arrow arrw = arrow.GetComponent<Arrow>();
            arrw.Shoot(null, user.tag, user.id, statsByLevel[wepID].dmg[lvl]);
            //arrw.isShot = true;
            //arrw.id = user.id;
            //arrw.dmg = statsByLevel[wepID].dmg[lvl];
            //arrw.transform.Find("Tail").GetComponent<BoxCollider>().enabled = false;
            //arrw.attributeID = GameManager.gm.selectedAttribute;
            arrow = null;
            reloading = true;
            reloadTime = statsByLevel[wepID].timeToReload[lvl];
            if(user.IsMyPlayer())
                GameManager.gm.UseItem("Attribute", GameManager.gm.selectedAttribute);

            // Reset bowstring vertices
            List<Vector3> positions = new List<Vector3>();
            positions.Add(new Vector3(0, .5f, -.475f));
            positions.Add(new Vector3(0, 0, drawRange));
            positions.Add(new Vector3(0, -.5f, -.475f));
            lr.SetPositions(positions.ToArray());
            base.Shoot(chargePower);
        }
    }
}
