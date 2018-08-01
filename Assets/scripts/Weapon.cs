using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum UnlockCondition
{
    Free,
    Purchase,
    Quest,
    QuestThenPurchase
}

// Keeps track of weapon details per level
public class WeaponStats
{
    public string name,
                  description;
    //unlockCondition;
    public UnlockCondition unlockCondition;
    public int price = 0; // How much to initially buy from store
    public int[] dmg, // List of damage per level
                 costToUpgrade; // List of cost to upgrade to next level
    public float[] distance, // How far can shoot projectiles  (higher = farther)
                   timeToReload, // How long before shooting in percentage (lower = faster)
                   chargeAccelation; // How fast to charge (for bows, cannons, etc.) in percentage ([0.0,1.0] for 0%-100%)
    public WeaponStats(string nm,int price, UnlockCondition unlockCond, int[] dmgs,int[] costs, float[] distances, float[] timeToReloads, float[] chargeAccelerations, string description)
    {
        name = nm;
        this.price = price;
        unlockCondition = unlockCond;
        dmg = dmgs;
        costToUpgrade = costs;
        distance = distances;
        timeToReload = timeToReloads;
        chargeAccelation = chargeAccelerations;
        this.description = description;
    }
}

public abstract class Weapon : MonoBehaviour
{
    // List of all weapons and their stats
    public static WeaponStats[] weaponStats = new WeaponStats[] {
        // Bow
        new WeaponStats
        (
            "Bow",
            0,
            UnlockCondition.Free,
            new int[] {1,1,1,1,1},
            new int[] {50,100,200,500},
            new float[] {16, 17,19,23,29},
            new float[] {2f,1.75f,1.5f,1.25f,1f},
            new float[] {.75f,.70f,.65f,.55f,.5f},
            "Your standard wooden bow"
        ),
        // Dual-Shot Bow
        new WeaponStats
        (
            "Twin-Fang Bow",
            0,
            UnlockCondition.Quest,
            new int[] {1,1,1,1,1},
            new int[] {50,100,200,500},
            new float[] {16, 17,19,23,29},
            new float[] {2f,1.75f,1.5f,1.25f,1f},
            new float[] {.75f,.70f,.65f,.55f,.5f},
            "Bites the enemy like a viper. HISSSS "
        ),
        // Sniper Bow
        new WeaponStats
        (
            "Sniper Bow",
            0,
            UnlockCondition.Quest,
            new int[] {1,1,1,1,1},
            new int[] {50,100,200,500},
            new float[] {24, 30,36,43,50},
            new float[] {2.5f,2.3f,2.0f,1.7f,1.3f},
            new float[] {.75f,.70f,.65f,.55f,.5f},
            "Steady focused aim wins the game"
        ),
        // Machine Bow
        new WeaponStats
        (
            "Machine Bow",
            2500,
            UnlockCondition.QuestThenPurchase,//UnlockCondition.QuestThenPurchase,
            new int[] {1,1,1,1,1},
            new int[] {50,100,200,500},
            new float[] {16, 17,19,23,29},
            new float[] {.5f,.45f,.4f,.35f,.3f},
            new float[] {.5f,.55f,.6f,.65f,.7f},
            "Spraying arrows!? For the love of God..."
        )
    };
    public static bool[] enableWeps = { true, false, true, false };
    public static int MAX_DMG = 2;
    public static float MAX_BOW_STR = 50f,
                        
                        MIN_BOW_TTR = .5f; 
    public static int WeaponCount = 0; // Counts number of weapons created
    public GameObject[] bulletSpawns; // Spawn point(s) for bullets
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
                  shotCount, // counts number of times weapon shot projectile
                  wepID; // Distinguishes what weapon this is
    public PlayerController user; // Who is using this weapon
    protected int shootTouchID, // Finger ID that is used for shooting
                  aimTouchID; // Finger ID that is used for aiming camera perspective

    protected Animator anim; // controls animation

    protected AudioSource audioSrc; // controls audio
    public List<AudioClip> soundClips; // List of sounds used for weapon
    protected Image reloadBar; // Visual indicator for reload timer

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
        if (!user.IsMyPlayer()) {
            return;
        }
            //bulletSpawn[0] = transform.Find("BulletSpawn");
        Transform shootBtnUI = GameManager.gm.playerStatusCanvas.transform.Find("ShootBtnUI");
        chargeBarGuage = Instantiate(chargeBarGuage);
        chargeBar = chargeBarGuage.transform.GetChild(0).gameObject;
        // Display the non-Touch Interactive UIs if not using Touch Interaction and is my player
        bool canDisplay = !GameManager.gm.interactiveTouch && GameManager.gm.player == user.gameObject;
        //{
        chargeBarGuage.transform.SetParent(GameManager.gm.playerStatusCanvas.transform);
        chargeBarGuage.transform.localScale = new Vector3(1, 1, 1);//.875f, .5f, 1);
        //chargeBarGuage.transform.localEulerAngles = new Vector3(0, 0, -90);
        chargeBarGuage.transform.localPosition = GameManager.gm.playerStatusCanvas.transform.Find("ChargeBarGaugePlaceholder").localPosition;
        //}
        chargeBarGuage.SetActive(canDisplay);
        chargeBarGuage.SetActive(false);

        shootBtn = shootBtnUI.Find("ShootBtnMask").gameObject;//Instantiate(shootBtn);
        //shootBtn.transform.SetParent(GameManager.gm.playerStatusCanvas.transform.Find("ShootBtnPlaceholder"));
        //shootBtn.transform.localScale = new Vector3(1, 1, 1);
        //shootBtn.transform.localPosition = Vector3.zero; //GameManager.gm.playerStatusCanvas.transform.Find("ShootBtnPlaceholder").localPosition;
        
        shootBtn.SetActive(canDisplay);
        reloadBar = shootBtnUI.Find("ReloadGauge").Find("ReloadBar").GetComponent<Image>();//GameManager.gm.playerStatusCanvas.transform.Find("ReloadGauge").Find("ReloadBar").GetComponent<Image>();


        // Add item UI to store with respect to if item was purchased or not
        //itemUI = Instantiate(GameManager.gm.inGameWepItemUIPrefab);
        itemUI = GameManager.gm.inGameWepItemUI;
        //itemUI.transform.Find("ItemDescription").GetComponent<RectTransform>().sizeDelta = new Vector2(1200,150);
        //itemUI.transform.localPosition = new Vector3(-325, 0, 0);
        //itemUI.transform.Find("ItemDescription").transform.localPosition = new Vector3(-400, 0, 0);
        SetItemDescription();
        //itemUI.transform.Find("ItemDescription").GetComponent<Text>().text = statsByLevel[wepID].name + " Lvl " + lvl;
        itemUI.transform.Find("BuyBtn").GetComponent<Button>().onClick.AddListener(Purchase);
        itemUI.transform.Find("ItemImageBG").Find("ItemImage").GetComponent<RawImage>().texture = GameManager.gm.weaponItemIcons[wepID].texture;
       
        //itemUI.transform.Find("ItemDescription").GetComponent<Text>().text = statsByLevel[wepID].name + " Lvl." + lvl;
        if (purchased)
        {
            itemUI.transform.Find("BuyBtn").GetChild(0).GetComponent<Text>().text = "Upgrade\n" + weaponStats[wepID].costToUpgrade[lvl];// + "\nLvl " + (lvl + 2);
            if (user && user.gameObject == GameManager.gm.player)
            {
                //Debug.Log("?");
                //itemUI.transform.SetParent(GameManager.gm.quickAccessCanvas.transform.Find("WepItemUI Placeholder"));//GameManager.gm.quickAccessUpgradeDescription.transform);//GameManager.gm.quickAccessCanvas.transform.Find("DescriptionDisplay").Find("Inventory Descriptions").Find("Upgrade Descriptions"));
                //itemUI.transform.localPosition = Vector2.zero;//new Vector3(-325, 0, 0);
                //itemUI.transform.SetParent(itemUI.transform.parent.parent);

            }
        }
        else
        {
            itemUI.transform.Find("BuyBtn").GetChild(0).GetComponent<Text>().text = "Purchase\n" + weaponStats[wepID].price;
            if (user == null)
                itemUI.transform.SetParent(GameManager.gm.shopCanvas.transform.Find("Displays").Find("StoreWeaponsDisplay").GetChild(0));
        }
        //itemUI.transform.localScale = new Vector3(1, 1, 1);

        if(wepID == 1)
        {

        }
        itemUI.SetActive(false);
    }

    public void ToggleStatsUI()
    {
        GameManager.gm.NotifyButtonPressed();
        itemUI.SetActive(!itemUI.activeSelf);
    }
	
    public void SetItemDescription()
    {
        Transform itemStats = itemUI.transform.Find("ItemStats");
        string[] txts = {
            weaponStats[wepID].dmg[lvl] + "", // dmg
            weaponStats[wepID].chargeAccelation[lvl] + "", // charge acceleration
            weaponStats[wepID].timeToReload[lvl] + "", // time to reload
            weaponStats[wepID].distance[lvl] + "", // distance
            "Lv " + (lvl + 1) // lvl
        };
         //weaponStats[wepID].name + " Lvl " + lvl;// + "   DMG:" + statsByLevel[wepID].dmg[lvl] + "   CHRG:" + (1.0f / statsByLevel[wepID].chargeAccelation[lvl]).ToString("F2") + "   RLD:" + statsByLevel[wepID].timeToReload[lvl] + "s   BOWSTR:" + statsByLevel[wepID].distance[lvl];

        // if reached max level
        if (lvl == Weapon.weaponStats[wepID].costToUpgrade.Length)
        {
            txts[0] += " (-)";
            txts[1] += " (-)";
            txts[2] += " (-)";
            txts[3] += " (-)";
            txts[4] += " (MAX)";
        }
        else
        {
            txts[0] += " (" + weaponStats[wepID].dmg[lvl + 1] + ")";
            txts[1] += " (" + weaponStats[wepID].chargeAccelation[lvl + 1] + ")";
            txts[2] += " (" + weaponStats[wepID].timeToReload[lvl + 1] + ")";
            txts[3] += " (" + weaponStats[wepID].distance[lvl + 1] + ")";
            txts[4] += " (Next)";
        }

        itemStats.Find("Damage").Find("Text").GetComponent<Text>().text = txts[0];//weaponStats[wepID].dmg[lvl] + "";
        itemStats.Find("ChargeAcceleration").Find("Text").GetComponent<Text>().text = txts[1];//weaponStats[wepID].chargeAccelation[lvl] + "";
        itemStats.Find("Reload").Find("Text").GetComponent<Text>().text = txts[2];// weaponStats[wepID].timeToReload[lvl] + "";
        itemStats.Find("BowStr").Find("Text").GetComponent<Text>().text = txts[3];// weaponStats[wepID].distance[lvl] + "";
        itemUI.transform.Find("ItemDescription").GetComponent<Text>().text = txts[4];//"Lv " + (lvl + 1);
    }

    // Update is called once per frame
    protected virtual bool Update () {
        // Don't do anything if no user exists or game is over
        if (!user || GameManager.gm.data.gameOver)
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

    public virtual void SetAttribute(int attributeID)
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
        // If buying from store
        if (!purchased)
        {
            if(GameManager.gm.data.inGameCurrency >= weaponStats[wepID].price)
            {
                purchased = true;
                itemUI.transform.Find("BuyBtn").GetChild(0).GetComponent<Text>().text = "Upgrade\n" + weaponStats[wepID].costToUpgrade[lvl];// + "\nLvl " + (lvl + 2);
                itemUI.transform.SetParent(GameManager.gm.shopCanvas.transform.Find("Displays").Find("UpgradeWeaponsDisplay").GetChild(0));
                GameManager.gm.NotifyTransactionSuccess();
                Debug.Log("PURCHSE");
            }
            else
            {
                Debug.Log("CANT BUYT");
                GameManager.gm.NotifyInvalid();
                //StartCoroutine(GameManager.gm.PlaySFX(GameManager.gm.invalidSFX));
            }
        }
        // If upgrading weapon
        else
        {
            if(GameManager.gm.data.inGameCurrency >= weaponStats[wepID].costToUpgrade[lvl])
            {
                Debug.Log("CAN UPGRADE");
                //GameManager.gm.inGameCurrency -= ;
                lvl++;
                GameManager.gm.data.wepLvl++;
                // MAX level?
                if (lvl == weaponStats[wepID].costToUpgrade.Length)
                {
                    Debug.Log("NO MORE UPGRADES");
                    itemUI.transform.Find("BuyBtn").GetComponent<Button>().interactable = false;//.onClick.AddListener(Purchase);
                    itemUI.transform.Find("BuyBtn").GetChild(0).GetComponent<Text>().text = "Upgrade\nMAXED";
                    itemUI.transform.Find("BuyBtn").Find("Image").gameObject.SetActive(false);

                }
                else
                {
                    itemUI.transform.Find("BuyBtn").GetChild(0).GetComponent<Text>().text = "Upgrade\n" + weaponStats[wepID].costToUpgrade[lvl];// + "\nLvl " + (lvl + 2);
                }
                SetItemDescription();
                if (NetworkManager.nm.isStarted)
                {
                    NetworkManager.nm.NotifyWeaponUpgraded(wepID, lvl);
                }
                GameManager.gm.UpdateInGameCurrency(-weaponStats[wepID].costToUpgrade[lvl - 1]);
                GameManager.gm.NotifyTransactionSuccess();
                //itemUI.transform.Find("ItemDescription").GetComponent<Text>().text = statsByLevel[wepID].name + " Lvl." + lvl;
            }
            else
            {
                Debug.Log("CANTS UPGRADE");
                GameManager.gm.NotifyInvalid();
                //StartCoroutine(GameManager.gm.PlaySFX(GameManager.gm.invalidSFX));
            }
            
        }
    }
    
    public void SetLevel(int lvl)
    {
        this.lvl = lvl;
    }

    // Format:
    // WEP|Using weapon|charge power|wep ID|wep lvl|
    public virtual string NetworkInformation()
    {
        return "WEP|" + inUse + "|" + chargePower + "|" + wepID + "|" + lvl + "|";
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
        print(1123);
        inUse = true;
        charging = true;
        chargePower = 0;
        if (!GameManager.gm.interactiveTouch)
        {
            //chargeBar.transform.localScale = new Vector3(0, 0, 0);
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
            //chargeBar.transform.localScale = new Vector3(0, 0, 0);
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
        chargePower = chargePower + (.03f * weaponStats[wepID].chargeAccelation[0]) / (float)DebugManager.dbm.fps * 60.0f; // increment charge power
        if (chargePower >= chargeLimit)
            chargePower = chargeLimit;
        //chargeBar.transform.localScale = new Vector3(1, chargePower / chargeLimit, 1); // Visual indicator of charge percentage
        if(user.IsMyPlayer())
            chargeBar.GetComponent<Image>().fillAmount = (chargePower) / chargeLimit;

        this.chargePower = chargePower;
        /*
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
        */
    }
}
