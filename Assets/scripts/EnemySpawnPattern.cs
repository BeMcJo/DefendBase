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
    // Number of enemies to spawn in the spawning interval. Each value represents
    // spawning a certain enemy
    public List<List<int>> spawnCts;
    // Assign each respective spawned enemy their levels (difficulty stats)
    public List<List<int>> enemyLvls;
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
        enemyLvls = new List<List<int>>();
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
        // Create Pattern for Maps with 1 Spawn Point
        List<Pattern> patterns = new List<Pattern>();
        patternsBySpawnPointCt.Add(patterns);

        //////////////////////////////////////////////////
        // Wave 1
        Pattern pattern = new Pattern();
        patterns.Add(pattern);
        List<int> enemyLvls = new List<int>();
        List<int> spawnCt = new List<int>();
        for (int i = 0; i < 3; i++)
        //for (int i = 0; i < 10; i++)
        {
            spawnCt.Add(0);
            enemyLvls.Add(0);
        }
        pattern.enemyLvls.Add(enemyLvls);
        pattern.spawnCts.Add(spawnCt);
        //pattern.spawnTimes.Add(10f);
        pattern.spawnTimes.Add(30f);

        //////////////////////////////////////////////////
        // Wave 2

        pattern = new Pattern();
        patterns.Add(pattern);
        enemyLvls = new List<int>();
        spawnCt = new List<int>();
        for (int i = 0; i < 5; i++)
        {
            spawnCt.Add(0);
            enemyLvls.Add(0);
        }
        pattern.enemyLvls.Add(enemyLvls);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(40f);

        //////////////////////////////////////////////////
        // Wave 3

        pattern = new Pattern();
        patterns.Add(pattern);
        enemyLvls = new List<int>();
        spawnCt = new List<int>();
        for (int i = 0; i < 2; i++)
        {
            spawnCt.Add(0);
            enemyLvls.Add(0);
        }
        pattern.enemyLvls.Add(enemyLvls);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(8f);
        pattern.endIterationTime = 5f;
        pattern.iterations = 3;

        //////////////////////////////////////////////////
        // Wave 4

        pattern = new Pattern();
        patterns.Add(pattern);
        enemyLvls = new List<int>();
        spawnCt = new List<int>();
        
        for (int i = 0; i < 3; i++)
        {
            spawnCt.Add(0);
            enemyLvls.Add(0);
        }
        pattern.enemyLvls.Add(enemyLvls);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(9f);
        pattern.endIterationTime = 10f;
        pattern.spawnFreqs.Add(6f);

        spawnCt = new List<int>();
        enemyLvls = new List<int>();
        for (int i = 0; i < 5; i++)
        {
            spawnCt.Add(1);
            enemyLvls.Add(0);
        }
        pattern.enemyLvls.Add(enemyLvls);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(40f);
        pattern.iterations = 1;

        //////////////////////////////////////////////////
        // Wave 5

        /*pattern = new Pattern();
        patterns.Add(pattern);
        spawnCt = new List<int>();
        for (int i = 0; i < 1; i++)
            spawnCt.Add(1);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(1f);
        */
        // /*
        pattern = new Pattern();
        patterns.Add(pattern);
        enemyLvls = new List<int>();
        spawnCt = new List<int>();
        for (int i = 0; i < 4; i++)
        {
            spawnCt.Add(0);
            enemyLvls.Add(0);
        }
        spawnCt.Add(1);
        enemyLvls.Add(0);
        pattern.enemyLvls.Add(enemyLvls);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(4f);
        pattern.spawnFreqs.Add(8f);
        
        spawnCt = new List<int>();
        enemyLvls = new List<int>();


        spawnCt.Add(0);
        enemyLvls.Add(0);
        spawnCt.Add(1);
        enemyLvls.Add(0);
        spawnCt.Add(0);
        enemyLvls.Add(0);

        pattern.enemyLvls.Add(enemyLvls);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(10f);

        pattern.endIterationTime = 5f;
        pattern.iterations = 2;
        //*/

        //////////////////////////////////////////////////
        // Wave 6

        pattern = new Pattern();
        patterns.Add(pattern);
        enemyLvls = new List<int>();
        spawnCt = new List<int>();
        for (int i = 0; i < 5; i++)
        {
            spawnCt.Add(0);
            enemyLvls.Add(0);
        }
        pattern.enemyLvls.Add(enemyLvls);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(5f);
        pattern.spawnFreqs.Add(4f);

        spawnCt = new List<int>();
        enemyLvls = new List<int>();
        for (int i = 0; i < 4; i++)
        {
            spawnCt.Add(1);
            enemyLvls.Add(0);
        }
        pattern.enemyLvls.Add(enemyLvls);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(8f);

        pattern.endIterationTime = 5f;
        pattern.iterations = 1;

        //////////////////////////////////////////////////
        // Wave 7

        pattern = new Pattern();
        patterns.Add(pattern);
        enemyLvls = new List<int>();
        spawnCt = new List<int>();
        for (int i = 0; i < 2; i++)
        {
            spawnCt.Add(1);
            enemyLvls.Add(0);
        }
        pattern.enemyLvls.Add(enemyLvls);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(3f);

        pattern.endIterationTime = 4f;
        pattern.iterations = 5;

        //////////////////////////////////////////////////
        // Wave 8

        pattern = new Pattern();
        patterns.Add(pattern);
        enemyLvls = new List<int>();
        spawnCt = new List<int>();
        for (int i = 0; i < 2; i++)
        {
            spawnCt.Add(1);
            enemyLvls.Add(0);
            spawnCt.Add(0);
            enemyLvls.Add(0);
        }
        pattern.enemyLvls.Add(enemyLvls);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(4f);

        pattern.endIterationTime = 4f;
        pattern.iterations = 5;

        //////////////////////////////////////////////////
        // Wave 9

        pattern = new Pattern();
        patterns.Add(pattern);
        enemyLvls = new List<int>();
        spawnCt = new List<int>();
        for (int i = 0; i < 4; i++)
        {
            spawnCt.Add(1);
            enemyLvls.Add(0);
            spawnCt.Add(0);
            enemyLvls.Add(0);
        }
        pattern.enemyLvls.Add(enemyLvls);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(8f);

        pattern.spawnFreqs.Add(3f);
        spawnCt = new List<int>();
        for (int i = 0; i < 2; i++)
        {
            spawnCt.Add(0);
            enemyLvls.Add(0);
            spawnCt.Add(0);
            enemyLvls.Add(0);
            spawnCt.Add(1);
            enemyLvls.Add(0);
        }
        pattern.enemyLvls.Add(enemyLvls);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(10f);

        pattern.endIterationTime = 4f;
        pattern.iterations = 2;

        //////////////////////////////////////////////////
        // Wave 10

        pattern = new Pattern();
        patterns.Add(pattern);
        enemyLvls = new List<int>();
        spawnCt = new List<int>();
        for (int i = 0; i < 1; i++)
        {
            spawnCt.Add(0);
            enemyLvls.Add(0);
            spawnCt.Add(0);
            enemyLvls.Add(0);
            spawnCt.Add(0);
            enemyLvls.Add(0);

            spawnCt.Add(1);
            enemyLvls.Add(0);
            spawnCt.Add(1);
            enemyLvls.Add(0);
            spawnCt.Add(1);
            enemyLvls.Add(0);
        }
        pattern.enemyLvls.Add(enemyLvls);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(12f);

        pattern.endIterationTime = 9f;
        pattern.iterations = 5;

        //////////////////////////////////////////////////
        // Wave 11

        pattern = new Pattern();
        patterns.Add(pattern);
        enemyLvls = new List<int>();
        spawnCt = new List<int>();
        for (int i = 0; i < 2; i++)
        {
            spawnCt.Add(0);
            enemyLvls.Add(1);
            spawnCt.Add(1);
            enemyLvls.Add(1);
        }
        pattern.enemyLvls.Add(enemyLvls);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(20f);

        pattern.endIterationTime = 5f;
        pattern.iterations = 2;

        //////////////////////////////////////////////////
        // Wave 12

        pattern = new Pattern();
        patterns.Add(pattern);
        enemyLvls = new List<int>();
        spawnCt = new List<int>();

        spawnCt.Add(0);
        enemyLvls.Add(1);
        for (int i = 0; i < 2; i++)
        {
            spawnCt.Add(1);
            enemyLvls.Add(1);
        }
        pattern.enemyLvls.Add(enemyLvls);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(20f);

        pattern.endIterationTime = 3.5f;
        pattern.iterations = 2;

        //////////////////////////////////////////////////
        // Wave 13

        pattern = new Pattern();
        patterns.Add(pattern);
        enemyLvls = new List<int>();
        spawnCt = new List<int>();

        spawnCt.Add(2);
        enemyLvls.Add(0);
        for (int i = 0; i < 2; i++)
        {
            spawnCt.Add(0);
            enemyLvls.Add(1);
            spawnCt.Add(1);
            enemyLvls.Add(1);
        }
        pattern.enemyLvls.Add(enemyLvls);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(14f);

        pattern.endIterationTime = 4f;
        pattern.iterations = 2;

        //////////////////////////////////////////////////
        // Wave 14

        pattern = new Pattern();
        patterns.Add(pattern);
        enemyLvls = new List<int>();
        spawnCt = new List<int>();

        for (int i = 0; i < 3; i++)
        {
            spawnCt.Add(2);
            enemyLvls.Add(0);
            spawnCt.Add(0);
            enemyLvls.Add((1+i) % 2);
            spawnCt.Add(1);
            enemyLvls.Add(1);
        }
        pattern.enemyLvls.Add(enemyLvls);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(27f);

        pattern.endIterationTime = 6f;
        pattern.iterations = 2;


        //////////////////////////////////////////////////
        // Wave 15

        pattern = new Pattern();
        patterns.Add(pattern);
        enemyLvls = new List<int>();
        spawnCt = new List<int>();

        for (int i = 0; i < 2; i++)
        {
            spawnCt.Add(2);
            enemyLvls.Add(0);
            spawnCt.Add(2);
            enemyLvls.Add(0);
            spawnCt.Add(2);
            enemyLvls.Add(0);

            spawnCt.Add(1);
            enemyLvls.Add(1);
            spawnCt.Add(1);
            enemyLvls.Add(1);
        }
        pattern.enemyLvls.Add(enemyLvls);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(10f);

        pattern.endIterationTime = 5f;
        pattern.iterations = 3;


    }
}
