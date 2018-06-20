using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveHitIndicator : MonoBehaviour {
    float visibleTime = .5f, invisibleTime = .35f, timer, timeToLive;
    bool isBlinking;

    Coroutine blinker;

	// Use this for initialization
	void Start () {
        //warning = transform.Find("Icon").GetComponent<Image>();
	}

    public IEnumerator Blink()
    {
        if (!isBlinking)
        {
            Image warning = transform.Find("Icon").GetComponent<Image>();
            Color c = warning.color;
            isBlinking = true;

            c.a = 1f;
            warning.color = c;
            yield return new WaitForSeconds(visibleTime);

            c.a = .3f;
            warning.color = c;
            yield return new WaitForSeconds(invisibleTime);
            isBlinking = false;
        }
    }

    public void SignalObjectiveIsHit()
    {
        //print(blinker == null);
        timeToLive = 5f + Time.time;

    }

	// Update is called once per frame
	void Update () {
        if(timeToLive > Time.time && !isBlinking)
        {
            blinker = StartCoroutine(Blink());
        }
        transform.GetChild(0).gameObject.SetActive(timeToLive > Time.time);
        if(timeToLive <= Time.time && blinker != null)
        {
            StopCoroutine(blinker);
        }
    }
}
