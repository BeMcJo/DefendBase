using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Enables versatility with creating enemies and their stats details
 */
public class EnemyStats
{
    public int maxHP, dmg;
    public float moveSpd, atkSpd;
    public EnemyStats(int hp, int dmg, float ms, float atkSpd)
    {
        maxHP = hp;
        this.dmg = dmg;
        moveSpd = ms;
        this.atkSpd = atkSpd;
    }
}
public class Enemy : MonoBehaviour
{
    public static int EnemyCount = 0; // Keeps track of enemies created during game session
    // List of stats per level
    public static EnemyStats[] difficulties = new EnemyStats[] {
        new EnemyStats(1, 0, 1, 1),
        new EnemyStats(2, 2, 1.2f, 1.2f),
        new EnemyStats(2, 2, 1.25f, 1.2f),
        new EnemyStats(3, 2, 1.5f, 1.2f),
        new EnemyStats(3, 2, 1.5f, 1.2f)
    };
    public int health = 2, // Current health 
               maxHP = 2, // Max health
               dmg = 1, // How much enemy can inflict to others
               id, // Unique way to identify this enemy
               attackCt, // Keeps track of attack for multiplayer synchronization
               level; // Used to determine what the stats are
    public float originalMoveSpd = 0.075f, // Default move speed
                 effectiveMoveSpd, // Move speed calculated and used 
                 originalTimeToAttack = 2.5f, // Default amount of time before inflicting damage
                 effectiveTimeToAttack, // Time calculated and used
                 atkTimer, // Used to check if enemy can inflict damage
                 originalAttackSpd = 1f, // Default Attack Speed
                 effectiveAttackSpd; // Attack speed calculated and used
    public GameObject target; // What the enemy prioritizes
    public Vector3 targetPos; // What the enemy faces and moves to

    // Used to create the enemy and identify it
    public static void AssignEnemy(Enemy e)
    {
        e.id = EnemyCount;
        EnemyCount++;
        if (GameManager.gm.enemies != null)
        {
            GameManager.gm.enemies[e.id] = e;
        }
        e.name = "Enemy " + e.id;
    }
    // Use this for initialization
    protected virtual void Start()
    {
        maxHP = difficulties[level].maxHP;
        health = maxHP;
        dmg = difficulties[level].dmg;
        effectiveMoveSpd = originalMoveSpd * difficulties[level].moveSpd;
        effectiveTimeToAttack = originalTimeToAttack / difficulties[level].atkSpd;
        atkTimer = effectiveTimeToAttack;
        effectiveAttackSpd = originalAttackSpd / difficulties[level].atkSpd;
        attackCt = 0;
    }

    // Format: ENEMY|enemy id|enemy relative pos to target|target tag|target id|
    public string NetworkInformation()
    {
        string msg = "";

        msg = "ENEMYINFO|" + id + "|";

        // Set parent to target temporarily to get relative position to target
        Transform parent = transform.parent;
        transform.SetParent(target.transform);
        msg += "" + transform.localPosition.x + "," + transform.localPosition.y + "," + transform.localPosition.z + "|";
        transform.SetParent(parent);

        string tag = target.gameObject.tag;
        msg += tag + "|";
        switch(tag) 
        {
            case "Path":
                msg += target.transform.GetComponent<PlatformPath>().id + "|";
                break;
            case "Objective":
                msg += target.transform.GetComponent<Objective>().id + "|";
                break;
        }
        return msg;
    }

    // Extract Network Information
    public void SetNetworkInformation(string[] data)
    {
        string[] xyz = data[2].Split(',');
        Vector3 pos = new Vector3(float.Parse(xyz[0]), float.Parse(xyz[1]), float.Parse(xyz[2]));
        int tid = int.Parse(data[4]);
        string tag = data[3];

        switch (tag){
            case "Path":
                SetTarget(MapManager.mapManager.platforms[tid].gameObject);
                break;

            case "Objective":
                SetTarget(GameManager.gm.objective);
                //target =;
                break;
        }
        Transform parent = transform.parent;
        transform.SetParent(target.transform);
        transform.localPosition = pos;
        transform.SetParent(parent);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!GameManager.gm.inGame || GameManager.gm.gameOver)
            return;

        if (NetworkManager.nm.isStarted && NetworkManager.nm.isDisconnected)
        {
            return;
        }

        if (health <= 0)
        {
            Die();
            return;
        }

        if (target != null)
        {
            float dist = Vector3.Distance(transform.position, targetPos);
            // Move towards target if it is a Path
            if (target.tag == "Path")
            {
                if (dist <= .1f)
                {
                    PlatformPath p = target.transform.GetComponent<PlatformPath>();
                    target = null;
                    // Get new path if reached destination
                    if (p.destTargets.Count > 0)
                    {
                        SetTarget(p.destTargets[Random.Range(0, p.destTargets.Count)]);
                    }
                    // No more paths, target objective
                    else
                    {
                        SetTarget(GameManager.gm.objective);
                    }
                }
                // Move and face towards target
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, effectiveMoveSpd);
                    transform.LookAt(targetPos);
                }
            }
            // Move towards objective and attack it if in range
            else if (target.tag == "Objective")
            {
                if (dist <= 2f)
                {
                    Attack(target);
                }
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, effectiveMoveSpd);
                    transform.LookAt(targetPos);
                }
            }
        }
        // No target? Find objective to attack then
        else
        {
            SetTarget(GameManager.gm.objective);
        }
    }

    public IEnumerator IndicateHasBeenHit()
    {
        Renderer r = GetComponent<Renderer>();
        Color prevColor = r.material.color;
        r.material.color = Color.red;
        Debug.Log(1);
        yield return new WaitForSeconds(.15f);
        r.material.color = prevColor;
        Debug.Log(2);
    }

    public void OnHit()
    {
        StartCoroutine(IndicateHasBeenHit());
    }

    // Attack target
    public virtual void Attack(GameObject target)
    {
        atkTimer -= Time.deltaTime;
        if (atkTimer > 0)
            return;
        // If multiplayer, synchronize the attack to prevent duplicate attacks
        if (NetworkManager.nm.isStarted)
        {
            NetworkManager.nm.QueueAttackOnObject(gameObject, target);
        }
        // Not online, just attack target
        else
        {
            if (target.tag == "Objective")
            {
                Objective o = target.transform.GetComponent<Objective>();
                o.TakeDamage(dmg);
            }
        }
        atkTimer = effectiveTimeToAttack;
    }

    // Update game rewards upon death
    public virtual void Die()
    {
        GameManager.gm.AddScore(50);
        GameManager.gm.kills++;
        GameManager.gm.UpdateInGameCurrency(1);
        // If online, remove enemy attack that was queued
        if (NetworkManager.nm.isStarted)
        {
            NetworkManager.nm.RemoveEnemyAttacks(id);
        }
        GameManager.gm.enemies.Remove(id);
        Destroy(gameObject);
    }

    // Creates target to move towards and face properly then assigns target to chase/attack
    public virtual void SetTarget(GameObject g)
    {
        target = g;
        Vector3 pos = new Vector3(
                        g.transform.position.x,
                        transform.position.y,
                        g.transform.position.z
                       );
        targetPos = pos;
    }

    // Inflict damage to health
    public virtual bool TakeDamage(int dmg)
    {
        health -= dmg;
        return true;
    }
}
