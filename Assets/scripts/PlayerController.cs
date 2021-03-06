﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {
    public float driftTolerance = .05f;
    public int id; // Used to assign who controls this player online
    public int selectedAttribute, // current arrow selected
               criticalShotCount, // count of critical shots landed
               shotsHit; // count of shots that hit target(s)       

    public bool gyroEnabled, // Is there gyroscope feature on this device? 
                useGyro, // Are we using gyroscope?
                canPerformActions; // Am I stunned, frozen, or restricted?

    private Gyroscope gyro; // Point to gyroscope object
    private GameObject cameraContainer; // Holds the camera that represents your view
    public Camera playerCam; // Your perspective in game
    private Quaternion rot,startRot; // Used for orienting your perspective
    public float prevXAngle, // Used for limiting how far you can look down
                 maxLookDownLimit; // Limit on how far your look down

    public Weapon wep; // Weapon player is currently holding and using
    Vector3 rotation;
    int touchID=-1;
    Vector2 touchPos;

    //Quaternion origin; unused?

    public Dictionary<int,Dictionary<int,Buff>> buffs; // Categorize buffs by buff types, then sorted by buff ID
    public Dictionary<int, float> immunityTimers; // immune to certain buffs by ID after they expire 

    // Use this for initialization
	void Start () {
        prevXAngle = 0;
        maxLookDownLimit = 60f;
        cameraContainer = transform.Find("Camera Container").gameObject;
        gyroEnabled = EnableGyro();
        playerCam.transform.SetParent(cameraContainer.transform);
        SetOrientation(GameManager.gm.playerOrientation);
        useGyro = gyroEnabled;
        buffs = new Dictionary<int, Dictionary<int, Buff>>();
        immunityTimers = new Dictionary<int, float>();
        canPerformActions = true;
        rotation = Vector3.zero;
        
        //transform.rot
        //origin = Input.gyro.attitude;
    }
    
    public bool ToggleGyro()
    {
        useGyro = !useGyro;
        if (useGyro)
        {
            cameraContainer.transform.rotation = Quaternion.Euler(90f, 90f, 0f);
        }
        else
        {
            cameraContainer.transform.eulerAngles = Vector3.zero;
        }
            playerCam.transform.eulerAngles = Vector3.zero;
        return useGyro;
    }

    // Disable using gyro if existent
    private void DisableGyro()
    {
        if (SystemInfo.supportsGyroscope)
        {
            gyro.enabled = false;
            cameraContainer.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            playerCam.transform.localEulerAngles = new Vector3(0, 0, 0);
        }
    }
    // Enable gyro capability if existent
    private bool EnableGyro()
    {
        if (SystemInfo.supportsGyroscope)
        {
            gyro = Input.gyro;
            gyro.enabled = true;
            cameraContainer.transform.rotation = Quaternion.Euler(90f, 90f, 0f);
            rot = new Quaternion(0, 0, .5f, 0);
            return true;
        }
        return false;
    }

    public bool ToggleCameraZoom()
    {
        return playerCam.GetComponent<MapViewCamera>().ToggleZoom();
    }

    // Check if this object is the player I control
    public bool IsMyPlayer()
    {
        return gameObject == GameManager.gm.player;
    }

    // Sets arrow attribute to aID
    public void SetAttribute(int aID)
    {
        selectedAttribute = aID;
        wep.SetAttribute(aID);
    }

    // Update is called once per frame
    void Update () {
        // Don't do anything if this player isn't in my control
        if (!IsMyPlayer() || !canPerformActions)
        {
            return;
        }
        PlayerMobileInput();
        PlayerPCInput();
        
        /* TAP REWARD BUBBLE TO OBTAIN
        if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray raycast = playerCam.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit raycastHit;
            if(Physics.Raycast(raycast, out raycastHit))
            {
                if (raycastHit.collider.tag == "Reward")
                    raycastHit.collider.GetComponent<Floater>().OnHit();
            }
        }
        */
    }

    // Calculates the status of player based on the buffType that changed
    public void EvaluateBuffs(int buffType)
    {
        switch (buffType)
        {
            // can't perform actions
            case 1:
                CheckIfCanPerformActions();
                break;
        }
    }

    // Removes buff and re-evaluates player status
    public void RemoveBuff(Buff b)
    {
        if (b == null)
        {
            //print("INVALID BUFF... NULL");
            return;
        }
        int buffType = b.buffType, buffID = b.buffID;

        // Remove buff if player contains it
        if (buffs[buffType].ContainsKey(buffID))
        {
            //print("REMOVING BUFF" + buffType + "," + buffID);
            Destroy(buffs[buffType][buffID].gameObject);
            buffs[buffType].Remove(buffID);

            // if buff type is a disabler, grant player 3 second immunity
            if (buffID > 0 && buffType == 1)
            {
                /*
                if (!immunityTimers.ContainsKey(buffID))
                {
                    immunityTimers.Add(buffID, 3f + Time.time);
                }
                immunityTimers[buffID] = 3f + Time.time;
                */
                AddBuff(-buffID, -buffType);
                //Buff immunity = Instantiate(GameManager.gm.buffs[0]).GetComponent<Buff>();
                //buffs[buffType].Add(-buffID, immunity);
            }
        }
        else
        {
            //print("BUFF DNE" + buffType + "," + buffID);
        }
        EvaluateBuffs(buffType);
    }

    // Adds buff and re-evaluates player status
    public void AddBuff(int buffID, int buffType)
    {
        //int buffType = b.buffType, buffID = b.buffID;

        if (buffType > 0 && buffID > 0 && buffs.ContainsKey(-buffType) && buffs[-buffType].ContainsKey(-buffID))
        {
            //print("IMMUNE TO THIS BUFF");
            return;
        }

        if (!buffs.ContainsKey(buffType))
        {
            //print("instantiating buff type list " + buffType);
            buffs.Add(buffType, new Dictionary<int, Buff>());
        }

        if (buffs[buffType].ContainsKey(buffID))
        {
            //print("Restarting BUFF" + buffType + "," + buffID);
            buffs[buffType][buffID].ReActivate();
            //buffs[buffType].Remove(buffID);
        }
        else
        {
            //print("CREATING BUFF " + buffID + "," + buffType);
            Buff b;
            // if negative ID, instantiate an immunity buff
            if(buffID < 0)
                b = Instantiate(GameManager.gm.buffs[0]).GetComponent<Buff>();
            else
                b = Instantiate(GameManager.gm.buffs[buffID]).GetComponent<Buff>();
            b.buffID = buffID;
            b.buffType = buffType;
            buffs[buffType].Add(buffID, b);
            b.player = this;
        }
        EvaluateBuffs(buffType);
    }

    public void CheckIfCanPerformActions()
    {
        bool canPerformActions = buffs[1].Count == 0;
        if(!canPerformActions && this.canPerformActions)
        {
            wep.CancelUse();
        }
        this.canPerformActions = canPerformActions;
    }

    public Vector3 SetOrientation(Vector3 orientation)
    {
        //print("set orientatiation to" + orientation);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y - orientation.y, 0);
        return -transform.eulerAngles;
        //if (playerCam)
        //    playerCam.transform.parent.eulerAngles = new Vector3(90, -orientation.y, -90);
        //print(playerCam.transform.parent.eulerAngles);
    }

    // Bind weapon w to this player
    public void EquipWeapon(Weapon w)
    {
        if (wep)
        {
            //Debug.Log("Unequipping weapon");
            wep.user = null;
        }
        
        wep = w;
        w.user = this;
        wep.transform.SetParent(transform.Find("Camera Container").Find("Player Camera"));
        wep.transform.position = transform.Find("Camera Container").Find("Player Camera").Find("WeaponPlaceholder").position;
        // Allow zoom feature if using sniper bow
        playerCam.GetComponent<MapViewCamera>().canZoom = wep.wepID == 2;
    }

    // Sets Player Camera orientation based on joystick information
    public void SetOrientation(Vector2 orientation)
    {
        Vector3 prevAngle = playerCam.transform.eulerAngles;
        float sensitivity = GameManager.gm.moveSensitivity * 60f / DebugManager.dbm.fps;
        playerCam.transform.eulerAngles += new Vector3(-orientation.y, 0, 0) * sensitivity;// * .01f;
        playerCam.transform.parent.eulerAngles +=  new Vector3(0, orientation.x , 0) * sensitivity;// * .01f;

        playerCam.transform.localEulerAngles = new Vector3(playerCam.transform.localEulerAngles.x, 0, 0);
        //playerCam.transform.rotation = Quaternion.Euler(playerCam.transform.rotation.x, playerCam.transform.rotation.y, 0);// = new Vector3(playerCam.transform.eulerAngles.x, playerCam.transform.eulerAngles.y, 0);
        //playerCam.transform.rotation = Quaternion.Euler(playerCam.transform.eulerAngles.x, playerCam.transform.eulerAngles.y, 0);
    }

    // Handle Mobile inputs
    private void PlayerMobileInput()
    {
        // Using gyro if existent? Rotate player perspective
        if (gyroEnabled && useGyro)
        {
            RotatePlayer();
        }
        else
        {
        }
    }

    private void PlayerPCInput()
    {
        if (Input.GetKey(KeyCode.A))
        {
            transform.eulerAngles += new Vector3(0, -1, 0);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.eulerAngles -= new Vector3(0, -1, 0);
        }
        if (Input.GetKey(KeyCode.W))
        {
            transform.GetChild(0).localEulerAngles += new Vector3(-1, 0, 0);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.GetChild(0).localEulerAngles -= new Vector3(-1, 0, 0);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
        }

    }
    private Quaternion localRotation; // 
    public float speed = 1.0f; // ajustable speed from Inspector in Unity editor
    // Orient player perspective
    private void RotatePlayer()
    {
        //attempt to not use gyro
        /*
        
        float zVal = Input.acceleration.z;
        if (zVal > 1f)
            zVal = 1;
        else if (zVal < -1f)
            zVal = -1;
        //playerCam.transform.eulerAngles = Vector3.Lerp(Vector3.zero, new Vector3(Mathf.Rad2Deg * -Mathf.Asin(zVal), 0, 0), Time.deltaTime);
        playerCam.transform.localEulerAngles =  new Vector3(Mathf.Rad2Deg * -Mathf.Asin(zVal), 0, 0);
        return;
        */

        //playerCam.transform.Rotate(Input.gyro.rotationRate);
        //Debug.Log(gyro.attitude + " ...." + gyro.rotationRateUnbiased + "...a>>" +dist );
        //playerCam.transform.Rotate(-Input.gyro.rotationRateUnbiased.x, -Input.gyro.rotationRateUnbiased.y, Input.gyro.rotationRateUnbiased.z);
        //playerCam.transform.Rotate(-Input.gyro.rotationRateUnbiased.x, 0, 0);
        //  playerCam.transform.localRotation = playerCam.transform.rotation * rot;
        // Temporarily prevents camera from sliding undesireably due to gyroscope errors
        float dist = Vector3.Distance(gyro.rotationRateUnbiased, Vector3.zero);
        if (dist < driftTolerance)
        {
            return;
        }
        //Debug.Log(dist);
        //Quaternion oldRotation = playerCam.transform.localRotation;
        playerCam.transform.localRotation = gyro.attitude * rot;

        if (playerCam.transform.eulerAngles.x >= maxLookDownLimit && playerCam.transform.eulerAngles.x <= 90f)
        {
            //playerCam.transform.eulerAngles = new Vector3(maxLookDownLimit, playerCam.transform.eulerAngles.y, playerCam.transform.eulerAngles.z);
        }
    }
}
