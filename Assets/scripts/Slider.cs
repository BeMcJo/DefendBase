using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slider : MonoBehaviour {
    public float[] xBounds = { -100.0f, -45.0f },
                    yBounds;
    public float slideRate; // how fast of slide translation
    public int slideDir; // slide direction: left = 0, right = 1, up = 2, down = 3
    public bool vertical, horizontal; // check if can slide vertical and/or horizontal
    Coroutine sliderCoroutine;
    
    List<System.Func<bool>> slideMethods;

    // Switches between sliding in one direction and its opposite
    public void ToggleSlider()
    {
        if(sliderCoroutine != null)
        {
            StopCoroutine(sliderCoroutine);
        }
        if (horizontal)
        {
            sliderCoroutine = StartCoroutine(Slide((slideDir+1) % 2));
            slideDir = (slideDir+1)%2;
        }
        else if (vertical)
        {
            sliderCoroutine = StartCoroutine(Slide(2 + ((slideDir + 1) % 2)));
            slideDir = 2 + ((slideDir + 1) % 2);
        }
    }

    // Slides in selected direction 
    public void SlideInDirection(int dir)
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

    // Slides toward upper bound
    public bool SlideUp()
    {
        transform.position += new Vector3(0, slideRate, 0);
        if (transform.localPosition.y >= yBounds[1])
            transform.localPosition = new Vector3(transform.localPosition.x, yBounds[1], transform.localPosition.z);
        return transform.localPosition.y >= yBounds[1];
    }

    // Slides toward lower bound
    public bool SlideDown()
    {
        transform.position -= new Vector3(0, slideRate, 0);
        if (transform.localPosition.y <= yBounds[0])
            transform.localPosition = new Vector3(transform.localPosition.x,yBounds[0], transform.localPosition.z);
        return transform.localPosition.y <= yBounds[0];
    }


    // dir: left = true, right = false
    public IEnumerator Slide(int dir)
    {
        // if sliding in same direction already, don't perform slide
        if (slideDir != dir)
        {
            yield return new WaitUntil(slideMethods[dir]);
            /*
            if (dir)
                yield return new WaitUntil(SlideLeft);
            else
                yield return new WaitUntil(SlideRight);
                */
        }
    }

	// Use this for initialization
	void Start () {
        slideMethods = new List<System.Func<bool>>();
        slideMethods.Add(SlideLeft);
        slideMethods.Add(SlideRight);
        slideMethods.Add(SlideUp);
        slideMethods.Add(SlideDown);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
