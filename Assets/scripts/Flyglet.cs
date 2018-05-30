using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flyglet : Enemy
{
    float height;
    bool isFalling;
    
    protected override void Awake()
    {
        base.Awake();
        ename = "flyglet";
    }
    // Use this for initialization
    protected override void Start()
    {
        enemyID = 4;
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

        StartCoroutine(MoveAnimation());
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
        p.Shoot(target, tag, id, 0);

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
        if (target.tag == "Player")
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
        //anim.SetBool("isMoving", false);
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
        print(1);
        transform.position -= new Vector3(0, .03f, 0);
        height += .03f;
        return height >= 0;
    }

    public bool MoveUp()
    {
        print(2);
        transform.position += new Vector3(0, .05f, 0);
        height -= .05f;
        return height <= 0;
    }

    public bool IsPreFlapping()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName("pre-flap");
    }

    public bool PreFlap()
    {
        float len = anim.GetCurrentAnimatorClipInfo(0)[0].clip.length * anim.GetCurrentAnimatorStateInfo(0).normalizedTime;

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("pre-flap") && len > 0)
        {
            //print(len + "...@@@@@@");
            height -= .05f;

            transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, height + originalPos.y, transform.position.z), .2f);
            //transform.position = new Vector3(transform.position.x, height + originalPos.y, transform.position.z); //-0.01f, 0);
        }
        return anim.GetCurrentAnimatorStateInfo(0).IsName("pre-flap");
    }

    public bool IsFlapping()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName("flap");
    }

    public bool Flap()
    {
        //height += .02f;
        // print("##########");
        //print(anim.GetCurrentAnimatorStateInfo(0).normalizedTime);
        //print(anim.GetCurrentAnimatorClipInfo(0)[0].clip.length);
        //print("##########");
        float len = anim.GetCurrentAnimatorClipInfo(0)[0].clip.length * anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("flap") && len > 0)
        {
            height += .2f;
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, height + originalPos.y, transform.position.z), .2f);
            //transform.position = new Vector3(transform.position.x, height + originalPos.y, transform.position.z);
            //transform.position += new Vector3(0, height, 0); //-0.01f, 0);
        }
        //transform.position += new Vector3(0, 0.02f, 0);
        return anim.GetCurrentAnimatorStateInfo(0).IsName("flap");
    }

    public bool IsPostFlapping()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName("post-flap");
    }

    public bool PostFlap()
    {
        //height -= .01f;
        //transform.position += new Vector3(0, -0.01f, 0);
        float len = anim.GetCurrentAnimatorClipInfo(0)[0].clip.length * anim.GetCurrentAnimatorStateInfo(0).normalizedTime;

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("post-flap") && len > 0)
        {
            //print(len + "...%%%%%%%%%%%%%d");
            height -= .1f;
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, height + originalPos.y, transform.position.z),.2f);
            //transform.position += new Vector3(0, height, 0); //-0.01f, 0);
        }
        return anim.GetCurrentAnimatorStateInfo(0).IsName("post-flap");
    }


    // Perform move animation
    public override IEnumerator MoveAnimation()
    {
        if (actionPerformed != "move")
        {
            //print(1);
            actionPerformed = "move";
            height = 0;
            // Denote current action moving
            //isPerformingAction = true;
            yield return new WaitUntil(IsPreFlapping);
            //print(2);
            //targetPos += new Vector3(0, -1f, 0);
            yield return new WaitWhile(PreFlap);
            //print(3);


            yield return new WaitUntil(IsFlapping);
            //print(4);
            //targetPos += new Vector3(0, 2f, 0);
            yield return new WaitWhile(Flap);

            //print(5);
            yield return new WaitUntil(IsPostFlapping);
            //print(6);
            //targetPos += new Vector3(0, -1f, 0);
            yield return new WaitWhile(PostFlap);
            //print(7);
            //transform.position += new Vector3(0, -height, 0);
            //height = -1.5f;
            //yield return new WaitUntil(MoveDown);
            //height = 1.5f;
            //yield return new WaitUntil(MoveUp);


            // End of move action
            //isPerformingAction = false;
            actionPerformed = "idle";
        }
    }

    // Handles move action
    public override void Move()
    {
        // Don't perform move action if already performing an action 
        //if (isPerformingAction)
        //{
        //    if (actionPerformed == "move")
        //    {
        Turn();
        Vector3 heightOffset;
        //Vector2 targetPos2d = new Vector2(targetPos.x, targetPos.z);
        //Vector2 pos2d = new Vector2(transform.position.x, transform.position.z);
        //print("moving" + id + ",..." + Vector2.Distance(targetPos2d, pos2d));
        
        transform.position = Vector3.MoveTowards(transform.position, targetPos, effectiveMoveSpd);
        
        //    }
        //    return;
        //}
        //anim.SetBool("isMoving", true);
        //if (!anim.GetCurrentAnimatorStateInfo(0).IsName("move"))
        //{
        //    anim.Play("move", -1, 0);
        //}
        //if (health > (maxHP / 2))
        //else
        //{
        //    anim.Play(ename + "_dash", -1, 0);
        //}
        //anim.SetBool("isAttacking", false);
        // Denote current action moving
        //isPerformingAction = true;
        StartCoroutine(MoveAnimation());
        //transform.position = Vector3.MoveTowards(transform.position, targetPos, effectiveMoveSpd);
    }



}
