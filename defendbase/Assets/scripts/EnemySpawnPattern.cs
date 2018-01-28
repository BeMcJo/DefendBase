using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pattern
{
    public List<List<int>> spawnCts;
    public List<float> spawnTimes;
    public List<float> spawnFreqs;
    public float endIterationTime;
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
    public static List<List<Pattern>> patternsBySpawnPointCt;
    public static void InstantiatePatterns()
    {
        Debug.Log("Instantiate Enemy Spawn Patterns");
        patternsBySpawnPointCt = new List<List<Pattern>>();

        List<Pattern> patterns = new List<Pattern>();
        patternsBySpawnPointCt.Add(patterns);

        Pattern pattern = new Pattern();
        patterns.Add(pattern);
        List<int> spawnCt = new List<int>();
        for (int i = 0; i < 5; i++)
            spawnCt.Add(1);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(50f);

        //////////////////////////////////////////////////
        //patterns = new List<Pattern>();
        //patternsBySpawnPointCt.Add(patterns);

        pattern = new Pattern();
        patterns.Add(pattern);
        spawnCt = new List<int>();
        for (int i = 0; i < 5; i++)
            spawnCt.Add(1);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(40f);

        //////////////////////////////////////////////////
        //patterns = new List<Pattern>();
        //patternsBySpawnPointCt.Add(patterns);

        pattern = new Pattern();
        patterns.Add(pattern);
        spawnCt = new List<int>();
        for (int i = 0; i < 2; i++)
            spawnCt.Add(1);
        pattern.spawnCts.Add(spawnCt);
        pattern.spawnTimes.Add(8f);
        pattern.endIterationTime = 10f;
        pattern.iterations = 3;

        //////////////////////////////////////////////////
        //patterns = new List<Pattern>();
        //patternsBySpawnPointCt.Add(patterns);

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
        //patterns = new List<Pattern>();
        //patternsBySpawnPointCt.Add(patterns);

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

        pattern.endIterationTime = 10f;
        pattern.iterations = 2;

    }
}
