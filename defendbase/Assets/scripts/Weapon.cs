using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour {
    protected Transform bulletSpawn;
    public GameObject bulletPrefab;
    public GameObject chargeBarGuage, chargeBar;
    public bool charging, reloading;
    public float chargePower, chargeAccelerator, distance = 1000;
    public float reloadTime, timeToReload;
    private int chargeBarAlt = 1;
    public PlayerController user;
    // Use this for initialization
    protected virtual void Start () {
        charging = false;
        reloadTime = 0;
        reloading = false;
        bulletSpawn = transform.Find("BulletSpawn");
    }
	
	// Update is called once per frame
	protected virtual void Update () {
        if (charging)
        {
            Debug.Log("charge");
            Charge();
        }
	}

    public void Use()
    {
        if (charging)
        {
            //Debug.Log("Charg" + chargePower + " " + chargeAccelerator + " ");
            
        }
    }

    public virtual bool StartUse(Touch t)
    {
        Debug.Log("?");
        charging = true;
        chargePower = 0;
        chargeBarGuage.SetActive(true);
        chargeBar.transform.localScale = new Vector3(0, 0, 0);
        chargeBarAlt = 1;
        chargeAccelerator = 0;
        return true;
    }

    public virtual void EndUse()
    {
        Debug.Log("FIRE");
        Shoot();
    }

    public virtual void Shoot()
    {
        charging = false;
        chargeBarGuage.SetActive(false);
        GameObject bullet = Instantiate(bulletPrefab);

        bullet.transform.position = bulletSpawn.transform.position;
        bullet.transform.GetComponent<Rigidbody>().AddForce(user.playerCam.transform.forward * chargePower * distance);
    }

    public virtual void Charge()
    {
        chargePower += .025f * chargeBarAlt;
        chargeAccelerator += .05f * chargeBarAlt;
        chargeBar.transform.localScale = new Vector3(1, chargePower, 1);

        //chargePower
        if (chargeBar.transform.localScale.y >= 1)
        {
            chargeBar.transform.localScale = new Vector3(
                chargeBar.transform.localScale.x,
                1,
                chargeBar.transform.localScale.z);
            chargePower = 1;
            chargeAccelerator *= -1;
            chargeBarAlt = -1;
        }
        else if (chargeBar.transform.localScale.y < 0)
        {
            chargeBar.transform.localScale = new Vector3(
                chargeBar.transform.localScale.x,
                0,
                chargeBar.transform.localScale.z);
            chargeAccelerator *= -1;
            chargePower = 0;
            chargeBarAlt = 1;
        }
    }
}
