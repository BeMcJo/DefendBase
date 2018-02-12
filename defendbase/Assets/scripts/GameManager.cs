using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
public class PlayerData
{
    public bool savedGame;
    public int wave,
        objectiveHP,
        score,
        totalKills;

    public PlayerData()
    {
        savedGame = false;
        wave = 0;
        objectiveHP = 0;
        score = 0;
        totalKills = 0;
    }
}

public class GameManager : MonoBehaviour {
    public static GameManager gm;

    public PlayerData data;

    public Dictionary<int, Enemy> enemies;

    public bool inGame,
                gameOver,
                startWaves,
                continuedGame,
                spawning,
                doneSpawningWave,
                setupRotation,
                gyroEnabled,
                onIntermission,
                playingOnline,
                paused;

    public int wave,
               kills,
               totalKills,
               spawnIndex,
               intervalIndex,
               patternIterations,
               enemiesSpawned,
               playerCurrency,
               inGameCurrency,
               difficulty,
               score;

    public Vector3 playerOrientation;

    public GameObject statusIndicatorPrefab,
                      playerPrefab,
                      playerUIPrefab,
                      buttonPrefab,
                      enemyArmorPrefab,
                      enemyPrefab;

    public GameObject playerStatusCanvas,
                      intermissionCanvas,
                      player,
                      playerSpawnPoints,
                      playerRotation,
                      projectilesContainer,
                      enemiesContainer,
                      particleEffectsContainer,
                      objective,
                      resultNotification,
                      mainMenuCanvas,
                      optionsCanvas,
                      hostListCanvas,
                      multiplayerCanvas,
                      lobbyCanvas,
                      waveNotification;

    public Text scoreTxt;

    public string scene;

    public float spawnTimer, timeToSpawn;

    Pattern pattern;

    public void Save(string type)
    {
        BinaryFormatter bf = new BinaryFormatter();
        string path = Application.persistentDataPath + "/" + type + ".dat";
        //if (type == "continuedGame")
        if (!File.Exists(path))
        {
            File.Create(path);
        }
        FileStream file = File.Open(path, FileMode.Open);

        switch (type)
        {
            case "continuedGame":
                PlayerData data = new PlayerData();
                data.savedGame = inGame;
                data.totalKills = totalKills;
                data.score = score;
                Debug.Log(inGame + " " + wave);
                if (objective != null)
                {
                    data.objectiveHP = objective.transform.GetComponent<Objective>().HP;
                }
                data.wave = wave;
                //gm.data = data;
                bf.Serialize(file, data);
                break;
        }
        
        file.Close();
        Debug.Log("Saved " + type);
    }

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
        //Debug.Log("START");
        gm = this;
        gm.data = new PlayerData();
        //data = new PlayerData();
        DontDestroyOnLoad(gm);
        playerOrientation = Vector3.zero;
        Screen.orientation = ScreenOrientation.Landscape;
        LoadMainScene();
        EnemySpawnPattern.InstantiatePatterns();
    }

    private void OnLevelWasLoaded(int level)
    {
        if (this != gm)
            return;
        //ClearGame();
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
            // Online Lobby
            case 3:
                LoadLobbyScene();
                break;
            /*// Game (Multiplayer)
            case 4:
                LoadMultiplayerGameScene();
                break;*/
        }
    }

    public void LoadLobbyScene()
    {
        projectilesContainer = GameObject.Find("ProjectilesContainer");
        GameObject.Find("Canvas").transform.Find("LeaveBtn").GetComponent<Button>().onClick.AddListener(GoToMainScene);
    }

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
        //playingOnline = false;
        setupRotation = true;
        mainMenuCanvas = GameObject.Find("MainMenuCanvas");

        Transform btnContainer = mainMenuCanvas.transform.Find("ButtonsContainer");
        btnContainer.Find("PlayBtn").GetComponent<Button>().onClick.AddListener(GoToGameScene);
        btnContainer.Find("OnlineBtn").GetComponent<Button>().onClick.AddListener(ToggleMultiplayerCanvas);
        btnContainer.Find("OnlineBtn").GetComponent<Button>().onClick.AddListener(ToggleMainMenuCanvas);
        btnContainer.Find("SettingsBtn").GetComponent<Button>().onClick.AddListener(GoToCalibrationScene);
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
        //lobbyCanvas.transform.Find("BackBtn").GetComponent<Button>().onClick.AddListener(ToggleMultiplayerCanvas);
        //lobbyCanvas.transform.Find("BackBtn").GetComponent<Button>().onClick.AddListener(ToggleLobbyCanvas);
        lobbyCanvas.transform.Find("BackBtn").GetComponent<Button>().onClick.AddListener(NetworkManager.nm.RequestLeaveLobby);
        lobbyCanvas.transform.Find("ReadyBtn").GetComponent<Button>().onClick.AddListener(NetworkManager.nm.RequestReady);
    }

    public void LoadGameScene()
    {
        Save("continuedGame");
        scene = "game";
        inGame = true;
        gameOver = false;
        paused = false;
        ResetSpawnSetup(0);
        wave = 0;
        kills = 0;
        //setupRotation = false;
        //gyroEnabled = true;
        totalKills = 0;
        intermissionCanvas = GameObject.Find("IntermissionCanvas");
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
        intermissionCanvas.SetActive(false);

        enemiesContainer = GameObject.Find("EnemiesContainer");
        projectilesContainer = GameObject.Find("ProjectilesContainer");
        particleEffectsContainer = GameObject.Find("ParticleEffectsContainer");
        playerRotation = GameObject.Find("Player Rotation");
        playerRotation.transform.eulerAngles = playerOrientation;
        playerStatusCanvas = GameObject.Find("PlayerStatusCanvas").gameObject;
        playerStatusCanvas.transform.Find("OptionsBtn").GetComponent<Button>().onClick.AddListener(DisplayOptions);
        playerSpawnPoints = playerRotation.transform.Find("PlayerSpawnPoints").gameObject;
        resultNotification = playerStatusCanvas.transform.Find("Result Notification").gameObject;
        resultNotification.SetActive(false);
        scoreTxt = playerStatusCanvas.transform.Find("ScoreTxt").GetComponent<Text>();
        waveNotification = playerStatusCanvas.transform.Find("Wave Notification").gameObject;
        waveNotification.SetActive(false);
        objective = playerRotation.transform.Find("Castle").Find("Gate").gameObject;
        resultNotification.transform.Find("RetryBtn").GetComponent<Button>().onClick.AddListener(ResetGame);

        optionsCanvas = GameObject.Find("OptionsCanvas");
        optionsCanvas.transform.Find("ResumeBtn").GetComponent<Button>().onClick.AddListener(ResumeGame);
        optionsCanvas.transform.Find("ExitBtn").GetComponent<Button>().onClick.AddListener(LeaveGame);
        optionsCanvas.SetActive(false);
        StartCoroutine(MapManager.mapManager.LoadGameScene());
        StartCoroutine(NetworkManager.nm.LoadGameScene());


        if (!NetworkManager.nm.isStarted || player == null)
        {
            player = Instantiate(playerPrefab);
            if (!NetworkManager.nm.isStarted)
            {
                player.transform.position = playerSpawnPoints.transform.GetChild(0).position;
                player.transform.SetParent(playerRotation.transform);
            }
        }

        StartGame();
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
        if(NetworkManager.nm.isStarted && NetworkManager.nm.isHost)
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

    bool DisplayWaveNotification()
    {
        //Debug.Log(1);
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

    bool HideWaveNotification()
    {
        //Debug.Log(2);
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

    public void ResetPlayerStats()
    {
        //kills = 0;
        score = 0;
        //wave = 0;
        totalKills = 0;
        objective.transform.GetComponent<Objective>().Reset();
    }

    public void ResetSpawnSetup(int w)
    {
        //timeToSpawn = 10f;
        spawnTimer = 0;
        startWaves = false;
        //intervalIndex = 0;
        wave = w;
        //totalKills += kills;
        kills = 0;
        enemiesSpawned = 0;
        doneSpawningWave = false;
    }

    public void ResetGame()
    {
        ClearEnemyObjects();
        ClearProjectiles();
        ClearParticleEffects();
        ResetPlayerStats();
        resultNotification.SetActive(false);
        //StartGame();
        //ResetSpawnSetup(0);
        objective.transform.GetComponent<Objective>().Reset();
        inGame = true;
        gameOver = false;
        paused = false;
        StartWave(0);
        //score = 0;
        //kills = 0;
        //totalKills = 0;
        //SceneManager.LoadScene("game");
    }

    void StartGame()
    {
        Debug.Log("Starting Game");
        inGame = true;
        paused = false;
        gameOver = false;
        Enemy.EnemyCount = 0;
        Objective.ObjectiveCount = 0;
        score = 0;
        kills = 0;
        difficulty = 0;
        wave = 0;
        totalKills = 0;
        MapManager.mapManager.LoadMap(0);
        if (continuedGame)
        {
            Debug.Log("COntinued");
            if (data != null && data.savedGame)
            {
                Debug.Log("fetching saved data");
                score = data.score;
                totalKills = data.totalKills;
                wave = data.wave;
                objective.transform.GetComponent<Objective>().HP = data.objectiveHP;
            }
        }
        //data = new PlayerData();
        if (NetworkManager.nm.isStarted)
            return;
        StartWave(wave);
    }

    public void StartWave(int w)
    {
        //Debug.Log("Starting Wave " + w);
        //Debug.Log("Displaying wave");
        //spawnIndex = 0;

        intermissionCanvas.SetActive(false);
        onIntermission = false;
        ResetSpawnSetup(w);
        pattern = EnemySpawnPattern.patternsBySpawnPointCt[0][w % EnemySpawnPattern.patternsBySpawnPointCt[0].Count];   
        patternIterations = pattern.iterations;
        timeToSpawn = pattern.spawnTimes[intervalIndex] / pattern.spawnCts[intervalIndex].Count;
        StartCoroutine(NotifyIncomingWave(w));
        //if (NetworkManager.nm.isStarted)
        //{
            //NetworkManager.nm.StartWave(wave);
        //}
    }

    public IEnumerator SpawnEnemy(int sp)
    {
        if (inGame)
        {
            spawning = true;
            enemiesSpawned++;
            GameObject enemy = Instantiate(enemyPrefab);
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
            enemyUI.transform.SetParent(enemiesContainer.transform);
            Enemy e = enemy.transform.GetComponent<Enemy>();
            e.level = (pattern.enemyLvls[intervalIndex][spawnIndex] + difficulty) % Enemy.difficulties.Length;

            spawnIndex++;
            //if (NetworkManager.nm.isStarted && NetworkManager.nm.isHost)
            //{
            //    NetworkManager.nm.NotifySpawnEnemyAt(sp);
            //}
            Debug.Log(spawnIndex + " " +pattern.spawnCts[intervalIndex].Count);

            if (spawnIndex >= pattern.spawnCts[intervalIndex].Count)
            {
                spawnIndex = 0;
                if (intervalIndex >= pattern.spawnFreqs.Count)
                {
                    intervalIndex = 0;
                    yield return new WaitForSeconds(pattern.endIterationTime);
                    patternIterations--;
                    if (patternIterations <= 0)
                    {
                        doneSpawningWave = true;
                    }
                    else
                    {
                        //spawnIndex = 0;
                        //pattern = EnemySpawnPattern.patternsBySpawnPointCt[0][wave % 2];
                        spawnTimer = 0;
                        //patternIterations = pattern.iterations;
                        timeToSpawn = pattern.spawnTimes[intervalIndex] / pattern.spawnCts[intervalIndex].Count;
                    }
                }
                else
                {
                    yield return new WaitForSeconds(pattern.spawnFreqs[intervalIndex]);
                    intervalIndex++;
                    //pattern = EnemySpawnPattern.patternsBySpawnPointCt[0][wave % 2];
                    spawnTimer = 0;
                    //patternIterations = pattern.iterations;
                    timeToSpawn = pattern.spawnTimes[intervalIndex] / pattern.spawnCts[intervalIndex].Count;
                }
            }
            else
            {
                spawnTimer = timeToSpawn;
            }
            spawning = false;
        }
    }

    // Update is called once per frame
    void Update ()
    {
        switch (scene)
        {
            case "calibration":
                if (Input.touchCount > 0)
                {
                    playerRotation.transform.eulerAngles = new Vector3(0, player.transform.GetComponent<PlayerController>().playerCam.transform.eulerAngles.y, 0);
                    //setupRotation = true;
                    playerOrientation = playerRotation.transform.eulerAngles;
                    //player.transform.position = playerSpawnPoint.transform.position;
                    //player.transform.SetParent(playerRotation.transform);

                }
                break;

            case "main":
                break;

            case "game":
                if (player) {

                    /*Camera cam = player.GetComponent<PlayerController>().playerCam;
                    Vector3 ea = cam.transform.localEulerAngles;
                    Debug.Log(ea.ToString());
                    //Debug.Log("Euler" + cam.transform.localRotation.x + "," + cam.transform.localRotation.y + "," + cam.transform.localRotation.z);// + ".> Localeu" + "," + cam.localEulerAngles.x + "," + cam.localEulerAngles.y + "," + cam.localEulerAngles.z);
                    Quaternion test = Quaternion.Euler(cam.transform.localRotation.x, cam.transform.localRotation.y, cam.transform.localRotation.z);
                    Vector3 v = new Vector3(cam.transform.localRotation.x, cam.transform.localRotation.y, cam.transform.localRotation.z);
                    cam.transform.localEulerAngles = ea;//Quaternion.Euler(v);
                    Debug.Log("ADTER" + cam.transform.localEulerAngles.ToString());
                    *///Debug.Log("EulerAFTER" + cam.transform.localRotation.x + "," + cam.transform.localRotation.y + "," + cam.transform.localRotation.z);// + ".> Localeu" + "," + cam.localEulerAngles.x + "," + cam.localEulerAngles.y + "," + cam.localEulerAngles.z);

                    //Debug.Log("test quat" + test.x + "," + test.y + "," + test.z);// calEulerAngles.y + "," + cam.localEulerAngles.z);

                    //Debug.Log(cam.)
                    //Quaternion target = Quaternion.Euler(0, 0, .5f);
                    //cam.transform.localRotation = target;//Quaternion.Slerp(cam.transform.localRotation, target, Time.deltaTime * 1f);
                }
                break;
        }
        /*
        if (gyroEnabled && !setupRotation)
        {
            //Debug.Log("Tap to set this as your front view");
            if (Input.touchCount > 0)
            {
                //Debug.Log("SETTING THIS TO YOUR FRONT VIEW");

                playerRotation.transform.eulerAngles = new Vector3(0, player.transform.GetComponent<PlayerController>().playerCam.transform.eulerAngles.y, 0);
                setupRotation = true;
                player.transform.position = playerSpawnPoint.transform.position;
                player.transform.SetParent(playerRotation.transform);

            }
            playerStatusCanvas.transform.Find("Calibration Notification").gameObject.SetActive(!setupRotation);
            if (setupRotation)
            {
                StartGame();
            }
            return;
        }*/
        if (!inGame || !startWaves)
        {
            return;
        }
        scoreTxt.text = "Score: " + score + ", spawned" + enemiesSpawned + "/kills" + kills;
        if (!doneSpawningWave)
        {
            if (!spawning && (!NetworkManager.nm.isStarted || NetworkManager.nm.isHost))
            {
                spawnTimer -= Time.deltaTime;
                if (spawnTimer <= 0)
                {
                    Debug.Log("SPAWED");
                    if (NetworkManager.nm.isStarted)
                        NetworkManager.nm.SpawnEnemy(-1);
                    else
                        StartCoroutine(SpawnEnemy(-1));
                    //spawnTimer = timeToSpawn;
                }
            }
        }
        else
        {
            if(kills == enemiesSpawned && !onIntermission)
            {
                startWaves = false;
                wave++;
                totalKills += kills;
                kills = 0;
                if (wave % 10 == 0)
                {
                    difficulty++;
                }
                if(wave % 5 == 0)
                {
                    DisplayIntermission();
                    //intermissionCanvas.SetActive(true);
                }
                else
                {
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
