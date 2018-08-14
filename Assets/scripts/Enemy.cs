﻿//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * Enables versatility with creating enemies and their stats details
 */
public class EnemyStats
{
    public int maxHP, dmg, scoreReward, currencyReward;
    public float moveSpd, atkSpd,atkRange;
    public EnemyStats(int hp, int dmg, int scoreReward, int currencyReward, float ms, float atkSpd, float atkRange=0)
    {
        maxHP = hp;
        this.dmg = dmg;
        moveSpd = ms;
        this.atkSpd = atkSpd;
        this.atkRange = atkRange;
        this.scoreReward = scoreReward;
        this.currencyReward = currencyReward;
    }
}
public class Enemy : MonoBehaviour
{
    public static int EnemyCount = 0; // Keeps track of enemies created during game session
    // List of stats per level
    public static EnemyStats[][] enemyStats = new EnemyStats[][] {
        // Stats for Slimes ignore -> Normies
        new EnemyStats[]
        {
            new EnemyStats(1, 1, 50,1,1, 1),
            new EnemyStats(2, 1, 50,1,1.2f, 1.2f),
            new EnemyStats(2, 2, 50,1,1.25f, 1.25f),
        },
        // Stats for Funguys ignore -> Infuries
        new EnemyStats[]
        {
            new EnemyStats(2, 1, 75,1,1, 1),
            new EnemyStats(2, 2, 75,1,1.2f, 1.2f),
            new EnemyStats(4, 3, 75,1,1.25f, 1.25f),
        },
        // Stats for Flyglet ignore -> Flyies
        new EnemyStats[]
        {
            new EnemyStats(1, 0, 70,1,1, 1),
            new EnemyStats(1, 1, 70,1,1f, 1f),
            new EnemyStats(4, 3, 70,1,1.25f, 1.25f),
        },
        // Stats for King Slime
        new EnemyStats[]
        {
            new EnemyStats(30, 5, 2500,50,.5f, .75f),
            new EnemyStats(1, 1, 70,1,1f, 1f),
            new EnemyStats(4, 3, 70,1,1.25f, 1.25f)
        }
    };
    public static string[] names =
    {
        "Slime",
        "Shroom",
        "Flyglet",
        "King Slime"
    };
    public Animator anim;
    public int health = 2, // Current health 
               maxHP = 2, // Max health
               dmg = 1, // How much enemy can inflict to others
               id, // Unique way to identify this enemy
               attackTypes, // Number of different unique attacks (animations and other purposes)
               attackType, // Current attack option
               enemyID = 0, // Distinguishes what type of enemy this is
               attackCt, // Keeps track of attack for multiplayer synchronization
               curTarget, // Pursue current target pathing
               dmgSourceID, // Damage Source ID ( most recent unit that attacked this object )
               level; // Used to determine what the stats are
    public float originalMoveSpd = 0.075f, // Default move speed
                 effectiveMoveSpd, // Move speed calculated and used 
                 originalTimeToAttack = 2.5f, // Default amount of time before inflicting damage
                 effectiveTimeToAttack, // Time calculated and used
                 atkTimer, // Used to check if enemy can inflict damage
                 originalAttackSpd = 1f, // Default Attack Speed
                 attackRange, // Distance before being able to attack
                 prevDist, // Distance from target beforehand
                 effectiveAttackSpd; // Attack speed calculated and used
    public bool isGrounded = false, isDoneMoving, isAttacking, isPerformingAction, isDead=false, canPerformAction, isJumping;
    protected string actionPerformed = "idle", ename = "enemy", 
                     dmgSourceType; // who or what dealt damage to me? (Player, trap, etc)
    protected Color originalColor;
    public GameObject[] statusEffectObjects;
    public GameObject go;
    public GameObject target; // What the enemy prioritizes
    public Vector3 targetPos; // What the enemy faces and moves to
    public Vector3 originalPos; // Keeps track of original position 
    public List<GameObject> pathing; // Path enemy follows
    protected List<GameObject> grounds = new List<GameObject>();

    protected AudioSource audioSrc;
    public List<AudioClip> soundClips;
    public Dictionary<string,Coroutine> coroutines;
    public Dictionary<string, bool> statusEffects;

    // Used to create the enemy and identify it
    /*public static void AssignEnemy(Enemy e)
    {
        e.id = EnemyCount;
        EnemyCount++;
        if (GameManager.gm.enemies != null)
        {
            GameManager.gm.enemies[e.id] = e;

        }
        else
        {
            print("DUPLICATE HOW");
            NetworkManager.nm.debugLog.Add("DUPLICATE HOW" + e.id);
        }
        e.name = "Enemy " + e.id;
    }*/

    protected virtual void Awake()
    {
        Physics.IgnoreLayerCollision(8, 8, true);
        id = EnemyCount;
        EnemyCount++;
        attackType = -1;
        if (GameManager.gm.enemies != null)
        {
            GameManager.gm.enemies[id] = this;
        }
        else
        {
            print("DUPLICATE HOW");
            NetworkManager.nm.debugLog.Add("DUPLICATE HOW" +id);
        }
        name = "Enemy " + id;
        prevDist = -1;

        curTarget = 0;
    }

    // Use this for initialization
    protected virtual void Start()
    {
        isPerformingAction = false;
        maxHP = enemyStats[enemyID][level].maxHP;
        health = maxHP;
        dmg = enemyStats[enemyID][level].dmg;
        effectiveMoveSpd = originalMoveSpd * enemyStats[enemyID][level].moveSpd;
        effectiveTimeToAttack = originalTimeToAttack / enemyStats[enemyID][level].atkSpd;
        atkTimer = effectiveTimeToAttack;
        effectiveAttackSpd = originalAttackSpd * enemyStats[enemyID][level].atkSpd;
        attackCt = 0;
        anim = GetComponent<Animator>();
        isDoneMoving = true;
        gameObject.AddComponent<ConstantForce>().force = new Vector3(0, -9, 0);
        audioSrc = gameObject.AddComponent<AudioSource>();// GetComponent<AudioSource>();
        //print(ename);
        attackRange = 2.7f;
        if (ename == "infurie")
            go = transform.Find("PivotPoint").Find("EnemyObject").gameObject;
        else //if (ename != "slime")
            go = transform.Find("EnemyObject").gameObject;
        canPerformAction = true;
        originalColor = go.GetComponent<Renderer>().material.color;
        originalPos = transform.position;
        coroutines = new Dictionary<string, Coroutine>();
        statusEffects = new Dictionary<string, bool>();

        GameObject hpBar = Instantiate(GameManager.gm.statusIndicatorPrefab);
        hpBar.transform.GetComponent<StatusIndicator>().target = gameObject;
        hpBar.transform.SetParent(transform.Find("HP Placeholder"));
        hpBar.transform.localPosition = Vector3.zero;


    }


    // Update is called once per frame
    protected virtual void Update()
    {
        //print(isGrounded);
        //PerformAction();
        //AttemptAttackAction();
        if (!GameManager.gm.inGame || GameManager.gm.data.gameOver)
        {
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("idle"))
                anim.Play("idle", -1, 0);
            return;
        }

        if (NetworkManager.nm.isStarted && NetworkManager.nm.isDisconnected)
        {
            return;
        }
        if (!isJumping)// && GetComponent<Rigidbody>().velocity.y>0)
        {
            //GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
        if (health <= 0)
        {
            //Die();
            return;
        }
        if (!isPerformingAction)
            PerformAction();

    }

    // Based on enemy type, will determine how player targets will be selected
    protected virtual GameObject SelectPlayerTarget()
    {
        return GameManager.gm.player;
    }

    // Generates random pathing towards objective
    public static List<GameObject> GeneratePathing(GameObject sp)
    {
        List<GameObject> pathing = new List<GameObject>();
        if (sp == null)
            return pathing;
        pathing.Add(sp);
        if(sp.tag == "Path")
        {
            PlatformPath pp = sp.GetComponent<PlatformPath>();
            if (pp.destTargets.Count == 0)
                return pathing;
            int nextPath = Random.Range(0, pp.destTargets.Count);
            sp = pp.destTargets[nextPath];
            pathing.AddRange(GeneratePathing(sp));
        }
        return pathing;
    }

    // Format: ENEMY|enemy id|enemy relative pos to target|target tag|target id|current target index
    public virtual string NetworkInformation()
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
        switch (tag)
        {
            case "Path":
                msg += target.transform.GetComponent<PlatformPath>().id + "|";
                break;
            case "Objective":
                msg += target.transform.GetComponent<Objective>().id + "|";
                break;
            case "Player":
                msg += target.transform.GetComponent<PlayerController>().id + "|";
                break;
        }
        msg += curTarget + "|";
        //msg += enemyID + "|";
        return msg;
    }

    // Extract Network Information
    public virtual void SetNetworkInformation(string[] data)
    {
        string[] xyz = data[2].Split(',');
        Vector3 pos = new Vector3(float.Parse(xyz[0]), float.Parse(xyz[1]), float.Parse(xyz[2]));
        int tid = int.Parse(data[4]);
        string tag = data[3];
        int curTargetIndex = int.Parse(data[5]);

        switch (tag) {
            case "Path":
                SetTarget(MapManager.mapManager.platforms[tid].gameObject);
                break;

            case "Objective":
                SetTarget(GameManager.gm.objective);
                //target =;
                break;
            case "Player":
                SetTarget(NetworkManager.nm.players[tid].playerGO);
                break;
        }
        Transform parent = transform.parent;
        transform.SetParent(target.transform);
        transform.localPosition = pos;
        transform.SetParent(parent);
        curTarget = curTargetIndex;
    }

    // Applies status effects on enemy (freeze, ...)
    public void ApplyEffect(Effect eff)//, int dmg=0, string ownerType="", int ownerID = -1)
    {
        switch (eff)
        {
            case Effect.freeze:
                /*
                if(coroutines.ContainsKey("freeze"))
                    coroutines["freeze"] = StartCoroutine(Freeze());
                else
                    coroutines.Add("freeze",StartCoroutine(Freeze()));
                    */
                
                StartCoroutine(Freeze());
                break;

            
        }
    }
    
    // slowly unfreeze enemy
    public bool HasThawed()
    {
        print(statusEffectObjects[0].transform.localScale.magnitude);
        statusEffectObjects[0].transform.localScale /= 1.0015f;
        return statusEffectObjects[0].transform.localScale.magnitude < .5f;
    }

    public IEnumerator Freeze()
    {
        if (!statusEffects.ContainsKey("freeze") || !statusEffects["freeze"])
        {
            statusEffects["freeze"] = true;
            print(1);
            anim.enabled = false;
            enabled = false;
            canPerformAction = false;
            statusEffectObjects[0].SetActive(true);
            statusEffectObjects[0].transform.localScale = new Vector3(1, 1, 1);
            
            yield return new WaitUntil(HasThawed);
          
            //yield return new WaitForSeconds(20);
            statusEffectObjects[0].SetActive(false);
            anim.enabled = true;
            canPerformAction = true;
            enabled = true;
            statusEffects["freeze"] = false;
            print(2);
        }
    }

    public void MoveForward()
    {
        transform.position += transform.forward * effectiveMoveSpd * DebugManager.dbm.FrameRate();
        Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
        Vector2 pos2D2 = new Vector2(targetPos.x, targetPos.z);
        float dist = Vector2.Distance(pos2D, pos2D2);
        if(prevDist >= 0 && dist > prevDist)
        {
            print("LARGERasdasd");
            SetTarget(target);
        }
        prevDist = dist;
        //transform.position = Vector3.MoveTowards(transform.position, targetPos, effectiveMoveSpd * 60f / (float)DebugManager.dbm.fps);
        AssignMoveTargetIfReached();
    }

    // Move forward while not reaching ground
    public bool ReachGround()
    {
        //transform.position = Vector3.MoveTowards(transform.position, targetPos, effectiveMoveSpd);
        //MoveForward();
        return grounds.Count > 0; //|| isGrounded && anim.GetCurrentAnimatorStateInfo(0).IsName("enemy_fall");
    }
    // Move forward while haven't reached peak of jump
    public bool ReachPeak()
    {
        Rigidbody rb = transform.GetComponent<Rigidbody>();
        //print(rb.velocity);
        //MoveForward();
        //transform.position = Vector3.MoveTowards(transform.position, targetPos, effectiveMoveSpd);
        return rb.velocity.y < 0;
    }

    public bool IsJumping()
    {
        Rigidbody rb = transform.GetComponent<Rigidbody>();
        GetComponent<Rigidbody>().AddForce(new Vector3(0, 300, 0));
        //transform.position = Vector3.MoveTowards(transform.position, targetPos, effectiveMoveSpd);
        return rb.velocity.y > 0;
    }

    public bool IsLandingAnimation()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName(ename + "_land");
    }

    // Check if current animation is falling
    public bool IsFallingAnimation()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName(ename + "_fall");
    }
    // Check if current animation is jumping
    public bool IsJumpingAnimation()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName(ename +"_jump");
    }
    // Check if current animation is pre-jumping
    public bool IsPreJumpingAnimation()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName(ename + "_prejump");
    }
    // Perform move animation
    public virtual IEnumerator MoveAnimation()
    {
        //if (!isPerformingAction)
        //{
        // Denote current action moving
        isPerformingAction = true;

        float prevSpeed = anim.speed;
        anim.speed = enemyStats[enemyID][level].moveSpd;

            // Animation before jumping
        anim.Play(ename + "_prejump", -1, 0);
        yield return new WaitUntil(IsPreJumpingAnimation);
        anim.SetBool("isJumping", true);
        yield return new WaitWhile(IsPreJumpingAnimation);

        isJumping = true;
        // Jump by adding upward force
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        //yield return new WaitUntil(IsJumpingAnimation);
        yield return new WaitUntil(IsJumping);
        yield return new WaitUntil(ReachPeak);

        // After reaching peak of jump, fall and detect landing
        grounds.Clear();
        isGrounded = false;
        anim.SetBool("isGrounded", false);
        anim.SetBool("isJumping", false);
        anim.SetBool("isFalling", true);
        //yield return new WaitUntil(IsFallingAnimation);
        yield return new WaitUntil(ReachGround);
        anim.Play(ename + "_land", -1, 0);
        // Upon touching ground, perform land animation
        rb.velocity -= new Vector3(0, rb.velocity.y, 0);
        anim.SetBool("isFalling", false);
        anim.SetBool("isGrounded", true);
        yield return new WaitUntil(IsLandingAnimation);

        //if (ename == "slime")
        //    anim.Play(ename + "_land", -1, 0);

        //yield return new WaitForSeconds(1);
        // End of move action
        anim.SetBool("isMoving", false);
        isJumping = false;
        yield return new WaitWhile(IsLandingAnimation);
        if (enemyID == 3)
        {
            yield return new WaitForSeconds(2f);
        }
        actionPerformed = "idle";
        anim.speed = prevSpeed;
        isPerformingAction = false;
        //}
    }
    
    // Handles move action
    public virtual void Move()
    {
        // Don't perform move action if already performing an action 
        if(isPerformingAction)
            return;
        actionPerformed = "move";
        anim.SetBool("isMoving", true);
        anim.SetBool("isAttacking", false);
        StartCoroutine(MoveAnimation());
        //actionPerformed = "idle";
        //transform.position = Vector3.MoveTowards(transform.position, targetPos, effectiveMoveSpd);
    }

    // Handle turning action
    public virtual void Turn()
    {
        transform.LookAt(targetPos);
    }

    public void AssignMoveTargetIfReached()
    {
        Vector2 targetPos2d = new Vector2(targetPos.x, targetPos.z);
        Vector2 pos2d = new Vector2(transform.position.x, transform.position.z);//go.transform.position.x, go.transform.position.z);
        float dist = Vector3.Distance(pos2d, targetPos2d);

        // Move towards target if it is a Path
        if (target.tag == "Path")
        {
            //if (enemyID == 1)
            //print("outhere?");

            if (dist <= .3f)
            {
                //print("CHANGE PATH");
                //PlatformPath p = target.transform.GetComponent<PlatformPath>();
                target = null;

                curTarget++;
                // Get new path if reached destination
                if (curTarget < pathing.Count) //p.destTargets.Count > 0)
                {
                    SetTarget(pathing[curTarget]);
                    //SetTarget(p.destTargets[Random.Range(0, p.destTargets.Count)]);
                }
                // No more paths, target objective
                else
                {
                    SetTarget(GameManager.gm.objective);
                }
            }
        }else if(target.tag == "Objective")
        {
            if(dist <= attackRange)
            {
                //AttemptAttackAction();
                isJumping = false;
                //StopAllCoroutines();
            }
        }
    }

    // Handles performing action
    public virtual void PerformAction()
    {
        if (!isGrounded)
        {
            return;
        }
        //print(">>>>>>>>>>>>>>>" + isDoneMoving);
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
            if (target.tag == "Path")
            {
                //if (enemyID == 1)
                    //print("outhere?");

                if (dist <= .3f)
                {
                    //print("CHANGE PATH");
                    PlatformPath p = target.transform.GetComponent<PlatformPath>();
                    target = null;

                    curTarget++;
                    // Get new path if reached destination
                    if (curTarget < pathing.Count) //p.destTargets.Count > 0)
                    {
                        SetTarget(pathing[curTarget]);
                        //SetTarget(p.destTargets[Random.Range(0, p.destTargets.Count)]);
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
                    //if(enemyID == 1)
                    //print("here?");
                    Move();
                }
            }
            // Move towards objective and attack it if in range
            else if (target.tag == "Objective")
            {
                if (dist <= attackRange)
                {
                    AttemptAttackAction();
                }
                else
                {
                    Move();
                }
            }
        }
        // No target? Find objective to attack then
        else
        {
            ///*
            //GameObject go = new GameObject();
            //go.transform.position = new Vector3(1000, 0, 1000);
            //go.tag = "Objective";
            //SetTarget(go);
            //*/
            SetTarget(GameManager.gm.objective);
        }
    }

    public GameObject GetTarget()
    {
        return target;
    }

    // Provides a way to notify player that this object has been hit
    public IEnumerator IndicateHasBeenHit()
    {
        Renderer r = go.GetComponent<Renderer>();
        //Color prevColor = r.material.color;
        r.material.color = Color.red;
        //Debug.Log(1);
        yield return new WaitForSeconds(.15f);
        r.material.color = originalColor;//prevColor;
        //Debug.Log(2);
    }

    // Callable function to notify this object has been hit
    public virtual void OnHit()
    {
        StartCoroutine(IndicateHasBeenHit());
        //audioSrc.clip = soundClips[0];
        //audioSrc.Play();
    }
 
    // Handles performing attack action
    public virtual void AttemptAttackAction()
    {
        if(isPerformingAction)
        //if (isAttacking || (actionPerformed != "idle" && actionPerformed != "attack"))
            return;
        actionPerformed = "attack";
        //anim.SetBool("isMoving", false);
        StartCoroutine(PerformAttack());
        //actionPerformed = "idle";
    }
    // Checks if current animation is charging
    public bool IsChargingAnimation()
    {
        if(attackType != -1)
            return anim.GetCurrentAnimatorStateInfo(0).IsName("charge"+attackType);

        return anim.GetCurrentAnimatorStateInfo(0).IsName("charge");
    }
    // Checks if current animation is reloading
    public bool IsReloadingAnimation()
    {
        if (attackType != -1)
            return anim.GetCurrentAnimatorStateInfo(0).IsName("reload" + attackType);
        return anim.GetCurrentAnimatorStateInfo(0).IsName("reload");
    }
    // Checks if current animation is attacking
    public bool IsAttackingAnimation()
    {
        if (attackType != -1)
            return anim.GetCurrentAnimatorStateInfo(0).IsName("attack" + attackType);
        return anim.GetCurrentAnimatorStateInfo(0).IsName("attack");
    }
    // Checks if enemy can attack after timer reaches 0
    public bool CanAttack()
    {
        anim.SetFloat("timeToAtk", anim.GetFloat("timeToAtk") - Time.deltaTime);
        return anim.GetFloat("timeToAtk") < .01f;
    }
    // Handles attack action and animation
    public virtual IEnumerator PerformAttack()
    {
        isAttacking = true;
        isPerformingAction = true;

        
        //anim.SetFloat("timeToAtk", effectiveTimeToAttack);
        float prevSpd = anim.speed;
        
        anim.speed = effectiveAttackSpd;
        
        /*
        // Enemy can attack after timer reaches 0
        anim.Play(ename + "_timetoatk", -1, 0);
        yield return new WaitUntil(CanAttack);

        // Perform charge animation
        yield return new WaitUntil(IsChargingAnimation);
        yield return new WaitWhile(IsChargingAnimation);
        */

        yield return new WaitForSeconds(effectiveTimeToAttack);
        string attackVariations = "";
        if (attackTypes > 1)
        {
            attackType = Random.Range(0, attackTypes);
            attackVariations += attackType;
        }
        anim.Play("charge" + attackVariations, -1, 0);

        // Perform attack animation
        //anim.SetBool("isAttacking", isAttacking);
        yield return new WaitUntil(IsAttackingAnimation);
        yield return new WaitWhile(IsAttackingAnimation);
        
        // After attack animation finishes, inflict damage
        Attack(target);

        // Perform reload animation
        //anim.SetBool("isReloading", true);
        yield return new WaitUntil(IsReloadingAnimation);

        // End attack action
        anim.speed = prevSpd;
        //anim.SetBool("isReloading", false);
        isPerformingAction = false;
        isAttacking = false;
    }
    /*
     // Handles attack action and animation
    public IEnumerator PerformAttack()
    {
        isAttacking = true;
        isPerformingAction = true;
        anim.SetFloat("timeToAtk", effectiveTimeToAttack);
        float prevSpd = anim.speed;
        anim.speed = effectiveAttackSpd;
        // Enemy can attack after timer reaches 0
        anim.Play("enemy_timetoatk", -1, 0);
        yield return new WaitUntil(CanAttack);
        
        // Perform charge animation
        yield return new WaitUntil(IsChargingAnimation);
        yield return new WaitWhile(IsChargingAnimation);
        
        // Perform attack animation
        anim.SetBool("isAttacking", isAttacking);
        yield return new WaitUntil(IsAttackingAnimation);
        yield return new WaitWhile(IsAttackingAnimation);

        // After attack animation finishes, inflict damage
        Attack(target);

        // Perform reload animation
        anim.SetBool("isReloading", true);
        yield return new WaitUntil(IsReloadingAnimation);

        // End attack action
        anim.speed = prevSpd;
        anim.SetBool("isReloading", false);
        isPerformingAction = false;
        isAttacking = false;
    }
     */

    // take damage dmg from source. upon death we can determine what the source was (player, trap, etc)
    public virtual void TakeDamageFrom(int dmg, string dmgSourceType, int sid)
    {
        TakeDamage(dmg);

        if (dmg != 0)
        {
            // Visual indication of damage dealt
            GameObject damageIndicator = Instantiate(GameManager.gm.indicatorPrefabs[0]);
            damageIndicator.transform.position = transform.Find("HP Placeholder").position;//healthBarGauge.position + new Vector3(0, healthBarGauge.GetComponent<RectTransform>().rect.height, 0);
            damageIndicator.transform.LookAt(GameManager.gm.player.transform.GetComponent<PlayerController>().playerCam.transform);
            damageIndicator.transform.GetChild(0).GetComponent<Text>().text = "" + (dmg);
        }
        // Keep track of last damage source just in case die and need to reward source
        this.dmgSourceType = dmgSourceType;
        dmgSourceID = sid;
        PlayerController pc = GameManager.gm.player.GetComponent<PlayerController>();
        if (dmgSourceType == "Player" && sid == pc.id)
        {
            print("MY PLAYER hit:"+pc.shotsHit + "/" + pc.wep.shotCount);
        }
        if(health <= 0)
        {
            Die();
        }
    }

    // Attack target
    public virtual void Attack(GameObject target)
    {
        //atkTimer -= Time.deltaTime;
        //if (atkTimer > 0)
        //    return;
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
        if (isDead)
            return;
        GameManager.gm.UpdateInGameCurrency(enemyStats[enemyID][level].currencyReward);
        GameManager.gm.kills++;

        if (dmgSourceType == "Player")
        {
            // if I killed the enemy
            if(dmgSourceID == GameManager.gm.player.GetComponent<PlayerController>().id)
            {
                GameManager.gm.UpdateKillCount(1);
                GameManager.gm.UpdateScore(enemyStats[enemyID][level].scoreReward);
                GameManager.gm.data.killsByEnemy[enemyID]++;
            }
        }
        

        // If online, remove enemy attack that was queued
        if (NetworkManager.nm.isStarted)
        {
            NetworkManager.nm.RemoveEnemyAttacks(id);
        }
        //GameManager.gm.enemies.Remove(id);
        List<GameObject> rewards = new List<GameObject>();
        //GameManager.gm.UpdateItem("Attribute", 1, 1);
        float spawnRewardChance = Random.Range(0.0f, 1.25f);
        //spawnRewardChance = 1;
        //print((int)spawnRewardChance);
        //spawnRewardChance = 1;
        for (int i = 0; i < (int)spawnRewardChance; i++)
        {
            GameObject reward = Instantiate(GameManager.gm.rewardPrefabs[0]);
            reward.transform.position = transform.Find("SpawnPt").position;
            rewards.Add(reward);
            reward.GetComponent<Collider>().enabled = false;
        }

        GameObject ps = Instantiate(GameManager.gm.VFXPrefabs[0]);//enemyDeathVFXPrefab);
        ps.transform.position = transform.Find("SpawnPt").position;
        isDead = true;
        ps.transform.localScale = transform.Find("SpawnPt").localScale;
        Destroy(gameObject);
    }

    // Creates target to move towards and face properly then assigns target to chase/attack
    public virtual void SetTarget(GameObject g)
    {
        target = g;
        Vector3 pos;
        if (g.tag == "Objective" && enemyID == 2)
        {
            pos = g.transform.position;
        }
        else
        {
            pos = new Vector3(
                            g.transform.position.x,
                            transform.position.y,
                            g.transform.position.z
                           );
        }
        targetPos = pos;
        prevDist = -1;
        Turn();
    }

    // Inflict damage to health
    public virtual bool TakeDamage(int dmg)
    {
        //print("enemy " + id + " took dmg" + dmg);
        NetworkManager.nm.debugLog.Add("enemy " + id + " took dmg" + dmg);
        health -= dmg;
        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        print(12);
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        // Ignore colliding with enemies
        //GameObject gameobject = collision.gameObject;
        //while(gameobject.tag == "Untagged" && gameobject.transform.parent != null)
        //{
        //    gameobject = gameobject.transform.gameObject;
        //}
        if (collision.gameObject.tag == "Enemy")
        {

            print("A?SDssssASD");
            Collider c = collision.collider;
            print(c.name);
            if (collision.gameObject.name != "EnemyObject")
            {
                c = collision.gameObject.GetComponent<Enemy>().go.GetComponent<Collider>();
            }
            Physics.IgnoreCollision(c, go.GetComponent<Collider>());
            return;
        }
        // Check if touching ground
        if (collision.gameObject.tag == "Ground" || collision.gameObject.tag == "Path")
        {
            //print("GRND");
            grounds.Add(collision.gameObject);
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;
        }
        isGrounded = grounds.Count > 0;
        if(enemyID == 0)
            anim.SetBool("isGrounded", isGrounded);
    }
    protected virtual void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            print("A?SDASD");
            Collider c = collision.collider;
            if (collision.gameObject.name != "EnemyObject")
            {
                c = collision.gameObject.GetComponent<Enemy>().go.GetComponent<Collider>();
            }
            Physics.IgnoreCollision(c, go.GetComponent<Collider>());

            return;
        }
    }
    /*
    private void OnTriggerStay(Collider collision)
    {
        if (grounds.Count > 0)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.velocity -= new Vector3(0, rb.velocity.y, 0);
        }
        // print(collision.gameObject.tag);
    }*/
    protected virtual void OnCollisionExit(Collision collision)
    {/*
        if (collision.gameObject.tag == "Enemy")
        {
            Collider c = collision.collider;
            if (collision.gameObject.name != "EnemyObject")
            {
                c = collision.gameObject.GetComponent<Enemy>().go.GetComponent<Collider>();
            }
            Physics.IgnoreCollision(c, go.GetComponent<Collider>());

            return;
        }*/
        if (collision.gameObject.tag == "Ground" || collision.gameObject.tag == "Path")
        {
            //print("LEAVEING");
            grounds.Add(collision.gameObject);
        }
        isGrounded = grounds.Count > 0;
        if (enemyID == 0)
            anim.SetBool("isGrounded", isGrounded);
    }


}
