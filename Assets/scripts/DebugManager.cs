using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugManager : MonoBehaviour {
    public static DebugManager dbm;
    bool isDebugging;
    public int frameCount = 0;
    public float dt = 0.0f, 
          fps = 0.0f, 
          updateRate = 4.0f;
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
        if(dt > 1.0f / updateRate)
        {
            fps = frameCount / dt;
            frameCount = 0;
            dt -= 1.0f / updateRate;
            transform.Find("Canvas").Find("Text").GetComponent<Text>().text = "FPS:" + fps.ToString("F1");
        }
    }

    public float FrameRate()
    {
        return 60f / fps;
    }

    public void SetDebugMode(bool debug)
    {
        isDebugging = debug;
        transform.Find("Canvas").gameObject.SetActive(isDebugging);
    }
}
