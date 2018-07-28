using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSoundHandler : MonoBehaviour {
    int particles = 0;
    ParticleSystem ps;
    public AudioClip fireworkRise, fireworkExplode;
	// Use this for initialization
	void Start () {
        ps = GetComponent<ParticleSystem>();
	}
	
	// Update is called once per frame
	void Update () {
		if(particles != ps.particleCount)
        {
            if(particles > ps.particleCount)
            {
                //StartCoroutine(GameManager.gm.PlaySFX(fireworkExplode));
            }
            else
            {

                //StartCoroutine(GameManager.gm.PlaySFX(fireworkRise));
            }
        }
        particles = ps.particleCount;
	}
}
