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
        enemyID = 4;
        base.Start();
    }

    public override void Move()
    {
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("walk"))
        {
            actionPerformed = "move";
            anim.Play("walk", -1, 0);
        }
        transform.position = Vector3.MoveTowards(transform.position, targetPos, effectiveMoveSpd);
    }
    // Checks if current animation is attacking
    public new bool IsAttackingAnimation()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName("attack0");
    }

    // Checks if current animation is reloading
    public new bool IsReloadingAnimation()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName( "reload0");
    }

    public override IEnumerator PerformAttack()
    {
        isAttacking = true;
        isPerformingAction = true;
        float prevSpd = anim.speed;

        anim.speed = effectiveAttackSpd;
        anim.Play("charge", -1, 0);

        yield return new WaitForSeconds(effectiveTimeToAttack);

        anim.Play("attack0", -1, 0);
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
