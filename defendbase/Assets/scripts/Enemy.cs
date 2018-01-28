using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health = 2, maxHP = 2, dmg = 1;
    public float moveSpd = 1f,
                 timeToAttack = 2.5f,
                 atkTimer,
                 attackSpd = 1f;
    public GameObject target;
    public Vector3 targetPos;
    // Use this for initialization
    void Start()
    {
        atkTimer = timeToAttack;
        //target = GameObject.Find("Gate");
    }

    // Update is called once per frame
    void Update()
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
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpd);
            transform.LookAt(targetPos);
            float dist = Vector3.Distance(transform.position, targetPos);
            if(dist <= .1f)
            {
                //Debug.Log("close");
                if(target.tag == "Path")
                {
                   // Debug.Log("paths");
                    PlatformPath p = target.transform.GetComponent<PlatformPath>();
                    target = null;
                    
                    if(p.destTargets.Count > 0)
                    {
                     //   Debug.Log("another oen");
                        SetTarget(p.destTargets[Random.Range(0, p.destTargets.Count)]);
                    }
                } else if(target.tag == "Objective")
                {
                    Attack(target);
                }
            }
        }
        else
        {
            SetTarget(GameManager.gm.objective);
        }
    }

    public void Attack(GameObject target)
    {
        atkTimer -= Time.deltaTime;
        if (atkTimer > 0)
            return;
        Debug.Log("attacking" + target.name);
        if (target.tag == "Objective")
        {
            Objective o = target.transform.GetComponent<Objective>();
            o.TakeDamage(dmg);
        }
        atkTimer = timeToAttack;
    }

    public void Die()
    {
        GameManager.gm.AddScore(50);
        GameManager.gm.kills++;
        Destroy(gameObject);
    }

    public void SetTarget(GameObject g)
    {
        target = g;
        Vector3 pos = new Vector3(
                        g.transform.position.x,
                        transform.position.y,
                        g.transform.position.z
                       );
        targetPos = pos;
    }

    public void TakeDamage(int dmg)
    {
        health -= dmg;
    }
}
