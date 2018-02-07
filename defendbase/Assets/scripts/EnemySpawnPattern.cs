using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternBuff
{
    float spawnCt,
          spawnFreq,
          spawnTime,
          atk;
}

// Each pattern corresponds to one wave of enemies in game
public class Pattern
{
    // Number of enemies to spawn in the spawning interval
    public List<List<int>> spawnCts;
    // Time to spawn the enemies in the spawning interval
    public List<float> spawnTimes;
    // How long to wait before spawning the next interval if existent
    public List<float> spawnFreqs;
    // How long before wave is officially finished after spawning all enemies
    public float endIterationTime;
    // How many times to spawn current pattern for the wave
    public int iterations;

    public Pattern()
    {
        spawnCts = new List<List<int>>();
        spawnTimes = new List<float>();
        spawnFreqs = new List<float>();
        endIterationTime = 5f;
        iterations = 1;
    }
}

public class EnemySpawnPattern {
    // List of all patterns depending on how many spawn points exist on the map
    public static List<List<Pattern>> patternsBySpawnPointCt;

    // Initialize all the patterns used for the game.
    public static void InstantiatePatterns()
    {
        Debug.Log("Instantiate Enemy Spawn Patterns");
        patternsBySpawnPointCt = new List<List<Pattern>>();

        List<Pattern> patterns = new List<Pattern>();
        patternsBySpawnPointCt.Add(patterns);

        //////////////////////////////////////////////////
        // Wave 1
        Pattern pattern = new Pattern();
        patterns.Add(pattern);
        List<int> spawnCt = new List<int>();
        for (int i = 0; i < 5; i++)
            spawnCt.Add(1);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(50f);

        //////////////////////////////////////////////////
        // Wave 2

        pattern = new Pattern();
        patterns.Add(pattern);
        spawnCt = new List<int>();
        for (int i = 0; i < 5; i++)
            spawnCt.Add(1);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(40f);

        //////////////////////////////////////////////////
        // Wave 3

        pattern = new Pattern();
        patterns.Add(pattern);
        spawnCt = new List<int>();
        for (int i = 0; i < 2; i++)
            spawnCt.Add(1);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(8f);
        pattern.endIterationTime = 5f;
        pattern.iterations = 3;

        //////////////////////////////////////////////////
        // Wave 4

        pattern = new Pattern();
        patterns.Add(pattern);
        spawnCt = new List<int>();
        for (int i = 0; i < 3; i++)
            spawnCt.Add(1);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(9f);
        pattern.endIterationTime = 10f;
        pattern.spawnFreqs.Add(6f);
        spawnCt = new List<int>();
        for (int i = 0; i < 5; i++)
            spawnCt.Add(1);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(40f);
        pattern.iterations = 1;

        //////////////////////////////////////////////////
        // Wave 5

        pattern = new Pattern();
        patterns.Add(pattern);
        spawnCt = new List<int>();
        for (int i = 0; i < 4; i++)
            spawnCt.Add(1);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(4f);
        pattern.spawnFreqs.Add(8f);

        spawnCt = new List<int>();
        for (int i = 0; i < 3; i++)
            spawnCt.Add(1);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(10f);

        pattern.endIterationTime = 5f;
        pattern.iterations = 2;

        //////////////////////////////////////////////////
        // Wave 6

        pattern = new Pattern();
        patterns.Add(pattern);
        spawnCt = new List<int>();
        for (int i = 0; i < 5; i++)
            spawnCt.Add(1);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(5f);
        pattern.spawnFreqs.Add(4f);

        spawnCt = new List<int>();
        for (int i = 0; i < 4; i++)
            spawnCt.Add(1);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(8f);

        pattern.endIterationTime = 5f;
        pattern.iterations = 1;

        //////////////////////////////////////////////////
        // Wave 7

        pattern = new Pattern();
        patterns.Add(pattern);
        spawnCt = new List<int>();
        for (int i = 0; i < 3; i++)
            spawnCt.Add(1);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(3f);

        pattern.endIterationTime = 4f;
        pattern.iterations = 5;

        //////////////////////////////////////////////////
        // Wave 8

        pattern = new Pattern();
        patterns.Add(pattern);
        spawnCt = new List<int>();
        for (int i = 0; i < 4; i++)
            spawnCt.Add(1);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(4f);

        pattern.endIterationTime = 4f;
        pattern.iterations = 5;

        //////////////////////////////////////////////////
        // Wave 9

        pattern = new Pattern();
        patterns.Add(pattern);
        spawnCt = new List<int>();
        for (int i = 0; i < 8; i++)
            spawnCt.Add(1);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(8f);

        pattern.spawnFreqs.Add(3f);
        spawnCt = new List<int>();
        for (int i = 0; i < 5; i++)
            spawnCt.Add(1);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(10f);

        pattern.endIterationTime = 4f;
        pattern.iterations = 2;

        //////////////////////////////////////////////////
        // Wave 10

        pattern = new Pattern();
        patterns.Add(pattern);
        spawnCt = new List<int>();
        for (int i = 0; i < 6; i++)
            spawnCt.Add(1);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(6f);

        pattern.endIterationTime = 5f;
        pattern.iterations = 6;



    }
}
