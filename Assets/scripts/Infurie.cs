using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Infurie : Enemy
{
    protected override void Awake()
    {
        base.Awake();
        ename = "infurie";
    }
    // Use this for initialization
    protected override void Start()
    {
        enemyID = 1;
        base.Start();
        anim.SetInteger("hp", health);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
       
        //print(id + " " + actionPerformed);
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
    // Inflict damage to health
    public override bool TakeDamage(int dmg)
    {
        base.TakeDamage(dmg);
        int dif = health - (maxHP / 2);
        //infuriated = dif <= 0;
        if (dif <= 0)
        {
            //print("RAGED");
            //Color c = go.transform.GetComponent<Renderer>().material.color;
            originalColor.g = 120.0f / 255.0f;
            //c.g = 120.0f/255.0f;
            //c = Color.black;
            go.transform.GetComponent<Renderer>().material.color = originalColor;
            //print(go.transform.GetComponent<Renderer>().material.color);
            effectiveMoveSpd = originalMoveSpd * difficulties[enemyID][level].moveSpd * 2f;
            effectiveTimeToAttack = originalTimeToAttack / difficulties[enemyID][level].atkSpd / 1.5f;
            atkTimer = effectiveTimeToAttack;
            effectiveAttackSpd = originalAttackSpd * difficulties[enemyID][level].atkSpd * 2f;
        }
        anim.SetInteger("hp", dif);
        return true;
    }

    // Handles move action
    public override void Move()
    {
        // Don't perform move action if already performing an action 
        if (isPerformingAction)
        {
            if (actionPerformed == "move")
            {
                /*
                Vector2 targetPos2d = new Vector2(targetPos.x, targetPos.z);
                Vector2 pos2d = new Vector2(transform.position.x, transform.position.z);
                print("moving" + id + ",..." + Vector2.Distance(targetPos2d, pos2d));
                */
                transform.position = Vector3.MoveTowards(transform.position, targetPos, effectiveMoveSpd);
            }
            return;
        }
        anim.SetBool("isMoving", true);
        if (health > (maxHP / 2))
            anim.Play(ename + "_move", -1, 0);
        else
        {
            anim.Play(ename + "_dash", -1, 0);
        }
        anim.SetBool("isAttacking", false);
        // Denote current action moving
        isPerformingAction = true;
        actionPerformed = "move";
        //StartCoroutine(MoveAnimation());
        //transform.position = Vector3.MoveTowards(transform.position, targetPos, effectiveMoveSpd);
    }
    

}
