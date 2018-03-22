using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour {
    public static MapManager mapManager; // Singleton
    public float dz = 7f, // z offset to accurately align HexNodes
                 dx = 6.0625f; // x offset to accurately align HexNodes
    public GameObject hexPlatformPrefab; // HexNode object
    public GameObject mapContainer; // Holds all the HexNode objects representing the map

    public List<GameObject> spawnPoints; // List of areas where enemy can spawn
    public Dictionary<int, GameObject> platforms; // Keeps track of Platforms by their ID
    private bool createdMap; // Used to denote if map is done creating
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
        platforms = new Dictionary<int, GameObject>();
        spawnPoints = new List<GameObject>();
        MapLibrary.Instatiate();
    }

    private void OnLevelWasLoaded(int level)
    {
        if (this != mapManager)
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
        }
    }

    public void LoadMainScene()
    {

    }

    public IEnumerator LoadGameScene()
    {
        mapContainer = new GameObject("MapContainer");
        // Align map to face in forward direction as player is oriented
        mapContainer.transform.SetParent(GameObject.Find("Player Rotation").transform);
        mapContainer.transform.localEulerAngles = Vector3.zero;
        yield return new WaitForSeconds(0);
    }

    // Instantiates the platforms from the Map Library based on the map
    public void LoadMap(int map)
    {
        platforms.Clear();
        spawnPoints.Clear();
        createdMap = false;
        PlatformPath.PlatformCount = 0; // Reset platform count
        Map m = MapLibrary.maps[map]; // Get the map

        // Instantiate all the platforms and keep track of them by their ID
        for (int i = 0; i < m.nodes.Count; i++)
        {
            platforms.Add(i,Instantiate(hexPlatformPrefab));
            platforms[i].name = "Path " + i;
            platforms[i].transform.SetParent(mapContainer.transform);
        }

        // For each platform,...
        for (int i = 0; i < m.nodes.Count; i++)
        {
            HexNode node = m.nodes[i];
            PlatformPath src = platforms[i].transform.GetComponent<PlatformPath>();
            // Check if platform is a spawn point and add to list if true
            if (node.isSpawnPoint)
            {
                spawnPoints.Add(platforms[i]);
            }

            // Check if platform needs a reference positioning
            if(node.neighborReference != -1)
            {
                int j = node.neighborReference;
                float x = dx, z = dz;
                j = (j + 3) % 6;
                if (j >= 2 && j <= 4)
                {
                    z = -z;
                }
                if (j != 0 && j != 3)
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
                src.transform.position = platforms[node.neighbors[node.neighborReference].id].transform.position + new Vector3(x, 0, z);
            }

            // Check each neighbor to bind the hexNodes as source and destination nodes (creating digraph)
            for (int j = 0; j < 6; j++)
            {
                HexNode nbr = node.neighbors[j];
                // If neighbor exist, align neighbor with respect to current hexNode
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
        mapContainer.transform.localEulerAngles = GameManager.gm.playerOrientation; // Orient map to face in forward direction as player
        createdMap = true; // Done creating map
    }
}
