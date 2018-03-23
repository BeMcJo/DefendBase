using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flyie : Enemy {
    float height;
    private void Awake()
    {
        ename = "flyie";
    }
    // Use this for initialization
    protected override void Start()
    {
        enemyID = 2;
        base.Start();
        //anim.SetInteger("hp", health);
        SetTarget(GameManager.gm.player);
        //anim.Play(ename + "_move", -1, 0);
    }

    // Update is called once per frame
    protected override void Update()
    {
        //PerformAction();
        base.Update();

        //print(id + " " + actionPerformed);
    }

    public override void PerformAction()
    {
        //print("perf");
        //Move();
        
        //base.PerformAction();
        if (target != null)
        {
            Vector2 targetPos2d = new Vector2(targetPos.x, targetPos.z);
            Vector2 pos2d = new Vector2(transform.position.x, transform.position.z);//go.transform.position.x, go.transform.position.z);
            float dist = Vector3.Distance(pos2d, targetPos2d);
            //if (!isDoneMoving)
            //{
            //    Move();
            //}
            //print("DIST:::" + dist);
            // Move towards target if it is a Path
            if (target.tag == "Player")
            {
                //if (enemyID == 1)
                //print("outhere?");
                if (dist <= 15f)
                {
                    //print("CHANGE PATH");
                    /*PlatformPath p = target.transform.GetComponent<PlatformPath>();
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
                    }*/
                    AttemptAttackAction();
                }
                // Move and face towards target
                else
                {
                    //if(enemyID == 1)
                    //print("here?");
                    Move();
                }
            }
            // Move towards objective and attack it if in range
            else if (target.tag == "Objective")
            {
                if (dist <= 2.8f)
                {
                    AttemptAttackAction();
                }
                else
                {
                    Move();
                }
            }
        }
        /*
        // No target? Find objective to attack then
        else
        {
            ///*
            GameObject go = new GameObject();
            go.transform.position = new Vector3(1000, 0, 1000);
            go.tag = "Objective";
            SetTarget(go);
            //*/
            //SetTarget(GameManager.gm.objective);
        //}
    }

    public void Shoot(GameObject target)
    {
        GameObject projectile = Instantiate(GameManager.gm.projectilePrefabs[0]);
        Projectile p = projectile.GetComponent<Projectile>();
        projectile.transform.position = transform.position;
        projectile.GetComponent<Rigidbody>().AddForce(0, 500, 0);
        p.Shoot(target, tag, id);

    }

    // Attack target
    public override void Attack(GameObject target)
    {
        
        //if (NetworkManager.nm.isStarted)
        //{
        //    NetworkManager.nm.QueueAttackOnObject(gameObject, target);
        //}
        // Not online, just attack target
        /*
        else
        {
            if (target.tag == "Objective")
            {
                Objective o = target.transform.GetComponent<Objective>();
                o.TakeDamage(dmg);
            }
        }*/
        if(target.tag == "Player")
        {
            print("FIRE");
            Shoot(target);
        }
        atkTimer = effectiveTimeToAttack;
    }

    // Handles performing attack action
    public override void AttemptAttackAction()
    {
        if (actionPerformed != "attack")
        {
            isPerformingAction = false;
        }
        if (isPerformingAction)
            //if (isAttacking || (actionPerformed != "idle" && actionPerformed != "attack"))
            return;
        Turn();
        actionPerformed = "attack";
        anim.SetBool("isMoving", false);
        StartCoroutine(PerformAttack());
        //actionPerformed = "idle";
    }
    /*
    // Perform move animation
    public override IEnumerator MoveAnimation()
    {
        // End of move action
        isPerformingAction = false;
        actionPerformed = "idle";
        //}
    }
    */
    public bool MoveDown()
    {
        transform.position -= new Vector3(0, .05f, 0);
        height += .05f;
        return height >= 0;
    }

    public bool MoveUp()
    {
        transform.position += new Vector3(0, .05f, 0);
        height -= .05f;
        return height <= 0;
    }

    // Perform move animation
    public override IEnumerator MoveAnimation()
    {
        // Denote current action moving
        isPerformingAction = true;
        height = -1f;
        yield return new WaitUntil(MoveDown);
        height = 1f;
        yield return new WaitUntil(MoveUp);

        // End of move action
        isPerformingAction = false;
        //}
    }

    // Handles move action
    public override void Move()
    {
        // Don't perform move action if already performing an action 
        if (isPerformingAction)
        {
            if (actionPerformed == "move")
            {
                Turn();
                Vector2 targetPos2d = new Vector2(targetPos.x, targetPos.z);
                Vector2 pos2d = new Vector2(transform.position.x, transform.position.z);
                //print("moving" + id + ",..." + Vector2.Distance(targetPos2d, pos2d));
                
                transform.position = Vector3.MoveTowards(transform.position, targetPos, effectiveMoveSpd);
            }
            return;
        }
        anim.SetBool("isMoving", true);
        //if (health > (maxHP / 2))
        //else
        //{
        //    anim.Play(ename + "_dash", -1, 0);
        //}
        anim.SetBool("isAttacking", false);
        // Denote current action moving
        isPerformingAction = true;
        actionPerformed = "move";
        //StartCoroutine(MoveAnimation());
        //transform.position = Vector3.MoveTowards(transform.position, targetPos, effectiveMoveSpd);
    }
    
}
