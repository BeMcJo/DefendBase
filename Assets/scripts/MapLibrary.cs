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
    public int neighborReference; // place hexnode with respect to nbr dest hexnode (used ideally when hexnode not referenced as dest node)
    public bool isSpawnPoint; // Denotes if this node is a spawn point for enemies
    public HexNode(HexNode[] nbrs = null)
    {
        id = nodeCount;
        isSpawnPoint = false;
        neighborReference = -1;
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
     * MAKE SURE WHEN DIRECTING ONE HEXNODE TO ANOTHER, THE HEXNODE IS SETUP BEFORE THE NBR
     * TO ENSURE APPROPRIATE POSITIONING. IDEALLY MAKE DEST HEXNODE A NODE OF HIGHER ID THAN
     * ITS SRC HEXNODE
     */
    static void CreateMaps()
    {
        Debug.Log("Creating Map Blueprints");

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        Debug.Log("Creating Map 1");
        Map newMap = new Map();
        // Create 16 HexNodes for Map 1
        for(int i = 0; i < 64; i++)
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

        //// Expansion RHS

        newMap.nodes[3].neighbors[2] = newMap.nodes[16];

        newMap.nodes[16].neighbors[1] = newMap.nodes[17];

        newMap.nodes[17].neighbors[2] = newMap.nodes[18];

        newMap.nodes[18].neighbors[2] = newMap.nodes[19];

        newMap.nodes[19].neighbors[3] = newMap.nodes[20];

        newMap.nodes[20].neighbors[4] = newMap.nodes[21];

        newMap.nodes[21].neighbors[4] = newMap.nodes[22];

        newMap.nodes[22].neighbors[4] = newMap.nodes[23];

        newMap.nodes[23].neighbors[4] = newMap.nodes[24];

        newMap.nodes[24].neighbors[4] = newMap.nodes[10];

        newMap.nodes[20].neighbors[3] = newMap.nodes[25];

        newMap.nodes[25].neighbors[3] = newMap.nodes[26];

        newMap.nodes[26].neighbors[3] = newMap.nodes[27];

        newMap.nodes[27].neighbors[2] = newMap.nodes[28];

        newMap.nodes[28].neighbors[3] = newMap.nodes[29];

        newMap.nodes[29].neighbors[3] = newMap.nodes[30];

        newMap.nodes[30].neighbors[4] = newMap.nodes[31];

        newMap.nodes[31].neighbors[4] = newMap.nodes[32];

        newMap.nodes[32].neighbors[5] = newMap.nodes[33];

        newMap.nodes[33].neighbors[5] = newMap.nodes[34];

        newMap.nodes[34].neighbors[4] = newMap.nodes[35];

        newMap.nodes[35].neighbors[4] = newMap.nodes[14];

        ////// Expansion LHS
        
        newMap.nodes[6].neighbors[4] = newMap.nodes[36];

        newMap.nodes[36].neighbors[4] = newMap.nodes[37];

        newMap.nodes[37].neighbors[5] = newMap.nodes[38];

        newMap.nodes[38].neighbors[4] = newMap.nodes[39];

        newMap.nodes[39].neighbors[3] = newMap.nodes[40];

        newMap.nodes[40].neighbors[3] = newMap.nodes[41];

        newMap.nodes[41].neighbors[2] = newMap.nodes[42];

        newMap.nodes[42].neighbors[2] = newMap.nodes[45];

        newMap.nodes[10].neighbors[4] = newMap.nodes[43];

        newMap.nodes[43].neighbors[4] = newMap.nodes[44];

        newMap.nodes[44].neighbors[4] = newMap.nodes[45];


        newMap.nodes[45].neighbors[3] = newMap.nodes[46];

        newMap.nodes[46].neighbors[2] = newMap.nodes[47];

        newMap.nodes[47].neighbors[2] = newMap.nodes[48];

        //newMap.nodes[48].neighbors[3] = newMap.nodes[49];

        //newMap.nodes[49].neighbors[2] = newMap.nodes[15];

        newMap.nodes[48].neighbors[2] = newMap.nodes[14];

        newMap.nodes[41].neighbors[3] = newMap.nodes[50];

        newMap.nodes[50].neighbors[4] = newMap.nodes[51];

        newMap.nodes[51].neighbors[3] = newMap.nodes[52];

        newMap.nodes[52].neighbors[2] = newMap.nodes[53];

        newMap.nodes[53].neighbors[1] = newMap.nodes[54];

        newMap.nodes[54].neighbors[1] = newMap.nodes[46];

        ////// Spawn Point 2

        newMap.nodes[55].neighbors[4] = newMap.nodes[20];
        newMap.nodes[55].neighborReference = 4;

        newMap.nodes[56].neighbors[4] = newMap.nodes[55];
        newMap.nodes[56].neighborReference = 4;

        newMap.nodes[57].neighbors[3] = newMap.nodes[56];
        newMap.nodes[57].neighborReference = 3;

        newMap.nodes[58].neighbors[3] = newMap.nodes[57];
        newMap.nodes[58].neighborReference = 3;
        newMap.nodes[58].isSpawnPoint = true;

        ////// Spawn Point 3

        newMap.nodes[59].neighbors[2] = newMap.nodes[4];
        newMap.nodes[59].neighborReference = 2;

        newMap.nodes[60].neighbors[2] = newMap.nodes[59];
        newMap.nodes[60].neighborReference = 2;
        
        newMap.nodes[61].neighbors[2] = newMap.nodes[60];
        newMap.nodes[61].neighborReference = 2;
        newMap.nodes[61].isSpawnPoint = true;

        newMap.nodes[61].neighbors[3] = newMap.nodes[62];

        newMap.nodes[62].neighbors[3] = newMap.nodes[63];

        newMap.nodes[63].neighbors[3] = newMap.nodes[38];
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
