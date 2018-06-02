using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : Enemy {
    protected override void Awake()
    {
        base.Awake();
        ename = "slime";
    }
    // Use this for initialization
    protected override void Start()
    {
        enemyID = 0;
        base.Start();
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        // Ignore colliding with enemies
        //GameObject gameobject = collision.gameObject;
        //while(gameobject.tag == "Untagged" && gameobject.transform.parent != null)
        //{
        //    gameobject = gameobject.transform.gameObject;
        //}
        if (collision.gameObject.tag == "Enemy")
        {
            print(1);
            Collider c = collision.collider;
            //Physics.IgnoreCollision(c, GetComponentInChildren<Collider>());
            return;
        }
        // Check if touching ground
        if (collision.gameObject.tag == "Ground" || collision.gameObject.tag == "Path") ;
        {
            //print("GRND");
            grounds.Add(collision.gameObject);

        }
        isGrounded = grounds.Count > 0;
        if (enemyID == 0)
            anim.SetBool("isGrounded", isGrounded);
    }
    protected override void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            print(2);
            Collider c = collision.collider;
            /*
            if (collision.gameObject.name != "EnemyObject")
            {
                c = collision.gameObject.GetComponent<Enemy>().go.GetComponent<Collider>();
            }
            Physics.IgnoreCollision(c, go.GetComponent<Collider>());
            */
            //Physics.IgnoreCollision(c, GetComponentInChildren<Collider>());
            return;
        }
    }
}
