﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Advertisements;
using UnityEngine.UI;

[Serializable]
// Used to save data that changes quickly (EX: arrow qty)
public class DynamicData
{
    public int[] arr; // array of integers
    public DynamicData(int len)
    {
        arr = new int[len];
    }
}

[Serializable]
// Used to save in game session
public class PlayerData
{
    public bool savedGame, // Determines if game is being loaded or saved
                hasWon, // game ended by winning, else lost
                gameOver; // has game session ended
    public int wave,
               inGameCurrency,
               objectiveHP,
               score,
               difficulty,
               wepLvl,
               wepID,
               totalKills;
    public int[] arrowQuantities;

    // Cumulative Records
    public int cumulativeDamageObjectiveTook;
    public int[] killsByEnemy,
                 killsByArrowAttribute,
                 weakSpotsHitByEnemy,
                 arrowsShotByAttribute;

    public PlayerData()
    {
        savedGame = false;
        hasWon = false;
        gameOver = false;
        wave = 0;
        inGameCurrency = 0;
        difficulty = 0;
        objectiveHP = 0;
        score = 0;
        totalKills = 0;
        wepID = 0;
        wepLvl = 0;
        arrowQuantities = new int[Projectile.projectileStats.Length];
        
        weakSpotsHitByEnemy = new int[Enemy.enemyStats.Length];
        killsByEnemy = new int[Enemy.enemyStats.Length];
        killsByArrowAttribute = new int[Projectile.projectileStats.Length];
        arrowsShotByAttribute = new int[Projectile.projectileStats.Length];
    }

}

[Serializable]
// Saves data outside game session
public class PersonalData
{
    public int playerCurrency,
               equippedWep;
    public int[] arrowQuantities;
    public bool[] isWeaponUnlocked, // If there is a condition to obtain, did we satisfy it to obtain?
                  isWeaponPurchased, // Is weapon available to purchase and obtain?
                  isArrowUnlocked; // Is arrow available to purchase?

    // Best Score Records
    public int bestScore, // highest score obtained in a game session
               mostCurrencySavedInGame, // most currency saved in a session
               highestWaveSurvived, // highest wave survived in session
               mostWeakSpotsHitInAGame, // most weak spots hit in session
               mostKillsInGame; // most kills obtained in session

    // Cumulative Records
    public int cumulativeDamageObjectiveTook, // how much damage objective sustained
               totalDefeats,totalVictories; // cumulative wins and losses
    public int[] killsByEnemy, // how many of each enemy I killed
                 killsByArrowAttribute, // how many of each enemy killed based on arrow type
                 arrowsShotByAttribute, // how many of each arrow shot by type
                 weakSpotsHitByEnemy, // how many weak spots hit by enemy
                 weaponsUsedByGame, // how often i used a certain bow in session
                 enemiesKilledByWeapon, // how many kills i got using a certain bow
                 weakSpotsHitByWeapon, // how many weak spots using certain bow
                 upgradedLevelsByWeapon; // permanent level upgraded for each bow

    // Achievement Unlocks
    public bool[] isAchievementUnlocked;

    public PersonalData()
    {
        playerCurrency = 0;
        equippedWep = 0;
        isWeaponUnlocked = new bool[Weapon.weaponStats.Length];
        for (int i = 0; i < Weapon.weaponStats.Length; i++)
        {
            isWeaponUnlocked[i] = Weapon.weaponStats[i].unlockCondition == UnlockCondition.Free;
            //Debug.Log("aa>" + isWeaponUnlocked[i]);
        }
        isWeaponPurchased = new bool[Weapon.weaponStats.Length];
        for (int i = 0; i < Weapon.weaponStats.Length; i++)
        {
            isWeaponPurchased[i] = Weapon.weaponStats[i].unlockCondition == UnlockCondition.Free;
            //Debug.Log(">" + isWeaponPurchased[i]);
        }
        isArrowUnlocked = new bool[Projectile.projectileStats.Length];
        for (int i = 0; i < Projectile.projectileStats.Length; i++)
        {
            isArrowUnlocked[i] = Projectile.projectileStats[i].unlockCondition == UnlockCondition.Free;
        }
        isAchievementUnlocked = new bool[Achievement.conditionalAchievements.Length];
        arrowQuantities = new int[Projectile.projectileStats.Length];

        totalDefeats = 0;
        totalVictories = 0;
        bestScore = 0;
        mostCurrencySavedInGame = 0;
        highestWaveSurvived = 0;
        mostKillsInGame = 0;
        cumulativeDamageObjectiveTook = 0;
        mostWeakSpotsHitInAGame = 0;
        weakSpotsHitByEnemy = new int[Enemy.enemyStats.Length];
        enemiesKilledByWeapon = new int[Weapon.weaponStats.Length];
        weaponsUsedByGame = new int[Projectile.projectileStats.Length];
        killsByEnemy = new int[Enemy.enemyStats.Length];
        killsByArrowAttribute = new int[Projectile.projectileStats.Length];
        arrowsShotByAttribute = new int[Projectile.projectileStats.Length];
        weakSpotsHitByWeapon = new int[Weapon.weaponStats.Length];
        upgradedLevelsByWeapon = new int[Weapon.weaponStats.Length];
    }
}

/*
[Serializable]
// Keeps track of player achievements
public class PlayerAchievements
{
    
    

}
*/
public class GameManager : MonoBehaviour {
    public static GameManager gm; // Single existing game manager
    public static bool Debugging; // Enables in game debugging
    public PlayerData data; // Information about any saved game
    public PersonalData personalData; // Information about player stats and records
    public DynamicData arrowQty; // Keeps track of each arrow qty. Used to store in file dynamically and for performance

    public Dictionary<int, Enemy> enemies; // Keeps track of enemies spawned in game
    public Dictionary<int, Trap> traps; // Keeps track of traps spawned in game

    public bool inGame, // In game scene?
                //gameOver, // Game session end?
                startWaves, // Wave spawning?
                continuedGame, // Was this game saved then loaded? If so load game from data
                spawning, // Determines if in process of spawning an enemy
                doneSpawningWave, // Is current wave all spawned?
                isSettingPlayerOrientation, // Are you done setting up the orientation of your forward?
                //gyroEnabled, // Used????
                isBlackingOut, // Used for blackout effect for player view
                //hasWon,
                interactiveTouch, // Using touch interactions that isn't just shoot button?
                onIntermission, // Are we on break from defending waves of enemies?
                playingOnline, // Are we playing with players online?
                edittingMap, // Are we adjusting map defenses?
                realTimeAction, // Trade off between live player movement and in game activities
                paused; // Game paused?

    public int //wave, // Determines current wave to spawn
               playerID, // Unique identifier for player
               kills, // Total enemies killed in one wave
               //personalKills, // Total enemies you killed in one wave (multiplayer purpose)
               //totalPersonalKills, // Total enemies you killed throughout game session
               //totalKills, // Total enemies killed cumulative
               spawnIndex, // Which enemy to spawn in an interval
               intervalIndex, // Which group of enemies to spawn in a wave
               patternIterations, // How many times you spawn this spawn pattern
               enemiesSpawned; // Total number of enemies spawned
                               //playerCurrency, // Currency used outside game session
                               //inGameCurrency, // Currency used inside game session
                               //difficulty; // How hard enemies will be (Used to offset enemy level)
                               //score; // Keeps track of how awesome you are

    public float moveSensitivity = .1f;

    public Vector3 playerOrientation; // Keeps track of where your forward is

    public GameObject[] trapPrefabs, // List of trap objects
                        trapSpawnPrefab, // Pre-image of trap being spawned on map
                        enemyPrefabs, // List of enemy objects
                        projectilePrefabs, // List of projectile objects
                        attributePrefabs, // List of attribute objects
                        rewardPrefabs, // List of reward objects
                        indicatorPrefabs, // List of indicator objects
                        buffs, // List of buffs
                        interactiveUIPrefabs, // list of UIs used for buff conditions
                        areaEffectPrefabs, // list of area effects
                        VFXPrefabs, // list of particle effects
                        weaponPrefabs; // List of weapon objects

    public GameObject[] arrowUIItems; // List of arrow UI interactables in game session

    public GameObject statusIndicatorPrefab, // Shows health and other status for object
                      playerPrefab, // Your player in game
                      playerUIPrefab, // Your object in lobby
                      buttonPrefab, // Used for any general purposes as button
                      postedNoteButtonPrefab, // Button that looks like a post
                      itemUIPrefab, // Used to display items in store in game
                      inventoryUIPrefab, // Used to display inventory items in out of game
                      descriptionPrefab, // Used to provide details about item
                      enemyArmorPrefab, // ???
                      iconPrefab, // icon of item
                      inGameWepItemUIPrefab, // used to display weapon stats in game
                      achievementUIPrefab, // displays description (and progress if exists) of achievement
                      questAchievementUIPrefab, // displays quest achievement description
                      notificationUIPrefab, // used to display notification (EX: quest rewards)
                                            //enemyDeathVFXPrefab, // used after enemy dies
                      joystick, // virtual joystick for non gyroscope movement
                      enemyPrefab; // Enemy object

    public GameObject playerStatusCanvas, // Information used for player to see
                      notificationCanvas, // Displays any notifications (EX: finish quest and receive reward)
                      inventoryCanvas, // Displays shop items and inventory
                      inventoryItemPanel, // displays different selected item categories
                      playerOrientationObjects, // Holds objects pertaining to setting up player orientation
                      quickAccessCanvas, // Holds touch-interactive UI for quick access purposes
                      effectsCanvas, // Holds special effects in the view of the player
                      quickAccessUpgradeDescription, // Used for toggling description of item to upgrade
                      itemWheel, // Holds selectable UI. Touch/Drag to spin wheel
                      intermissionCanvas, // Displays things to do during break 
                      player, // Points to your player
                      playerSpawnPoints, // Container for all potentail spawn points for each player
                      playerRotation, // Container for the map in game
                      projectilesContainer, // Contains projectiles
                      enemiesContainer, // Contains enemies
                      particleEffectsContainer, // Contains particle effects
                      objective, // What you need to defend
                      resultNotification, // Notifies player of victory/defeat
                      mainMenuCanvas, // Displays what you can do when game starts
                      optionsCanvas, // Displays settings to adjust in game
                      optionsPanel, // Holds buttons to available options
                      settingsPanel, // Holds buttons for available settings
                      hostListCanvas, // Displays list of hosts broadcasting availability
                      multiplayerCanvas, // Displays host or client option
                      lobbyCanvas, // Displays players connected in the lobby
                      shopCanvas, // Displays items to buy/upgrade
                      settingsCanvas, // Displays settings to ajust
                      mapUICanvas, // Displays content used to edit map (Defenses, traps, etc.)
                      buttonDisplayer, // Contains lists of buttons 
                      displayOptions, // Contains buttons that map to type of items to display (defense, store, inventory,...)
                      defensesContainer, // Contains defenses available to place onto map
                      inventoryContainer, // Contains weapons and other non-defense items available to upgrade
                      storeContainer, // List of items, defenses, etc., available to purchase
                      descriptionDisplay, // Displays description and details about selected item
                      selectedOption, // Selects which option container will be selected
                      selectedItem, // Potential item to purchase
                      addToMapBtn, // Button used to spawn defenses onto map
                      changeArrowBtn, // Press and hold to display item wheel to change arrow
                      hitObjectiveIndicator, // Warns player that the objective is under attack
                      buffIconContainer, // holds buff icons
                      interactiveUIContainer, // holds interactive UI
                      itemDropdownList, // holds list of (attribute) items
                      firework, // particle effect for victory
                      achievementsCanvas, // Displays player achievements/records
                      inGameWepItemUI, // Details of player's weapon stats in game
                      targetContainer, // contains targets available during intermission
                      waveNotification; // Notifies player that enemies will spawn

    public Text scoreTxt; // Indicates how awesome you are

    public string scene, // Indicates which scene is loaded
                  selectedTab, // Used to check which category to display in inventory
                  shopType, // Type of purchse: Store, Upgrade
                  itemType; // Type of item: Weapon, Defense, Trap, Objective, ...

    public float spawnTimer, // Time currently before spawning enemy 
                 timeToSpawn; // Default time to assign to spawn enemy

    public Camera mapCamera, // Bird eye view of map, used to also edit defenses onto map
                  selectedCamera; // Determines which camera is being used (unused)
    public int mapFingerID, // Used for detecting player selecting/dragging item onto map
               calibrateFingerID, // Used for settings purposes
               quickAccessFingerID; // Used for holding down on quick-access buttons (upgrade weapon, swap arrows)
    public GameObject selectedDescription, // Selects type of item description to display
                      selectedDefense; // Currently selected defense to place onto map
    public Dictionary<int, int> myDefenses, // Counts number of each type of defense in inventory
                                myProjectiles, // Counts number of each type of projectile in inventory
                                //myAttributes, // Counts number of each type of attribute in inventory
                                myTraps; // Counts number of each type of trap in inventory 
    public string mapAction,
                  quickAccessDetail,
                  objectDetail;
    public int selectedAttribute; // Used to determine extra feature applied onto projectile
               
    public GameObject hitIndicator; // indicates if you hit an enemy
    Vector2 initialPos;
    float initialAngle;
    Pattern pattern; // Points to enemy spawn pattern

    // list of itcons to represent such items
    public Sprite[] arrowItemIcons, weaponItemIcons, currencyIcons;
    public Sprite scoreIcon,repairIcon, zoomInIcon,zoomOutIcon;
    public Texture[] borderIcons;
    //Button upgradeWepBtn;
    Coroutine blackoutCoroutine,
              waveNotificationCoroutine,
              frostBorderCoroutine;

    public AudioClip onEnemyHitSFX,
                     obtainCoinSFX,
                     purchaseSFX,
                     invalidSFX,
                     equipSFX,
                     btnClickSFX,
                     victorySFX,
                     defeatSFX,
                     objectiveHitSFX,
                     repairSFX,
                     bubblePopSFX,
                     playerUnfreezeSFX,
                     breakIceSFX,
                     playerFreezeSFX;
    //public List<AudioClip> 
    public AudioSource audioSrc;
    public List<AudioSource> inGameAudio, outOfGameAudio;

    Transform notificationContainer; // holds list of notifications

    public List<int> availableAttributes; // list of attributes able to be used/spawned (based on Unlock Conditions)
    
    // Safely copy contents from source to dest
    public void CopyArrayData<T>(T[] dest, T[] source)
    {
        if (dest == null || source == null)
            return;
        int len = Mathf.Min(dest.Length, source.Length);
        for (int i = 0; i < len; i++)
        {
            dest[i] = source[i];
        }
    }

    public void CopyPlayerData(PlayerData dest, PlayerData source)
    {
        dest.gameOver = source.gameOver;
        dest.hasWon = source.hasWon;
        dest.difficulty = source.difficulty;
        dest.inGameCurrency = source.inGameCurrency;
        dest.objectiveHP = source.objectiveHP;
        dest.savedGame = source.savedGame;
        dest.totalKills = source.totalKills;
        dest.wave = source.wave;
        dest.wepID = source.wepID;
        dest.wepLvl = source.wepLvl;

        CopyArrayData(dest.arrowQuantities, source.arrowQuantities);
        CopyArrayData(dest.killsByArrowAttribute, source.killsByArrowAttribute);
        CopyArrayData(dest.arrowsShotByAttribute, source.arrowsShotByAttribute);
    }


    public void CopyPersonalData(PersonalData dest, PersonalData source)
    {
        dest.equippedWep = source.equippedWep;
        dest.playerCurrency = source.playerCurrency;
        dest.bestScore = source.bestScore;
        dest.mostCurrencySavedInGame = source.mostCurrencySavedInGame;
        dest.highestWaveSurvived = source.highestWaveSurvived;
        dest.mostKillsInGame = source.mostKillsInGame;
        dest.totalDefeats = source.totalDefeats;
        dest.totalVictories = source.totalVictories;
        dest.mostWeakSpotsHitInAGame = source.mostWeakSpotsHitInAGame;
        dest.cumulativeDamageObjectiveTook = source.cumulativeDamageObjectiveTook;

        CopyArrayData(dest.arrowQuantities, source.arrowQuantities);
        CopyArrayData(dest.isArrowUnlocked, source.isArrowUnlocked);
        CopyArrayData(dest.arrowsShotByAttribute, source.arrowsShotByAttribute);
        CopyArrayData(dest.killsByArrowAttribute, source.killsByArrowAttribute);
        CopyArrayData(dest.isWeaponPurchased, source.isWeaponPurchased);
        CopyArrayData(dest.isWeaponUnlocked, source.isWeaponUnlocked);
        CopyArrayData(dest.enemiesKilledByWeapon, source.enemiesKilledByWeapon);
        CopyArrayData(dest.weakSpotsHitByEnemy, source.weakSpotsHitByEnemy);
        CopyArrayData(dest.upgradedLevelsByWeapon, source.upgradedLevelsByWeapon);
        CopyArrayData(dest.killsByEnemy, source.killsByEnemy);
        CopyArrayData(dest.weakSpotsHitByWeapon, source.weakSpotsHitByWeapon);
        CopyArrayData(dest.isAchievementUnlocked, source.isAchievementUnlocked);
    }

    // Saves data based on the type
    public void Save(string type)
    {
        BinaryFormatter bf = new BinaryFormatter();
        string path = Application.persistentDataPath + "/" + type + ".dat";
        FileStream file;
        if (!File.Exists(path))
        {
            print("nonexistent making");
            file = File.Create(path);
        }
        else
        {
            //print("found");
            file = File.Open(path, FileMode.Open);
        }

        switch (type)
        {
            // Save in game progress
            case "continuedGame":
                PlayerData data = gm.data;//new PlayerData();
                data.savedGame = inGame;
                
                /*
                data.totalKills = totalKills;
                
                data.inGameCurrency = inGameCurrency;
                data.score = score;
                data.difficulty = difficulty;
                data.wave = wave;
                */
                 
                /*
                if (myAttributes != null)
                {
                    print(Projectile.projectileStats.Length + " " + myAttributes.Count);
                    foreach(KeyValuePair<int,int> kvp in myAttributes)
                    {
                        data.arrowQuantities[kvp.Key] = kvp.Value;
                    }
                }
                */
                //data.currency = playerCurrency;
                Debug.Log(inGame + " " + data.wave);
                if (objective != null)
                {
                    data.objectiveHP = objective.transform.GetComponent<Objective>().HP;
                }
                if (player)
                {
                    PlayerController pc = player.GetComponent<PlayerController>();
                    if (pc && pc.wep)
                    {
                        data.wepLvl = pc.wep.lvl;
                        data.wepID = pc.wep.wepID;
                    }
                }
                bf.Serialize(file, data);
                break;

            case "setupMain":
                //print("seting");
                PersonalData personalData = new PersonalData();
                if (gm.personalData != null)
                    personalData = gm.personalData;

                    //personalData.playerCurrency = playerCurrency;
                bf.Serialize(file, personalData);
                break;

            case "arrowQuantity":
                bf.Serialize(file, arrowQty); 
                break;
        }
        
        file.Close();
        Debug.Log("Saved " + type);
    }

    // Load data based on the type
    public void Load(string type)
    {
        print(Application.persistentDataPath);
        string path = Application.persistentDataPath + "/" + type + ".dat";
        if (File.Exists(path))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(path, FileMode.Open);

            if (file.Length <= 0)
            {
                Debug.Log("File is empty. No loading");
                file.Close();
                return;
            }

            switch (type)
            {
                // Load in game progress
                case "continuedGame":
                    PlayerData data = (PlayerData)bf.Deserialize(file);
                    //gm.data = data;
                    CopyPlayerData(gm.data, data);
                    break;

                // Load player stats/achievements
                case "setupMain":
                    PersonalData personalData = (PersonalData)bf.Deserialize(file);
                    CopyPersonalData(gm.personalData, personalData);
                    //gm.personalData = personalData;
                    break;
                // Load array of arrow quantities
                case "arrowQuantity":
                    arrowQty = (DynamicData)bf.Deserialize(file);
                    break;
            }
            //Debug.Log(file.Length);

            file.Close();
        }
        else
        {
            Debug.Log("File Does Not Exists");
            Save(type);
        }
    }

    // Use this for initialization

    void Start () {

        if (gm)
            return;
        //Sprite icon = Instantiate(Resources.Load(")
        Achievement.Instantiate();
        enemies = new Dictionary<int, Enemy>();
        gm = this;
        gm.data = new PlayerData();
        gm.arrowQty = new DynamicData(Projectile.projectileStats.Length);
        gm.personalData = new PersonalData();
        DontDestroyOnLoad(gm);
        interactiveTouch = false;
        traps = new Dictionary<int, Trap>();
        myTraps = new Dictionary<int, int>();
        inGameAudio = new List<AudioSource>();
        outOfGameAudio = new List<AudioSource>();
        //myAttributes = new Dictionary<int, int>();
        myProjectiles = new Dictionary<int, int>();
        myDefenses = new Dictionary<int, int>();
        audioSrc = gameObject.AddComponent<AudioSource>();
        playerOrientation = Vector3.zero;
        realTimeAction = true;
        selectedTab = "Weapons";
        arrowUIItems = new GameObject[Projectile.projectileStats.Length];
        Screen.orientation = ScreenOrientation.LandscapeRight; // Landscape mode for mobile phones
        LoadMainScene(); // Default start game in main scene
        EnemySpawnPattern.InstantiatePatterns();
    }

    private void OnLevelWasLoaded(int level)
    {
        if (this != gm)
            return;

        switch (level)
        {
            // Main
            case 0:
                LoadMainScene();
                break;
            // Game
            case 1:
                LoadGameScene();
                break;
            
            // Calibration
            case 2:
                LoadCalibrationScene();
                break;
            // Online Lobby (unused)
            case 3:
                LoadLobbyScene();
                break;
        }
    }

    // unused
    public void LoadLobbyScene()
    {
        projectilesContainer = GameObject.Find("ProjectilesContainer");
        GameObject.Find("Canvas").transform.Find("LeaveBtn").GetComponent<Button>().onClick.AddListener(GoToMainScene);
    }

    // Scene where you adjust your forward direction
    public void LoadCalibrationScene()
    {
        calibrateFingerID = -1;
        scene = "calibration";
        player = GameObject.Find("Player");
        //player.GetComponent<PlayerController>().SetOrientation(playerOrientation);
        isSettingPlayerOrientation = true;
        playerRotation = GameObject.Find("Player Rotation");
        //playerRotation.transform.eulerAngles = playerOrientation;
        optionsCanvas = GameObject.Find("OptionsCanvas");
        optionsCanvas.transform.Find("FinishedBtn").GetComponent<Button>().onClick.AddListener(GoToMainScene);
    }

    public void LoadMainScene()
    {
        for(int i = 0; i < inGameAudio.Count; i++)
        {
            if(inGameAudio[i] != null)
            {
                Destroy(inGameAudio[i]);
            }
        }
        inGameAudio.Clear();
        Load("continuedGame");
        Load("setupMain");
        Load("arrowQuantity");
        scene = "main";
        selectedTab = "Weapons";
        inGame = false;

        isSettingPlayerOrientation = false;
        mainMenuCanvas = GameObject.Find("MainMenuCanvas");

        mainMenuCanvas.transform.Find("PlayerCurrency").GetChild(0).GetComponent<Text>().text = "" + personalData.playerCurrency;

        Transform btnContainer = mainMenuCanvas.transform.Find("ButtonsContainer");

        foreach(Transform child in btnContainer)
        {
            child.GetComponent<Button>().onClick.AddListener(NotifyButtonPressed);
        }

        if (data != null && data.savedGame)
        {
            btnContainer.Find("PlayBtn").GetComponent<Button>().onClick.AddListener(NotifyContinuedSessionExists);
        }
        else
        {
            btnContainer.Find("PlayBtn").GetComponent<Button>().onClick.AddListener(GoToGameScene);
        }
        btnContainer.Find("OnlineBtn").GetComponent<Button>().onClick.AddListener(ToggleMultiplayerCanvas);

        btnContainer.Find("OnlineBtn").GetComponent<Button>().onClick.AddListener(ToggleMainMenuCanvas);

        btnContainer.Find("SettingsBtn").GetComponent<Button>().onClick.AddListener(ToggleMainMenuCanvas);
        btnContainer.Find("SettingsBtn").GetComponent<Button>().onClick.AddListener(ToggleSettingsCanvas);

        btnContainer.Find("InventoryBtn").GetComponent<Button>().onClick.AddListener(ToggleInventoryCanvas);
        btnContainer.Find("InventoryBtn").GetComponent<Button>().onClick.AddListener(ToggleMainMenuCanvas);

        btnContainer.Find("AchievementsBtn").GetComponent<Button>().onClick.AddListener(ToggleMainMenuCanvas);
        btnContainer.Find("AchievementsBtn").GetComponent<Button>().onClick.AddListener(ToggleAchievementsCanvas);

        // Go to continued game if saved game progress exists
        btnContainer.Find("ContinueBtn").GetComponent<Button>().onClick.AddListener(GoToContinuedGameScene);
        //btnContainer.Find("ContinueBtn").Find("Text").GetComponent<Text>().text += (data != null && data.savedGame) ? " (Wave " + (data.wave+1) + ")" : "";
        btnContainer.Find("ContinueBtn").gameObject.SetActive(data != null && data.savedGame);

        multiplayerCanvas = GameObject.Find("MultiplayerCanvas");
        multiplayerCanvas.SetActive(false);
        btnContainer = multiplayerCanvas.transform.Find("ButtonsContainer");
        foreach (Transform child in btnContainer)
        {
            child.GetComponent<Button>().onClick.AddListener(NotifyButtonPressed);
        }

        btnContainer.Find("HostBtn").GetComponent<Button>().onClick.AddListener(ToggleLobbyCanvas);
        btnContainer.Find("HostBtn").GetComponent<Button>().onClick.AddListener(ToggleMultiplayerCanvas);
        btnContainer.Find("HostBtn").GetComponent<Button>().onClick.AddListener(NetworkManager.nm.SetupAsHost);
        btnContainer.Find("HostBtn").GetComponent<Button>().onClick.AddListener(NetworkManager.nm.StartUpNetworkActivities);

        btnContainer.Find("FindHostBtn").GetComponent<Button>().onClick.AddListener(ToggleHostListCanvas);
        btnContainer.Find("FindHostBtn").GetComponent<Button>().onClick.AddListener(ToggleMultiplayerCanvas);
        btnContainer.Find("FindHostBtn").GetComponent<Button>().onClick.AddListener(NetworkManager.nm.SetupAsClient);
        btnContainer.Find("FindHostBtn").GetComponent<Button>().onClick.AddListener(NetworkManager.nm.StartUpNetworkActivities);

        btnContainer.Find("BackBtn").GetComponent<Button>().onClick.AddListener(ToggleMainMenuCanvas);
        btnContainer.Find("BackBtn").GetComponent<Button>().onClick.AddListener(ToggleMultiplayerCanvas);
        
        hostListCanvas = GameObject.Find("HostListCanvas");
        hostListCanvas.SetActive(false);
        hostListCanvas.transform.Find("BackBtn").GetComponent<Button>().onClick.AddListener(ToggleMultiplayerCanvas);
        hostListCanvas.transform.Find("BackBtn").GetComponent<Button>().onClick.AddListener(ToggleHostListCanvas);
        hostListCanvas.transform.Find("BackBtn").GetComponent<Button>().onClick.AddListener(NetworkManager.nm.Disconnect);
        hostListCanvas.transform.Find("BackBtn").GetComponent<Button>().onClick.AddListener(NotifyButtonPressed);

        lobbyCanvas = GameObject.Find("LobbyCanvas");
        lobbyCanvas.SetActive(false);
        lobbyCanvas.transform.Find("BackBtn").GetComponent<Button>().onClick.AddListener(NetworkManager.nm.RequestLeaveLobby);
        lobbyCanvas.transform.Find("BackBtn").GetComponent<Button>().onClick.AddListener(NotifyButtonPressed);

        lobbyCanvas.transform.Find("ReadyBtn").GetComponent<Button>().onClick.AddListener(NetworkManager.nm.RequestReady);

        settingsCanvas = GameObject.Find("SettingsCanvas");
        settingsCanvas.SetActive(false);
        string isOn = "ON";
        if (!interactiveTouch)
            isOn = "OFF";
        settingsCanvas.transform.Find("ButtonsContainer").Find("InteractiveTouchBtn").GetChild(0).GetComponent<Text>().text = "Interactive Touch: " + isOn;
        settingsCanvas.transform.Find("ButtonsContainer").Find("InteractiveTouchBtn").gameObject.SetActive(false);
        if (Debugging)
            isOn = "ON";
        else
            isOn = "OFF";
        settingsCanvas.transform.Find("ButtonsContainer").Find("DebugBtn").GetChild(0).GetComponent<Text>().text = "Debug Mode: " + isOn;
        if (realTimeAction)
            isOn = "ON";
        else
            isOn = "OFF";
        settingsCanvas.transform.Find("ButtonsContainer").Find("LiveActionBtn").GetChild(0).GetComponent<Text>().text = "Real Time Action: " + isOn;

        btnContainer = settingsCanvas.transform.Find("ButtonsContainer");

        btnContainer.Find("InteractiveTouchBtn").GetComponent<Button>().onClick.AddListener(ToggleInteractiveTouch);

        btnContainer.Find("DebugBtn").GetComponent<Button>().onClick.AddListener(ToggleDebugging);
        btnContainer.Find("LiveActionBtn").GetComponent<Button>().onClick.AddListener(ToggleRealTimeAction);

        btnContainer.Find("SetupOrientationBtn").GetComponent<Button>().onClick.AddListener(GoToCalibrationScene);
        btnContainer.Find("SetupOrientationBtn").gameObject.SetActive(SystemInfo.supportsGyroscope);

        btnContainer.Find("BackBtn").GetComponent<Button>().onClick.AddListener(ToggleMainMenuCanvas);
        btnContainer.Find("BackBtn").GetComponent<Button>().onClick.AddListener(ToggleSettingsCanvas);

        // setup notification canvas
        notificationCanvas = GameObject.Find("NotificationCanvas");
        notificationCanvas.transform.Find("NotificationUI").Find("ConfirmBtn").GetComponent<Button>().onClick.AddListener(ToggleNotificationCanvas);
        notificationCanvas.transform.Find("WarningUI").Find("ConfirmBtn").GetComponent<Button>().onClick.AddListener(GoToGameScene);
        notificationCanvas.transform.Find("WarningUI").Find("CancelBtn").GetComponent<Button>().onClick.AddListener(ToggleNotificationCanvas);
        notificationCanvas.SetActive(false);
        StartCoroutine(CheckForQuestCompletions());
        // Setup Inventory and Shop

        inventoryCanvas = GameObject.Find("InventoryCanvas");
        inventoryCanvas.SetActive(false);
        inventoryCanvas.transform.Find("BackBtn").GetComponent<Button>().onClick.AddListener(ToggleInventoryCanvas);
        inventoryCanvas.transform.Find("BackBtn").GetComponent<Button>().onClick.AddListener(ToggleMainMenuCanvas);
        inventoryCanvas.transform.Find("BackBtn").GetComponent<Button>().onClick.AddListener(NotifyButtonPressed);
        // update currency for a purpose????
        //UpdatePlayerCurrency(0);
        inventoryItemPanel = inventoryCanvas.transform.Find("ItemUIPanel").gameObject;

        // Set up weapon section
        Transform UIContainer = inventoryItemPanel.transform.Find("WeaponsUIContainer");
        //UIContainer.transform.localPosition = new Vector3(0, -1000, 0);
        GameObject itemUI, scoreObj, unlockObj;
        for (int i = 0; i < Weapon.weaponStats.Length; i++)
        {
            itemUI = Instantiate(inventoryUIPrefab);
            itemUI.transform.SetParent(UIContainer);
            //csf.AddItem(itemUI);
            itemUI.transform.Find("ItemImageBG").GetComponent<RawImage>().color = new Color(249f / 255, 88f/255, 0);// / 255;
            itemUI.transform.Find("ItemName").GetComponent<Text>().text = Weapon.weaponStats[i].name + " Lvl " + (personalData.upgradedLevelsByWeapon[i] + 1);
            itemUI.transform.Find("ItemImageBG").GetChild(0).GetComponent<RawImage>().texture = weaponItemIcons[i % weaponItemIcons.Length].texture;
            itemUI.transform.Find("ItemStats").Find("Damage").Find("Text").GetComponent<Text>().text = "" + Weapon.weaponStats[i].dmg[0];
            itemUI.transform.Find("ItemStats").Find("Reload").Find("Text").GetComponent<Text>().text = "" + Weapon.weaponStats[i].timeToReload[0];
            itemUI.transform.Find("ItemStats").Find("ChargeAcceleration").Find("Text").GetComponent<Text>().text = "" + Weapon.weaponStats[i].chargeAccelation[0];
            itemUI.transform.Find("ItemStats").Find("BowStr").Find("Text").GetComponent<Text>().text = "" + Weapon.weaponStats[i].distance[0];
            itemUI.transform.Find("ItemDescription").GetComponent<Text>().text = Weapon.weaponStats[i].description;
            itemUI.transform.Find("QtyTxt").gameObject.SetActive(false);
            itemUI.name = "wepUI " + i;
            //wepUI.tag = "wepUI";
            //wepUI.transform.GetComponent<Button>().onClick.AddListener(PerformInventoryAction);
            //print("ASD" + personalData.isWeaponUnlocked[i]);
            //print(Weapon.weaponStats[i].unlockCondition);
            //print((Weapon.weaponStats[i].unlockCondition == UnlockCondition.Quest || Weapon.weaponStats[i].unlockCondition == UnlockCondition.QuestThenPurchase));
            //print( !personalData.isWeaponUnlocked[i]);
            itemUI.transform.Find("LockedItemImage").gameObject.SetActive(
                (Weapon.weaponStats[i].unlockCondition == UnlockCondition.Quest || Weapon.weaponStats[i].unlockCondition == UnlockCondition.QuestThenPurchase) 
                && !personalData.isWeaponUnlocked[i]);
            Transform actionBtn;
            GameObject wepActionBtns = itemUI.transform.Find("WepActionButtons").gameObject;
            //actionBtn = itemUI.transform.Find("ActionBtn");
            //actionBtn.gameObject.SetActive(false);
            //actionBtn = itemUI.transform.Find("WepActionButtons").Find("ActionBtn");
            int cost;

            // Handles button to buy weapon
            actionBtn = itemUI.transform.Find("ActionBtn");
            //actionBtn.gameObject.SetActive(false);
            actionBtn.Find("Text").GetComponent<Text>().text = "Unlock\n";
            actionBtn.Find("Currency").Find("Text").GetComponent<Text>().text = "" + Weapon.weaponStats[i].price;
            actionBtn.GetComponent<Button>().interactable = personalData.playerCurrency >= Weapon.weaponStats[i].price;
            //wepActionBtns.SetActive(true);

            // Handles button to equip weapon
            actionBtn = wepActionBtns.transform.Find("ActionBtn");
            actionBtn.GetComponent<Button>().interactable = personalData.equippedWep != i;
            actionBtn.Find("Text").GetComponent<Text>().text = (personalData.equippedWep == i) ? "Equipped" : "Equip";
            actionBtn.Find("Currency").gameObject.SetActive(false);

            // Handles button to upgrade weapon
            actionBtn = wepActionBtns.transform.Find("UpgradeBtn");
            //actionBtn.name = "upgrade " + i;
           if (HasWeaponReachedMaxLevel(i))
            {
                actionBtn.Find("Text").GetComponent<Text>().text = "Upgrade\nMAXED";
                actionBtn.Find("Currency").gameObject.SetActive(false);
                actionBtn.GetComponent<Button>().interactable = false;
            }
            else
            {
                cost = Weapon.weaponStats[i].costToUpgrade[personalData.upgradedLevelsByWeapon[i]] * 10;
                actionBtn.GetComponent<Button>().interactable = personalData.upgradedLevelsByWeapon[i] < Weapon.weaponStats[i].costToUpgrade.Length && personalData.playerCurrency >= cost;

                actionBtn.Find("Text").GetComponent<Text>().text = "Upgrade\n";
                actionBtn.Find("Currency").gameObject.SetActive(true);
                actionBtn.Find("Currency").Find("Text").GetComponent<Text>().text = "" + cost;
            }
            switch (Weapon.weaponStats[i].unlockCondition)
            {
                case UnlockCondition.Free:
                    actionBtn = itemUI.transform.Find("ActionBtn");
                    actionBtn.gameObject.SetActive(false);
                    wepActionBtns.SetActive(true);
                    break;
                case UnlockCondition.Purchase:
                    actionBtn = itemUI.transform.Find("ActionBtn");
                    actionBtn.gameObject.SetActive(!personalData.isWeaponPurchased[i]);
                    wepActionBtns.SetActive(personalData.isWeaponPurchased[i]);
                    break;
                case UnlockCondition.Quest:
                    actionBtn = itemUI.transform.Find("ActionBtn");
                    actionBtn.gameObject.SetActive(false);
                    wepActionBtns.SetActive(true);
                    break;
                case UnlockCondition.QuestThenPurchase:
                    actionBtn = itemUI.transform.Find("ActionBtn");
                    actionBtn.gameObject.SetActive(!personalData.isWeaponPurchased[i]);
                    wepActionBtns.SetActive(personalData.isWeaponPurchased[i]);

                    break;
            }
            itemUI.SetActive(Weapon.enableWeps[i]);
        }
        
        if (availableAttributes != null)
            availableAttributes.Clear();
        else
            availableAttributes = new List<int>();
        // Set up arrow section
        UIContainer = inventoryItemPanel.transform.Find("ArrowsUIContainer");
        //UIContainer.transform.localPosition = new Vector3(0, -1000, 0);
        //csf = UIContainer.GetComponent<ContentSizeFitter>();
        for (int i = 0; i < Projectile.projectileStats.Length; i++)
        {
            itemUI = Instantiate(inventoryUIPrefab);
            //csf.AddItem(itemUI);
            itemUI.transform.SetParent(UIContainer);
            itemUI.transform.Find("ItemImageBG").GetChild(0).GetComponent<RawImage>().texture = arrowItemIcons[i].texture;
            itemUI.transform.Find("ItemImageBG").GetComponent<RawImage>().color = new Color(149f / 255, 1, 1);// / 255;
            itemUI.transform.Find("ItemName").GetComponent<Text>().text = Projectile.projectileStats[i].name;
            itemUI.transform.Find("ItemStats").gameObject.SetActive(false);//Find("Damage").Find("Text").GetComponent<Text>().text = "" + Weapon.weaponStats[i].dmg[0];
            //itemUI.transform.Find("ItemStats").Find("Reload").Find("Text").GetComponent<Text>().text = "" + Weapon.weaponStats[i].timeToReload[0];
            //itemUI.transform.Find("ItemStats").Find("ChargeAcceleration").Find("Text").GetComponent<Text>().text = "" + Weapon.weaponStats[i].chargeAccelation[0];
            //itemUI.transform.Find("ItemStats").Find("BowStr").Find("Text").GetComponent<Text>().text = "" + Weapon.weaponStats[i].distance[0];
            itemUI.transform.Find("ItemDescription").GetComponent<Text>().text = Projectile.projectileStats[i].description;
            itemUI.transform.Find("QtyTxt").gameObject.SetActive(true);
            itemUI.transform.Find("InfinityIcon").gameObject.SetActive(i==0);
            itemUI.transform.Find("WepActionButtons").gameObject.SetActive(false);
            itemUI.transform.Find("QtyTxt").GetComponent<Text>().text = "Quantity: " + ((i == 0)? "": "" + arrowQty.arr[i]);
            itemUI.name = "arrowUI " + i;
            //wepUI.tag = "wepUI";
            //wepUI.transform.GetComponent<Button>().onClick.AddListener(PerformInventoryAction);
            // show if item is unlocked or free
            itemUI.transform.Find("LockedItemImage").gameObject.SetActive(
                Projectile.projectileStats[i].unlockCondition == UnlockCondition.QuestThenPurchase && !personalData.isArrowUnlocked[i]);

            // add to attribute pool that is spawnable in game as reward under these conditions
            if (i != 0 && Projectile.enableArrows[i] &&
              //  (Projectile.projectileStats[i].unlockCondition == UnlockCondition.Free) ||
                ((Projectile.projectileStats[i].unlockCondition != UnlockCondition.Quest && Projectile.projectileStats[i].unlockCondition != UnlockCondition.QuestThenPurchase) ||
                personalData.isArrowUnlocked[i])
               )
            {
                availableAttributes.Add(i);
            }
            Transform actionBtn = itemUI.transform.Find("ActionBtn");
            actionBtn.GetComponent<Button>().interactable = i != 0 && personalData.playerCurrency >= Projectile.projectileStats[i].price;
            switch (Projectile.projectileStats[i].unlockCondition)
            {
                case UnlockCondition.Free:
                    actionBtn.Find("Text").GetComponent<Text>().text = "";
                    actionBtn.Find("Currency").gameObject.SetActive(false);
                    break;
                case UnlockCondition.Purchase:
                    actionBtn.Find("Text").GetComponent<Text>().text = "Buy\n";
                    actionBtn.Find("Currency").gameObject.SetActive(true);
                    actionBtn.Find("Currency").Find("Text").GetComponent<Text>().text = "" + Projectile.projectileStats[i].price;
                    break;
                case UnlockCondition.QuestThenPurchase:
                    actionBtn.Find("Text").GetComponent<Text>().text = "Buy\n";
                    actionBtn.Find("Currency").gameObject.SetActive(true);
                    actionBtn.Find("Currency").Find("Text").GetComponent<Text>().text = "" + Projectile.projectileStats[i].price;
                    break;
            }
            itemUI.SetActive(Projectile.enableArrows[i]);
        }

        //print(GameManager.gm.availableAttributes.Count);
        UIContainer.gameObject.SetActive(false);

        // set up achievement canvas

        achievementsCanvas = GameObject.Find("AchievementsCanvas");
        achievementsCanvas.transform.Find("BackBtn").GetComponent<Button>().onClick.AddListener(ToggleAchievementsCanvas);
        achievementsCanvas.transform.Find("BackBtn").GetComponent<Button>().onClick.AddListener(ToggleMainMenuCanvas);
        achievementsCanvas.transform.Find("BackBtn").GetComponent<Button>().onClick.AddListener(NotifyButtonPressed);
        btnContainer = achievementsCanvas.transform.Find("ButtonsContainer");
        Transform achievementsContainer = achievementsCanvas.transform.Find("ItemUIPanel").Find("RecordsUIContainer");//"AchievementsContainer");


        // fetch all best score achievements
        for (int i = 0; i < Achievement.bestScoreAchievements.Length; i++)
        {
            itemUI = Instantiate(achievementUIPrefab);
            itemUI.transform.SetParent(achievementsContainer);
            itemUI.transform.Find("HideProgress").gameObject.SetActive(!Achievement.CanShowProgress(AchievementType.BestScore, i));
            itemUI.transform.Find("HeaderTxt").GetComponent<Text>().text = Achievement.bestScoreAchievements[i].header;
            itemUI.transform.localScale = new Vector3(1, 1, 1);
            itemUI.transform.Find("AchievementTypes").GetChild(1).gameObject.SetActive(false);
            scoreObj = itemUI.transform.Find("AchievementTypes").GetChild(0).gameObject;//.SetActive(false);
            scoreObj.SetActive(true);
            scoreObj.transform.Find("ScoreTxt").GetComponent<Text>().text = Achievement.GetAchievementDetails(AchievementType.BestScore, i);

            //itemUI.transform.Find("AchievementType").GetChild(achievementTypeIndex).gameObject.SetActive(true);//GetComponent<Text>().text = Achievement.achievements[i].header;
        }

        // Handle Cumulative Victories
        itemUI = Instantiate(achievementUIPrefab);
        itemUI.transform.SetParent(achievementsContainer);
        itemUI.transform.Find("HideProgress").gameObject.SetActive(false);
        itemUI.transform.Find("HeaderTxt").GetComponent<Text>().text = "Total Victories";
        itemUI.transform.localScale = new Vector3(1, 1, 1);
        itemUI.transform.Find("AchievementTypes").GetChild(1).gameObject.SetActive(false); // disable unlock type
        scoreObj = itemUI.transform.Find("AchievementTypes").GetChild(0).gameObject; // enable score type
        scoreObj.SetActive(true);
        scoreObj.transform.Find("ScoreTxt").GetComponent<Text>().text = "" + personalData.totalVictories;

        // Handle Cumulative Defeats
        itemUI = Instantiate(achievementUIPrefab);
        itemUI.transform.SetParent(achievementsContainer);
        itemUI.transform.Find("HideProgress").gameObject.SetActive(false);
        itemUI.transform.Find("HeaderTxt").GetComponent<Text>().text = "Total Defeats";
        itemUI.transform.localScale = new Vector3(1, 1, 1);
        itemUI.transform.Find("AchievementTypes").GetChild(1).gameObject.SetActive(false); // disable unlock type
        scoreObj = itemUI.transform.Find("AchievementTypes").GetChild(0).gameObject; // enable score type
        scoreObj.SetActive(true);
        scoreObj.transform.Find("ScoreTxt").GetComponent<Text>().text = "" + personalData.totalDefeats;

        // fetch all cumulative score achievements
        for (int j = 0; j < Achievement.cumulativeScoreAchievements.Length; j++)
        {
            for (int i = 0; i < Achievement.cumulativeScoreAchievements[j].Length; i++)
            {
                itemUI = Instantiate(achievementUIPrefab);
                itemUI.SetActive(Achievement.IsItemActive(AchievementType.Cumulative, i,j));
                itemUI.transform.SetParent(achievementsContainer);
                itemUI.transform.localScale = new Vector3(1, 1, 1);
                itemUI.transform.Find("HideProgress").gameObject.SetActive(!Achievement.CanShowProgress(AchievementType.Cumulative, i, j));
                itemUI.transform.Find("HeaderTxt").GetComponent<Text>().text = Achievement.cumulativeScoreAchievements[j][i].header;
                itemUI.transform.Find("AchievementTypes").GetChild(1).gameObject.SetActive(false);
                scoreObj = itemUI.transform.Find("AchievementTypes").GetChild(0).gameObject;//.SetActive(false);
                scoreObj.SetActive(true);
                scoreObj.transform.Find("ScoreTxt").GetComponent<Text>().text = Achievement.GetAchievementDetails(AchievementType.Cumulative, i, j);
            }
        }


        achievementsContainer = achievementsCanvas.transform.Find("ItemUIPanel").Find("QuestsUIContainer");//"AchievementsContainer");

        // fetch all quests
        for (int i = 0; i < Achievement.conditionalAchievements.Length; i++)
        {
            itemUI = Instantiate(questAchievementUIPrefab);
            itemUI.transform.SetParent(achievementsContainer);
            itemUI.transform.localScale = new Vector3(1, 1, 1);
            itemUI.SetActive(Achievement.enableQuests[i]);
            itemUI.transform.Find("HideProgress").gameObject.SetActive(!Achievement.CanShowProgress(AchievementType.Conditional, i));
            itemUI.transform.Find("HeaderTxt").GetComponent<Text>().text = Achievement.conditionalAchievements[i].header;
            itemUI.transform.Find("AchievementTypes").GetChild(0).gameObject.SetActive(false);
            unlockObj = itemUI.transform.Find("AchievementTypes").GetChild(1).gameObject;//.SetActive(false);
            unlockObj.SetActive(true);
            unlockObj.transform.Find("RewardImage").GetChild(0).gameObject.SetActive(false);// !personalData.isAchievementUnlocked[i]);
            unlockObj.transform.Find("RewardImage").GetComponent<Image>().sprite = Achievement.GetRewardIcon(i);
            Transform progress = unlockObj.transform.Find("Progress");
            progress.Find("ProgressTxt").GetComponent<Text>().text = Achievement.GetAchievementDetails(AchievementType.Conditional, i);
            progress.Find("ProgressGauge").Find("ProgressBar").GetComponent<Image>().fillAmount = Achievement.GetQuestProgressPercentage(i);
            unlockObj.transform.Find("Completed").gameObject.SetActive(personalData.isAchievementUnlocked[i]);
        }
        achievementsContainer.gameObject.SetActive(false);
        achievementsCanvas.SetActive(false);

        UpdatePlayerCurrency(0);

        print("done main");
    }

    // Load game scene. If there was any saved game progress, remove it
    public void LoadGameScene()
    {

        personalData.equippedWep = 2;

        for (int i = 0; i < outOfGameAudio.Count; i++)
        {
            if(outOfGameAudio[i] != null)
            {
                Destroy(outOfGameAudio[i]);
            }
        }

        // don't remove saved state if playing multiplayer
        if(!NetworkManager.nm.isStarted)
            Save("continuedGame"); // Removes saved game progress
        scene = "game";
        //Needed here???////////
        //inGame = true;
        //gameOver = false;
        //paused = false;
        //ResetSpawnSetup(0);
        //wave = 0;
        //kills = 0;
        //setupRotation = false;
        //gyroEnabled = true;
        //totalKills = 0;
        //////////////////////////
        //print("player orient:" + playerOrientation);
        outOfGameAudio.Clear();
        Enemy.EnemyCount = 0;
        Objective.ObjectiveCount = 0;
        Weapon.WeaponCount = 0;
        Trap.TrapCount = 0;

        mapFingerID = -1;
        quickAccessFingerID = -1;
        selectedAttribute = 0;

        firework = GameObject.Find("Firework");
        firework.SetActive(false);
        targetContainer = GameObject.Find("TargetContainer");
        intermissionCanvas = GameObject.Find("IntermissionCanvas");
        // If online, make sure everyone is ready to start next wave on intermission
        if (NetworkManager.nm.isStarted)
        {
            intermissionCanvas.transform.Find("ResumeBtn").GetComponent<Button>().onClick.AddListener(NetworkManager.nm.RequestReady);
            intermissionCanvas.transform.Find("ResumeBtn").GetChild(0).GetComponent<Text>().text = "Ready";
            //intermissionCanvas.transform.Find("SaveAndQuitBtn").GetComponent<Button>().onClick.AddListener(NetworkManager.nm.LeaveGame);
            intermissionCanvas.transform.Find("SaveAndQuitBtn").gameObject.SetActive(false);
        }
        else
        {
            intermissionCanvas.transform.Find("ResumeBtn").GetComponent<Button>().onClick.AddListener(NextWave);
            intermissionCanvas.transform.Find("SaveAndQuitBtn").GetComponent<Button>().onClick.AddListener(SaveAndQuit);
        }
        intermissionCanvas.transform.Find("ResumeBtn").GetComponent<Button>().onClick.AddListener(NotifyButtonPressed);
        intermissionCanvas.transform.Find("SaveAndQuitBtn").GetComponent<Button>().onClick.AddListener(NotifyButtonPressed);


        //intermissionCanvas.transform.Find("ShopBtn").GetComponent<Button>().onClick.AddListener(ToggleShopCanvas);
        //intermissionCanvas.transform.Find("ShopBtn").GetComponent<Button>().onClick.AddListener(ToggleIntermissionCanvas);
        intermissionCanvas.SetActive(false);

        enemiesContainer = GameObject.Find("EnemiesContainer");
        projectilesContainer = GameObject.Find("ProjectilesContainer");
        particleEffectsContainer = GameObject.Find("ParticleEffectsContainer");

        playerRotation = GameObject.Find("Player Rotation");
        //playerRotation.transform.eulerAngles = playerOrientation;

        playerStatusCanvas = GameObject.Find("PlayerStatusCanvas").gameObject;
        playerStatusCanvas.transform.Find("OptionsBtn").GetComponent<Button>().onClick.AddListener(ToggleOptionsCanvas);//DisplayOptions);
        playerStatusCanvas.transform.Find("OptionsBtn").GetComponent<Button>().onClick.AddListener(NotifyButtonPressed);
        playerStatusCanvas.transform.Find("MapBtn").GetComponent<Button>().onClick.AddListener(ToggleMapUICanvas);
        playerStatusCanvas.transform.Find("MapBtn").gameObject.SetActive(false);
        joystick = playerStatusCanvas.transform.Find("Joystick").gameObject;
        hitObjectiveIndicator = playerStatusCanvas.transform.Find("ObjectiveHitIndicator").gameObject;
        buffIconContainer = playerStatusCanvas.transform.Find("BuffIconContainer").gameObject;
        interactiveUIContainer = playerStatusCanvas.transform.Find("InteractiveUIContainer").gameObject;

        playerOrientationObjects = playerStatusCanvas.transform.Find("PlayerOrientationObjects").gameObject;
        playerOrientationObjects.transform.Find("SetOrientationBtn").GetComponent<Button>().onClick.AddListener(SetPlayerOrientation);
        playerOrientationObjects.transform.Find("SetOrientationBtn").GetComponent<Button>().onClick.AddListener(NotifyButtonPressed);
        playerOrientationObjects.transform.Find("DoneSetupBtn").GetComponent<Button>().onClick.AddListener(TogglePlayerOrientationSetup);
        playerOrientationObjects.transform.Find("DoneSetupBtn").GetComponent<Button>().onClick.AddListener(NotifyButtonPressed);
        playerOrientationObjects.SetActive(false);
        //upgradeWepBtn = playerStatusCanvas.transform.Find("UpgradeWepBtn").GetComponent<Button>();
        //upgradeWepBtn.OnPointerDown()

        hitIndicator = playerStatusCanvas.transform.Find("HitIndicator").gameObject;

        waveNotification = playerStatusCanvas.transform.Find("Wave Notification").gameObject;
        waveNotification.SetActive(false);

        playerSpawnPoints = playerRotation.transform.Find("PlayerSpawnPoints").gameObject;

        resultNotification = playerStatusCanvas.transform.Find("Result Notification").gameObject;
        resultNotification.transform.Find("StatisticsMask").GetChild(0).Find("RetryBtn").GetComponent<Button>().onClick.AddListener(FinishGame);//ResetGame);
        resultNotification.transform.Find("StatisticsMask").GetChild(0).Find("RetryBtn").GetComponent<Button>().onClick.AddListener(NotifyButtonPressed);
        resultNotification.SetActive(false);

        scoreTxt = playerStatusCanvas.transform.Find("ScoreTxt").GetComponent<Text>();

        objective = playerRotation.transform.Find("Castle").Find("Gate").gameObject;
        
        optionsCanvas = GameObject.Find("OptionsCanvas");
        optionsPanel = optionsCanvas.transform.Find("OptionsPanel").gameObject;
        optionsPanel.transform.Find("ResumeBtn").GetComponent<Button>().onClick.AddListener(ResumeGame);
        optionsPanel.transform.Find("ResumeBtn").GetComponent<Button>().onClick.AddListener(NotifyButtonPressed);
        optionsPanel.transform.Find("ExitBtn").GetComponent<Button>().onClick.AddListener(ToggleNotificationCanvas);
        optionsPanel.transform.Find("ExitBtn").GetComponent<Button>().onClick.AddListener(NotifyButtonPressed);
        optionsPanel.transform.Find("ExitBtn").GetComponent<Button>().onClick.AddListener(ToggleOptionsCanvas);
        optionsPanel.transform.Find("SettingsBtn").GetComponent<Button>().onClick.AddListener(ToggleInGameSettings);
        optionsPanel.transform.Find("SettingsBtn").GetComponent<Button>().onClick.AddListener(NotifyButtonPressed);
        //Transform leaveGameNotif = optionsPanel.transform.Find("LeaveGameNotification");//.GetComponent<Button>().onClick.AddListener(ToggleInGameSettings);
        //leaveGameNotif.Find("CancelBtn").GetComponent<Button>().onClick.AddListener(ToggleLeaveGameNotification);
        //leaveGameNotif.Find("LeaveBtn").GetComponent<Button>().onClick.AddListener(LeaveGame);

        settingsPanel = optionsCanvas.transform.Find("SettingsPanel").gameObject;
        settingsPanel.transform.Find("BackBtn").GetComponent<Button>().onClick.AddListener(ToggleInGameSettings);
        settingsPanel.transform.Find("BackBtn").GetComponent<Button>().onClick.AddListener(NotifyButtonPressed);
        settingsPanel.transform.Find("EnableGyroBtn").GetComponent<Button>().onClick.AddListener(ToggleGyro);
        settingsPanel.transform.Find("EnableGyroBtn").gameObject.SetActive(false && SystemInfo.supportsGyroscope);
        settingsPanel.transform.Find("SetOrientationBtn").GetComponent<Button>().onClick.AddListener(TogglePlayerOrientationSetup);
        settingsPanel.transform.Find("SetOrientationBtn").GetComponent<Button>().onClick.AddListener(NotifyButtonPressed);
        string isOn = "ON";
        if (!interactiveTouch)
            isOn = "OFF";
        //settingsPanel.transform.Find("InteractiveTouchBtn").GetChild(0).GetComponent<Text>().text = "Interactive Touch: " + isOn;
        //settingsPanel.transform.Find("InteractiveTouchBtn").GetComponent<Button>().onClick.AddListener(ToggleInteractiveTouch);
        //settingsPanel.transform.Find("InteractiveTouchBtn").gameObject.SetActive(false);

        settingsPanel.transform.Find("ShowFPSBtn").GetComponent<Button>().onClick.AddListener(ToggleDebugging);
        settingsPanel.transform.Find("ShowFPSBtn").GetChild(0).GetComponent<Text>().text = "Show FPS: " + ((Debugging) ? "ON":"OFF");//GetComponent<Button>().onClick.AddListener(ToggleDebugging);
        //settingsPanel.transform.Find("ShowFPSBtn").GetComponent<Button>().onClick.AddListener(NotifyButtonPressed);

        settingsPanel.SetActive(false);
        optionsCanvas.SetActive(false);

        shopCanvas = GameObject.Find("ShopCanvas");
        shopCanvas.transform.Find("CloseBtn").GetComponent<Button>().onClick.AddListener(ToggleShopCanvas);
        //shopCanvas.transform.Find("CloseBtn").GetComponent<Button>().onClick.AddListener(ToggleIntermissionCanvas);
        Transform btns1 = shopCanvas.transform.Find("ButtonContainer1");
        btns1.Find("StoreBtn").GetComponent<Button>().onClick.AddListener(DisplayStoreSelection);
        btns1.Find("UpgradeBtn").GetComponent<Button>().onClick.AddListener(DisplayUpgradeSelection);
        Transform btns2 = shopCanvas.transform.Find("ButtonContainer2");
        btns2.Find("WeaponsBtn").GetComponent<Button>().onClick.AddListener(DisplayWeaponItems);
        btns2.Find("ObjectivesBtn").GetComponent<Button>().onClick.AddListener(DisplayObjectiveItems);
        shopCanvas.SetActive(false);

        mapCamera = playerRotation.transform.Find("MapCamera").GetComponent<Camera>();
        mapCamera.gameObject.SetActive(false);//enabled = false;
        //mapCamera.
        mapUICanvas = GameObject.Find("MapUICanvas");
        mapUICanvas.SetActive(false);
        //mapUICanvas.transform.Find("BackBtn").GetComponent<Button>().onClick.AddListener(ToggleMapUICanvas);

        buttonDisplayer = mapUICanvas.transform.Find("ButtonDisplayer").gameObject;
        displayOptions = buttonDisplayer.transform.Find("DisplayOptions").gameObject;
        defensesContainer = buttonDisplayer.transform.Find("DefensesContainer").gameObject;
        inventoryContainer = buttonDisplayer.transform.Find("InventoryContainer").gameObject;
        storeContainer = buttonDisplayer.transform.Find("StoreContainer").gameObject;
        descriptionDisplay = mapUICanvas.transform.Find("DescriptionDisplay").gameObject;
        selectedOption = displayOptions;
        defensesContainer.SetActive(false);
        inventoryContainer.SetActive(false);
        storeContainer.SetActive(false);
        displayOptions.transform.Find("DisplayDefensesBtn").GetComponent<Button>().onClick.AddListener(DisplayDefensesOptions);
        //displayOptions.transform.Find("DisplayInventoryBtn").GetComponent<Button>().onClick.AddListener(DisplayInventoryOptions);
        displayOptions.transform.Find("DisplayStoreBtn").GetComponent<Button>().onClick.AddListener(DisplayStoreOptions);
        mapUICanvas.transform.Find("BackBtn").GetComponent<Button>().onClick.AddListener(ShowDisplayOptions);

        quickAccessCanvas = GameObject.Find("QuickAccessCanvas");
        quickAccessUpgradeDescription = quickAccessCanvas.transform.Find("DescriptionDisplay").Find("Inventory Descriptions").Find("Upgrade Descriptions").gameObject;
        quickAccessUpgradeDescription.SetActive(false);
        changeArrowBtn = quickAccessCanvas.transform.Find("ChangeArrowsBtn").gameObject;
        quickAccessCanvas.transform.Find("UpgradeWepBtn").Find("WepIcon").GetComponent<Image>().sprite = weaponItemIcons[personalData.equippedWep];
        inGameWepItemUI = quickAccessCanvas.transform.Find("InGameWepItemUI").gameObject;
        quickAccessCanvas.transform.Find("ZoomBtn").gameObject.SetActive(personalData.equippedWep == 2);//.Find("WepIcon").GetComponent<Image>().sprite = weaponItemIcons[personalData.equippedWep];
        itemWheel = quickAccessCanvas.transform.Find("ItemWheel").gameObject;
        itemWheel.SetActive(false);

        itemDropdownList = quickAccessCanvas.transform.Find("ArrowItemList").Find("ItemDropdownList").Find("ItemContainer").gameObject;//GetChild(0).gameObject;

        effectsCanvas = GameObject.Find("EffectsCanvas");

        notificationCanvas = GameObject.Find("NotificationCanvas");
        notificationCanvas.transform.Find("WindowBG").Find("LeaveGameBtn").GetComponent<Button>().onClick.AddListener(LeaveGame);
        notificationCanvas.transform.Find("WindowBG").Find("LeaveGameBtn").GetComponent<Button>().onClick.AddListener(NotifyButtonPressed);
        notificationCanvas.transform.Find("WindowBG").Find("ResumeBtn").GetComponent<Button>().onClick.AddListener(ToggleNotificationCanvas);
        notificationCanvas.transform.Find("WindowBG").Find("ResumeBtn").GetComponent<Button>().onClick.AddListener(ToggleOptionsCanvas);
        notificationCanvas.transform.Find("WindowBG").Find("ResumeBtn").GetComponent<Button>().onClick.AddListener(NotifyButtonPressed);
        notificationCanvas.SetActive(false);

        myProjectiles.Clear();
        //myAttributes.Clear();

        //Attribute.names = new string[attributePrefabs.Length];

        // Add all attributes to inventory and instantiate them
        for(int i = 0; i < Projectile.projectileStats.Length; i++)
        {
            //Attribute.names[i] = "Attr " + i;
            //myAttributes[i] = 1;
            GameObject icon = Instantiate(iconPrefab);
            /*
            GameObject border = new GameObject("Border");
            border.AddComponent<RawImage>();
            border.GetComponent<RawImage>().texture = borderIcons[1];
            border.GetComponent<RawImage>().color = borderColor;
            border.GetComponent<RectTransform>().sizeDelta = new Vector2(105, 105);// = 105; border.GetComponent<RectTransform>().rect.height = 105;
            border.transform.SetParent(icon.transform);
            border.transform.localPosition = Vector2.zero;
            */
            icon.GetComponent<Selector>().id = i;
            //csf.AddItem(icon);
            arrowUIItems[i] = icon;
            
            icon.transform.SetParent(itemDropdownList.transform);
            icon.name = "Attribute " + i;
            icon.tag = "QuickAccess";
            icon.transform.localScale = new Vector3(1, 1, 1);
            icon.transform.Find("ItemIcon").GetComponent<Image>().sprite = arrowItemIcons[i];
            icon.transform.Find("InfinityIcon").gameObject.SetActive(i == 0);
            icon.transform.Find("QtyTxt").gameObject.SetActive(i != 0);
            icon.transform.Find("Selected BG").gameObject.SetActive(i == 0);
        }
        itemDropdownList.transform.parent.parent.gameObject.SetActive(false);

        trapSpawnPrefab = new GameObject[trapPrefabs.Length];
        Transform trapDescriptions = descriptionDisplay.transform.Find("Trap Descriptions");
        RectTransform rt;
        myTraps.Clear();
        for(int i = 0; i < trapPrefabs.Length; i++)
        {
            // Create buy icon for each trap
            GameObject btn = Instantiate(buttonPrefab);
            btn.name = "Trap " + i;
            btn.tag = "Buy";
            btn.transform.GetChild(0).GetComponent<Text>().text = Trap.names[i];
            rt = btn.transform.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(100, 100);
            btn.transform.SetParent(storeContainer.transform);

            // Create available defense icon for each trap
            btn = Instantiate(buttonPrefab);
            btn.name = "Trap " + i;
            btn.tag = "Inventory";
            btn.transform.GetChild(0).GetComponent<Text>().text = Trap.names[i] + " x0";
            rt = btn.transform.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(100, 100);
            btn.transform.SetParent(defensesContainer.transform);
            btn.SetActive(false);
            // Default have no traps
            myTraps.Add(i, 0);

            // Description for buy section
            GameObject descrip = Instantiate(descriptionPrefab);
            descrip.transform.GetChild(0).GetComponent<Text>().text = "Trap " + i + "\nDescription Goes Here";
            descrip.transform.SetParent(trapDescriptions.Find("Buy Descriptions"));
            descrip.transform.localPosition = Vector3.zero;
            descrip.SetActive(false);

            // Description for inventory purpose
            descrip = Instantiate(descriptionPrefab);
            descrip.transform.GetChild(0).GetComponent<Text>().text = "Trap " + i + "\nSpawn this onto map";
            descrip.transform.SetParent(trapDescriptions.Find("AddToMap Descriptions"));
            descrip.transform.localPosition = Vector3.zero;
            descrip.SetActive(false);
            // Create trap spawn object
            trapSpawnPrefab[i] = Instantiate(trapPrefabs[i]);
            trapSpawnPrefab[i].SetActive(false);
            trapSpawnPrefab[i].name = "Trap " + i;
            trapSpawnPrefab[i].layer = LayerMask.NameToLayer("Ignore Raycast");
            Collider c = trapSpawnPrefab[i].transform.GetComponent<Collider>();
            c.isTrigger = true;
            if (c.GetType() == typeof(CapsuleCollider))
                ((CapsuleCollider)c).center += new Vector3(0, -.75f, 0);
            //btn.transform.GetComponent<Button>().onClick.AddListener(Purchase);
        }
        GameObject buyBtn = Instantiate(buttonPrefab);
        buyBtn.name = "BuyBtn";
        buyBtn.transform.GetChild(0).GetComponent<Text>().text = "Buy\n$50";
        rt = buyBtn.transform.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100, 100);
        buyBtn.transform.GetComponent<Button>().onClick.AddListener(Purchase);
        buyBtn.transform.SetParent(trapDescriptions.Find("Buy Descriptions"));
        buyBtn.transform.localPosition = new Vector3(200, 0, 0);
        trapDescriptions.gameObject.SetActive(false);

        addToMapBtn = Instantiate(buttonPrefab);
        addToMapBtn.name = "AddToMapBtn";
        addToMapBtn.tag = "PlaceDefense";
        addToMapBtn.transform.GetChild(0).GetComponent<Text>().text = "Add To Map";
        rt = addToMapBtn.transform.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100, 100);
        //addToMapBtn.transform.GetComponent<Button>().onClick.AddListener(SpawnPotentialDefense);
        addToMapBtn.transform.SetParent(trapDescriptions.Find("AddToMap Descriptions"));
        addToMapBtn.transform.localPosition = new Vector3(200, 0, 0);

        StartCoroutine(MapManager.mapManager.LoadGameScene());
        StartCoroutine(NetworkManager.nm.LoadGameScene());
        //arrowQty.arr[3] = 9999;
        print("starting game");
        StartGame();
        // Create available defense icon for each trap
        GameObject b = Instantiate(buttonPrefab);
        b.name = "Inventory " + 0;
        b.tag = "Inventory";
        b.transform.GetChild(0).GetComponent<Text>().text = "Bow";
        rt = b.transform.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100, 100);
        b.transform.SetParent(inventoryContainer.transform);
        //b.transform.localPosition = new Vector3(0,0,0);
        b.SetActive(true);
    }

    public void UpgradeWeapon(int wepID)
    {
        int cost = GetWeaponPermanentUpgradeCost(wepID);
        if(personalData.playerCurrency >= cost)
        {
            print("can upgrade");
            personalData.upgradedLevelsByWeapon[wepID]++;
            Transform upgradeBtn = inventoryCanvas.transform.Find("ItemUIPanel").Find("WeaponsUIContainer").GetChild(wepID).Find("WepActionButtons").Find("UpgradeBtn");
            if (HasWeaponReachedMaxLevel(wepID))
            {
                upgradeBtn.Find("Text").GetComponent<Text>().text = "Upgrade\nMAXED";
                upgradeBtn.Find("Currency").gameObject.SetActive(false);
            }
            else
            {
                upgradeBtn.Find("Currency").Find("Text").GetComponent<Text>().text = "" + GetWeaponPermanentUpgradeCost(wepID);
            }
            upgradeBtn.parent.parent.Find("ItemName").GetComponent<Text>().text = Weapon.weaponStats[wepID].name + " Lvl " + (1+personalData.upgradedLevelsByWeapon[wepID]);
            UpdatePlayerCurrency(-cost);
            StartCoroutine(PlaySFX(purchaseSFX));
        }
        else
        {
            print("CANT UPGRADE WEP");
        }
    }
    
    // Returns a multiple amount needed to upgrade in game session
    public int GetWeaponPermanentUpgradeCost(int wepID)
    {
        if (HasWeaponReachedMaxLevel(wepID))
            return -1;
        return Weapon.weaponStats[wepID].costToUpgrade[personalData.upgradedLevelsByWeapon[wepID]] * 10;
    }

    // Returns whether weapon has been max upgraded
    public bool HasWeaponReachedMaxLevel(int wepID)
    {
        print(wepID + " " + personalData.upgradedLevelsByWeapon[wepID] + " " + Weapon.weaponStats[wepID].costToUpgrade.Length);
        return personalData.upgradedLevelsByWeapon[wepID] >= Weapon.weaponStats[wepID].costToUpgrade.Length;
    }

    public void NotifyButtonPressed()
    {
        //StartCoroutine(PlaySFX(btnClickSFX));
    }

    public void ToggleGyro()
    {
        bool useGyro = player.GetComponent<PlayerController>().ToggleGyro();// = !player.GetComponent<PlayerController>().useGyro;

        optionsCanvas.transform.Find("SettingsPanel").Find("EnableGyroBtn").Find("Text").GetComponent<Text>().text = "Enable Gyro: " + ((useGyro) ? "ON" : "OFF");
        optionsCanvas.transform.Find("SettingsPanel").Find("SetOrientationBtn").gameObject.SetActive(useGyro);
    }

    public void ToggleLeaveGameNotification()
    {
        Transform leaveGameNotif = optionsPanel.transform.Find("LeaveGameNotification");//.GetComponent<Button>().onClick.AddListener(ToggleInGameSettings);
        leaveGameNotif.gameObject.SetActive(!leaveGameNotif.gameObject.activeSelf);//.GetComponent<Button>().onClick.AddListener(ToggleInGameSettings);
    }

    public void SetPlayerOrientation()
    {
        playerOrientation = new Vector3(0, player.transform.GetComponent<PlayerController>().playerCam.transform.eulerAngles.y, 0);// playerRotation.transform.eulerAngles;
        print(player.transform.GetComponent<PlayerController>().playerCam.transform.eulerAngles.y);
        print("ORIENTATIONT  " + playerOrientation);

        playerOrientation = player.transform.GetComponent<PlayerController>().SetOrientation(playerOrientation);//.playerCam.transform.parent.eulerAngles = -playerOrientation;
        print("player is now:" + player.transform.eulerAngles);
        
    }

    public void NotifyContinuedSessionExists()
    {
        notificationCanvas.SetActive(true);
        notificationCanvas.transform.Find("WarningUI").gameObject.SetActive(true);
        notificationCanvas.transform.Find("NotificationUI").gameObject.SetActive(false);
    }

    public void NotifyQuestCompleted()
    {
        notificationCanvas.SetActive(true);
        //ToggleNotificationCanvas();
        notificationCanvas.transform.Find("WarningUI").gameObject.SetActive(false);
        notificationCanvas.transform.Find("NotificationUI").gameObject.SetActive(true);
    }

    public void ToggleNotificationCanvas()
    {
        notificationCanvas.SetActive(!notificationCanvas.activeSelf);
        NotifyButtonPressed();
    }

    public bool NotificationIsActive()
    {
        return notificationCanvas.activeSelf;
    }

    // Looks for any quests that are recently compeleted and notifies player of rewards
    public IEnumerator CheckForQuestCompletions()
    {
        List<int> questsCompleted = new List<int>(); // list of quests recently completed by achievementID
        Transform notification = notificationCanvas.transform.Find("NotificationUI");

        // get list of quests completed for first time
        for (int i = 0; i < Achievement.conditionalAchievements.Length; i++)
        {
            // if just completed, keep track of it to not notify another time and add to list to notify player
            if (!personalData.isAchievementUnlocked[i] && Achievement.CompletedQuest(i))
            {
                personalData.isAchievementUnlocked[i] = true;
                questsCompleted.Add(i);
            }
        }

        // notify player of each quest completed one at a time
        for(int i = 0; i < questsCompleted.Count; i++)
        {
            print("come");
            NotifyQuestCompleted();
            //notificationCanvas.SetActive(true);
            notification.Find("HeaderTxt").GetComponent<Text>().text = Achievement.conditionalAchievements[questsCompleted[i]].header;
            notification.Find("ProgressTxt").GetComponent<Text>().text = Achievement.GetAchievementDetails(AchievementType.Conditional, questsCompleted[i]);
            notification.Find("RewardImage").GetComponent<Image>().sprite = Achievement.GetRewardIcon(questsCompleted[i]);
            yield return new WaitWhile(NotificationIsActive);
        }

        // save state if any quests were completed
        if(questsCompleted.Count> 0)
            Save("setupMain");
    }

    public void UpdatePlayerCurrency(int amt)
    {
        personalData.playerCurrency += amt;
        Save("setupMain");
        inventoryCanvas.transform.Find("PlayerCurrency").GetChild(0).GetComponent<Text>().text = "" + personalData.playerCurrency;
        UpdateItemPurchasability();
    }

    public void UpdateItemPurchasability()
    {
        if (!inGame)
        {
            Transform container = inventoryCanvas.transform.Find("ItemUIPanel").Find("ArrowsUIContainer");
            for (int i = 1; i < Projectile.projectileStats.Length; i++)
            {
                container.GetChild(i).Find("ActionBtn").GetComponent<Button>().interactable = personalData.playerCurrency >= Projectile.projectileStats[i].price;
            }
            container = inventoryCanvas.transform.Find("ItemUIPanel").Find("WeaponsUIContainer");
            for (int i = 1; i < Weapon.weaponStats.Length; i++)
            {
                container.GetChild(i).Find("ActionBtn").GetComponent<Button>().interactable =
                    Weapon.weaponStats[i].unlockCondition != UnlockCondition.QuestThenPurchase ||
                    personalData.playerCurrency >= Weapon.weaponStats[i].price;
                Transform wepActionBtns = container.GetChild(i).Find("WepActionButtons");
                print(i + " " + HasWeaponReachedMaxLevel(i));
                wepActionBtns.Find("UpgradeBtn").GetComponent<Button>().interactable =
                    !HasWeaponReachedMaxLevel(i) &&
                    personalData.playerCurrency >= Weapon.weaponStats[i].costToUpgrade[personalData.upgradedLevelsByWeapon[i]]*10;
            }
        }
    }

    public void UpdateItem(string itemType, int itemID, int qty)
    {
        if(itemType == "Attribute")
        {
            //data.arrowQuantities[itemID] += qty;
            //print(itemID);
            //print(personalData.arrowQuantities.Length);
            //print(Projectile.projectileStats.Length);
            //personalData.arrowQuantities[itemID] += qty;
            arrowQty.arr[itemID] += qty;
            //myAttributes[itemID] += qty;
            //print("CT " + personalData.arrowQuantities[itemID]);
            UpdateArrowQty(itemID);
            //itemWheel.GetComponent<Rotator>().ResetItemWheel(selectedAttribute);
        }
        UpdateItemPurchasability();
    }

    public void UseItem(string itemType, int itemID)
    {
        if(itemType == "Attribute")
        {
            data.arrowsShotByAttribute[itemID]++;
            if (itemID <= 0)
                return;
            //myAttributes[itemID]--;
            arrowQty.arr[itemID]--;
            //personalData.arrowQuantities[itemID]--;
            //print(itemID + ":" + myAttributes[itemID]);
            if(arrowQty.arr[itemID] <= 0)//personalData.arrowQuantities[itemID] <= 0)//myAttributes[itemID] <= 0)
            {
                arrowQty.arr[itemID] = 0;
                print("OUT OF ITEM");
                ChangeSelectedAttribute(0);
                //itemWheel.GetComponent<Rotator>().ResetItemWheel(0);
            }
            UpdateArrowQty(itemID);
                //itemWheel.GetComponent<Rotator>().UpdateItemUI();
            
        }
    }

    public int GetNextItem(int index)
    {
        for(int i = 1; i < Projectile.projectileStats.Length; i++)
        {
            int nextIndex = (index + i) % Projectile.projectileStats.Length;
            if(arrowQty.arr[index] > 0 || nextIndex == 0) //myAttributes[nextIndex] > 0 || nextIndex == 0)
            {
                index = nextIndex;
                break;
            }
        }
        return index;
    }

    public int GetPrevItem(int index)
    {
        for (int i = 1; i < Projectile.projectileStats.Length; i++)
        {
            int prevIndex = (index - i + Projectile.projectileStats.Length) % Projectile.projectileStats.Length;
            if (arrowQty.arr[index] > 0 || prevIndex == 0)
            {
                index = prevIndex;
                break;
            }
        }
        return index;
    }

    // Updates the visual indication of arrow amount
    public void UpdateArrowQty(int itemID)
    {
        Save("arrowQuantity");
        if (!inGame)
        {
            print("?");
            inventoryItemPanel.transform.Find("ArrowsUIContainer").GetChild(itemID).Find("QtyTxt").GetComponent<Text>().text = "Quantity: " + arrowQty.arr[itemID];// personalData.arrowQuantities[itemID];
            return;
        }
        if (itemID == selectedAttribute)
        {
            changeArrowBtn.transform.Find("ArrowIcon").GetComponent<Image>().sprite = arrowItemIcons[itemID];
            changeArrowBtn.transform.Find("QtyTxt").GetComponent<Text>().text = "";
            if (itemID == 0)
            {
                changeArrowBtn.transform.Find("QtyTxt").gameObject.SetActive(false);
                changeArrowBtn.transform.Find("InfinityIcon").gameObject.SetActive(true);
            }
            else
            {

                changeArrowBtn.transform.Find("QtyTxt").gameObject.SetActive(true);
                changeArrowBtn.transform.Find("InfinityIcon").gameObject.SetActive(false);
                changeArrowBtn.transform.Find("QtyTxt").GetComponent<Text>().text += arrowQty.arr[itemID];// personalData.arrowQuantities[itemID];//myAttributes[itemID];
            }
        }
        itemDropdownList.transform.GetChild(itemID).Find("QtyTxt").GetComponent<Text>().text = "" + ((itemID == 0) ? "---" : "" + arrowQty.arr[itemID]);// personalData.arrowQuantities[itemID]);
        bool isEmpty = arrowQty.arr[itemID] <= 0 && itemID != 0;
        print(isEmpty+" "+itemID) ;
        arrowUIItems[itemID].SetActive(!isEmpty);
        //itemDropdownList.transform.GetChild(itemID).gameObject.SetActive(!isEmpty);
        //itemDropdownList.GetComponent<ContentSizeFitter>().SetItemActive(itemID, !isEmpty);
        
    }

    public void ChangeSelectedAttribute(int attributeID)
    {
        if (attributeID != selectedAttribute)
        {
            //return;
            NotifyButtonPressed();
            itemDropdownList.transform.GetChild(selectedAttribute).GetComponent<Selector>().SetSelected(false);
            itemDropdownList.transform.GetChild(attributeID).GetComponent<Selector>().SetSelected(true);
            selectedAttribute = attributeID;
            player.GetComponent<PlayerController>().SetAttribute(attributeID);//.wep.ChangeAttribute();
            UpdateArrowQty(attributeID);
            if (NetworkManager.nm.isStarted)
            {
                NetworkManager.nm.NotifyPlayerChangedArrowAttribute(attributeID);
            }
        }
        //itemDropdownList.GetComponent<Slider>().SlideInDirection(0);
        //ToggleArrowQtyList();

        itemDropdownList.transform.parent.parent.gameObject.SetActive(false);
    }

    public void OnHitEnemy()
    {
        Color c = hitIndicator.GetComponent<Image>().color;
        c.a = 1;
        hitIndicator.GetComponent<Image>().color = c;
        audioSrc.clip = onEnemyHitSFX;
        audioSrc.time = .01f;
        audioSrc.Play();
    }

    public void SpawnPotentialDefense()
    {
    }

    public void DeselectDescription()
    {
        if (selectedDescription)
        {
            selectedDescription.SetActive(false);
            selectedDescription.transform.parent.gameObject.SetActive(false);
            selectedDescription.transform.parent.parent.gameObject.SetActive(false);
        }
        selectedDescription = null;
    }

    public void HandleWeaponItemAction(int wepID)
    {
        print("HANDLING " + wepID);
        switch (Weapon.weaponStats[wepID].unlockCondition)
        {
            case UnlockCondition.Free:
                if (wepID != personalData.equippedWep)
                {
                    EquipWeapon(wepID);
                }
                break;
            case UnlockCondition.Purchase:
                if (personalData.isWeaponPurchased[wepID])
                {
                    EquipWeapon(wepID);
                }
                else
                {
                    BuyWeapon(wepID);
                }
                break;
            case UnlockCondition.Quest:
                if (wepID != personalData.equippedWep)
                {
                    EquipWeapon(wepID);
                }
                break;
            case UnlockCondition.QuestThenPurchase:
                if (personalData.isWeaponPurchased[wepID])
                {
                    EquipWeapon(wepID);
                }
                else
                {
                    BuyWeapon(wepID);
                }
                break;
        }
    }

    // Notify player action is invalid
    public void NotifyInvalid()
    {
        StartCoroutine(PlaySFX(invalidSFX));
    }

    // Notfiy player transaction is successful
    public void NotifyTransactionSuccess()
    {
        StartCoroutine(PlaySFX(purchaseSFX));
    }

    public void EquipWeapon(int wepID)
    {
        print("EQUOP");
        print(wepID);
        print(personalData.equippedWep);
        Transform wepUIContainer = inventoryItemPanel.transform.Find("WeaponsUIContainer");
        wepUIContainer.GetChild(personalData.equippedWep).Find("WepActionButtons").Find("ActionBtn").Find("Text").GetComponent<Text>().text = "Equip";
        wepUIContainer.GetChild(personalData.equippedWep).Find("WepActionButtons").Find("ActionBtn").GetComponent<Button>().interactable = true;
        wepUIContainer.GetChild(wepID).Find("WepActionButtons").Find("ActionBtn").Find("Text").GetComponent<Text>().text = "Equipped";
        personalData.equippedWep = wepID;
        wepUIContainer.GetChild(personalData.equippedWep).Find("WepActionButtons").Find("ActionBtn").GetComponent<Button>().interactable = false;

        StartCoroutine(PlaySFX(equipSFX));
        //audioSrc.clip = equipSFX;
        //audioSrc.Play();
        Save("setupMain");
    }

    public void BuyWeapon(int wepID)
    {
        if(Weapon.weaponStats[wepID].price > personalData.playerCurrency)
        {
            print("cant buy wep");
            StartCoroutine(PlaySFX(invalidSFX));
            //audioSrc.clip = invalidSFX;
            //audioSrc.Play();
            return;
        }
        UpdatePlayerCurrency(Weapon.weaponStats[wepID].price);
        //personalData.playerCurrency -= Weapon.weaponStats[wepID].price;
        inventoryItemPanel.transform.Find("WeaponsUIContainer").GetChild(wepID).Find("ActionBtn").gameObject.SetActive(false);
        inventoryItemPanel.transform.Find("WeaponsUIContainer").GetChild(wepID).Find("WepActionButtons").gameObject.SetActive(true);// Find("Currency").gameObject.SetActive(false);
        personalData.isWeaponPurchased[wepID] = true;
        UpdateItem("", 0,0);
        print("BOUGHT");
        EquipWeapon(wepID);
        StartCoroutine(PlaySFX(purchaseSFX));
        //audioSrc.clip = purchaseSFX;
        //audioSrc.Play();
        //personalData.equippedWep = wepID;
    }

    public void ChangeSelectedTab(string selectedTab)
    {
        NotifyButtonPressed();
        GameObject canvas = inventoryCanvas;
        if (gm.selectedTab == "Records" || gm.selectedTab == "Quests")
        {
            canvas = achievementsCanvas;
        }
        inventoryItemPanel = canvas.transform.Find("ItemUIPanel").gameObject;
        canvas.transform.Find("ButtonsContainer").Find(gm.selectedTab + "TabBtn").GetComponent<Button>().interactable = true;
        inventoryItemPanel.transform.Find(gm.selectedTab + "UIContainer").gameObject.SetActive(false);
        if (selectedTab == "Records" || selectedTab == "Quests")
        {
            canvas = achievementsCanvas;
        }
        else
        {
            canvas = inventoryCanvas;
        }
        inventoryItemPanel = canvas.transform.Find("ItemUIPanel").gameObject;
        inventoryItemPanel.transform.Find(selectedTab + "UIContainer").gameObject.SetActive(true);
        canvas.transform.Find("ButtonsContainer").Find(selectedTab + "TabBtn").GetComponent<Button>().interactable = false;
        gm.selectedTab = selectedTab;
        inventoryItemPanel.GetComponent<ScrollRect>().content = inventoryItemPanel.transform.Find(selectedTab + "UIContainer").GetComponent<RectTransform>();
        inventoryItemPanel.GetComponent<ScrollRect>().velocity = Vector2.zero;
        inventoryItemPanel.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;

    }

    public void HandleArrowItemAction(int aID)
    {
        print("handle arrow " + aID);
        switch (Projectile.projectileStats[aID].unlockCondition)
        {
            case UnlockCondition.Purchase:
                BuyArrow(aID);
                break;
            case UnlockCondition.QuestThenPurchase:
                BuyArrow(aID);
                break;
        }
    }

    public IEnumerator PlaySFX(AudioClip clip)
    {
        AudioSource audio = gameObject.AddComponent<AudioSource>();
        if (inGame)
        {
            inGameAudio.Add(audio);
        }
        else
        {
            outOfGameAudio.Add(audio);
        }
        audio.clip = clip;
        //if (clip == btnClickSFX)
        //    audio.volume = .5f;
        audio.Play();
        yield return new WaitWhile(() => audio != null && audio.isPlaying);//2);//IsClipFinished(audio));
        if (audio)
        {
            if (inGame)
            {
                inGameAudio.Remove(audio);
            }
            else
            {
                outOfGameAudio.Remove(audio);
            }
            Destroy(audio);
        }
        //yield return new WaitForSeconds(0);
        //Destroy(audio,audio.clip.length);
    }

    public void BuyArrow(int aID)
    {
        if(Projectile.projectileStats[aID].price > personalData.playerCurrency)
        {
            print("CANT BUY");
            audioSrc.clip = invalidSFX;
            audioSrc.Play();
            return;
        }
        print("BOUGHT");
        UpdatePlayerCurrency(-Projectile.projectileStats[aID].price);
        //personalData.playerCurrency -= Projectile.projectileStats[aID].price;
        UpdateItem("Attribute", aID, Projectile.projectileStats[aID].purchaseQty);
        
        //audioSrc.clip = purchaseSFX;
        //audioSrc.Play();
        StartCoroutine(PlaySFX(purchaseSFX));
        //UpdateArrowQty(aID);
    }

    public void Purchase()
    {
        Debug.Log("BUY" + selectedItem.name);
        if (selectedItem == null)
            return;
        string[] details = selectedItem.name.Split(' ');
        if (details.Length < 2)
            return;
        string itemType = details[0];
        int id = int.Parse(details[1]);
        switch (itemType)
        {
            case "Trap":
                Debug.Log("Buying trap");
                if (data.inGameCurrency < Trap.costs[id])
                    return;
                UpdateInGameCurrency(-Trap.costs[id]);
                //inGameCurrency -= Trap.costs[id];
                myTraps[id] += 1;
                defensesContainer.transform.GetChild(id).GetChild(0).GetComponent<Text>().text = Trap.names[id] + " x" + myTraps[id];
                defensesContainer.transform.GetChild(id).gameObject.SetActive(true);
                Debug.Log("BOUGHT");
                break;
        }
    }

    public bool BlackoutFaded()
    {
        Transform blackout = effectsCanvas.transform.Find("Blackout Fader");
        Color c = blackout.GetComponent<Image>().color;
        c.a -= 1.0f/255.0f;
        if (c.a <= 0)
            c.a = 0;
        blackout.GetComponent<Image>().color = c;
        return c.a <= 0;
    }

    public IEnumerator FadeBlackout()
    {
        isBlackingOut = true;
        yield return new WaitForSeconds(3);
        yield return new WaitUntil(BlackoutFaded);
        isBlackingOut = true;
    }

    public void Blackout()
    {
        if (isBlackingOut)
        {
            print("stahps");
            StopCoroutine(blackoutCoroutine);
        }
        Transform blackout = effectsCanvas.transform.Find("Blackout Fader");
        Color c = blackout.GetComponent<Image>().color;
        c.a = 1;
        blackout.GetComponent<Image>().color = c;
        print("staht");
        blackoutCoroutine = StartCoroutine(FadeBlackout());
    }

    public void ToggleArrowQtyList()
    {
        //itemDropdownList.GetComponent<Slider>().ToggleSlider();
        GameObject arrowQtyList = itemDropdownList.transform.parent.parent.gameObject;
        NotifyButtonPressed();
        arrowQtyList.SetActive(!arrowQtyList.activeSelf);
    }


    public void DisplayEndGameNotifications(bool won)
    {
        print("ENDGAME");
        if (selectedDefense)
            selectedDefense.SetActive(false);
        // Hide in game UI
        playerStatusCanvas.transform.Find("OptionsBtn").gameObject.SetActive(false);
        playerStatusCanvas.transform.Find("HitIndicator").gameObject.SetActive(false);
        playerStatusCanvas.transform.Find("ShootBtnUI").gameObject.SetActive(false);
        playerStatusCanvas.transform.Find("StatisticsBG").gameObject.SetActive(false);
        playerStatusCanvas.transform.Find("Score").gameObject.SetActive(false);
        playerStatusCanvas.transform.Find("Kills").gameObject.SetActive(false);
        playerStatusCanvas.transform.Find("Currency").gameObject.SetActive(false);
        playerStatusCanvas.transform.Find("Wave").gameObject.SetActive(false);
        playerStatusCanvas.transform.Find("ObjectiveHitIndicator").gameObject.SetActive(false);
        interactiveUIContainer.SetActive(false);
        effectsCanvas.SetActive(false);


        playerOrientationObjects.SetActive(false);
        isSettingPlayerOrientation = false;
        optionsCanvas.SetActive(false);
        playerStatusCanvas.transform.Find("OptionsBtn").GetComponent<Button>().interactable = false;

        quickAccessCanvas.SetActive(false);

        mapUICanvas.SetActive(edittingMap);
        playerStatusCanvas.SetActive(true);
        mapFingerID = -1;
        selectedDefense = null;

        data.gameOver = true;
        resultNotification.SetActive(true);

        data.hasWon = won;


        // Show Eng Game Results Notification
        Transform stats = resultNotification.transform.Find("StatisticsMask").GetChild(0).Find("StatisticsContainer");
        if (won)
        {
            StartCoroutine(PlaySFX(victorySFX));
            stats.parent.Find("WinOrLoseTxt").GetComponent<Text>().text = "VICTORY";
        }
        else
        {
            StartCoroutine(PlaySFX(defeatSFX));
            stats.parent.Find("WinOrLoseTxt").GetComponent<Text>().text = "Defeat...";
        }
        stats.Find("Kills").Find("Stats").GetComponent<Text>().text = "" + data.totalKills;// (personalKills + totalPersonalKills);
        int weakspotCt = 0;
        for(int i = 0; i < data.weakSpotsHitByEnemy.Length; i++)
        {
            weakspotCt += data.weakSpotsHitByEnemy[i];
        }
        stats.Find("WeakSpotsHit").Find("Stats").GetComponent<Text>().text = "" + weakspotCt;
        //if(pc.shotsHit > 0 && pc.wep.GetComponent<Weapon>().shotCount > 0)
        //    stats.Find("Accuracy").Find("Stats").GetComponent<Text>().text = "" + ((float) pc.shotsHit / pc.wep.GetComponent<Weapon>().shotCount * 100).ToString("#.0") + "%";
        stats.Find("WavesSurvived").Find("Stats").GetComponent<Text>().text = "" + (data.wave);

        stats.Find("OverallScore").Find("Stats").GetComponent<Text>().text = "" + data.score;

        Achievement.CheckForAchievementProgress();
        int earnings = data.score / 1000 + data.inGameCurrency / 20 + (won ? 30 : 0);
        stats.Find("Earnings").Find("Stats").GetComponent<Text>().text = "" + earnings;
        personalData.playerCurrency += earnings;
        firework.SetActive(won);
        Save("setupMain");
    }

    public bool FrostBorderIsVisible()
    {
        Transform frostBorder = effectsCanvas.transform.Find("Frost Border");
        Color c = frostBorder.GetComponent<Image>().color;
        c.a += .05f;
        frostBorder.GetComponent<Image>().color = c;
        return c.a >= .83f;
    }

    public bool FrostBorderIsInvisible()
    {
        Transform frostBorder = effectsCanvas.transform.Find("Frost Border");
        Color c = frostBorder.GetComponent<Image>().color;
        c.a -= .04f;
        if (c.a <= 0f)
            c.a = 0f;
        frostBorder.GetComponent<Image>().color = c;
        return c.a <= 0f;
    }

    public IEnumerator ShowFrostBorder()
    {
        yield return new WaitUntil(FrostBorderIsVisible);
    }

    public IEnumerator HideFrostBorder()
    {
        yield return new WaitUntil(FrostBorderIsInvisible);
    }

    public void FreezePlayer()
    {
        if (frostBorderCoroutine != null)
            StopCoroutine(frostBorderCoroutine);
        StartCoroutine(PlaySFX(playerFreezeSFX));
        playerStatusCanvas.transform.Find("Tap Action").gameObject.SetActive(true);
        frostBorderCoroutine = StartCoroutine(ShowFrostBorder());
    }

    public void UnfreezePlayer()
    {
        if (frostBorderCoroutine != null)
            StopCoroutine(frostBorderCoroutine);
        StartCoroutine(PlaySFX(playerUnfreezeSFX));
        playerStatusCanvas.transform.Find("Tap Action").gameObject.SetActive(false);
        frostBorderCoroutine = StartCoroutine(HideFrostBorder());
    }

    public void DisplayVictoryNotification()
    {
        if(selectedDefense)
            selectedDefense.SetActive(false);

        playerOrientationObjects.SetActive(false);
        isSettingPlayerOrientation = false;
        optionsCanvas.SetActive(false);
        playerStatusCanvas.transform.Find("OptionsBtn").GetComponent<Button>().interactable = false;

        mapUICanvas.SetActive(edittingMap);
        playerStatusCanvas.SetActive(true);
        mapFingerID = -1;
        selectedDefense = null;

        data.gameOver = true;
        data.hasWon = true;
        resultNotification.SetActive(true);
        resultNotification.transform.Find("ResultTxt").GetComponent<Text>().text = "VICTORY!\nYou have successfully\ndefended the kingdom!";
    }

    public void DisplayDefeatNotification()
    {
        if(selectedDefense)
            selectedDefense.SetActive(false);

        playerOrientationObjects.SetActive(false);
        isSettingPlayerOrientation = false;
        optionsCanvas.SetActive(false);
        playerStatusCanvas.transform.Find("OptionsBtn").GetComponent<Button>().interactable = false;
        mapUICanvas.SetActive(edittingMap);
        playerStatusCanvas.SetActive(true);
        mapFingerID = -1;
        selectedDefense = null;

        data.gameOver = true;
        data.hasWon = false;
        resultNotification.SetActive(true);
        resultNotification.transform.Find("ResultTxt").GetComponent<Text>().text = "Oh No! The enemies broke\nthrough our defenses!";
    }

    public void DisplayDefensesOptions()
    {
        selectedOption.SetActive(false);
        selectedOption = defensesContainer;
        buttonDisplayer.transform.GetComponent<ScrollRect>().content = selectedOption.transform.GetComponent<RectTransform>();
        selectedOption.SetActive(true);
    }

    public void DisplayStoreOptions()
    {
        selectedOption.SetActive(false);
        selectedOption = storeContainer;
        buttonDisplayer.transform.GetComponent<ScrollRect>().content = selectedOption.transform.GetComponent<RectTransform>();
        selectedOption.SetActive(true);
    }

    public void DisplayInventoryOptions()
    {
        selectedOption.SetActive(false);
        selectedOption = inventoryContainer;
        buttonDisplayer.transform.GetComponent<ScrollRect>().content = selectedOption.transform.GetComponent<RectTransform>();
        selectedOption.SetActive(true);
    }

    public void ShowDisplayOptions()
    {
        DeselectDescription();
        selectedItem = null;
        selectedDefense = null;
        selectedOption.SetActive(false);
        selectedOption = displayOptions;
        selectedOption.SetActive(true);
    }

    public void ToggleAchievementsCanvas()
    {
        achievementsCanvas.SetActive(!achievementsCanvas.activeSelf);
        if (achievementsCanvas.activeSelf)
            GameObject.Find("BackgroundCanvas").transform.Find("Header").Find("Text").GetComponent<Text>().text = "Achievements";
        //selectedTab = "Scores";
        ChangeSelectedTab("Records");
    }

    public void TogglePlayerOrientationSetup()
    {
        isSettingPlayerOrientation = !isSettingPlayerOrientation;
        optionsCanvas.SetActive(!isSettingPlayerOrientation);
        playerOrientationObjects.SetActive(isSettingPlayerOrientation);
    }

    public void ToggleInGameSettings()
    {
        optionsPanel.SetActive(!optionsPanel.activeSelf);
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void ToggleInventoryCanvas()
    {
        inventoryCanvas.SetActive(!inventoryCanvas.activeSelf);
        if (inventoryCanvas.activeSelf)
            GameObject.Find("BackgroundCanvas").transform.Find("Header").Find("Text").GetComponent<Text>().text = "Inventory";
        //selectedTab = "Weapons";
        ChangeSelectedTab("Weapons");
    }

    public void ToggleMapUICanvas()
    {
        // Don't go to map if performing quick access methods
        if (quickAccessFingerID != -1)
            return;
        // Can't perform if setting up orientation
        if (isSettingPlayerOrientation)
            return;
        mapFingerID = -1;
        edittingMap = !edittingMap;
        ShowDisplayOptions();
        //selectedOption = null;
        mapUICanvas.SetActive(edittingMap);
        mapCamera.gameObject.SetActive(edittingMap);//enabled = edittingMap;
        quickAccessCanvas.SetActive(!edittingMap);
        player.GetComponent<PlayerController>().playerCam.gameObject.SetActive(!edittingMap);//.enabled = !edittingMap;
        playerStatusCanvas.transform.Find("ShootBtnMask(Clone)").gameObject.SetActive(!edittingMap);
        mapCamera.transform.GetComponent<MapViewCamera>().Reset();
        settingsPanel.transform.Find("SetOrientationBtn").GetComponent<Button>().interactable = !edittingMap;
    }

    public void ToggleSettingsCanvas()
    {
        settingsCanvas.SetActive(!settingsCanvas.activeSelf);
        if (settingsCanvas.activeSelf)
            GameObject.Find("BackgroundCanvas").transform.Find("Header").Find("Text").GetComponent<Text>().text = "Settings";
    }

    public void ToggleDebugging()
    {
        Debugging = !Debugging;
        string isOn = "ON";
        if (!Debugging)
            isOn = "OFF";
        DebugManager.dbm.SetDebugMode(Debugging);
        optionsCanvas.transform.Find("SettingsPanel").Find("ShowFPSBtn").GetChild(0).GetComponent<Text>().text = "Show FPS: " + isOn;
    }

    public void ToggleInteractiveTouch()
    {
        interactiveTouch = !interactiveTouch;
        string isOn = "ON";
        if (!interactiveTouch)
            isOn = "OFF";
        if (!inGame)
            settingsCanvas.transform.Find("ButtonsContainer").Find("InteractiveTouchBtn").GetChild(0).GetComponent<Text>().text = "Interactive Touch: " + isOn;
        else
        {
            player.GetComponent<PlayerController>().wep.SetTouchInteraction(interactiveTouch);
            settingsPanel.transform.Find("InteractiveTouchBtn").GetChild(0).GetComponent<Text>().text = "Interactive Touch: " + isOn;
        }
    }

    public void ToggleRealTimeAction()
    {
        realTimeAction = !realTimeAction;
        string isOn = "ON";
        if (!realTimeAction)
            isOn = "OFF";
        settingsCanvas.transform.Find("ButtonsContainer").Find("LiveActionBtn").GetChild(0).GetComponent<Text>().text = "Real Time Action: " + isOn;
    }

    public void ToggleShopCanvas()
    {
        shopCanvas.SetActive(!shopCanvas.activeSelf);
        shopType = "Store";
        itemType = "Weapons";
        DisplaySelectedItems();
    }

    public void DisplayStoreSelection()
    {
        shopType = "Store";
        DisplaySelectedItems();
    }

    public void DisplayUpgradeSelection()
    {
        shopType = "Upgrade";
        DisplaySelectedItems();
    }

    public void DisplayWeaponItems()
    {
        itemType = "Weapons";
        DisplaySelectedItems();
    }

    public void DisplayObjectiveItems()
    {
        itemType = "Objectives";
        DisplaySelectedItems();
    }

    // Displays items based on shopType and itemType
    public void DisplaySelectedItems()
    {
        Transform btns1 = shopCanvas.transform.Find("ButtonContainer1");
        Transform btns2 = shopCanvas.transform.Find("ButtonContainer2");
        Transform displays = shopCanvas.transform.Find("Displays");
        for (int i = 0; i < btns1.childCount; i++)
        {
            btns1.GetChild(i).GetComponent<Button>().interactable = btns1.GetChild(i).name != shopType + "Btn";
        }
        for (int i = 0; i < btns2.childCount; i++)
        {
            btns2.GetChild(i).GetComponent<Button>().interactable = btns2.GetChild(i).name != itemType + "Btn";
        }
        string displayType = shopType + itemType + "Display";
        for (int i = 0; i < displays.childCount; i++)
        { 
            displays.GetChild(i).gameObject.SetActive(displayType == displays.GetChild(i).name);
        }
    }

    public void ToggleMainMenuCanvas()
    {
        mainMenuCanvas.SetActive(!mainMenuCanvas.activeSelf);
        if (mainMenuCanvas.activeSelf)
            GameObject.Find("BackgroundCanvas").transform.Find("Header").Find("Text").GetComponent<Text>().text = "Archery Castle Defense"; //"Main Menu";
    }

    public void ToggleLobbyCanvas()
    {
        lobbyCanvas.SetActive(!lobbyCanvas.activeSelf);
        if (lobbyCanvas.activeSelf)
            GameObject.Find("BackgroundCanvas").transform.Find("Header").Find("Text").GetComponent<Text>().text = "Lobby";
    }

    public void ToggleHostListCanvas()
    {
        hostListCanvas.SetActive(!hostListCanvas.activeSelf);
        if (hostListCanvas.activeSelf)
            GameObject.Find("BackgroundCanvas").transform.Find("Header").Find("Text").GetComponent<Text>().text = "Finding a Host";
    }

    public void ToggleMultiplayerCanvas()
    {
        multiplayerCanvas.SetActive(!multiplayerCanvas.activeSelf);
        if (multiplayerCanvas.activeSelf)
            GameObject.Find("BackgroundCanvas").transform.Find("Header").Find("Text").GetComponent<Text>().text = "Multiplayer";
    }
    /*
    public void ToggleIntermissionCanvas()
    {
        intermissionCanvas.SetActive(!intermissionCanvas.activeSelf);
        print(intermissionCanvas.activeSelf);
        targetContainer.SetActive(intermissionCanvas.activeSelf);
    }
    */
    public void UpdateKillCount(int kills)
    {
        data.totalKills += kills;
        //personalKills += kills;
        playerStatusCanvas.transform.Find("Kills").Find("Text").GetComponent<Text>().text = "" + data.totalKills;//(totalPersonalKills + personalKills) + "";
    }

    public void UpdateScore(int s)
    {
        data.score += s;
        Transform scoreObj = playerStatusCanvas.transform.Find("Score");
        string scoreTxt = "";
        for(int i = 0; i < 9; i++)
        {
            scoreTxt = (data.score / (int)Mathf.Pow(10, i)) % 10 + scoreTxt;
        }
        scoreObj.Find("Text").GetComponent<Text>().text = scoreTxt;
    }

    public void SaveAndQuit()
    {
        Save("continuedGame");
        GoToMainScene();
    }

    public void NextWave()
    {
        //intermissionCanvas.SetActive(false);
        //targetContainer.SetActive(false);
        StartWave(data.wave);
    }

    public void DisplayIntermission()
    {
        intermissionCanvas.GetComponent<Canvas>().sortingOrder = playerStatusCanvas.GetComponent<Canvas>().sortingOrder + 1;

        onIntermission = true;
        startWaves = false;
        intermissionCanvas.SetActive(true);
        targetContainer.SetActive(true);
        //intermissionCanvas.transform.Find("StatsTxt").GetComponent<Text>().text = "Score: " + score + "\tKills: " + totalKills + "\nNext Wave: " + (wave + 1);
        //intermissionCanvas.transform.Find("ShopBtn").GetComponent<Button>().interactable = false;// wave % 10 == 0;
        if (NetworkManager.nm.isStarted && NetworkManager.nm.isHost)
        {
            intermissionCanvas.transform.Find("ResumeBtn").GetComponent<Button>().interactable = false;
        }
    }

    public void ToggleOptionsCanvas()
    {
        if (isSettingPlayerOrientation)
            return;

        optionsCanvas.SetActive(!optionsCanvas.activeSelf);
        if (optionsCanvas.activeSelf)
        {
            optionsCanvas.GetComponent<Canvas>().sortingOrder = playerStatusCanvas.GetComponent<Canvas>().sortingOrder + 1;
        }
        else
        {
            optionsCanvas.GetComponent<Canvas>().sortingOrder = playerStatusCanvas.GetComponent<Canvas>().sortingOrder - 1;
        }
        optionsCanvas.transform.Find("OptionsPanel").gameObject.SetActive(true);
        optionsCanvas.transform.Find("SettingsPanel").gameObject.SetActive(false);
    }

    public void DisplayOptions()
    {
        if (isSettingPlayerOrientation)
            return;
        optionsCanvas.SetActive(true);
        optionsCanvas.GetComponent<Canvas>().sortingOrder = playerStatusCanvas.GetComponent<Canvas>().sortingOrder + 1;
    }

    public void ResumeGame()
    {
        optionsCanvas.GetComponent<Canvas>().sortingOrder = playerStatusCanvas.GetComponent<Canvas>().sortingOrder - 1;
        optionsCanvas.SetActive(false);
    }

    public void LeaveGame()
    {
        StopAllCoroutines();
        //if(waveNotificationCoroutine != null)
        //    StopCoroutine(waveNotificationCoroutine);
        GoToMainScene();
    }

    public void GoToLobbyScene()
    {
        SceneManager.LoadScene("lobby");
    }

    public void GoToMainScene()
    {
        SceneManager.LoadScene("main");
    }

    public void GoToContinuedGameScene()
    {
        continuedGame = true;
        SceneManager.LoadScene("game");
    }

    public void GoToGameScene()
    {
        continuedGame = false;
        SceneManager.LoadScene("game");
    }

    public void GoToCalibrationScene()
    {
        SceneManager.LoadScene("calibration");
    }

    public void AddScore(int s)
    {
        data.score += s;
    }

    // Updates in game currency and displays the information in shop
    public void UpdateInGameCurrency(int currency)
    {
        data.inGameCurrency += currency;
        playerStatusCanvas.transform.Find("Currency").Find("Text").GetComponent<Text>().text = data.inGameCurrency + "";
        quickAccessUpgradeDescription.transform.Find("Currency").Find("Text").GetComponent<Text>().text = data.inGameCurrency + "";
        shopCanvas.transform.Find("Currency").GetChild(0).GetComponent<Text>().text = "$" + data.inGameCurrency;
        inGameWepItemUI.transform.Find("BuyBtn").GetComponent<Button>().interactable = data.wepLvl < Weapon.weaponStats[data.wepID].costToUpgrade.Length && data.inGameCurrency >= Weapon.weaponStats[data.wepID].costToUpgrade[data.wepLvl];
    }

    // Animates wave notification text to slowly appear
    bool DisplayWaveNotification()
    {
        if (!inGame)
        {
            return true;
        }
        float rate = .01f * 60f / DebugManager.dbm.fps;
        Color c = waveNotification.transform.GetComponent<Image>().color;

        c.a += rate;
        waveNotification.transform.GetComponent<Image>().color = c;


        c = waveNotification.transform.Find("WaveTxt").GetComponent<Text>().color;
        c.a += rate;
        waveNotification.transform.Find("WaveTxt").GetComponent<Text>().color = c;
        c = waveNotification.transform.Find("MonsterIcon").GetComponent<Image>().color;
        c.a += rate;
        waveNotification.transform.Find("MonsterIcon").GetComponent<Image>().color = c;

        return c.a >= 1;
    }

    // Slowly makes wave notification text disappear
    bool HideWaveNotification()
    {
        if (!inGame)
        {
            return true;
        }
        float rate = .01f * 60f /DebugManager.dbm.fps;
        Color c = waveNotification.transform.GetComponent<Image>().color;
        c.a -= rate;
        waveNotification.transform.GetComponent<Image>().color = c;

        c = waveNotification.transform.Find("WaveTxt").GetComponent<Text>().color;
        c.a -= rate;
        waveNotification.transform.Find("WaveTxt").GetComponent<Text>().color = c;
        c = waveNotification.transform.Find("MonsterIcon").GetComponent<Image>().color;
        c.a -= rate;
        waveNotification.transform.Find("MonsterIcon").GetComponent<Image>().color = c;
        return c.a <= 0;
    }

    // Displays wave notification text and then hides it, the end indicates wave starts
    IEnumerator NotifyIncomingWave(int w)
    {
        waveNotification.SetActive(true);
        //waveNotification.GetComponent<AudioSource>().enabled = true;
        waveNotification.transform.Find("WaveTxt").GetComponent<Text>().text = "Wave " + (w+1);
        yield return new WaitUntil(DisplayWaveNotification);
        yield return new WaitForSeconds(3f);
        yield return new WaitUntil(HideWaveNotification);
        //waveNotification.GetComponent<AudioSource>().enabled = false;
        startWaves = true;
        Debug.Log("Wave started");
    }

    public void ClearEnemyObjects()
    {
        for(int i = 0; i < enemiesContainer.transform.childCount; i++)
        {
            Destroy(enemiesContainer.transform.GetChild(i).gameObject);
        }
        enemies.Clear();
    }

    public void ClearProjectiles()
    {
        for (int i = 0; i < projectilesContainer.transform.childCount; i++)
        {
            Destroy(projectilesContainer.transform.GetChild(i).gameObject);
        }
    }

    public void ClearParticleEffects()
    {
        for (int i = 0; i < particleEffectsContainer.transform.childCount; i++)
        {
            Destroy(particleEffectsContainer.transform.GetChild(i).gameObject);
        }
    }

    // Set player in game information to initial values
    public void ResetPlayerStats()
    {
        data.score = 0;
        data.totalKills = 0;
        objective.transform.GetComponent<Objective>().Reset();
    }

    // Reset enemy spawn related variables to initial values for wave w
    public void ResetSpawnSetup(int w)
    {
        spawnTimer = 0;
        spawnIndex = 0;
        startWaves = false;
        intervalIndex = 0;
        data.wave = w;
        kills = 0;
        //data.totalKills = 0;
        //personalKills = 0;
        enemiesSpawned = 0;
        doneSpawningWave = false;
    }

    void HandleAdResult(ShowResult result)
    {
        switch (result)
        {
            case ShowResult.Finished:
                print("watchedfulle");
                break;
            case ShowResult.Skipped:
                print("skipp");
                break;
            case ShowResult.Failed:
                print("no connection or failed");
                break;
        }
    }

    public void ShowAd()
    {

        if (Advertisement.IsReady())
            Advertisement.Show("", new ShowOptions() { resultCallback = HandleAdResult });
        else
            print("ad not rdy");
    }

    public void FinishGame()
    {
        print("done game");
        print(data.hasWon);
        //ShowAd();
        LeaveGame();
    }

    // Restart game session, (maybe remove)
    public void ResetGame()
    {
        ClearEnemyObjects();
        ClearProjectiles();
        ClearParticleEffects();
        ResetPlayerStats();
        resultNotification.SetActive(false);
        objective.transform.GetComponent<Objective>().Reset();
        inGame = true;
        data.gameOver = false;
        paused = false;
        StartWave(0);
    }

    void StartGame()
    {
        Debug.Log("Starting Game");
        /*
        Enemy.EnemyCount = 0;
        Objective.ObjectiveCount = 0;
        Weapon.WeaponCount = 0;
        Trap.TrapCount = 0;
        */
        enemies.Clear();
        traps.Clear();
        edittingMap = false;
        inGame = true;
        onIntermission = false;
        paused = false;
        spawning = false;
        data.gameOver = false;
        //inGameCurrency = 0;
        startWaves = false;
        //UpdateInGameCurrency(0);
        //data.score = 0;
        kills = 0;
        //personalKills = 0;
        //totalPersonalKills = 0;
        //data.difficulty = 0;
        //data.wave = 4;
        //data.totalKills = 0;
        MapManager.mapManager.LoadMap(0);
        Weapon w = null;
        /*GameObject trap = Instantiate(trapPrefabs[0]);
        traps.Add(0, trap.GetComponent<Trap>());
        trap.transform.SetParent(playerRotation.transform);
        trap.transform.localPosition = new Vector3(0, 2, -20);
        trap = Instantiate(trapPrefabs[0]);
        traps.Add(1, trap.GetComponent<Trap>());
        trap.transform.SetParent(playerRotation.transform);
        trap.transform.localPosition = new Vector3(5, 2, -72);
       */
        //DisplayIntermission();
        if (NetworkManager.nm.isStarted)
        {
            data = new PlayerData();
            UpdateInGameCurrency(0);
            UpdateKillCount(0);
            UpdateScore(0);
            for (int i = 0; i < arrowQty.arr.Length; i++)
                UpdateArrowQty(i);
            return;
        }
        
        // If not online, game manager handles creating player
        player = Instantiate(playerPrefab);
        PlayerController pc = player.transform.GetComponent<PlayerController>();
        //pc.SetOrientation(playerOrientation);
        player.transform.position = playerSpawnPoints.transform.GetChild(0).position;
        player.transform.SetParent(playerRotation.transform);
        /*
        for (int i = 0; i < Projectile.projectileStats.Length; i++)
        {
            personalData.arrowQuantities[i] = personalData.arrowQuantities[i];
        }
        */
        if (continuedGame)
        {
            Debug.Log("Continued Game");
            if(data == null)
            {
                print("ERROR SAVED FILE NOT FOUND");
            }
            else
            {
                Debug.Log("fetching saved data");
                objective.transform.GetComponent<Objective>().HP = data.objectiveHP;
                w = Instantiate(weaponPrefabs[data.wepID]).transform.GetComponent<Weapon>();
                w.lvl = data.wepLvl;
            }
        }
        else
        {
            data = new PlayerData();
        }
        //data.wave = 19;
        data.wepID = 2;
        if (w == null)
        {
            for (int i = 0; i < 1; i++)
            {
                GameObject wep = Instantiate(weaponPrefabs[personalData.equippedWep]);
                pc.EquipWeapon(wep.transform.GetComponent<Weapon>());
            }
        }
        else
        {
            pc.EquipWeapon(w);
        }

        data.wepID = personalData.equippedWep;
        pc.wep.purchased = true;
        UpdateInGameCurrency(0);
        UpdateKillCount(0);
        UpdateScore(0);
        playerStatusCanvas.transform.Find("Wave").Find("Text").GetComponent<Text>().text = (data.wave == 0) ? "": "" + data.wave +"/" + EnemySpawnPattern.patternsBySpawnPointCt[0].Count;
        //playerStatusCanvas.transform.Find("Wave").Find("Text").GetComponent<Text>().text = (wave) + "";
        for (int i = 0; i < arrowQty.arr.Length; i++)
            UpdateArrowQty(i);

        if (data.wave % 5 == 0)
        {
            DisplayIntermission();
            return;
        }
        StartWave(data.wave);
    }

    public void StartWave(int w)
    {
        intermissionCanvas.GetComponent<Canvas>().sortingOrder = playerStatusCanvas.GetComponent<Canvas>().sortingOrder - 1;
        intermissionCanvas.SetActive(false);
        targetContainer.SetActive(false);
        onIntermission = false;
        ResetSpawnSetup(w);
        pattern = EnemySpawnPattern.patternsBySpawnPointCt[0][w % EnemySpawnPattern.patternsBySpawnPointCt[0].Count]; // Get spawn pattern for the wave
        patternIterations = pattern.iterations;
        timeToSpawn = pattern.spawnTimes[intervalIndex] / pattern.spawnCts[intervalIndex].Count;

        playerStatusCanvas.transform.Find("Wave").Find("Text").GetComponent<Text>().text = (data.wave + 1) + "/" + EnemySpawnPattern.patternsBySpawnPointCt[0].Count;
        
        waveNotificationCoroutine = StartCoroutine(NotifyIncomingWave(w));
    }

    // Spawns enemy and selects pre-determined path (spPath) from map manager at a spawn point (sp)
    public void SpawnEnemy(int sp,int spPath = -1)
    {
        GameObject enemy = Instantiate(enemyPrefabs[pattern.spawnCts[intervalIndex][spawnIndex]]);
        List<GameObject> pathing;
        //Enemy.AssignEnemy(enemy.transform.GetComponent<Enemy>());
        // Get random spawn point
        if (sp == -1)
            sp = UnityEngine.Random.Range(0, MapManager.mapManager.spawnPoints.Count);
        GameObject spawnPoint = MapManager.mapManager.spawnPoints[sp];
        enemy.transform.position = new Vector3(
                                    spawnPoint.transform.position.x,
                                    enemy.transform.position.y + 1,
                                    spawnPoint.transform.position.z);
        Enemy e = enemy.transform.GetComponent<Enemy>();
        e.SetTarget(spawnPoint);
      
        if (spPath == -1)
            pathing = MapManager.mapManager.pathsBySpawnPoint[sp][UnityEngine.Random.Range(0, MapManager.mapManager.pathsBySpawnPoint[sp].Count)];//Enemy.GeneratePathing(spawnPoint);
        else
            pathing = MapManager.mapManager.pathsBySpawnPoint[sp][spPath];
        //print("path gen");
        e.pathing = pathing;
        //GameObject enemyUI = Instantiate(statusIndicatorPrefab);
        //enemyUI.transform.GetComponent<StatusIndicator>().target = enemy;
        enemy.transform.SetParent(enemiesContainer.transform);
        data.difficulty = 0;
        e.level = (pattern.enemyLvls[intervalIndex][spawnIndex] + data.difficulty) % Enemy.enemyStats.Length;

    }

    public void SpawnEnemy(int enemyID, GameObject location)
    {

    }

    // Spawn enemy onto same path as another object
    public void SpawnEnemy(int enemyID, int curTarget, List<GameObject> pathing, Vector3 location)
    {
        print("SPAWNED MINION");
        GameObject enemy = Instantiate(enemyPrefabs[enemyID]);
        Enemy e = enemy.GetComponent<Enemy>();
        e.curTarget = curTarget;
        e.pathing = pathing;
        e.SetTarget(pathing[curTarget]);
        enemy.transform.position = location;
        enemy.transform.SetParent(enemiesContainer.transform);
        enemiesSpawned++;
    }

    // Spawn enemy with same target
    public void SpawnEnemy(int enemyID, GameObject target, Vector3 location)
    {
        print("SPAWNED MINION");
        GameObject enemy = Instantiate(enemyPrefabs[enemyID]);
        Enemy e = enemy.GetComponent<Enemy>();
        enemy.transform.position = location;
        enemy.transform.SetParent(enemiesContainer.transform);
        e.SetTarget(target);
        enemiesSpawned++;
    }

    // Spawn enemy at the spawn point sp
    public IEnumerator SpawnEnemyCoroutine(int sp, int spPath = -1)
    { 
        if (inGame)
        {
            spawning = true; // Indicate currently spawning an enemy
            enemiesSpawned++;
            /*
            GameObject enemy = Instantiate(enemyPrefabs[pattern.spawnCts[intervalIndex][spawnIndex]]);
            //Enemy.AssignEnemy(enemy.transform.GetComponent<Enemy>());
            // Get random spawn point
            if (sp == -1)
                sp = UnityEngine.Random.Range(0, MapManager.mapManager.spawnPoints.Count);
            GameObject spawnPoint = MapManager.mapManager.spawnPoints[sp];
            enemy.transform.position = new Vector3(
                                        spawnPoint.transform.position.x,
                                        enemy.transform.position.y + 1,
                                        spawnPoint.transform.position.z);
            Enemy e = enemy.transform.GetComponent<Enemy>();
            e.SetTarget(spawnPoint);
            //if (pathing == null)
            //    pathing = Enemy.GeneratePathing(spawnPoint);
            //print("path gen");
            e.pathing = pathing;
            GameObject enemyUI = Instantiate(statusIndicatorPrefab);
            enemyUI.transform.GetComponent<StatusIndicator>().target = enemy;
            enemy.transform.SetParent(enemiesContainer.transform);
            difficulty = 0;
            e.level = (pattern.enemyLvls[intervalIndex][spawnIndex] + difficulty) % Enemy.difficulties.Length;
            */
            SpawnEnemy(sp, spPath);
            spawnIndex++;
            
            // If reached end of spawning enemies in current group
            if (spawnIndex >= pattern.spawnCts[intervalIndex].Count)
            {
                spawnIndex = 0;
                // If reached end of spawning from current wave
                if (intervalIndex >= pattern.spawnFreqs.Count)
                {
                    intervalIndex = 0;
                    yield return new WaitForSeconds(pattern.endIterationTime);
                    patternIterations--;
                    // If no more iterations of this pattern to spawn
                    if (patternIterations <= 0)
                    {
                        doneSpawningWave = true;
                    }
                    // Repeat this spawn pattern
                    else
                    {
                        spawnTimer = 0;
                        timeToSpawn = pattern.spawnTimes[intervalIndex] / pattern.spawnCts[intervalIndex].Count;
                    }
                }
                // Move to next group of enemies to spawn
                else
                {
                    yield return new WaitForSeconds(pattern.spawnFreqs[intervalIndex]);
                    intervalIndex++;
                    spawnTimer = 0;
                    timeToSpawn = pattern.spawnTimes[intervalIndex] / pattern.spawnCts[intervalIndex].Count;
                }
            }
            // Still got enemies to spawn in this group
            else
            {
                spawnTimer = timeToSpawn;
            }
            spawning = false; // Indicate the end of spawning current enemy
        }
    }

    public void RemoveSelectedDefense(string type, int uid)
    {
        Debug.Log("remove selected def " + type);
        //if (selectedDefense == null)
        //    return;
       
        if (type == "Trap")
        {
            if (!traps.ContainsKey(uid))
            {
                print("doesnt exist trp");
                return;
            }
            //Debug.Log("REMOVE " + selectedDefense.name);
            //string[] details = selectedDefense.name.Split(' ');
            //string type = details[0];
            //int id = int.Parse(details[1]);
            Destroy(traps[uid].gameObject);
            traps.Remove(uid);
            print("REMOVED" + uid);
            //Destroy(selectedDefense);
        }
        //selectedDefense = null;
    }

    public void RemoveSelectedDefense()
    {
        Debug.Log("remove selected def");
        if (selectedDefense == null)
            return;
        
        Debug.Log("REMOVE " + selectedDefense.name);
        string[] details = selectedDefense.name.Split(' ');
        string type = details[0];
        int uid = int.Parse(details[1]);
        if (NetworkManager.nm.isStarted)
        {
            NetworkManager.nm.NotifyRemoveDefense(type, uid);
            return;
        }
        if (type == "Trap")
        {
            UpdateInGameCurrency(Trap.costs[selectedDefense.GetComponent<Trap>().trapID]);
        }
        RemoveSelectedDefense(type, uid);
        selectedDefense = null;
    }

    public GameObject SpawnDefense(string type, int typeID)
    {
        //int id = typeID;
        int uid = -1;
        GameObject defense = null;
        Debug.Log(type);
        if (type == "Trap")
        {
            defense = Instantiate(trapPrefabs[typeID]);
            defense.GetComponent<Trap>().trapID = typeID;
            Trap t = defense.GetComponent<Trap>();
            defense.transform.GetComponent<ObjectPlacement>().isSet = true;
            defense.transform.GetComponent<ObjectPlacement>().canSet = true;
            NetworkManager.nm.debugLog.Add("Trap " + Trap.TrapCount);
            traps.Add(t.id, defense.GetComponent<Trap>());
            defense.GetComponent<Trap>().ownerID = player.GetComponent<PlayerController>().id;
            uid = t.id;
        }
        // Create description for spawned item/defense
        // Allow ability to adjust spawned item/defense
        GameObject descrip = CreateAndSetObjectDescription(type, typeID, uid);
            /*Instantiate(descriptionPrefab);
        descrip.transform.GetChild(0).GetComponent<Text>().text = "Trap " + id + "\nPlaced on field";
        descrip.transform.SetParent(descriptionDisplay.transform.Find("Trap Descriptions").Find("Upgrade Descriptions"));
        descrip.transform.localPosition = new Vector3(0, 0, 0);
        descrip.SetActive(false);
        GameObject removeBtn = Instantiate(buttonPrefab);
        removeBtn.name = "Trap " + Trap.TrapCount;
        removeBtn.transform.GetChild(0).GetComponent<Text>().text = "Refund\n$50";
        RectTransform rt;
        rt = removeBtn.transform.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100, 100);
        removeBtn.transform.SetParent(descriptionDisplay.transform.Find("Trap Descriptions").Find("Upgrade Descriptions"));
        removeBtn.transform.localPosition = new Vector3(200, 0, 0);
        removeBtn.GetComponent<Button>().onClick.AddListener(RemoveSelectedDefense);
        removeBtn.transform.SetParent(descrip.transform);*/
        defense.GetComponent<ObjectPlacement>().description = descrip;

        return defense;
    }
    /*
    public GameObject CreateAndSetItemDescription(string type, int id)
    {
        GameObject descrip = Instantiate(itemUIPrefab);
        descrip.transform.GetChild(0).GetComponent<Text>().text = "Trap " + (Trap.TrapCount - 1) + "\nPlaced on field";
        descrip.transform.SetParent(descriptionDisplay.transform.Find("Trap Descriptions").Find("Upgrade Descriptions"));
        descrip.transform.localPosition = new Vector3(0, 0, 0);
        descrip.SetActive(false);
        GameObject removeBtn = Instantiate(buttonPrefab);
        removeBtn.name = "Trap " + Trap.TrapCount;
        removeBtn.transform.GetChild(0).GetComponent<Text>().text = "Refund\n$50";
        RectTransform rt;
        rt = removeBtn.transform.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100, 100);
        removeBtn.transform.SetParent(descriptionDisplay.transform.Find("Trap Descriptions").Find("Upgrade Descriptions"));
        removeBtn.transform.localPosition = new Vector3(200, 0, 0);
        removeBtn.GetComponent<Button>().onClick.AddListener(RemoveSelectedDefense);
        removeBtn.transform.SetParent(descrip.transform);
        //defense.GetComponent<ObjectPlacement>().description = descrip;

        return descrip;
    }*/

    public GameObject CreateAndSetObjectDescription(string type, int typeID, int uid)
    {
        GameObject descrip = Instantiate(descriptionPrefab);

        descrip.transform.GetChild(0).GetComponent<Text>().text = type + (uid) + "\nPlaced on field";
        if (type == "Trap")
        {
            descrip.transform.SetParent(descriptionDisplay.transform.Find("Trap Descriptions").Find("Upgrade Descriptions"));
            descrip.transform.localPosition = new Vector3(0, 0, 0);
            descrip.SetActive(false);
            GameObject removeBtn = Instantiate(buttonPrefab);
            removeBtn.name = "Trap " + uid;
            removeBtn.transform.GetChild(0).GetComponent<Text>().text = "Refund\n$" + Trap.costs[typeID];
            RectTransform rt;
            rt = removeBtn.transform.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(100, 100);
            removeBtn.transform.SetParent(descriptionDisplay.transform.Find("Trap Descriptions").Find("Upgrade Descriptions"));
            removeBtn.transform.localPosition = new Vector3(200, 0, 0);
            removeBtn.GetComponent<Button>().onClick.AddListener(RemoveSelectedDefense);
            removeBtn.transform.SetParent(descrip.transform);
        }
        //defense.GetComponent<ObjectPlacement>().description = descrip;

        return descrip;
    }

    public void SpawnDefense(string type, int typeID, GameObject location)
    {
        GameObject defense = SpawnDefense(type, typeID);
        defense.transform.position = location.transform.position;
    }

    public static float CosineFormula(float a, float b, float c)
    {
        float sumOfSquares = a * a + b * b - c * c;
        return Mathf.Acos(sumOfSquares / 2 / a / b) * Mathf.Rad2Deg;
    }

    public void ToggleCameraZoom()
    {
        bool zoomOption = player.GetComponent<PlayerController>().ToggleCameraZoom();
        quickAccessCanvas.transform.Find("ZoomBtn").Find("ZoomOptionIcon").GetComponent<Image>().sprite = (!zoomOption) ? zoomInIcon : zoomOutIcon;// Resources.Load<Sprite>("sprite/" + sprite);
    }

    // Handle quick access actions (upgrade weapon or switch arrows) and other tap actions
    public void HandleQuickAccessActivities()
    {
        if (edittingMap)
            return;
        //print("ct:" + Input.touchCount + "...mapfindgerid:" + mapFingerID + ", quickaccessid:" + quickAccessFingerID + ", detail;:" + quickAccessDetail);
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            if (t.phase == TouchPhase.Began)
            {
                //print("BEING");
                //print(0);
                if (quickAccessFingerID != -1 && mapFingerID != -1)
                    return;
                // Is there an object we are touching?
                if (EventSystem.current.IsPointerOverGameObject(t.fingerId))
                {
                    //print(1);
                    //if (EventSystem.current.currentSelectedGameObject)
                    //    Debug.Log(EventSystem.current.currentSelectedGameObject.tag);
                    // Is this a UI object?
                    if (EventSystem.current.currentSelectedGameObject)
                    {

                        GameObject selected = EventSystem.current.currentSelectedGameObject;
                        //Debug.Log(selected.name + " " + EventSystem.current.currentSelectedGameObject.tag);
                        string tag = selected.tag;
                       
                        if (tag != "QuickAccess")
                            return;
                        if (selected.name == "UpgradeWepBtn" && quickAccessDetail == "")
                        {
                            quickAccessDetail = "upgrade";
                            //quickAccessUpgradeDescription.SetActive(true);
                            //print("upgrade wep");
                            //mapUICanvas.SetActive(true);
                        }
                        else if (selected.name == "ChangeArrowsBtn" && quickAccessDetail == "")
                        {
                            quickAccessDetail = "change";
                            //print("change arrow");
                            //itemWheel.SetActive(true);
                            //itemWheel.transform.GetComponent<Rotator>().SetInteractable(true);

                            //mapUICanvas.SetActive(true);
                        }
                        else if (selected.name == "ZoomBtn" && quickAccessDetail == "")
                        {
                            quickAccessDetail = "zoom";
                            //print("change arrow");
                            //itemWheel.SetActive(true);
                            //itemWheel.transform.GetComponent<Rotator>().SetInteractable(true);

                            //mapUICanvas.SetActive(true);
                        }
                        /*else if (selected.name == "ItemWheel" && quickAccessDetail == "change" && mapFingerID == -1)
                        {
                           // print("????");
                            mapFingerID = t.fingerId;
                            initialAngle = quickAccessCanvas.transform.Find("ItemWheel").localEulerAngles.z;
                            initialPos = t.position;
                        }*/
                        if (quickAccessDetail != "" && quickAccessFingerID == -1)
                        {
                            quickAccessFingerID = t.fingerId;
                            quickAccessCanvas.GetComponent<Canvas>().sortingOrder = playerStatusCanvas.GetComponent<Canvas>().sortingOrder+1;
                        }

                    }
                }
                
                if (quickAccessDetail == "" && !player.GetComponent<PlayerController>().useGyro && quickAccessFingerID == -1 && t.position.x < Screen.width / 2)
                {
                    quickAccessFingerID = t.fingerId;
                    initialPos = t.position;
                    joystick.transform.position = initialPos;
                    joystick.transform.Find("Joystick").position = initialPos;
                    joystick.SetActive(true);
                    //player.GetComponent<PlayerController>().

                }

            }
            else
            {
                
                if (t.phase == TouchPhase.Ended)
                {
                   // print("ended");
                    if (t.fingerId == mapFingerID)
                    {
                       // print("DROPPED");
                        mapFingerID = -1;

                    }
                    else if (t.fingerId == quickAccessFingerID)
                    {
                        //print("qucikdrpo");
                        if (quickAccessDetail == "upgrade")
                        {
                            //mapUICanvas.SetActive(false);
                            inGameWepItemUI.SetActive(!inGameWepItemUI.activeSelf);
                            //player.GetComponent<PlayerController>().wep.GetComponent<Weapon>().ToggleStatsUI();
                            //w.itemUI.SetActive(!w.itemUI.activeSelf);
                            //quickAccessUpgradeDescription.SetActive(!quickAccessUpgradeDescription.activeSelf);//false);
                        }
                        else if (quickAccessDetail == "change")
                        {
                            // print("hide uui");
                            ToggleArrowQtyList();
                            //itemWheel.transform.GetComponent<Rotator>().SetInteractable(false);
                            //itemWheel.SetActive(false);
                            mapFingerID = -1;
                        }
                        else if(quickAccessDetail == "zoom")
                        {
                            ToggleCameraZoom();
                        }
                        else if(quickAccessDetail == "")
                        {
                            joystick.SetActive(false);
                        }
                        quickAccessDetail = "";
                        quickAccessCanvas.GetComponent<Canvas>().sortingOrder = playerStatusCanvas.GetComponent<Canvas>().sortingOrder - 1;
                        quickAccessFingerID = -1;
                    }
                    else
                    {
                        //itemDropdownList.GetComponent<Slider>().SlideInDirection(true);
                    }
                }else if(t.fingerId == quickAccessFingerID)
                {
                    if(quickAccessDetail == "")
                    {
                        float clampRange = 30 * joystick.transform.localScale.x;
                        float threshold = 10;
                        Vector2 offset = Vector2.ClampMagnitude(t.position - initialPos, clampRange);
                        print(offset.magnitude);
                        if (offset.magnitude < threshold)
                            return;
                        
                        joystick.transform.Find("Joystick").position = initialPos + offset; //new Vector2(Mathf.Clamp(t.position.x, initialPos.x - clampRange, initialPos.x + clampRange), Mathf.Clamp(t.position.y, initialPos.y - clampRange, initialPos.y + clampRange));

                        player.GetComponent<PlayerController>().SetOrientation(offset / clampRange);
                    }
                }
            }
        }
    }

    public void HandleMapEditActivities()
    {
        //mapUICanvas.SetActive(selectedDefense == null);
        //playerStatusCanvas.SetActive(selectedDefense == null);
        // Check to see if any finger touch ID is valid for selecting defense
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            if (t.phase == TouchPhase.Began && mapFingerID == -1)
            {
                
                // Is there an object we are touching?
                if (EventSystem.current.IsPointerOverGameObject(t.fingerId))
                {
                    //if (EventSystem.current.currentSelectedGameObject)
                    //    Debug.Log(EventSystem.current.currentSelectedGameObject.tag);
                    // Is this a UI object?
                    if (EventSystem.current.currentSelectedGameObject)
                    {
                        GameObject selected = EventSystem.current.currentSelectedGameObject;
                            Debug.Log(selected.name + " " + EventSystem.current.currentSelectedGameObject.tag);
                        string tag = selected.tag;
                        if (tag == "Untagged")
                            return;
                        /*
                        if (selectedDescription)
                        {
                            selectedDescription.SetActive(false);
                            selectedDescription.transform.parent.gameObject.SetActive(false);
                            selectedDescription.transform.parent.parent.gameObject.SetActive(false);
                        }
                        */
                        string[] details = selected.name.Split(' ');
                        string type = details[0];
                        int typeID = int.Parse(details[1]);
                        Debug.Log(type + " " + typeID);
                        mapAction = tag;
                        // Are you spawning a defense?
                        if (tag == "PlaceDefense")
                        {
                            //SpawnPotentialDefense();
                            Debug.Log("spawn?");
                            // Do I have any of this defense in my inventory?
                            if (myTraps[typeID] == 0)
                                return;

                            mapUICanvas.SetActive(false);
                            playerStatusCanvas.SetActive(false);
                            Debug.Log("canspawn");
                            //myTraps[0] -= 1;
                            //defensesContainer.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = "TNT x" + myTraps[0];
                            mapFingerID = t.fingerId;
                            selectedDefense = trapSpawnPrefab[typeID];
                            selectedDefense.GetComponent<ObjectPlacement>().Clear();
                            selectedDefense.SetActive(true);

                            /*Instantiate(trapPrefabs[0]);
                        //selectedDefense.transform.GetComponent<Collider>().enabled = false;
                        selectedDefense.layer = LayerMask.NameToLayer("Ignore Raycast");
                        Debug.Log(selectedDefense.transform.GetComponent<Collider>().GetType());
                        Collider c = selectedDefense.transform.GetComponent<Collider>();
                        if (c.GetType() == typeof(CapsuleCollider))
                            ((CapsuleCollider)c).center += new Vector3(0, -.75f, 0);*/
                            Vector3 cam2world = mapCamera.ScreenToWorldPoint(t.position);
                            selectedDefense.transform.position = new Vector3(cam2world.x, 2, cam2world.z + (15 * (mapCamera.orthographicSize / 55)));
                            Debug.Log("HOLD");
                            return;
                        }
                        // Are you selecting item to buy?
                        else if(tag == "Buy")
                        {
                            //Debug.Log("1");
                            DeselectDescription();
                            selectedItem = selected;
                            selectedDescription = descriptionDisplay.transform.Find(type + " Descriptions").gameObject;
                            selectedDescription.SetActive(true);
                            selectedDescription = selectedDescription.transform.Find("Buy Descriptions").gameObject;
                            selectedDescription.SetActive(true);
                            selectedDescription.transform.Find("BuyBtn").GetChild(0).GetComponent<Text>().text = "Buy\n$" + Trap.costs[typeID];
                            selectedDescription = selectedDescription.transform.GetChild(typeID).gameObject;
                            selectedDescription.SetActive(true);

                            //myTraps[id] = myTraps[id] + 1;
                            //defensesContainer.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = "TNT x" + myTraps[0];
                            Debug.Log("CANBUY");
                            return;
                        }
                        // Display detail for selected item in inventory
                        else if(tag == "Inventory")
                        {
                            Debug.Log(2);
                            DeselectDescription();
                            selectedItem = selected;
                            selectedDescription = descriptionDisplay.transform.Find(type + " Descriptions").gameObject;
                            selectedDescription.SetActive(true);
                            // Display Upgrade description
                            if(type == "Inventory")
                            {
                                selectedDescription = selectedDescription.transform.Find("Upgrade Descriptions").gameObject;

                            }
                            // Display button to add defense to map
                            else
                                selectedDescription = selectedDescription.transform.Find("AddToMap Descriptions").gameObject;
                            selectedDescription.SetActive(true);
                            addToMapBtn.name = selected.name;//GetChild(0).GetComponent<Text>().text = "Add To Map";
                            selectedDescription = selectedDescription.transform.GetChild(typeID).gameObject;
                            selectedDescription.SetActive(true);
                            return;
                        }
                    }
                }
            }
            else if (t.fingerId == mapFingerID)
            {
                // Release the selected defense, regardless if can set or not
                if (t.phase == TouchPhase.Ended)
                {
                    // Do I have a defense to place?
                    if (selectedDefense)
                    {
                        string[] details = selectedDefense.name.Split(' ');
                        string type = details[0];
                        int typeID = int.Parse(details[1]);
                        Debug.Log("placing" + typeID);
                        ObjectPlacement op = selectedDefense.GetComponent<ObjectPlacement>();
                        // Upon releasing my finger, is that spot legal to place defense?
                        if (op.canSet)
                        {
                            Debug.Log("Can set");
                            // Let Network Manager handle spawning defense
                            if (NetworkManager.nm.isStarted)
                            {
                                Debug.Log("LEMME HANDLE");
                                //NetworkManager.nm.
                                NetworkManager.nm.NotifySpawnDefenseAt(type,typeID);
                                return;
                            }
                            else
                            {
                                // Spawn Trap?
                                if (type == "Trap")
                                {
                                    myTraps[typeID] -= 1;
                                    defensesContainer.transform.GetChild(typeID).GetChild(0).GetComponent<Text>().text = "TNT x" + myTraps[typeID];
                                    defensesContainer.transform.GetChild(typeID).gameObject.SetActive(myTraps[typeID] > 0);
                                }
                                // Don't show capability to spawn more of this item if no more in inventory
                                if (myTraps[typeID] == 0)
                                    DeselectDescription();
                                SpawnDefense(type, typeID, selectedDefense);
                            }
                        }
                        else
                        {
                            Debug.Log(op.canSet);
                        }
                        selectedDefense.SetActive(false);
                        mapUICanvas.SetActive(true);
                        playerStatusCanvas.SetActive(true);
                    }
                    else if (mapAction == "Buy")
                    {
                        
                    }


                    mapFingerID = -1;
                    selectedDefense = null;
                    //selectedDefense.transform.GetComponent<Collider>().enabled = true;
                    //selectedDefense.transform.GetComponent<ObjectPlacement>().isSet = true;
                    //Collider c = selectedDefense.transform.GetComponent<Collider>();
                    //if (c.GetType() == typeof(CapsuleCollider))
                    //    ((CapsuleCollider)c).center -= new Vector3(0, -.75f, 0);
                    //selectedDefense.transform.GetComponent<CapsuleCollider>().center -= new Vector3(0, -.75f, 0);
                    //selectedDefense.layer = LayerMask.NameToLayer("Default");
                    
                    return;
                }
                // Drag selected defense to desired spot on map
                else if (t.phase == TouchPhase.Moved)
                {
                    Vector3 cam2world = mapCamera.ScreenToWorldPoint(t.position);
                    Ray r = mapCamera.ScreenPointToRay(t.position);
                    RaycastHit hit;
                    float yOffset = 0;
                    if (Physics.Raycast(r, out hit, 100))
                    {
                        //Debug.Log("HIT");
                        //Debug.Log(hit.transform.tag);
                        if (hit.transform.tag == "Ground" || hit.transform.tag == "Path")
                        {
                            yOffset = hit.transform.position.y;
                        }
                    }
                    //Debug.Log(mapFingerID + "... " + t.fingerId);
                    selectedDefense.transform.position = new Vector3(cam2world.x, 2 + yOffset, cam2world.z + (15 * (mapCamera.orthographicSize / 55)));
                    return;
                }
            }
        }
        // Can select objects on the field
        //if (selectedDefense == null || mapFingerID == -1)
        //if(!selectedUI)
        //{
        if (NetworkManager.nm.isStarted && NetworkManager.nm.isSpawningObject)
            return;
        //don't interfere with dragging potential defense spawn
        if(mapFingerID == -1)
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            if (t.phase != TouchPhase.Began)
                continue;
            Debug.Log(">>>>>>>>>>>>>>"+t.fingerId);
            Ray r = mapCamera.ScreenPointToRay(t.position);
            RaycastHit hit;
            selectedDefense = null;
            if (Physics.Raycast(r, out hit, 100))
            {

                //Debug.Log("HIT");
                Debug.Log(hit.transform.tag + " " + hit.transform.name);
                DeselectDescription();
                if (hit.transform.tag == "Trap")
                {
                    //string[] details = hit.transform.name.Split(' ');
                    //string type = details[0];
                    //int id = //int.Parse(details[1]);
                    //selectedDefense = hit.transform.gameObject;'
                    /*
                    if (selectedDescription)
                    {
                        selectedDescription.SetActive(false);
                        selectedDescription.transform.parent.gameObject.SetActive(false);
                        selectedDescription.transform.parent.parent.gameObject.SetActive(false);
                    }*/
                    selectedDescription = descriptionDisplay.transform.Find("Trap Descriptions").gameObject;
                    selectedDescription.SetActive(true);
                    selectedDescription = selectedDescription.transform.Find("Upgrade Descriptions").gameObject;
                    selectedDescription.SetActive(true);
                    //selectedDescription.transform.Find("BuyBtn").GetChild(0).GetComponent<Text>().text = "Buy\n$50";
                    selectedDescription = hit.transform.GetComponent<ObjectPlacement>().description;
                    //selectedDescription = selectedDescription.transform.GetChild(id).gameObject;
                    selectedDescription.SetActive(true);
                    selectedDefense = hit.transform.gameObject;
                }
                else
                {

                }
            }
        }
        //}

    }

    // Update is called once per frame
    void Update ()
    {
        switch (scene)
        {
            case "calibration":
                // In calibration scene, tap anywhere to set direction forward
                if (Input.touchCount > 0)
                {
                    for (int i = 0; i < Input.touchCount; i++)
                    {
                        Touch t = Input.GetTouch(i);
                        if (calibrateFingerID == -1 && t.phase == TouchPhase.Began)
                            calibrateFingerID = t.fingerId;
                        else if (calibrateFingerID != -1 && t.phase == TouchPhase.Ended && t.fingerId == calibrateFingerID)
                        {
                            calibrateFingerID = -1;
                           //playerRotation.transform.eulerAngles = new Vector3(0, player.transform.GetComponent<PlayerController>().playerCam.transform.eulerAngles.y, 0);
                            playerOrientation = new Vector3(0, player.transform.GetComponent<PlayerController>().playerCam.transform.eulerAngles.y, 0);// playerRotation.transform.eulerAngles;
                            print(player.transform.GetComponent<PlayerController>().playerCam.transform.eulerAngles.y);
                            print("ORIENTATIONT  " + playerOrientation);

                            playerOrientation = player.transform.GetComponent<PlayerController>().SetOrientation(playerOrientation);//.playerCam.transform.parent.eulerAngles = -playerOrientation;
                            print("player is now:" + player.transform.eulerAngles);
                        }
                    }
                }
                break;

            case "main":
                break;

            case "game":
                break;
        }

        // Don't do anything if not in game or didn't start the waves
        if (!inGame || data.gameOver)
        {
            return;
        }
        // **** EVERYTHING BELOW IS WHEN GAME SESSION BEGINS ****
        /* DEBUG PURPOSE
        if (Input.GetKeyDown(KeyCode.Space) || Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            print(1);
            SpawnEnemy(0, 0);
        }*/

        // Slowly fade out the hit indicator
        Color c = hitIndicator.GetComponent<Image>().color;
        if (c.a > 0)
        {
            c.a = c.a - 5 / 255f;
            if (c.a < 0)
                c.a = 0;
            hitIndicator.GetComponent<Image>().color = c;
        }
        HandleQuickAccessActivities();
        // Handle Map editting
        if (edittingMap)
        {
            HandleMapEditActivities();
        }

        //scoreTxt.text = "Score: " + data.score + ", Kills: " + kills + "/" + enemiesSpawned + ", My Kills: " + personalKills + ", $" + data.inGameCurrency + ", Wave: " + (data.wave+1);
        if (!startWaves)
            return;
        // if there are enemies to spawn
        if (!doneSpawningWave)
        {
            // if not spawning an enemy and either not online or is online and the host
            if (!spawning && (!NetworkManager.nm.isStarted || NetworkManager.nm.isHost))
            {
                spawnTimer -= Time.deltaTime;
                if (spawnTimer <= 0)
                {
                    // if online, let host announce spawning enemy
                    if (NetworkManager.nm.isStarted)
                        NetworkManager.nm.SpawnEnemy(-1);
                    // not online, spawn the enmy
                    else
                        StartCoroutine(SpawnEnemyCoroutine(-1));
                }
            }
        }
        // done spawning
        else
        {
            // end current wave when all enemies are killed and not on break
            if(kills == enemiesSpawned && !onIntermission)
            {
                startWaves = false; // indicate end of wave
                data.wave++; // increment to next wave
                //data.totalKills += kills; // add to cumulative kills

                //totalPersonalKills += personalKills;
                //personalKills = 0;
                UpdateKillCount(0);
                kills = 0; // reset kill count
                UpdateInGameCurrency(3 * data.wave);
                // increment difficulty after every 10th wave
                if (data.wave % 10 == 0) 
                {
                    data.difficulty++;
                }
                if(data.wave >= EnemySpawnPattern.patternsBySpawnPointCt[0].Count)
                {
                    Debug.Log("VICTORY");
                    DisplayEndGameNotifications(true);
                    //DisplayVictoryNotification();
                    return;
                }
                // go on intermission after every 5th wave
                if(data.wave % 5 == 0)
                {
                    DisplayIntermission();
                }
                // Start wave else
                else
                { 
                    // If online, sync everyone to start wave
                    if(!NetworkManager.nm.isStarted)
                        StartWave(data.wave);
                    else
                    {
                        NetworkManager.nm.RequestReady();
                    }
                }
            }
        }
    }
}
