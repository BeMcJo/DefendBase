using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : Enemy {
    protected override void Awake()
    {
        base.Awake();
        ename = "slime";
    }
    // Use this for initialization
    protected override void Start()
    {
        enemyID = 3;
        base.Start();
    }

}
