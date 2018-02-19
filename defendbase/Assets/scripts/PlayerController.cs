using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {
    public int id;
    public bool gyroEnabled, useGyro;
    private Gyroscope gyro;
    private Vector2 initialTouchPosition;
    public float turnSpd = 1f;
    private GameObject cameraContainer;
    public Camera playerCam;
    private Quaternion rot;
    public float speed = 10f, prevXAngle, maxLookDownLimit;
    private int shootTouchID, aimTouchID;
    private Transform bulletSpawn;
    public GameObject bulletPrefab;
    public GameObject chargeBarGuage, chargeBar;
    private bool charging;
    private float chargePower, chargeAccelerator;
    private int chargeBarAlt = 1;
    public Weapon wep;
	// Use this for initialization
	void Start () {
        shootTouchID = -1;
        aimTouchID = -1;
        charging = false;
        bulletSpawn = playerCam.transform.Find("BulletSpawn");
        prevXAngle = 0;
        maxLookDownLimit = 60f;
        Screen.orientation = ScreenOrientation.Landscape;
        cameraContainer = transform.Find("Camera Container").gameObject;
        gyroEnabled = EnableGyro();
        //cameraContainer.transform.SetParent(transform);
        //cameraContainer.transform.localPosition = playerCam.transform.localPosition;
        playerCam.transform.SetParent(cameraContainer.transform);
        useGyro = true;
        //DisableGyro();
    }

    private void DisableGyro()
    {
        if (SystemInfo.supportsGyroscope)
        {
            gyro.enabled = false;
            //GameManager.gm.gyroEnabled = false;
            cameraContainer.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            //rot = new Quaternion(0, 0, .5f, 0);
            playerCam.transform.localEulerAngles = new Vector3(0, 0, 0);
            //return true;
        }
    }

    private bool EnableGyro()
    {
        if (SystemInfo.supportsGyroscope)
        {
            gyro = Input.gyro;
            gyro.enabled = true;
            //GameManager.gm.gyroEnabled = true;
            cameraContainer.transform.rotation = Quaternion.Euler(90f, 90f, 0f);
            rot = new Quaternion(0, 0, .5f, 0);

            return true;
        }
        return false;
    }

    public bool IsMyPlayer()
    {
        return gameObject == GameManager.gm.player;
    }
    // Update is called once per frame
    void Update () {
        //return;
        //GameObject.Find("PlayerUI").transform.Find("Text").GetComponent<Text>().text = "";
        if (!IsMyPlayer())
        {
            //Debug.Log("NOT palyer");
            return;
        }
        PlayerMobileInput();
        PlayerPCInput();
        
        
    }

    public void EquipWeapon(Weapon w)
    {
        if (wep)
        {
            Debug.Log("Unequipping weapon");
            wep.user = null;
        }
        
        wep = w;
        w.user = this;
        wep.transform.SetParent(transform.Find("Camera Container").Find("Player Camera"));
        wep.transform.position = transform.Find("Camera Container").Find("Player Camera").Find("WeaponPlaceholder").position;

    }

    private void PlayerMobileInput()
    {
        if (gyroEnabled && useGyro)
        {
            //Debug.Log("Rotate)");
            RotatePlayer();
            //transform.Translate(0, 0,
            //(-1 * Input.acceleration.z * speed * Time.deltaTime)); // adds movement on the Z axis alone.
            //GameObject.Find("txt").transform.GetChild(0).GetComponent<Text>().text = "" + transform.position + "\n" + Input.acceleration;
        }
        //if (wep == null)
        //    return;
        
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
            Debug.Log("Down");
            charging = true;
            chargePower = 0;
            chargeBarGuage.SetActive(true);
            chargeBar.transform.localScale = new Vector3(0, 0, 0);
            chargeBarAlt = 1;
            chargeAccelerator = 0;
        }
        /*
        if (charging)
        {
            Debug.Log("Charg" + chargePower + " " + chargeAccelerator  + " " );
            chargePower += .025f * chargeBarAlt;
            chargeAccelerator += .05f * chargeBarAlt;
            chargeBar.transform.localScale = new Vector3(1, chargePower, 1);
            
            //chargePower
            if(chargeBar.transform.localScale.y >= 1)
            {
                chargeBar.transform.localScale = new Vector3(
                    chargeBar.transform.localScale.x,
                    1,
                    chargeBar.transform.localScale.z);
                chargePower = 1;
                chargeAccelerator *= -1;
                chargeBarAlt = -1;
            } else if (chargeBar.transform.localScale.y < 0)
            {
                chargeBar.transform.localScale = new Vector3(
                    chargeBar.transform.localScale.x,
                    0,
                    chargeBar.transform.localScale.z);
                chargeAccelerator *= -1;
                chargePower = 0;
                chargeBarAlt = 1;
            }
        }*/
        if (Input.GetKeyUp(KeyCode.Space))
        {
            Debug.Log("Done");
            charging = false;
            chargeBarGuage.SetActive(false);

            GameObject bullet = Instantiate(bulletPrefab);

            bullet.transform.position = bulletSpawn.transform.position;
            bullet.transform.GetComponent<Rigidbody>().AddForce(transform.GetChild(0).forward * chargePower * 1000);
        }

    }

    private void RotatePlayer()
    {
        //Quaternion q = gyro.attitude;// * rot;
        //Vector3 angles = q.eulerAngles;
        //playerCam.transform.localEulerAngles = new Vector3(angles.x, 0,0);
        //transform.localEulerAngles = new Vector3(0, angles.y, 0);
        
        playerCam.transform.localRotation = gyro.attitude * rot;
        if(playerCam.transform.eulerAngles.x >= maxLookDownLimit && playerCam.transform.eulerAngles.x <= 90f)
        {
            playerCam.transform.eulerAngles = new Vector3(maxLookDownLimit, playerCam.transform.eulerAngles.y, playerCam.transform.eulerAngles.z);
        }
        //Debug.Log(playerCam.transform.eulerAngles + " " + playerCam.transform.localEulerAngles);
        //transform.localEulerAngles = new Vector3(0, playerCam.transform.eulerAngles.x, 0);
        //playerCam.transform.localEulerAngles = new Vector3(0,playerCam.transform.localEulerAngles.y, 0);
    }
}
