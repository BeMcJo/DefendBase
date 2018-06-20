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
    public Dictionary<int, List<List<GameObject>>> pathsBySpawnPoint; // List of paths given a map and based on the starting points
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

    public List<GameObject> CopyList(List<GameObject> path)
    {
        List<GameObject> cpy = new List<GameObject>();
        foreach(GameObject p in path)
        {
            cpy.Add(p);
        }
        return cpy;
    }

    // Recursive DFS for all possible start-to-end routes from starting point(s) sp and its index
    public void FindAllPaths(List<GameObject> path, GameObject sp, int spIndex)
    {
        // Invalid if starting point is null
        if (sp == null)
            return;

        if (sp.tag == "Path")
        {
            // Add current point to pathing list 
            path.Add(sp);
            PlatformPath pp = sp.GetComponent<PlatformPath>();

            // if have destinations, branch and find all paths from those points
            if (pp.destTargets.Count > 0)
            {
                for (int i = 0; i < pp.destTargets.Count; i++)
                {
                    List<GameObject> pathCpy = CopyList(path);
                    FindAllPaths(pathCpy, pp.destTargets[i], spIndex);
                }
            }
            // If no destinations, this is end point. Store current path
            else
            {
                pathsBySpawnPoint[spIndex].Add(path);
            }
        }
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

            bool hasNeighbors = false;

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
                    hasNeighbors = true;
                }
            }

            if (!hasNeighbors)
            {
                print("NODE " + i + " has no nebrs");
            }
            
        }
        //mapContainer.transform.localEulerAngles = GameManager.gm.playerOrientation; // Orient map to face in forward direction as player

        // Generate all possible pathing routes starting at each enemy spawn point
        pathsBySpawnPoint = new Dictionary<int, List<List<GameObject>>>();
        for(int i = 0; i < spawnPoints.Count; i++)
        {
            List<GameObject> path = new List<GameObject>();
            pathsBySpawnPoint.Add(i, new List<List<GameObject>>());
            FindAllPaths(path, spawnPoints[i], i);
        }

        createdMap = true; // Done creating map
    }
}
