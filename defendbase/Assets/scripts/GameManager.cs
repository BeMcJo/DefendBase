using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
// Used to save in game state
public class PlayerData
{
    public bool savedGame; // Determines if game is being loaded or saved
    public int wave,
               inGameCurrency,
               objectiveHP,
               score,
               difficulty,
               wepLvl,
               wepID,
               totalKills;

    public PlayerData()
    {
        savedGame = false;
        wave = 0;
        inGameCurrency = 0;
        difficulty = 0;
        objectiveHP = 0;
        score = 0;
        totalKills = 0;
        wepID = 0;
        wepLvl = 0;
    }
}

public class GameManager : MonoBehaviour {
    public static GameManager gm; // Single existing game manager
    public static bool Debugging; // Enables in game debugging
    public PlayerData data; // Information about any saved game

    public Dictionary<int, Enemy> enemies; // Keeps track of enemies spawned in game
    public Dictionary<int, Trap> traps; // Keeps track of traps spawned in game

    public bool inGame, // In game scene?
                gameOver, // Game session end?
                startWaves, // Wave spawning?
                continuedGame, // Was this game saved then loaded? If so load game from data
                spawning, // Determines if in process of spawning an enemy
                doneSpawningWave, // Is current wave all spawned?
                setupRotation, // Are you done setting up the orientation of your forward?
                //gyroEnabled, // Used????
                interactiveTouch, // Using touch interactions that isn't just shoot button?
                onIntermission, // Are we on break from defending waves of enemies?
                playingOnline, // Are we playing with players online?
                edittingMap, // Are we adjusting map defenses?
                paused; // Game paused?

    public int wave, // Determines current wave to spawn
               kills, // Total enemies killed in one wave
               totalKills, // Total enemies killed cumulative
               spawnIndex, // Which enemy to spawn in an interval
               intervalIndex, // Which group of enemies to spawn in a wave
               patternIterations, // How many times you spawn this spawn pattern
               enemiesSpawned, // Total number of enemies spawned
               playerCurrency, // Currency used outside game session
               inGameCurrency, // Currency used inside game session
               difficulty, // How hard enemies will be (Used to offset enemy level)
               score; // Keeps track of how awesome you are

    public Vector3 playerOrientation; // Keeps track of where your forward is

    public GameObject[] trapPrefabs, // List of trap objects
                        trapSpawnPrefab, // Pre-image of trap being spawned on map
                        weaponPrefabs; // List of weapon objects

    public GameObject statusIndicatorPrefab, // Shows health and other status for object
                      playerPrefab, // Your player in game
                      playerUIPrefab, // Your object in lobby
                      buttonPrefab, // Used for any general purposes as button
                      itemUIPrefab, // Used to display items in store in game
                      descriptionPrefab, // Used to provide details about item
                      enemyArmorPrefab, // ???
                      enemyPrefab; // Enemy object

    public GameObject playerStatusCanvas, // Information used for player to see
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
                      waveNotification; // Notifies player that enemies will spawn

    public Text scoreTxt; // Indicates how awesome you are

    public string scene, // Indicates which scene is loaded
                  shopType, // Type of purchse: Store, Upgrade
                  itemType; // Type of item: Weapon, Defense, Trap, Objective, ...

    public float spawnTimer, // Time currently before spawning enemy 
                 timeToSpawn; // Default time to assign to spawn enemy

    public Camera mapCamera; // Bird eye view of map, used to also edit defenses onto map

    public int mapFingerID; // Used for detecting player selecting/dragging item onto map
    public GameObject selectedDescription, // Selects type of item description to display
                      selectedDefense; // Currently selected defense to place onto map
    public Dictionary<int, int> myDefenses, // Counts number of each type of defense in inventory
                                myTraps; // Counts number of each type of trap in inventory 
    public string mapAction,
                  objectDetail;

    Pattern pattern; // Points to enemy spawn pattern

    // Saves data based on the type
    public void Save(string type)
    {
        BinaryFormatter bf = new BinaryFormatter();
        string path = Application.persistentDataPath + "/" + type + ".dat";
        if (!File.Exists(path))
        {
            File.Create(path);
        }
        FileStream file = File.Open(path, FileMode.Open);

        switch (type)
        {
            // Save in game progress
            case "continuedGame":
                PlayerData data = new PlayerData();
                data.savedGame = inGame;
                data.totalKills = totalKills;
                data.inGameCurrency = inGameCurrency;
                data.score = score;
                data.difficulty = difficulty;
                Debug.Log(inGame + " " + wave);
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
                data.wave = wave;
                bf.Serialize(file, data);
                break;
        }
        
        file.Close();
        Debug.Log("Saved " + type);
    }

    // Load data based on the type
    public void Load(string type)
    {
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
                    gm.data = data;
                    break;
            }
            //Debug.Log(file.Length);

            file.Close();
        }
        else
        {
            Debug.Log("File Does Not Exists");
        }
    }

    // Use this for initialization

    void Start () {
        if (gm)
            return;
        enemies = new Dictionary<int, Enemy>();
        gm = this;
        gm.data = new PlayerData();
        DontDestroyOnLoad(gm);
        interactiveTouch = false;
        traps = new Dictionary<int, Trap>();
        myTraps = new Dictionary<int, int>();
        myDefenses = new Dictionary<int, int>();
        playerOrientation = Vector3.zero;
        Screen.orientation = ScreenOrientation.Landscape; // Landscape mode for mobile phones
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
        scene = "calibration";
        player = GameObject.Find("Player");
        setupRotation = false;
        playerRotation = GameObject.Find("Player Rotation");
        playerRotation.transform.eulerAngles = playerOrientation;
        optionsCanvas = GameObject.Find("OptionsCanvas");
        optionsCanvas.transform.Find("FinishedBtn").GetComponent<Button>().onClick.AddListener(GoToMainScene);
    }

    public void LoadMainScene()
    {
        Load("continuedGame");
        scene = "main";
        inGame = false;

        setupRotation = true;
        mainMenuCanvas = GameObject.Find("MainMenuCanvas");

        Transform btnContainer = mainMenuCanvas.transform.Find("ButtonsContainer");
        btnContainer.Find("PlayBtn").GetComponent<Button>().onClick.AddListener(GoToGameScene);

        btnContainer.Find("OnlineBtn").GetComponent<Button>().onClick.AddListener(ToggleMultiplayerCanvas);

        btnContainer.Find("OnlineBtn").GetComponent<Button>().onClick.AddListener(ToggleMainMenuCanvas);

        btnContainer.Find("SettingsBtn").GetComponent<Button>().onClick.AddListener(ToggleMainMenuCanvas);
        btnContainer.Find("SettingsBtn").GetComponent<Button>().onClick.AddListener(ToggleSettingsCanvas);
        
        // Go to continued game if saved game progress exists
        btnContainer.Find("ContinueBtn").GetComponent<Button>().onClick.AddListener(GoToContinuedGameScene);
        btnContainer.Find("ContinueBtn").GetChild(0).GetComponent<Text>().text += (data != null && data.savedGame) ? " (Wave " + (data.wave+1) + ")" : "";
        btnContainer.Find("ContinueBtn").gameObject.SetActive(data != null && data.savedGame);

        multiplayerCanvas = GameObject.Find("MultiplayerCanvas");
        multiplayerCanvas.SetActive(false);
        btnContainer = multiplayerCanvas.transform.Find("ButtonsContainer");

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

        lobbyCanvas = GameObject.Find("LobbyCanvas");
        lobbyCanvas.SetActive(false);
        lobbyCanvas.transform.Find("BackBtn").GetComponent<Button>().onClick.AddListener(NetworkManager.nm.RequestLeaveLobby);

        lobbyCanvas.transform.Find("ReadyBtn").GetComponent<Button>().onClick.AddListener(NetworkManager.nm.RequestReady);

        settingsCanvas = GameObject.Find("SettingsCanvas");
        settingsCanvas.SetActive(false);
        string isOn = "ON";
        if (!interactiveTouch)
            isOn = "OFF";
        settingsCanvas.transform.Find("ButtonsContainer").Find("InteractiveTouchBtn").GetChild(0).GetComponent<Text>().text = "Interactive Touch: " + isOn;
        if (Debugging)
            isOn = "ON";
        else
            isOn = "OFF";
        settingsCanvas.transform.Find("ButtonsContainer").Find("DebugBtn").GetChild(0).GetComponent<Text>().text = "Debug Mode: " + isOn;

        btnContainer = settingsCanvas.transform.Find("ButtonsContainer");

        btnContainer.Find("InteractiveTouchBtn").GetComponent<Button>().onClick.AddListener(ToggleInteractiveTouch);

        btnContainer.Find("DebugBtn").GetComponent<Button>().onClick.AddListener(ToggleDebugging);

        btnContainer.Find("SetupOrientationBtn").GetComponent<Button>().onClick.AddListener(GoToCalibrationScene);
        btnContainer.Find("SetupOrientationBtn").gameObject.SetActive(SystemInfo.supportsGyroscope);

        btnContainer.Find("BackBtn").GetComponent<Button>().onClick.AddListener(ToggleMainMenuCanvas);
        btnContainer.Find("BackBtn").GetComponent<Button>().onClick.AddListener(ToggleSettingsCanvas);
    }

    // Load game scene. If there was any saved game progress, remove it
    public void LoadGameScene()
    {
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

        Enemy.EnemyCount = 0;
        Objective.ObjectiveCount = 0;
        Weapon.WeaponCount = 0;
        Trap.TrapCount = 0;

        intermissionCanvas = GameObject.Find("IntermissionCanvas");
        // If online, make sure everyone is ready to start next wave on intermission
        if (NetworkManager.nm.isStarted)
        {
            intermissionCanvas.transform.Find("ResumeBtn").GetComponent<Button>().onClick.AddListener(NetworkManager.nm.RequestReady);
            intermissionCanvas.transform.Find("ResumeBtn").GetChild(0).GetComponent<Text>().text = "Ready";
            intermissionCanvas.transform.Find("SaveAndQuitBtn").GetComponent<Button>().onClick.AddListener(NetworkManager.nm.LeaveGame);
        }
        else
        {
            intermissionCanvas.transform.Find("ResumeBtn").GetComponent<Button>().onClick.AddListener(NextWave);
            intermissionCanvas.transform.Find("SaveAndQuitBtn").GetComponent<Button>().onClick.AddListener(SaveAndQuit);
        }

        
        intermissionCanvas.transform.Find("ShopBtn").GetComponent<Button>().onClick.AddListener(ToggleShopCanvas);
        intermissionCanvas.transform.Find("ShopBtn").GetComponent<Button>().onClick.AddListener(ToggleIntermissionCanvas);
        intermissionCanvas.SetActive(false);

        enemiesContainer = GameObject.Find("EnemiesContainer");
        projectilesContainer = GameObject.Find("ProjectilesContainer");
        particleEffectsContainer = GameObject.Find("ParticleEffectsContainer");

        playerRotation = GameObject.Find("Player Rotation");
        playerRotation.transform.eulerAngles = playerOrientation;

        playerStatusCanvas = GameObject.Find("PlayerStatusCanvas").gameObject;
        playerStatusCanvas.transform.Find("OptionsBtn").GetComponent<Button>().onClick.AddListener(DisplayOptions);
        playerStatusCanvas.transform.Find("MapBtn").GetComponent<Button>().onClick.AddListener(ToggleMapUICanvas);

        waveNotification = playerStatusCanvas.transform.Find("Wave Notification").gameObject;
        waveNotification.SetActive(false);

        playerSpawnPoints = playerRotation.transform.Find("PlayerSpawnPoints").gameObject;

        resultNotification = playerStatusCanvas.transform.Find("Result Notification").gameObject;
        resultNotification.transform.Find("RetryBtn").GetComponent<Button>().onClick.AddListener(ResetGame);
        resultNotification.SetActive(false);

        scoreTxt = playerStatusCanvas.transform.Find("ScoreTxt").GetComponent<Text>();

        objective = playerRotation.transform.Find("Castle").Find("Gate").gameObject;
        
        optionsCanvas = GameObject.Find("OptionsCanvas");
        optionsCanvas.transform.Find("ResumeBtn").GetComponent<Button>().onClick.AddListener(ResumeGame);
        optionsCanvas.transform.Find("ExitBtn").GetComponent<Button>().onClick.AddListener(LeaveGame);
        optionsCanvas.SetActive(false);

        shopCanvas = GameObject.Find("ShopCanvas");
        shopCanvas.transform.Find("CloseBtn").GetComponent<Button>().onClick.AddListener(ToggleShopCanvas);
        shopCanvas.transform.Find("CloseBtn").GetComponent<Button>().onClick.AddListener(ToggleIntermissionCanvas);
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
        displayOptions.transform.Find("DisplayInventoryBtn").GetComponent<Button>().onClick.AddListener(DisplayInventoryOptions);
        displayOptions.transform.Find("DisplayStoreBtn").GetComponent<Button>().onClick.AddListener(DisplayStoreOptions);
        mapUICanvas.transform.Find("BackBtn").GetComponent<Button>().onClick.AddListener(ShowDisplayOptions);

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
            btn.transform.GetChild(0).GetComponent<Text>().text = "TNT";
            rt = btn.transform.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(100, 100);
            btn.transform.SetParent(storeContainer.transform);

            // Create available defense icon for each trap
            btn = Instantiate(buttonPrefab);
            btn.name = "Trap " + i;
            btn.tag = "Inventory";
            btn.transform.GetChild(0).GetComponent<Text>().text = "Tnt x0";
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
        
        StartGame();
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
                if (inGameCurrency < 50)
                    return;
                inGameCurrency -= 50;
                myTraps[id] += 1;
                defensesContainer.transform.GetChild(id).GetChild(0).GetComponent<Text>().text = "TNT x" + myTraps[id];
                defensesContainer.transform.GetChild(id).gameObject.SetActive(true);
                Debug.Log("BOUGHT");
                break;
        }
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

    public void ToggleMapUICanvas()
    {
        mapFingerID = -1;
        edittingMap = !edittingMap;
        ShowDisplayOptions();
        //selectedOption = null;
        mapUICanvas.SetActive(edittingMap);
        mapCamera.gameObject.SetActive(edittingMap);//enabled = edittingMap;
        player.GetComponent<PlayerController>().playerCam.gameObject.SetActive(!edittingMap);//.enabled = !edittingMap;
        playerStatusCanvas.transform.Find("ShootBtnMask(Clone)").gameObject.SetActive(!edittingMap);
        mapCamera.transform.GetComponent<MapViewCamera>().Reset();
    }

    public void ToggleSettingsCanvas()
    {
        settingsCanvas.SetActive(!settingsCanvas.activeSelf);
    }

    public void ToggleDebugging()
    {
        Debugging = !Debugging;
        string isOn = "ON";
        if (!Debugging)
            isOn = "OFF";
        settingsCanvas.transform.Find("ButtonsContainer").Find("DebugBtn").GetChild(0).GetComponent<Text>().text = "Debug Mode: " + isOn;
    }

    public void ToggleInteractiveTouch()
    {
        interactiveTouch = !interactiveTouch;
        string isOn = "ON";
        if (!interactiveTouch)
            isOn = "OFF";
        settingsCanvas.transform.Find("ButtonsContainer").Find("InteractiveTouchBtn").GetChild(0).GetComponent<Text>().text = "Interactive Touch: " + isOn;
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
    }

    public void ToggleLobbyCanvas()
    {
        lobbyCanvas.SetActive(!lobbyCanvas.activeSelf);
    }

    public void ToggleHostListCanvas()
    {
        hostListCanvas.SetActive(!hostListCanvas.activeSelf);
    }

    public void ToggleMultiplayerCanvas()
    {
        multiplayerCanvas.SetActive(!multiplayerCanvas.activeSelf);
    }

    public void ToggleIntermissionCanvas()
    {
        intermissionCanvas.SetActive(!intermissionCanvas.activeSelf);
    }

    public void SaveAndQuit()
    {
        Save("continuedGame");
        GoToMainScene();
    }

    public void NextWave()
    {
        intermissionCanvas.SetActive(false);
        StartWave(wave);
    }

    public void DisplayIntermission()
    {
        onIntermission = true;
        startWaves = false;
        intermissionCanvas.SetActive(true);
        intermissionCanvas.transform.Find("StatsTxt").GetComponent<Text>().text = "Score: " + score + "\tKills: " + totalKills + "\nNext Wave: " + (wave + 1);
        intermissionCanvas.transform.Find("ShopBtn").GetComponent<Button>().interactable = wave % 10 == 0;
        if (NetworkManager.nm.isStarted && NetworkManager.nm.isHost)
        {
            intermissionCanvas.transform.Find("ResumeBtn").GetComponent<Button>().interactable = false;
        }
    }

    public void DisplayOptions()
    {
        optionsCanvas.SetActive(true);
    }

    public void ResumeGame()
    {
        optionsCanvas.SetActive(false);
    }

    public void LeaveGame()
    {
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
        score += s;
    }

    // Updates in game currency and displays the information in shop
    public void UpdateInGameCurrency(int currency)
    {
        inGameCurrency += currency;
        shopCanvas.transform.Find("Currency").GetChild(0).GetComponent<Text>().text = "$" + inGameCurrency;
    }

    // Animates wave notification text to slowly appear
    bool DisplayWaveNotification()
    {
        if (!inGame)
        {
            return true;
        }
        Color c = waveNotification.transform.GetComponent<Image>().color;
        c.a += .01f;
        waveNotification.transform.GetComponent<Image>().color = c;

        c = waveNotification.transform.GetChild(0).GetComponent<Text>().color;
        c.a += .01f;
        waveNotification.transform.GetChild(0).GetComponent<Text>().color = c;
        return c.a >= 1;
    }

    // Slowly makes wave notification text disappear
    bool HideWaveNotification()
    {
        if (!inGame)
        {
            return true;
        }
        Color c = waveNotification.transform.GetComponent<Image>().color;
        c.a -= .01f;
        waveNotification.transform.GetComponent<Image>().color = c;

        c = waveNotification.transform.GetChild(0).GetComponent<Text>().color;
        c.a -= .01f;
        waveNotification.transform.GetChild(0).GetComponent<Text>().color = c;
        return c.a <= 0;
    }

    // Displays wave notification text and then hides it, the end indicates wave starts
    IEnumerator NotifyIncomingWave(int w)
    {
        
        waveNotification.SetActive(true);
        waveNotification.transform.GetChild(0).GetComponent<Text>().text = "Wave " + (w+1);
        yield return new WaitUntil(DisplayWaveNotification);
        yield return new WaitForSeconds(3f);
        yield return new WaitUntil(HideWaveNotification);
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
        score = 0;
        totalKills = 0;
        objective.transform.GetComponent<Objective>().Reset();
    }

    // Reset enemy spawn related variables to initial values for wave w
    public void ResetSpawnSetup(int w)
    {
        spawnTimer = 0;
        spawnIndex = 0;
        startWaves = false;
        intervalIndex = 0;
        wave = w;
        kills = 0;
        enemiesSpawned = 0;
        doneSpawningWave = false;
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
        gameOver = false;
        paused = false;
        StartWave(0);
    }

    void StartGame()
    {
        Debug.Log("Starting Game");
        Enemy.EnemyCount = 0;
        Objective.ObjectiveCount = 0;
        Weapon.WeaponCount = 0;
        Trap.TrapCount = 0;
        enemies.Clear();
        traps.Clear();
        edittingMap = false;
        inGame = true;
        onIntermission = false;
        paused = false;
        gameOver = false;
        inGameCurrency = 100;
        score = 0;
        kills = 0;
        difficulty = 0;
        wave = 0;
        totalKills = 0;
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
        if (continuedGame)
        {
            Debug.Log("Continued Game");
            if (data != null && data.savedGame)
            {
                Debug.Log("fetching saved data");
                score = data.score;
                totalKills = data.totalKills;
                wave = data.wave;
                inGameCurrency = data.inGameCurrency;
                objective.transform.GetComponent<Objective>().HP = data.objectiveHP;
                w = Instantiate(weaponPrefabs[data.wepID]).transform.GetComponent<Weapon>();
                w.lvl = data.wepLvl;
                w.purchased = true;
            }
            if (wave % 10 == 0)
            {
                DisplayIntermission();
                return;
            }
        }
        //DisplayIntermission();
        if (NetworkManager.nm.isStarted)
            return;
        // If not online, game manager handles creating player
        player = Instantiate(playerPrefab);
        PlayerController pc = player.transform.GetComponent<PlayerController>();
        player.transform.position = playerSpawnPoints.transform.GetChild(0).position;
        player.transform.SetParent(playerRotation.transform);
        if (w == null)
        {
            for (int i = 0; i < 1; i++)
            {
                GameObject wep = Instantiate(weaponPrefabs[0]);
                pc.EquipWeapon(wep.transform.GetComponent<Weapon>());
                pc.wep.purchased = true;
            }
        }
        else
        {
            pc.EquipWeapon(w);
        }
        StartWave(wave);
    }

    public void StartWave(int w)
    {
        intermissionCanvas.SetActive(false);
        onIntermission = false;
        ResetSpawnSetup(w);
        pattern = EnemySpawnPattern.patternsBySpawnPointCt[0][w % EnemySpawnPattern.patternsBySpawnPointCt[0].Count]; // Get spawn pattern for the wave
        patternIterations = pattern.iterations;
        timeToSpawn = pattern.spawnTimes[intervalIndex] / pattern.spawnCts[intervalIndex].Count;
        StartCoroutine(NotifyIncomingWave(w));
    }

    // Spawn enemy at the spawn point sp
    public IEnumerator SpawnEnemy(int sp)
    {
        if (inGame)
        {
            spawning = true; // Indicate currently spawning an enemy
            enemiesSpawned++;
            GameObject enemy = Instantiate(enemyPrefab);
            Enemy.AssignEnemy(enemy.transform.GetComponent<Enemy>());
            // Get random spawn point
            if (sp == -1)
                sp = UnityEngine.Random.Range(0, MapManager.mapManager.spawnPoints.Count);
            GameObject spawnPoint = MapManager.mapManager.spawnPoints[sp];
            enemy.transform.position = new Vector3(
                                        spawnPoint.transform.position.x,
                                        enemy.transform.position.y,
                                        spawnPoint.transform.position.z);
            enemy.transform.GetComponent<Enemy>().SetTarget(spawnPoint);
            GameObject enemyUI = Instantiate(statusIndicatorPrefab);
            enemyUI.transform.GetComponent<StatusIndicator>().target = enemy;
            enemy.transform.SetParent(enemiesContainer.transform);
            Enemy e = enemy.transform.GetComponent<Enemy>();
            e.level = (pattern.enemyLvls[intervalIndex][spawnIndex] + difficulty) % Enemy.difficulties.Length;

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

    public void RemoveSelectedDefense()
    {
        Debug.Log("remove selected def");
        if (selectedDefense == null)
            return;
        Debug.Log("REMOVE " + selectedDefense.name);
        string[] details = selectedDefense.name.Split(' ');
        string type = details[0];
        int id = int.Parse(details[1]);
        traps.Remove(id);
        UpdateInGameCurrency(50);
        Destroy(selectedDefense);
        selectedDefense = null;
    }

    public void HandleMapEditActivities()
    {
        //mapUICanvas.SetActive(selectedDefense == null);
        //playerStatusCanvas.SetActive(selectedDefense == null);
        bool selectedUI = false;
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
                    // Is that object a PlaceDefense object?
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
                        int id = int.Parse(details[1]);
                        Debug.Log(type + " " + id);
                        mapAction = tag;
                        // Are you spawning a defense?
                        if (tag == "PlaceDefense")
                        {
                            //SpawnPotentialDefense();
                            Debug.Log("spawn?");
                            // Do I have any of this defense in my inventory?
                            if (myTraps[0] == 0)
                                return;

                            mapUICanvas.SetActive(false);
                            playerStatusCanvas.SetActive(false);
                            Debug.Log("canspawn");
                            //myTraps[0] -= 1;
                            //defensesContainer.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = "TNT x" + myTraps[0];
                            mapFingerID = t.fingerId;
                            selectedDefense = trapSpawnPrefab[0];
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
                            Debug.Log("1");
                            DeselectDescription();
                            selectedItem = selected;
                            selectedDescription = descriptionDisplay.transform.Find(type + " Descriptions").gameObject;
                            selectedDescription.SetActive(true);
                            selectedDescription = selectedDescription.transform.Find("Buy Descriptions").gameObject;
                            selectedDescription.SetActive(true);
                            selectedDescription.transform.Find("BuyBtn").GetChild(0).GetComponent<Text>().text = "Buy\n$50";
                            selectedDescription = selectedDescription.transform.GetChild(id).gameObject;
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
                            selectedDescription = selectedDescription.transform.Find("AddToMap Descriptions").gameObject;
                            selectedDescription.SetActive(true);
                            addToMapBtn.name = selected.name;//GetChild(0).GetComponent<Text>().text = "Add To Map";
                            selectedDescription = selectedDescription.transform.GetChild(id).gameObject;
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
                        int id = int.Parse(details[1]);
                        Debug.Log("placing");
                        ObjectPlacement op = selectedDefense.GetComponent<ObjectPlacement>();
                        // Upon releasing my finger, is that spot legal to place defense?
                        if (op.canSet)
                        {
                            Debug.Log("Can set");
                            myTraps[id] -= 1;
                            defensesContainer.transform.GetChild(id).GetChild(0).GetComponent<Text>().text = "TNT x" + myTraps[id];
                            defensesContainer.transform.GetChild(id).gameObject.SetActive(myTraps[id] > 0);
                            // Don't show capability to spawn more of this item if no more in inventory
                            if (myTraps[id] == 0)
                                DeselectDescription();
                            GameObject defense = Instantiate(trapPrefabs[id]);
                            defense.transform.position = selectedDefense.transform.position;
                            defense.transform.GetComponent<ObjectPlacement>().isSet = true;
                            defense.transform.GetComponent<ObjectPlacement>().canSet = true;
                            traps.Add(Trap.TrapCount, defense.GetComponent<Trap>());

                            // Create description for spawned item/defense
                            // Allow ability to adjust spawned item/defense
                            GameObject descrip = Instantiate(descriptionPrefab);
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
                            removeBtn.transform.SetParent(descrip.transform);
                            defense.GetComponent<ObjectPlacement>().description = descrip;
                            //selectedDefense.transform.GetComponent<Collider>().enabled = false;
                            //selectedDefense.layer = LayerMask.NameToLayer("Ignore Raycast");
                            //Debug.Log(selectedDefense.transform.GetComponent<Collider>().GetType());
                            //Collider c = selectedDefense.transform.GetComponent<Collider>();
                            //if (c.GetType() == typeof(CapsuleCollider))
                            //    ((CapsuleCollider)c).center += new Vector3(0, -.75f, 0);
                            //Vector3 cam2world = mapCamera.ScreenToWorldPoint(t.position);
                            //selectedDefense.transform.position = new Vector3(cam2world.x, 2, cam2world.z + (15 * (mapCamera.orthographicSize / 55)));
                            //Debug.Log("HOLD");
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
                    //selectedDefense.transform.GetComponent<Collider>().enabled = true;
                    //selectedDefense.transform.GetComponent<ObjectPlacement>().isSet = true;
                    //Collider c = selectedDefense.transform.GetComponent<Collider>();
                    //if (c.GetType() == typeof(CapsuleCollider))
                    //    ((CapsuleCollider)c).center -= new Vector3(0, -.75f, 0);
                    //selectedDefense.transform.GetComponent<CapsuleCollider>().center -= new Vector3(0, -.75f, 0);
                    //selectedDefense.layer = LayerMask.NameToLayer("Default");
                    selectedDefense = null;
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
                    Debug.Log(mapFingerID + "... " + t.fingerId);
                    selectedDefense.transform.position = new Vector3(cam2world.x, 2 + yOffset, cam2world.z + (15 * (mapCamera.orthographicSize / 55)));
                    return;
                }
            }
        }
        // Can select objects on the field
        //if (selectedDefense == null || mapFingerID == -1)
        //if(!selectedUI)
        //{
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
                    playerRotation.transform.eulerAngles = new Vector3(0, player.transform.GetComponent<PlayerController>().playerCam.transform.eulerAngles.y, 0);
                    playerOrientation = playerRotation.transform.eulerAngles;
                }
                break;

            case "main":
                break;

            case "game":
                break;
        }

        // Don't do anything if not in game or didn't start the waves
        if (!inGame || gameOver)
        {
            return;
        }

        if (edittingMap)
        {
            HandleMapEditActivities();
        }

        scoreTxt.text = "Score: " + score + ", spawned" + enemiesSpawned + "/kills" + kills + ", $" + inGameCurrency;
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
                        StartCoroutine(SpawnEnemy(-1));
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
                wave++; // increment to next wave
                totalKills += kills; // add to cumulative kills
                kills = 0; // reset kill count
                // increment difficulty after every 10th wave
                if (wave % 10 == 0) 
                {
                    difficulty++;
                }
                // go on intermission after every 5th wave
                if(wave % 5 == 0)
                {
                    DisplayIntermission();
                }
                // Start wave else
                else
                { 
                    // If online, sync everyone to start wave
                    if(!NetworkManager.nm.isStarted)
                        StartWave(wave);
                    else
                    {
                        NetworkManager.nm.RequestReady();
                    }
                }
            }
        }
    }
}
