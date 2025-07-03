using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesAutoColor : MonoBehaviour {
	private Light Sun;
	private ParticleSystem _particleSys;
	public bool UseSunColor = true;
	// Use this for initialization
	void Awake () {

		if(UseSunColor){
		if(GameObject.FindGameObjectWithTag ("Sun").GetComponent<Light> () != null){

			_particleSys = gameObject.GetComponent<ParticleSystem> ();
			float _particlesA = _particleSys.main.startColor.color.a;

			var ps = _particleSys.emission;

			ps.enabled = false;
			_particleSys.Stop ();

			Sun = GameObject.FindGameObjectWithTag ("Sun").GetComponent<Light> ();

			Color newColor = new Color (Sun.color.r, Sun.color.g, Sun.color.b, _particlesA);

			var main = _particleSys.main;
			main.startColor = newColor;

			ps.enabled = true;
			_particleSys.Play ();
		}

	}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
