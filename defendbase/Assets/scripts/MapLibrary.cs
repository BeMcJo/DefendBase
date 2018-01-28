using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangularNode
{
    public static int nodeCount = 0;
    public int id;
    public List<TriangularNode> neighbors;
    public List<int[]> relations;
    public bool isSpawnPoint;
    public TriangularNode()
    {
        id = nodeCount;
        nodeCount++;
        isSpawnPoint = false;
        neighbors = new List<TriangularNode>();
        relations = new List<int[]>();
    }
}

public class HexNode
{
    public static int nodeCount = 0;
    public HexNode[] neighbors;
    public int id;
    public bool isSpawnPoint;
    public HexNode(HexNode[] nbrs = null)
    {
        id = nodeCount;
        isSpawnPoint = false;
        nodeCount++;
        neighbors = new HexNode[6];
        for (int i = 0; i < 6; i++)
            if (nbrs == null)
                neighbors[i] = null;
            else
                neighbors[i] = nbrs[i];
    }
}

public class Map
{
    //public TriangularNode initialBuildNode;
    public List<HexNode> nodes;
    public Map()
    {
        nodes = new List<HexNode>();
    }
}

public class MapLibrary {
    public static MapLibrary mapLib;
    public static List<Map> maps;
    // Use this for initialization
    void Start() {

    }

    public MapLibrary()
    {
        Debug.Log("map lib");
        if (mapLib != null)
        {
            //Destroy(gameObject);
            return;
        }
        Debug.Log("maplib made");
        mapLib = this;
    }

    public static void Instatiate()
    {
        Debug.Log("Instantiating Map Library");
        maps = new List<Map>();
        CreateMaps();
    }

    static void CreateMaps()
    {
        Debug.Log("Creating Map Blueprints");
        Map newMap = new Map();
        for(int i = 0; i < 16; i++)
            AddHexNode(newMap);
        maps.Add(newMap);
        HexNode node = newMap.nodes[0];
        newMap.nodes[0].neighbors[3] = newMap.nodes[1];
        newMap.nodes[0].isSpawnPoint = true;

        newMap.nodes[1].neighbors[4] = newMap.nodes[2];
        newMap.nodes[1].neighbors[2] = newMap.nodes[3];

        newMap.nodes[2].neighbors[3] = newMap.nodes[4];

        newMap.nodes[3].neighbors[3] = newMap.nodes[5];

        newMap.nodes[4].neighbors[3] = newMap.nodes[6];

        newMap.nodes[5].neighbors[3] = newMap.nodes[7];

        newMap.nodes[6].neighbors[2] = newMap.nodes[8];

        newMap.nodes[7].neighbors[4] = newMap.nodes[8];

        newMap.nodes[8].neighbors[3] = newMap.nodes[9];

        newMap.nodes[9].neighbors[3] = newMap.nodes[10];

        newMap.nodes[10].neighbors[3] = newMap.nodes[11];

        newMap.nodes[11].neighbors[3] = newMap.nodes[12];

        newMap.nodes[12].neighbors[3] = newMap.nodes[13];

        newMap.nodes[13].neighbors[3] = newMap.nodes[14];

        newMap.nodes[14].neighbors[3] = newMap.nodes[15];

        //newMap.nodes[15].neighbors[3] = newMap.nodes[16];

        //newMap.nodes[16].neighbors[3] = newMap.nodes[17];

        //newMap.nodes[17].neighbors[3] = newMap.nodes[18];
        //TriangularNode node = new TriangularNode();

        //int[] relation = new int[3];
        //node.isSpawnPoint = true;
        //newMap.initialBuildNode = node;
        //maps.Add(newMap);
        //AddRelationTo(node, 0, 1, 3);


    }

    static HexNode AddHexNode(Map m)
    {
        HexNode node = new HexNode();
        m.nodes.Add(node);
        return node;
    }

    void AddRelationTo(TriangularNode node,int hex1, int hex2, int side)
    {
        int[] relation = new int[3];
        relation[0] = hex1;
        relation[1] = hex2;
        relation[2] = side;
        node.relations.Add(relation);
    }

	// Update is called once per frame
	void Update () {
		
	}
}
