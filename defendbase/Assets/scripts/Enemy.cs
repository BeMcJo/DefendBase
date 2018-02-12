using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public static int EnemyCount = 0;
    public static EnemyStats[] difficulties = new EnemyStats[] {
        new EnemyStats(1, 1, 1, 1),
        new EnemyStats(2, 2, 1.2f, 1.2f),
        new EnemyStats(2, 2, 1.25f, 1.2f),
        new EnemyStats(3, 2, 1.5f, 1.2f),
        new EnemyStats(3, 2, 1.5f, 1.2f)
    };
    public int health = 2, maxHP = 2, dmg = 1, id, attackCt, level;
    public float originalMoveSpd = 0.075f,
                 effectiveMoveSpd,
                 originalTimeToAttack = 2.5f,
                 effectiveTimeToAttack,
                 atkTimer,
                 originalAttackSpd = 1f,
                 effectiveAttackSpd;
    public GameObject target;
    public Vector3 targetPos;
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
        //atkTimer = timeToAttack;
        id = EnemyCount;
        EnemyCount++;
        if (GameManager.gm.enemies != null)
        {
            GameManager.gm.enemies[id] = this;
        }
        //target = GameObject.Find("Gate");
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!GameManager.gm.inGame || GameManager.gm.gameOver)
            return;
        if (health <= 0)
        {
            Die();
            return;
        }
        if (target != null)
        {
            
            float dist = Vector3.Distance(transform.position, targetPos);
            if (target.tag == "Path")
            {
                // Debug.Log("paths");

                if (dist <= .1f)
                {
                    //Debug.Log("close");

                    PlatformPath p = target.transform.GetComponent<PlatformPath>();
                    target = null;

                    if (p.destTargets.Count > 0)
                    {
                        //   Debug.Log("another oen");
                        SetTarget(p.destTargets[Random.Range(0, p.destTargets.Count)]);
                    }
                }
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, effectiveMoveSpd);
                    transform.LookAt(targetPos);
                }
            }
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
        else
        {
            SetTarget(GameManager.gm.objective);
        }
    }

    public virtual void Attack(GameObject target)
    {
        atkTimer -= Time.deltaTime;
        if (atkTimer > 0)
            return;
        Debug.Log("attacking" + target.name);
        if (NetworkManager.nm.isStarted)
        {
            NetworkManager.nm.QueueAttackOnObject(gameObject, target);
            //NetworkManager.nm.NotifyObjectDamagedBy(target, gameObject);
        }
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

    public virtual void Die()
    {
        GameManager.gm.AddScore(50);
        GameManager.gm.kills++;
        if (NetworkManager.nm.isStarted)
        {
            NetworkManager.nm.RemoveEnemyAttacks(id);
        }
        GameManager.gm.enemies.Remove(id);
        Destroy(gameObject);
    }

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

    public virtual bool TakeDamage(int dmg)
    {
        health -= dmg;
        return true;
    }
}
