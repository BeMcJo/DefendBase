using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Funguy : Enemy {

    protected override void Awake()
    {
        base.Awake();
        ename = "funguy";
    }
    // Use this for initialization
    protected override void Start()
    {
        enemyID = 1;
        base.Start();
        attackRange = 3.5f;
        attackTypes = 2;
    }

    public override void Move()
    {
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("walk"))
        {
            actionPerformed = "move";
            //anim.speed = difficulties[enemyID][level].moveSpd;
            anim.Play("walk", -1, 0);
            
        }
        transform.position = Vector3.MoveTowards(transform.position, targetPos, effectiveMoveSpd * 60f / (float)DebugManager.dbm.fps);
    }
    // Checks if current animation is attacking
    public new bool IsAttackingAnimation()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName("attack" + attackType);
    }

    // Checks if current animation is reloading
    public new bool IsReloadingAnimation()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName( "reload" + attackType);
    }

    public override IEnumerator PerformAttack()
    {
        isAttacking = true;
        isPerformingAction = true;
        float prevSpd = anim.speed;

        anim.speed = effectiveAttackSpd;
        anim.Play("charge", -1, 0);

        yield return new WaitForSeconds(effectiveTimeToAttack);
        attackType = Random.Range(0, attackTypes);
        anim.Play("attack" + attackType, -1, 0);
        //transform.
        yield return new WaitUntil(IsAttackingAnimation);
        yield return new WaitWhile(IsAttackingAnimation);
        Attack(target);
        yield return new WaitUntil(IsReloadingAnimation);
        yield return new WaitWhile(IsReloadingAnimation);

        // End attack action
        anim.speed = prevSpd;
        //anim.SetBool("isReloading", false);
        isPerformingAction = false;
        isAttacking = false;
    }
}
