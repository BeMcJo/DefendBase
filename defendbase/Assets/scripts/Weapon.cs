using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponStats
{
    public string name;
    public int price = 0;
    public int[] dmg,
          costToUpgrade;
    public float[] distance,
          timeToReload,
          chargeAccelation;
    public WeaponStats(string nm, int[] dmgs,int[] costs, float[] distances, float[] timeToReloads, float[] chargeAccelerations)
    {
        name = nm;
        dmg = dmgs;
        costToUpgrade = costs;
        distance = distances;
        timeToReload = timeToReloads;
        chargeAccelation = chargeAccelerations;
    }
}

public abstract class Weapon : MonoBehaviour
{
    public static WeaponStats[] statsByLevel = new WeaponStats[] {
        // Bow Level Stats
        new WeaponStats
        ( 
            "Bow",
            new int[] {1,2,3,4,5},
            new int[] {50,100,200,500},
            new float[] {1600, 1700,1800,2900,2000},
            new float[] {2f,1.75f,1.5f,1.25f,1f},
            new float[] {.75f,.70f,.65f,.55f,.5f}
        )
    };
    public static int WeaponCount = 0;
    protected Transform bulletSpawn;
    public GameObject bulletPrefab;
    public GameObject chargeBarGuage, 
                      chargeBar,
                      shootBtn,
                      itemUI;
    public bool charging,
                inUse,
                purchased,
                reloading;
    public float chargePower, chargeAccelerator, distance = 1000, chargeLimit;
    public float reloadTime, timeToReload;
    protected int chargeBarAlt = 1, 
                  lvl, 
                  id, 
                  wepID;
    public PlayerController user;
    protected int shootTouchID, aimTouchID;

    private Vector2 initialTouchPosition;
    // Use this for initialization
    protected virtual void Start () {
        charging = false;
        reloadTime = 0;
        shootTouchID = -1;
        reloading = false;
        id = WeaponCount;
        WeaponCount++;
        bulletSpawn = transform.Find("BulletSpawn");
        //itemUI = Instantiate(GameManager.gm.itemUIPrefab);
        chargeBarGuage = Instantiate(chargeBarGuage);
        chargeBar = chargeBarGuage.transform.GetChild(0).gameObject;
        shootBtn = Instantiate(shootBtn);
        if (!GameManager.gm.interactiveTouch && GameManager.gm.player == user.gameObject)
        {
            chargeBarGuage.transform.SetParent(GameManager.gm.playerStatusCanvas.transform);
            chargeBarGuage.transform.localPosition = GameManager.gm.playerStatusCanvas.transform.Find("ChargeBarGaugePlaceholder").localPosition;
            shootBtn.transform.SetParent(GameManager.gm.playerStatusCanvas.transform);
            shootBtn.transform.localPosition = GameManager.gm.playerStatusCanvas.transform.Find("ShootBtnPlaceholder").localPosition;
        }
        chargeBarGuage.SetActive(false);
        itemUI = Instantiate(itemUI);
        itemUI.transform.Find("ItemDescription").GetComponent<Text>().text = statsByLevel[wepID].name + " Lvl" + lvl;
        itemUI.transform.Find("BuyBtn").GetComponent<Button>().onClick.AddListener(Purchase);
        itemUI.transform.Find("ItemDescription").GetComponent<Text>().text = statsByLevel[wepID].name + " Lvl." + lvl;
        if (purchased)
        {
            itemUI.transform.Find("BuyBtn").GetChild(0).GetComponent<Text>().text = "Upgrade\n" + statsByLevel[wepID].costToUpgrade[lvl] + "\nLvl " + (lvl + 1);
            if(user && user.gameObject == GameManager.gm.player)
                itemUI.transform.SetParent(GameManager.gm.shopCanvas.transform.Find("Displays").Find("UpgradeWeaponsDisplay").GetChild(0));
        }
        else
        {
            itemUI.transform.Find("BuyBtn").GetChild(0).GetComponent<Text>().text = "Purchase\n" + statsByLevel[wepID].price;
            if(user == null)
                itemUI.transform.SetParent(GameManager.gm.shopCanvas.transform.Find("Displays").Find("StoreWeaponsDisplay").GetChild(0));
        }
    }
	
	// Update is called once per frame
	protected virtual bool Update () {
        if (!user)
            return false;
        if(NetworkManager.nm.isStarted && NetworkManager.nm.isDisconnected)
        {
            CancelUse();
            return false;
        }

        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (shootTouchID == -1 && Input.GetTouch(i).position.x > Screen.width / 2)
                {
                    // touch on screen
                    if (Input.GetTouch(i).phase == TouchPhase.Began)
                    {
                        if (StartUse(Input.GetTouch(i)))
                        {
                            shootTouchID = Input.GetTouch(i).fingerId;
                        }
                    }
                }
                else if (Input.GetTouch(i).fingerId == shootTouchID)
                {
                    // release touch/dragging
                    if (Input.GetTouch(i).phase == TouchPhase.Ended || Input.GetTouch(i).phase == TouchPhase.Canceled)
                    {
                        EndUse();
                        shootTouchID = -1;
                    }
                }
            }
        }
        return true;
    }

    public virtual void CancelUse()
    {
        chargePower = 0;
        EndUse();
    }

    public void Purchase()
    {
        if (!purchased)
        {
            if(GameManager.gm.inGameCurrency >= statsByLevel[wepID].price)
            {
                purchased = true;
                itemUI.transform.Find("BuyBtn").GetChild(0).GetComponent<Text>().text = "Upgrade\n" + statsByLevel[wepID].costToUpgrade[lvl] + "\nLvl " + (lvl + 1);
                itemUI.transform.SetParent(GameManager.gm.shopCanvas.transform.Find("Displays").Find("UpgradeWeaponsDisplay").GetChild(0));
                Debug.Log("PURCHSE");
            }
            else
            {
                Debug.Log("CANT BUYT");
            }
        }
        else
        {
            if(GameManager.gm.inGameCurrency >= statsByLevel[wepID].costToUpgrade[lvl])
            {
                Debug.Log("CAN UPGRADE");
                GameManager.gm.UpdateInGameCurrency(-statsByLevel[wepID].costToUpgrade[lvl]);
                //GameManager.gm.inGameCurrency -= ;
                lvl++;
                if (lvl == statsByLevel[wepID].costToUpgrade.Length)
                {
                    Debug.Log("NO MORE UPGRADES");
                    itemUI.transform.Find("BuyBtn").GetComponent<Button>().interactable = false;//.onClick.AddListener(Purchase);
                    itemUI.transform.Find("BuyBtn").GetChild(0).GetComponent<Text>().text = "Upgrade\nMAXED";

                }
                else
                {
                    itemUI.transform.Find("BuyBtn").GetChild(0).GetComponent<Text>().text = "Upgrade\n" + statsByLevel[wepID].costToUpgrade[lvl] + "\nLvl " + (lvl + 1);
                }
                itemUI.transform.Find("ItemDescription").GetComponent<Text>().text = statsByLevel[wepID].name + " Lvl." + lvl;
            }
            else
            {
                Debug.Log("CANTS UPGRADE");
            }
        }
    }

    public void SetLevel(int lvl)
    {
        this.lvl = lvl;
    }

    public virtual string NetworkInformation()
    {
        return "WEP|" + inUse + "|" + chargePower + "|";
    }
    
    public virtual void SetNetworkInformation(string[] data)
    {
        
        bool inUse = bool.Parse(data[1]);
        //bool charging = bool.Parse(data[2]);
        float chargePower = float.Parse(data[2]);
        //this.chargePower = chargePower;
        /*if (inUse)
        {
            if (this.inUse)
            {
                this.chargePower = chargePower;
            }
            else
            {
                start
            }
        }*/
    }

    public virtual bool StartUse()
    {
        inUse = true;
        charging = true;
        chargePower = 0;
        if (!GameManager.gm.interactiveTouch)
        {
            chargeBar.transform.localScale = new Vector3(0, 0, 0);
            chargeBarAlt = 1;
            chargeAccelerator = 0;
        }
        return true;
    }

    public virtual bool StartUse(Touch t)
    {
        inUse = true;
        charging = true;
        chargePower = 0;

        if (!GameManager.gm.interactiveTouch)
        {
            chargeBarGuage.SetActive(true);
            chargeBar.transform.localScale = new Vector3(0, 0, 0);
            chargeBarAlt = 1;
            chargeAccelerator = 0;
        }
        //chargeBarGuage.SetActive(true);
        //chargeBar.transform.localScale = new Vector3(0, 0, 0);
        //chargeBarAlt = 1;
        //chargeAccelerator = 0;
        return true;
    }

    public virtual void EndUse()
    {
        //Debug.Log("FIRE");
        inUse = false;
        chargeBarGuage.SetActive(false);
        //Shoot(chargePower);
    }

    public virtual void Shoot(float chargePower)
    {
        charging = false;
        //chargeBarGuage.SetActive(false);
        GameObject bullet = Instantiate(bulletPrefab);

        bullet.transform.position = bulletSpawn.transform.position;
        bullet.transform.GetComponent<Rigidbody>().AddForce(user.playerCam.transform.forward * chargePower * distance);
    }

    public virtual void Charge(float chargePower)
    {
        chargePower += .025f * chargeBarAlt;
        chargeAccelerator += .05f * chargeBarAlt;
        chargeBar.transform.localScale = new Vector3(1, chargePower, 1);
        this.chargePower = chargePower;
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
