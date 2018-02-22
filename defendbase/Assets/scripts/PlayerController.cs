using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {
    public float noiseTolerance = .05f;
    public int id; // Used to assign who controls this player online
    public bool gyroEnabled, // Is there gyroscope feature on this device? 
                useGyro; // Are we using gyroscope?
    private Gyroscope gyro; // Point to gyroscope object
    private GameObject cameraContainer; // Holds the camera that represents your view
    public Camera playerCam; // Your perspective in game
    private Quaternion rot; // Used for orienting your perspective
    public float prevXAngle, // Used for limiting how far you can look down
                 maxLookDownLimit; // Limit on how far your look down
    // PC purpose?
    private Transform bulletSpawn;
    public GameObject bulletPrefab;
    public GameObject chargeBarGuage, chargeBar;
    private bool charging;
    private float chargePower, chargeAccelerator;
    private int chargeBarAlt = 1;

    public Weapon wep; // Weapon player is currently holding and using

    Quaternion origin;
    
    // Use this for initialization
	void Start () {
        charging = false;
        bulletSpawn = playerCam.transform.Find("BulletSpawn");
        prevXAngle = 0;
        maxLookDownLimit = 60f;
        Screen.orientation = ScreenOrientation.Landscape;
        cameraContainer = transform.Find("Camera Container").gameObject;
        gyroEnabled = EnableGyro();
        playerCam.transform.SetParent(cameraContainer.transform);
        useGyro = true;


        origin = Input.gyro.attitude;
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

    // Check if this object is the player I control
    public bool IsMyPlayer()
    {
        return gameObject == GameManager.gm.player;
    }

    // Update is called once per frame
    void Update () {
        // Don't do anything if this player isn't in my control
        if (!IsMyPlayer())
        {
            return;
        }
        PlayerMobileInput();
        PlayerPCInput();
    }

    // Bind weapon w to this player
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

    // Handle Mobile inputs
    private void PlayerMobileInput()
    {
        // Using gyro if existent? Rotate player perspective
        if (gyroEnabled && useGyro)
        {
            RotatePlayer();
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
            charging = true;
            chargePower = 0;
            chargeBarGuage.SetActive(true);
            chargeBar.transform.localScale = new Vector3(0, 0, 0);
            chargeBarAlt = 1;
            chargeAccelerator = 0;
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            charging = false;
            chargeBarGuage.SetActive(false);

            GameObject bullet = Instantiate(bulletPrefab);

            bullet.transform.position = bulletSpawn.transform.position;
            bullet.transform.GetComponent<Rigidbody>().AddForce(transform.GetChild(0).forward * chargePower * 1000);
        }

    }

    // Orient player perspective
    private void RotatePlayer()
    {
        float dist = Vector3.Distance(gyro.rotationRateUnbiased, Vector3.zero);
        //playerCam.transform.Rotate(Input.gyro.rotationRate);
        //Debug.Log(gyro.attitude + " ...." + gyro.rotationRateUnbiased + "...a>>" +dist );
        //playerCam.transform.Rotate(-Input.gyro.rotationRateUnbiased.x, -Input.gyro.rotationRateUnbiased.y, Input.gyro.rotationRateUnbiased.z);
        //playerCam.transform.Rotate(-Input.gyro.rotationRateUnbiased.x, 0, 0);
        //  playerCam.transform.localRotation = playerCam.transform.rotation * rot;
        // Temporarily prevents camera from sliding undesireably due to gyroscope errors
        if (dist < noiseTolerance)
        {
            return;
        }
        //Debug.Log(dist);
        //Quaternion oldRotation = playerCam.transform.localRotation;
        playerCam.transform.localRotation = gyro.attitude * rot;

        if (playerCam.transform.eulerAngles.x >= maxLookDownLimit && playerCam.transform.eulerAngles.x <= 90f)
        {
            playerCam.transform.eulerAngles = new Vector3(maxLookDownLimit, playerCam.transform.eulerAngles.y, playerCam.transform.eulerAngles.z);
        }
    }
}
