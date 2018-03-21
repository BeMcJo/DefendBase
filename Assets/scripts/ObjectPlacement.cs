using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacement : MonoBehaviour {
    public bool canSet, // Is this spot legal to place object?
                isSet; // Did we legitimately place object onto map?
    List<GameObject> touchedObjs;
    public GameObject description;
    void Start () {
        canSet = true;
        touchedObjs = new List<GameObject>();
    }

    private void OnDestroy()
    {
        if(GameManager.gm.selectedDescription == description)
        {
            GameManager.gm.DeselectDescription();
        }
        Destroy(description);
    }

    // Update is called once per frame
    void Update () {
        Renderer r = GetComponent<Renderer>();
        //bool invalidSpot = false;
        canSet = true;
        //Debug.Log(">>");
        foreach(GameObject go in touchedObjs)
        {
           // Debug.Log(go.tag);
            if(go.tag != "Ground")
            {
                canSet = false;
                break;
            }
        }
         //Debug.Log("<<");
        if (canSet)
        {
            r.material.color = Color.red;
        }
        else
        {
            r.material.color = Color.blue;
        }
        if (gameObject == GameManager.gm.selectedDefense)
        {
            if(!isSet)
                if (canSet)
                {
                    r.material.color = Color.red;
                }
                else
                {
                    r.material.color = Color.blue;
                }
            else
                r.material.color = Color.blue;
        }
        if (isSet && !canSet)
        {
            Debug.Log("FAILED");
            Destroy(gameObject);
        }
    }

    public void Clear()
    {
        if(touchedObjs != null)
            touchedObjs.Clear();
    }
    /*
    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("enter " + collision.transform.tag);
        if (isSet)
            return;
        if (collision.gameObject.tag == "Enemy")
        {
            print("bang");
            Collider c = collision.collider;
            if (collision.gameObject.name != "EnemyObject")
            {
                print(collision.gameObject.name);
                c = collision.gameObject.GetComponent<Enemy>().go.GetComponent<Collider>();

            }
            Physics.IgnoreCollision(c, GetComponent<Collider>());
        }
        touchedObjs.Add(collision.gameObject);
    }
    */
    /*
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            print("bstayang");
            Collider c = collision.collider;
            if (collision.gameObject.name != "EnemyObject")
            {
                print(collision.gameObject.name);
                c = collision.gameObject.GetComponent<Enemy>().go.GetComponent<Collider>();

            }
            Physics.IgnoreCollision(c, GetComponent<Collider>());

            return;
        }
        /*
        Debug.Log("stay " + collision.transform.tag);
        if (isSet)
            return;
        if (collision.gameObject.tag == "Ground")
        {
            canSet = true;
        }
        else
        {
            canSet = false;
        }*/
    //}
    /*
    private void OnCollisionExit(Collision collision)
    {
        //Debug.Log("exit " + collision.transform.tag);
        if (isSet)
            return;
        if (collision.gameObject.tag == "Enemy")
        {
            print("bangleave");
            Collider c = collision.collider;
            if (collision.gameObject.name != "EnemyObject")
            {
                print(collision.gameObject.name);
                c = collision.gameObject.GetComponent<Enemy>().go.GetComponent<Collider>();

            }
            Physics.IgnoreCollision(c, GetComponent<Collider>());

            return;
        }
        touchedObjs.Remove(collision.gameObject);
    }
    */
    private void OnTriggerEnter(Collider collision)
    {
        //Debug.Log("enter " + collision.transform.tag);
        if (isSet)
            return;
        touchedObjs.Add(collision.gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        
    }

    private void OnTriggerExit(Collider collision)
    {

        touchedObjs.Remove(collision.gameObject);
    }




}
