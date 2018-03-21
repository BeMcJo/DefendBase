using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Floater : MonoBehaviour {
    Rigidbody rb;
    Vector3 biasedDir, biasedTorq;
    float ttl;
    bool isHit;
	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
        biasedDir = new Vector3(Random.Range(-.5f, 0.5f) / 5, Random.Range(0, 0.5f), Random.Range(-.5f, 0.5f)/5);
        biasedTorq = new Vector3(Random.Range(-.5f, 0.5f), Random.Range(-.5f, 0.5f), Random.Range(-.5f, 0.5f));
        rb.AddForce(biasedDir* Random.Range(20,250));
        rb.AddTorque(biasedTorq* Random.Range(20, 250));
        //GetComponent<Collider>().enabled = true;
        StartCoroutine(Invincibility());
        ttl = 30f;
    }

    // Prevents object from immediately getting hit upon spawn
    IEnumerator Invincibility()
    {
        //GetComponent<Collider>().isTrigger = false;
        yield return new WaitForSeconds(.1f);
        GetComponent<Collider>().enabled = true;
    }

    private void FixedUpdate()
    {
        float x = Random.Range(-.05f, 0.05f), y = Random.Range(-.05f, 0.05f), z = Random.Range(-.05f, 0.05f);
        Vector3 randDir = new Vector3(Random.Range(-.5f, 0.5f), Random.Range(-.5f, 0.5f), Random.Range(-.5f, 0.5f));
        Vector3 randTorq = new Vector3(Random.Range(-.5f, 0.5f), Random.Range(-.5f, 0.5f), Random.Range(-.5f, 0.5f));
        randDir.x = x;
        randDir.y = y;
        randDir.z = z;
        rb.AddForce(randDir);
        //rb.AddForce(x*10,y*10,z*10);
        rb.AddTorque(randTorq);
    }

    bool Expand()
    {
        float expandRate = .45f;
        transform.localScale += new Vector3(expandRate,expandRate,expandRate);
        Color c = GetComponent<Renderer>().material.color;
        c.a -= .02f;
        if (c.a <= 0.1f)
            c.a = .1f;
        GetComponent<Renderer>().material.color = c;
        return transform.localScale.x >= 5;
    }

    IEnumerator WaitToPop()
    {
        yield return new WaitUntil(Expand);
        Destroy(gameObject);
    }

    public void OnHit()
    {
        if (isHit)
            return;
        GameManager.gm.UpdateItem("Attribute", 1, 2);
        GameObject itemGainIndicator = Instantiate(GameManager.gm.indicatorPrefabs[0]);
        itemGainIndicator.transform.position = transform.position;
        itemGainIndicator.transform.GetChild(0).GetComponent<Text>().text = "+2 Bomb Arrow";
        itemGainIndicator.transform.LookAt(GameManager.gm.player.GetComponent<PlayerController>().playerCam.transform);
        Die();
    }

    public void Die()
    {
        if (isHit)
            return;
        isHit = true;
        StartCoroutine(WaitToPop());
    }

    // Update is called once per frame
    void Update () {
        ttl -= Time.deltaTime;
        if (ttl <= 0)
            Die();
	}
}
