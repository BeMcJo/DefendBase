using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour {
    public static MapManager mapManager;
    public float dz = 7f, dx = 6.0625f;
    public GameObject hexPlatformPrefab;
    public GameObject mapContainer;

    public List<GameObject> spawnPoints;
    //public List<GameObject> platforms;// = new List<GameObject>();
    public Dictionary<int, GameObject> platforms;
    private bool createdMap;
	// Use this for initialization
	void Start () {
        if(mapManager != null)
        {
            Destroy(gameObject);
            return;
        }
        mapManager = this;
        DontDestroyOnLoad(mapManager);
        createdMap = false;
        //platforms = new List<GameObject>();
        platforms = new Dictionary<int, GameObject>();
        spawnPoints = new List<GameObject>();
        MapLibrary.Instatiate();
        //map = new GameObject("Map");
        //LoadMap(0);
    }

    private void OnLevelWasLoaded(int level)
    {
        if (this != mapManager)
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
                /*
                // Server
                case 2:
                    LoadServerScene();
                    break;
                // Client
                case 3:
                    LoadClientScene();
                    break;
                // Game (Multiplayer)
                case 4:
                    LoadMultiplayerGameScene();
                    break;*/
        }
    }

    public void LoadMainScene()
    {

    }

    public IEnumerator LoadGameScene()
    {
        mapContainer = new GameObject("MapContainer");
        mapContainer.transform.SetParent(GameObject.Find("Player Rotation").transform);
        mapContainer.transform.localEulerAngles = Vector3.zero;
        yield return new WaitForSeconds(0);
    }

    public void LoadMap(int map)
    {
        platforms.Clear();
        spawnPoints.Clear();
        createdMap = false;
        PlatformPath.PlatformCount = 0;
        Map m = MapLibrary.maps[map];
        for (int i = 0; i < m.nodes.Count; i++)
        {
            platforms.Add(i,Instantiate(hexPlatformPrefab));
            platforms[i].transform.SetParent(mapContainer.transform);
        }
        for (int i = 0; i < m.nodes.Count; i++)
        {
            HexNode node = m.nodes[i];
            PlatformPath src = platforms[i].transform.GetComponent<PlatformPath>();
            if (node.isSpawnPoint)
            {
                spawnPoints.Add(platforms[i]);
            }
            for (int j = 0; j < 6; j++)
            {
                HexNode nbr = node.neighbors[j];
                if(nbr != null)
                {
                    PlatformPath dest = platforms[nbr.id % platforms.Count].transform.GetComponent<PlatformPath>();
                    src.destTargets.Add(dest.gameObject);
                    dest.srcTargets.Add(src.gameObject);
                    float x = dx, z = dz;
                    if(j >= 2 && j <= 4)
                    {
                        z = -z;
                    }
                    if(j != 0 && j != 3)
                    {
                        z /= 2;
                        if (j > 3)
                        {
                            x = -x;
                        }
                    }
                    else
                    {
                        x = 0;
                    }
                    dest.transform.position = src.transform.position + new Vector3(x, 0, z);
                }
            }
        }
        createdMap = true;
        mapContainer.transform.localEulerAngles = GameManager.gm.playerOrientation;
    }

    // Update is called once per frame
    void Update () {
        if (!createdMap)
        {
            return;
        }
        
	}

   
}
