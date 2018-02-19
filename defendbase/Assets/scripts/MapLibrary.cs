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

// Node in the logical form of a hexagon, each side mapped as followed
/*
 *           0
 *          ___
 *       5 /   \ 1
 *       4 \___/ 2
 *           3
 */
public class HexNode
{
    public static int nodeCount = 0; // Keep track of # of numbers
    public HexNode[] neighbors; // Represents each of the 6 sides
    public int id;
    public bool isSpawnPoint; // Denotes if this node is a spawn point for enemies
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

// Holds the list of HexNodes that make up the map (pathing for enemies)
public class Map
{
    public List<HexNode> nodes;
    public Map()
    {
        nodes = new List<HexNode>();
    }
}

// List of all the maps for the game
public class MapLibrary {
    public static MapLibrary mapLib; // Public accessor to the maps (used)?
    public static List<Map> maps; // Holds all the maps created

    public MapLibrary()
    {
        if (mapLib != null)
        {
            return;
        }
        mapLib = this;
    }

    // Starts up and creates all the maps
    public static void Instatiate()
    {
        Debug.Log("Instantiating Map Library");
        maps = new List<Map>();
        CreateMaps();
    }

    /*
     * Creates all the maps
     * Map is represented by hexnodes, each hexnode have their neighbors assigned
     * Assigning HexNode A's neighbors[0] to HexNode B means that A directs to B
     * Essentially a digraph
     */
    static void CreateMaps()
    {
        Debug.Log("Creating Map Blueprints");

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        Debug.Log("Creating Map 1");
        Map newMap = new Map();
        // Create 16 HexNodes for Map 1
        for(int i = 0; i < 16; i++)
            AddHexNode(newMap);
        maps.Add(newMap);
        HexNode node = newMap.nodes[0];
        // Assign the neighbors for each HexNode
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
        Debug.Log("Done Creating Map 1");
        /////////////////////////////////////////////////////////////////////////////////////////////////////
    }

    // Creates HexNode and adds to Map m
    static HexNode AddHexNode(Map m)
    {
        HexNode node = new HexNode();
        m.nodes.Add(node);
        return node;
    }

    //Unused
    void AddRelationTo(TriangularNode node,int hex1, int hex2, int side)
    {
        int[] relation = new int[3];
        relation[0] = hex1;
        relation[1] = hex2;
        relation[2] = side;
        node.relations.Add(relation);
    }
}
