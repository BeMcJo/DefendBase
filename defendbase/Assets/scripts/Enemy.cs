using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public static int EnemyCount = 0;
    public int health = 2, maxHP = 2, dmg = 1, id, attackCt;
    public float moveSpd = 1f,
                 timeToAttack = 2.5f,
                 atkTimer,
                 attackSpd = 1f;
    public GameObject target;
    public Vector3 targetPos;
    // Use this for initialization
    protected virtual void Start()
    {
        attackCt = 0;
        atkTimer = timeToAttack;
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
        if (!GameManager.gm.inGame)
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
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpd);
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
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpd);
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
        atkTimer = timeToAttack;
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
