using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slider : MonoBehaviour {
    public float[] xBounds = { -100.0f, -45.0f },
                    yBounds;
    public float slideRate;
    public bool slideDir;
    Coroutine sliderCoroutine;

    // Switches between sliding in one direction and its opposite
    public void ToggleSlider()
    {
        if(sliderCoroutine != null)
        {
            StopCoroutine(sliderCoroutine);
        }
        sliderCoroutine = StartCoroutine(Slide(!slideDir));
        slideDir = !slideDir;
    }

    // Slides in selected direction (left = true, right = false)
    public void SlideInDirection(bool dir)
    {
        if (slideDir == dir)
            return;
        if (sliderCoroutine != null)
        {
            StopCoroutine(sliderCoroutine);
        }
        sliderCoroutine = StartCoroutine(Slide(dir));
        slideDir = dir;
    }

    // Slides toward left bound
    public bool SlideLeft()
    {
        transform.position -= new Vector3(slideRate, 0, 0);
        if (transform.localPosition.x <= xBounds[0])
            transform.localPosition = new Vector3(xBounds[0], transform.localPosition.y, transform.localPosition.z);
        return transform.localPosition.x <= xBounds[0];
    }

    // Slides toward right bound
    public bool SlideRight()
    {
        transform.position += new Vector3(slideRate, 0, 0);
        if (transform.localPosition.x >= xBounds[1])
            transform.localPosition = new Vector3(xBounds[1], transform.localPosition.y, transform.localPosition.z);
        return transform.localPosition.x >= xBounds[1];
    }
    

    // dir: left = true, right = false
    public IEnumerator Slide(bool dir)
    {
        // if sliding in same direction already, don't perform slide
        if (slideDir != dir)
        {
            if (dir)
                yield return new WaitUntil(SlideLeft);
            else
                yield return new WaitUntil(SlideRight);
        }
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
