using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public static GameManager gm;

    public bool inGame,
                startWaves,
                spawning,
                doneSpawningWave,
                setupRotation,
                gyroEnabled,
                paused;

    public int wave,
               kills,
               totalKills,
               spawnIndex,
               intervalIndex,
               patternIterations,
               enemiesSpawned,
               score;

    public Vector3 playerOrientation;

    public GameObject statusIndicatorPrefab,
                      playerPrefab,
                      enemyPrefab;

    public GameObject playerStatusCanvas,
                      player,
                      playerSpawnPoint,
                      playerRotation,
                      projectilesContainer,
                      enemiesContainer,
                      particleEffectsContainer,
                      objective,
                      resultNotification,
                      mainMenuCanvas,
                      optionsCanvas,
                      waveNotification;

    public Text scoreTxt;

    public string scene;

    public float spawnTimer, timeToSpawn;

    Pattern pattern;

    // Use this for initialization

    void Start () {
        if (gm)
            return;
        //Debug.Log("START");
        gm = this;
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
            /*// Client
            case 3:
                LoadClientScene();
                break;
            // Game (Multiplayer)
            case 4:
                LoadMultiplayerGameScene();
                break;*/
        }
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
        scene = "main";
        inGame = false;
        setupRotation = true;
        mainMenuCanvas = GameObject.Find("MainMenuCanvas");
        mainMenuCanvas.transform.Find("PlayBtn").GetComponent<Button>().onClick.AddListener(GoToGameScene);
        mainMenuCanvas.transform.Find("SettingsBtn").GetComponent<Button>().onClick.AddListener(GoToCalibrationScene);
    }

    public void LoadGameScene()
    {
        scene = "game";
        inGame = true;
        paused = false;
        ResetSpawnSetup(0);
        wave = 0;
        kills = 0;
        //setupRotation = false;
        //gyroEnabled = true;
        totalKills = 0;
        enemiesContainer = GameObject.Find("EnemiesContainer");
        projectilesContainer = GameObject.Find("ProjectilesContainer");
        particleEffectsContainer = GameObject.Find("ParticleEffectsContainer");
        playerRotation = GameObject.Find("Player Rotation");
        playerRotation.transform.eulerAngles = playerOrientation;
        player = Instantiate(playerPrefab);
        playerStatusCanvas = GameObject.Find("PlayerStatusCanvas").gameObject;
        playerStatusCanvas.transform.Find("OptionsBtn").GetComponent<Button>().onClick.AddListener(DisplayOptions);
        playerSpawnPoint = playerRotation.transform.Find("PlayerSpawnPoint").gameObject;
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
        StartGame();
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

    public void GoToMainScene()
    {
        SceneManager.LoadScene("main");
    }

    public void GoToGameScene()
    {
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
        yield return new WaitForSeconds(5);
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
        spawnIndex = 0;
        //timeToSpawn = 10f;
        spawnTimer = 0;
        startWaves = false;
        intervalIndex = 0;
        wave = w;
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
        StartWave(0);
        inGame = true;
        paused = false;
        //score = 0;
        //kills = 0;
        //totalKills = 0;
        //ResetSpawnSetup(0);
        //SceneManager.LoadScene("game");
    }

    void StartGame()
    {
        player.transform.position = playerSpawnPoint.transform.position;
        player.transform.SetParent(playerRotation.transform);
        Debug.Log("Starting Game");
        inGame = true;
        paused = false;
        score = 0;
        kills = 0;
        totalKills = 0;
        MapManager.mapManager.LoadMap(0);
        StartWave(0);
    }

    void StartWave(int w)
    {
        //Debug.Log("Starting Wave " + w);
        //Debug.Log("Displaying wave");
        //spawnIndex = 0;
        ResetSpawnSetup(w);
        pattern = EnemySpawnPattern.patternsBySpawnPointCt[0][w % EnemySpawnPattern.patternsBySpawnPointCt[0].Count];   
        patternIterations = pattern.iterations;
        timeToSpawn = pattern.spawnTimes[intervalIndex] / pattern.spawnCts[intervalIndex].Count;
        StartCoroutine(NotifyIncomingWave(w));
    }

    public IEnumerator SpawnEnemy()
    {
        spawning = true;
        enemiesSpawned++;
        GameObject enemy = Instantiate(enemyPrefab);
        GameObject spawnPoint = MapManager.mapManager.spawnPoints[Random.Range(0, MapManager.mapManager.spawnPoints.Count)];
        enemy.transform.position = new Vector3(
                                    spawnPoint.transform.position.x,
                                    enemy.transform.position.y,
                                    spawnPoint.transform.position.z);
        enemy.transform.GetComponent<Enemy>().SetTarget(spawnPoint);
        GameObject enemyUI = Instantiate(statusIndicatorPrefab);
        enemyUI.transform.GetComponent<StatusIndicator>().target = enemy;
        enemy.transform.SetParent(enemiesContainer.transform);
        enemyUI.transform.SetParent(enemiesContainer.transform);
        spawnIndex++;
        if(spawnIndex >= pattern.spawnCts[intervalIndex].Count)
        {
            if (intervalIndex >= pattern.spawnFreqs.Count)
            {
                yield return new WaitForSeconds(pattern.endIterationTime);
                patternIterations--;
                if (patternIterations <= 0)
                {
                    doneSpawningWave = true;
                }
                else
                {
                    spawnIndex = 0;
                    intervalIndex = 0;
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
            if (!spawning)
            {
                spawnTimer -= Time.deltaTime;
                if (spawnTimer <= 0)
                {
                    Debug.Log("SPAWED");
                    StartCoroutine(SpawnEnemy());
                    //spawnTimer = timeToSpawn;
                }
            }
        }
        else
        {
            if(kills == enemiesSpawned)
            {
                wave++;
                StartWave(wave);
            }
        }
    }
}
