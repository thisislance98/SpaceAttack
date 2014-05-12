using UnityEngine;
using System.Collections;

public class ParticleAttractor : MonoBehaviour {

	public ParticleSystem System;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		ParticleSystem.Particle[] particles = new ParticleSystem.Particle[1000];

		int size = System.GetParticles(particles);

		for  (int i=0; i < size; i++)
		{
			float percent = 1 - (particles[i].lifetime/particles[i].startLifetime);
			if (i == 0)
				Debug.Log("percent: " + percent);

			Vector3 dir = (transform.position - particles[i].position).normalized * particles[i].velocity.magnitude;
			particles[i].velocity = Vector3.Slerp(particles[i].velocity,dir, percent);

		}

		System.SetParticles (particles,size);
	}
}
