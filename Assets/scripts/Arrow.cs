using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 
 */
public class Arrow : Projectile {
	// Use this for initialization
	protected override void Awake () {
        base.Awake();
    }

    public override void SetAttribute(int aID)
    {
        //Handle deactivating previous attribute
        // normal arrow
        if (attributeID == 0)
        {
            /*
            Color c = transform.Find("Body").GetComponent<Renderer>().material.color;
            c = Color.red;
            transform.Find("Body").GetComponent<Renderer>().material.color = c;
            */
            //attributesContainer.Find("Attributes").gameObject.SetActive(false);
        }
        // bomb arrow
        else if (attributeID == 1)
        {
            //attributesContainer.Find("Attributes").gameObject.SetActive(true);
            attributesContainer.GetChild(0).gameObject.SetActive(false);
        }
        // bullet arrow
        else if (attributeID == 3)
        {
            transform.Find("Arrow").GetChild(0).GetComponent<MeshRenderer>().material.SetFloat(Shader.PropertyToID("_Metallic"), 0);
            transform.Find("Arrow").GetChild(0).GetComponent<MeshRenderer>().material.SetFloat(Shader.PropertyToID("_Glossiness"), 0);
            //transform.Find("Arrow").GetChild(0).GetComponent<MeshRenderer>().material = materials[0];
        }
        // ice arrow
        else if (attributeID == 4)
        {
            transform.Find("Arrow").GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
            attributesContainer.GetChild(1).gameObject.SetActive(false);
        }
        // fire arrow
        else if (attributeID == 5)
        {
            //transform.Find("Arrow").GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
            attributesContainer.GetChild(2).gameObject.SetActive(false);
        }

        //Enable current attribute
        // normal arrow
        if (aID == 0)
        {
            /*
            Color c = transform.Find("Body").GetComponent<Renderer>().material.color;
            c = Color.red;
            transform.Find("Body").GetComponent<Renderer>().material.color = c;
            */
            attributesContainer.gameObject.SetActive(false);
        }
        // bomb arrow
        else if (aID == 1)
        {
            attributesContainer.gameObject.SetActive(true);
            attributesContainer.GetChild(0).gameObject.SetActive(true);
        }
        // bullet arrow
        else if (aID == 3)
        {
            //attributesContainer.gameObject.SetActive(true);
            //transform.Find("Arrow").GetChild(0).GetComponent<MeshRenderer>().material = materials[1];
            //transform.Find("Arrow").GetChild(0).GetComponent<MeshRenderer>().material.SetTexture()
            transform.Find("Arrow").GetChild(0).GetComponent<MeshRenderer>().material.SetFloat(Shader.PropertyToID("_Metallic"), 1);
            transform.Find("Arrow").GetChild(0).GetComponent<MeshRenderer>().material.SetFloat(Shader.PropertyToID("_Glossiness"), 1);
        }
        // ice arrow
        else if (aID == 4)
        {
            transform.Find("Arrow").GetChild(0).GetComponent<MeshRenderer>().material.color = new Color(111.0f / 255, 255.0f / 255, 255.0f / 255);
            attributesContainer.gameObject.SetActive(true);
            attributesContainer.GetChild(1).gameObject.SetActive(true);
        }
        // fire arrow
        else if (aID == 5)
        {
            attributesContainer.gameObject.SetActive(true);
            attributesContainer.GetChild(2).gameObject.SetActive(true);
        }
        base.SetAttribute(aID);
        
    }

    public override void Shoot(GameObject t, string ownerType, int ownerID, int dmg)
    {
        base.Shoot(t, ownerType, ownerID,dmg);
        transform.Find("Tail").GetComponent<BoxCollider>().enabled = false;
        if(attributeID == 3)
        {
            transform.Find("Arrow Tip").GetComponent<Collider>().enabled = false;
            
        }
    }

    // Update is called once per frame
    protected virtual void FixedUpdate () {
        // Orient Arrow in the direction of its trajectory
        if (transform.GetComponent<Rigidbody>().velocity != Vector3.zero && attributeID != 3)
            transform.rotation = Quaternion.LookRotation(transform.GetComponent<Rigidbody>().velocity);
	}
}
