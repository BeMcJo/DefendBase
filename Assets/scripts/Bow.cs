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
    public GameObject[] bowstringPositionPlaceHolders;
    protected Vector3[] bowstringPositions;
	// Use this for initialization
	protected override void Start ()
    {
        wepID = 0;
        base.Start();
        //distance = 2000;
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
        bowstringPositions = new Vector3[3];
        for (int i = 0; i < bowstringPositionPlaceHolders.Length; i++)
            bowstringPositions[i] = bowstringPositionPlaceHolders[i].transform.position;
        lr.SetPositions(bowstringPositions);
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
        bowstringPositions[0] = bowstringPositionPlaceHolders[0].transform.position;
        //bowstringPositions[1] = bowstringPositionPlaceHolders[1].transform.position;

        bowstringPositions[1] = bowstringPositionPlaceHolders[1].transform.position;
        if (!reloading && arrow != null)
            //    bowstringPositions[1] += (new Vector3(0, 0, 0));// drawRange)); // Default position of middle vertex
            //else
            //{
            bowstringPositions[1] = arrow.transform.Find("Tail").position;// (new Vector3(0,0,-chargePower*2.35f));//arrow.position.z - 2.35f)); // Position of arrow tail
        //}
        bowstringPositions[2] = bowstringPositionPlaceHolders[2].transform.position;
        lr.SetPositions(bowstringPositions);//positions.ToArray()); // Assign vertices positions
        if (reloading || arrow == null)
        {
            Reload();
            return true;
        }

        base.Update();

        if (charging)
        {
            Charge(chargePower);
        }
        /*
        if (arrow == null)
        {
            Reload();
            return true;
        }*/
        //List<Vector3> positions = new List<Vector3>(); // Keeps track of the vertices of the bowstring
        //positions.Add(new Vector3(0, .5f, -.475f)); // Vertex at top of bow
        
        //positions.Add(new Vector3(0, -.5f, -.475f)); // Vertex at bottom of bow

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
        anim.speed = 0;
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
        anim.speed = 1;
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
            if (arrow)
                arrow.localPosition = bulletSpawn.localPosition + new Vector3(0, 0, chargePower * drawLimit);
            return;
        }


        audioSrc.clip = soundClips[0];

        // If not using Touch Interactive mode, progressively increase the chargePower as long as user holds shoot button
        if (!GameManager.gm.interactiveTouch)
        {
            chargePower = chargePower + .03f / (float)DebugManager.dbm.fps * 60.0f; // increment charge power
            if (chargePower >= chargeLimit)
                chargePower = chargeLimit;
            chargeBar.transform.localScale = new Vector3(1, chargePower / chargeLimit, 1); // Visual indicator of charge percentage

            /*
            float chargeDif = chargePower - this.chargePower;
            print(chargeDif);
            if ((!audioSrc.isPlaying && chargeDif > .03f))// || audioSrc.time > .1f * (1 + percentile) * audioSrc.clip.length)// || audioSrc.time / audioSrc.clip.length > .55f)
            {
                audioSrc.volume = .4f;
                audioSrc.time = audioSrc.clip.length * (chargePower*.62f);// * percentile;
                audioSrc.Play();
            }
            */
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
            /*
            print(chargePower +" " + this.chargePower);
            int percentile = ((int)(chargePower * 100)) / 33;
            int oldPercentile = ((int)(this.chargePower * 100)) / 33;
            print(percentile + "," + oldPercentile);
            if (percentile != oldPercentile)
            {
                if (percentile > 1)
                    audioSrc.time = audioSrc.clip.length * (.6f);
                else if (percentile > 0)
                    audioSrc.time = audioSrc.clip.length * (.5f);
                else
                    audioSrc.time = audioSrc.clip.length * (.4f);
                audioSrc.Play();
            }
            float audioTimePercentage = audioSrc.time / audioSrc.clip.length;
            if (percentile < 2 && audioTimePercentage >= .6f)
                audioSrc.Stop();
            else if (percentile < 1 && audioTimePercentage >= .5f)
                audioSrc.Stop();
                */
        }

        drawRange = drawOffset + chargePower * drawLimit * drawElasticity; // Calculate how far arrow and bowstring is drawn
        arrow.localPosition = bulletSpawn.localPosition + new Vector3(0, 0, chargePower * drawLimit); // Move arrow backwards based on charge power 
        

        this.chargePower = chargePower;
        anim.Play("charge", -1, chargePower);
        
       // print(audioSrc.time);
        //print(audioSrc.clip.length);
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

            //for (int i = 0; i < bowstringPositionPlaceHolders.Length; i++)
            //    bowstringPositions[i] = bowstringPositionPlaceHolders[i].transform.position;
            //lr.SetPositions(bowstringPositions);
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("release"))
                anim.Play("release", -1, 1 - chargePower);
            base.Shoot(chargePower);
            audioSrc.clip = soundClips[1];
            audioSrc.time = .1f;
            audioSrc.volume = 1f;
            print(audioSrc.clip.length);
            audioSrc.Play();
        }
        else
        {
            if (audioSrc.isPlaying)
                audioSrc.Stop();

            anim.Play("default", -1, 0);
        }
    }
}
