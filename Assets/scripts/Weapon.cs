using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Keeps track of weapon details per level
public class WeaponStats
{
    public string name;
    public int price = 0; // How much to initially buy from store
    public int[] dmg, // List of damage per level
                 costToUpgrade; // List of cost to upgrade to next level
    public float[] distance, // How far can shoot projectiles 
                   timeToReload, // How long before shooting in percentage
                   chargeAccelation; // How fast to charge (for bows, cannons, etc.) in percentage
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
    // List of all weapons and their stats
    public static WeaponStats[] statsByLevel = new WeaponStats[] {
        // Bow Level Stats
        new WeaponStats
        ( 
            "Bow",
            new int[] {1,1,1,2,2},
            new int[] {50,100,200,500},
            new float[] {16, 17,19,23,29},
            new float[] {2f,1.75f,1.5f,1.25f,1f},
            new float[] {.75f,.70f,.65f,.55f,.5f}
        )
    };

    public static int WeaponCount = 0; // Counts number of weapons created
    protected Transform bulletSpawn; // Spawn point for bullets
    public GameObject bulletPrefab; // Projectile to spawn
    public GameObject chargeBarGuage, // Visual indicator of max charge
                      chargeBar, // Visual indicator of charge percentage
                      shootBtn, // What to touch when player wants to shoot
                      itemUI; // Displays item detail in store
    public bool charging, // Am I charging up shot?
                inUse, // Used for online purpose to denote other players using their weapon
                purchased, // Did I already buy weapon?
                reloading; // Am I reloading?
    public float chargePower, // How much I have been charging (More = stronger or further shots)
                 chargeAccelerator, // How fast my shot will charge up
                 distance = 1000,  // How far my shot will go
                 chargeLimit; // How much my charge can go up to
    public float reloadTime, // How long before bullet spawns 
                 timeToReload; // Default time for reloading
    public int chargeBarAlt = 1, // Unused. Originally used to have charge increase and decrease if charging
                  lvl, // Determines stats of weapon
                  id, // Unique ID for this object
                  wepID; // Distinguishes what weapon this is
    public PlayerController user; // Who is using this weapon
    protected int shootTouchID, // Finger ID that is used for shooting
                  aimTouchID; // Finger ID that is used for aiming camera perspective

    protected Animator anim; // controls animation

    protected AudioSource audioSrc; // controls audio
    public List<AudioClip> soundClips; // List of sounds used for weapon

    // Use this for initialization
    protected virtual void Awake()
    {
        id = WeaponCount;
        WeaponCount++;
        name = "Wep " + id;
    }

    //private Vector2 initialTouchPosition; // Used for determining 
    // Use this for initialization
    protected virtual void Start() {
        charging = false;
        reloadTime = 0;
        shootTouchID = -1;
        reloading = false;
        inUse = false;

        anim = GetComponent<Animator>();
        audioSrc = GetComponent<AudioSource>();
        /*
        id = WeaponCount;
        WeaponCount++;
        name = "Wep " + id;
        */
        bulletSpawn = transform.Find("BulletSpawn");
        chargeBarGuage = Instantiate(chargeBarGuage);
        chargeBar = chargeBarGuage.transform.GetChild(0).gameObject;
        shootBtn = Instantiate(shootBtn);
        // Display the non-Touch Interactive UIs if not using Touch Interaction and is my player
        bool canDisplay = !GameManager.gm.interactiveTouch && GameManager.gm.player == user.gameObject;
        //{
        chargeBarGuage.transform.SetParent(GameManager.gm.playerStatusCanvas.transform);
        chargeBarGuage.transform.localScale = new Vector3(.875f, .5f, 1);
        chargeBarGuage.transform.localEulerAngles = new Vector3(0, 0, -90);
        chargeBarGuage.transform.localPosition = GameManager.gm.playerStatusCanvas.transform.Find("ChargeBarGaugePlaceholder").localPosition;
        shootBtn.transform.SetParent(GameManager.gm.playerStatusCanvas.transform.Find("ShootBtnPlaceholder"));
        shootBtn.transform.localScale = new Vector3(1, 1, 1);
        shootBtn.transform.localPosition = Vector3.zero; //GameManager.gm.playerStatusCanvas.transform.Find("ShootBtnPlaceholder").localPosition;
        //}
        chargeBarGuage.SetActive(canDisplay);
        shootBtn.SetActive(canDisplay);

        chargeBarGuage.SetActive(false);

        // Add item UI to store with respect to if item was purchased or not
        itemUI = Instantiate(itemUI);
        //itemUI.transform.Find("ItemDescription").GetComponent<RectTransform>().sizeDelta = new Vector2(1200,150);
        //itemUI.transform.localPosition = new Vector3(-325, 0, 0);
        itemUI.transform.Find("ItemDescription").transform.localPosition = new Vector3(-400, 0, 0);
        SetItemDescription();
        //itemUI.transform.Find("ItemDescription").GetComponent<Text>().text = statsByLevel[wepID].name + " Lvl " + lvl;
        itemUI.transform.Find("BuyBtn").GetComponent<Button>().onClick.AddListener(Purchase);
        //itemUI.transform.Find("ItemDescription").GetComponent<Text>().text = statsByLevel[wepID].name + " Lvl." + lvl;
        if (purchased)
        {
            itemUI.transform.Find("BuyBtn").GetChild(0).GetComponent<Text>().text = "Upgrade\n" + statsByLevel[wepID].costToUpgrade[lvl] + "\nLvl " + (lvl + 1);
            if (user && user.gameObject == GameManager.gm.player)
            {
                //Debug.Log("?");
                itemUI.transform.SetParent(GameManager.gm.quickAccessUpgradeDescription.transform);//GameManager.gm.quickAccessCanvas.transform.Find("DescriptionDisplay").Find("Inventory Descriptions").Find("Upgrade Descriptions"));
                itemUI.transform.localPosition = new Vector3(-325, 0, 0);
            }
        }
        else
        {
            itemUI.transform.Find("BuyBtn").GetChild(0).GetComponent<Text>().text = "Purchase\n" + statsByLevel[wepID].price;
            if (user == null)
                itemUI.transform.SetParent(GameManager.gm.shopCanvas.transform.Find("Displays").Find("StoreWeaponsDisplay").GetChild(0));
        }
        itemUI.transform.localScale = new Vector3(1, 1, 1);
    }
	
    public void SetItemDescription()
    {
        Transform itemStats = itemUI.transform.Find("ItemStats");
        itemStats.Find("Damage").Find("Text").GetComponent<Text>().text = statsByLevel[wepID].dmg[lvl] + "";
        itemStats.Find("ChargeAcceleration").Find("Text").GetComponent<Text>().text = statsByLevel[wepID].chargeAccelation[lvl] + "";
        itemStats.Find("Reload").Find("Text").GetComponent<Text>().text = statsByLevel[wepID].timeToReload[lvl] + "";
        itemStats.Find("BowStr").Find("Text").GetComponent<Text>().text = statsByLevel[wepID].distance[lvl] + "";
        itemUI.transform.Find("ItemDescription").GetComponent<Text>().text = statsByLevel[wepID].name + " Lvl " + lvl;// + "   DMG:" + statsByLevel[wepID].dmg[lvl] + "   CHRG:" + (1.0f / statsByLevel[wepID].chargeAccelation[lvl]).ToString("F2") + "   RLD:" + statsByLevel[wepID].timeToReload[lvl] + "s   BOWSTR:" + statsByLevel[wepID].distance[lvl];
    }

    // Update is called once per frame
    protected virtual bool Update () {
        // Don't do anything if no user exists or game is over
        if (!user || GameManager.gm.gameOver)
            return false;
        // If using online feature and disconnected, cancel using weapon, to prevent further complications
        // OR if can't perform action and was in process of charging
        if(NetworkManager.nm.isStarted && NetworkManager.nm.isDisconnected || !user.canPerformActions)
        {
            CancelUse();
            return false;
        }
        if (!user.IsMyPlayer())
            return false;
        //if (!GameManager.gm.startWaves)
        //{
        //    CancelUse();
        //    return false;
        //}

        // Check to see if any finger touch ID is valid for shooting
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

    public void SetTouchInteraction(bool isInteractive)
    {
        //chargeBar.SetActive(!isInteractive);
        print(isInteractive);
        shootBtn.SetActive(!isInteractive && !GameManager.gm.edittingMap);
    }

    public virtual void ChangeAttribute()
    {
    }

    // Stop using weapon, prevent charge and shooting
    public virtual void CancelUse()
    {
        chargePower = 0;
        EndUse();
    }

    // Handle either buying weapon or upgrading it
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
                SetItemDescription();
                //itemUI.transform.Find("ItemDescription").GetComponent<Text>().text = statsByLevel[wepID].name + " Lvl." + lvl;
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

    // Format:
    // WEP|Using weapon|charge power|
    public virtual string NetworkInformation()
    {
        return "WEP|" + inUse + "|" + chargePower + "|";
    }
    
    // Extract network information
    public virtual void SetNetworkInformation(string[] data)
    {
        bool inUse = bool.Parse(data[1]);
        float chargePower = float.Parse(data[2]);
    }

    // Handles the beginning of weapon usage for online feature
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
        //print(1);
        return true;
    }

    // Handles the beginning of weapon usage
    public virtual bool StartUse(Touch t)
    {
        //print(2);
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
        if (NetworkManager.nm.isStarted && user.IsMyPlayer())
            NetworkManager.nm.SendPlayerInformation();
        return true;
    }

    // Handles the end of weapon usage
    public virtual void EndUse()
    {
        inUse = false;
        chargePower = 0;
        charging = false;
        chargeBarGuage.SetActive(false);
    }

    // Handles what happens when weapon shoots at a certain charge power
    public virtual void Shoot(float chargePower)
    {
        if (NetworkManager.nm.isStarted && user.IsMyPlayer())
        {
            NetworkManager.nm.SendPlayerInformation();
            return;
        }
        /*
        charging = false;
        GameObject bullet = Instantiate(bulletPrefab);
        bullet.transform.position = bulletSpawn.transform.position;
        bullet.transform.GetComponent<Rigidbody>().AddForce(user.playerCam.transform.forward * chargePower * distance);
        */
    }

    // Handle charging weapon
    public virtual void Charge(float chargePower)
    {
        chargePower += .025f * chargeBarAlt;
        chargeAccelerator += .05f * chargeBarAlt;
        chargeBar.transform.localScale = new Vector3(1, chargePower, 1);
        this.chargePower = chargePower;
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
