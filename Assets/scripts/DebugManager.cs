using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugManager : MonoBehaviour {
    public static DebugManager dbm;
    bool isDebugging;
    public int frameCount = 0;
    public double dt = 0.0, 
          fps = 0.0, 
          updateRate = 4.0;
	// Use this for initialization
	void Start () {
        if (dbm)
        {
            Destroy(gameObject);
            return;
        }

        dbm = this;
        DontDestroyOnLoad(dbm);
        transform.Find("Canvas").gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
        frameCount++;
        dt += Time.deltaTime;
        if(dt > 1.0 / updateRate)
        {
            fps = frameCount / dt;
            frameCount = 0;
            dt -= 1.0 / updateRate;
            transform.Find("Canvas").Find("Text").GetComponent<Text>().text = "FPS:" + fps.ToString("F1");
        }
    }

    public void SetDebugMode(bool debug)
    {
        isDebugging = debug;
        transform.Find("Canvas").gameObject.SetActive(isDebugging);
    }
}
