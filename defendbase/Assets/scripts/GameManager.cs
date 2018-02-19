using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
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
    }
}

public class GameManager : MonoBehaviour {
    public static GameManager gm; // Single existing game manager

    public PlayerData data; // Information about any saved game

    public Dictionary<int, Enemy> enemies; // Keeps track of enemies spawned in game

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

    public GameObject[] weaponPrefabs; // List of weapon objects

    public GameObject statusIndicatorPrefab, // Shows health and other status for object
                      playerPrefab, // Your player in game
                      playerUIPrefab, // Your object in lobby
                      buttonPrefab, // Used for any general purposes as button
                      itemUIPrefab, // Used to display items in store in game
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
                      waveNotification; // Notifies player that enemies will spawn

    public Text scoreTxt; // Indicates how awesome you are

    public string scene, // Indicates which scene is loaded
                  shopType, // Type of purchse: Store, Upgrade
                  itemType; // Type of item: Weapon, Defense, Trap, Objective, ...

    public float spawnTimer, // Time currently before spawning enemy 
                 timeToSpawn; // Default time to assign to spawn enemy

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
        btnContainer = settingsCanvas.transform.Find("ButtonsContainer");

        btnContainer.Find("InteractiveTouchBtn").GetComponent<Button>().onClick.AddListener(ToggleInteractiveTouch);

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
        //inGame = true;
        //gameOver = false;
        //paused = false;
        //ResetSpawnSetup(0);
        //wave = 0;
        //kills = 0;
        //setupRotation = false;
        //gyroEnabled = true;
        //totalKills = 0;

        intermissionCanvas = GameObject.Find("IntermissionCanvas");
        // If online, make sure everyone is ready to start next wave on intermission
        if (NetworkManager.nm.isStarted)
        {
            intermissionCanvas.transform.Find("ResumeBtn").GetComponent<Button>().onClick.AddListener(NetworkManager.nm.RequestReady);
            intermissionCanvas.transform.Find("ResumeBtn").GetChild(0).GetComponent<Text>().text = "Ready";
        }
        else
        {
            intermissionCanvas.transform.Find("ResumeBtn").GetComponent<Button>().onClick.AddListener(NextWave);
        }

        intermissionCanvas.transform.Find("SaveAndQuitBtn").GetComponent<Button>().onClick.AddListener(SaveAndQuit);
        
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
        shopCanvas.SetActive(false);

        StartCoroutine(MapManager.mapManager.LoadGameScene());
        StartCoroutine(NetworkManager.nm.LoadGameScene()); 

        // If not online, game manager handles creating player
        if (!NetworkManager.nm.isStarted)
        {
            player = Instantiate(playerPrefab);

            player.transform.position = playerSpawnPoints.transform.GetChild(0).position;
            player.transform.SetParent(playerRotation.transform);

            for (int i = 0; i < 1; i++)
            {
                GameObject wep = Instantiate(weaponPrefabs[0]);
                PlayerController pc = player.transform.GetComponent<PlayerController>();
                pc.EquipWeapon(wep.transform.GetComponent<Weapon>());
                pc.wep.purchased = true;
            }
        }

        StartGame();
    }

    public void ToggleSettingsCanvas()
    {
        settingsCanvas.SetActive(!settingsCanvas.activeSelf);
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
        enemies.Clear();
        inGame = true;
        onIntermission = false;
        paused = false;
        gameOver = false;
        Enemy.EnemyCount = 0;
        Objective.ObjectiveCount = 0;
        Weapon.WeaponCount = 0;
        inGameCurrency = 0;
        score = 0;
        kills = 0;
        difficulty = 0;
        wave = 0;
        totalKills = 0;
        MapManager.mapManager.LoadMap(0);
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
            }
        }
        if (NetworkManager.nm.isStarted)
            return;
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
        if (!inGame || !startWaves)
        {
            return;
        }
        scoreTxt.text = "Score: " + score + ", spawned" + enemiesSpawned + "/kills" + kills;
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
