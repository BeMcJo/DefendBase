using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkmark : MonoBehaviour {
    Coroutine coroutine;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    bool CheckmarkIsFilled()
    {
        transform.Find("CheckmarkFiller").localScale += new Vector3(.1f, 0, 0);
        return transform.Find("CheckmarkFiller").localScale.x >= 1;
    }

    IEnumerator FillCheckmark()
    {
        yield return new WaitUntil(CheckmarkIsFilled);
    }

    private void OnEnable()
    {
        print(1);
        coroutine = StartCoroutine(FillCheckmark());
    }

    private void OnDisable()
    {
        StopCoroutine(coroutine);
        transform.Find("CheckmarkFiller").localScale = new Vector3(0, 1, 1);
    }
}
