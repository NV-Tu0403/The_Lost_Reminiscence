using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightRays : MonoBehaviour {
	private Light Sun;
	public bool AlignWithSunDirection = true;
	public bool GetColorFromSun = false;
	public string sunTag = "Sun";
	// Use this for initialization
	void Start () {

		Material mtl = GetComponent<MeshRenderer> ().material;

		if(AlignWithSunDirection){
		if(GameObject.FindGameObjectWithTag (sunTag) != null){
			Sun = GameObject.FindGameObjectWithTag (sunTag).GetComponent<Light>();
			transform.rotation = Sun.transform.rotation;
				if(GetColorFromSun){
					mtl.color = Sun.color;
				}
		}
		}

		float rndX = Random.Range (0f, 1f);
		float rndY = Random.Range (0f, 1f);

		mtl.SetTextureOffset ("_Noise", new Vector2(rndX, rndY));
	}
}
