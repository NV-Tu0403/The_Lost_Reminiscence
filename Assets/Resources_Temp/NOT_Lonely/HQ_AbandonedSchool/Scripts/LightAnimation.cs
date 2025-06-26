using UnityEngine;
using System.Collections;

public class LightAnimation : MonoBehaviour {
    private Transform _transform;
	private Light LightIntensity;
	private float StartIntensity;

    [SerializeField] private float _speed = 5f;
	[Range(0, 10)]
	[SerializeField] private float _randomIntensity = 0.2f;
	public float intensityMultiplier = 1;
	private float startIntensity;
	public MeshRenderer[] EmissiveObjects;
	private Color startColor;
	private bool objectsAdded = true;

    void Awake () {
		LightIntensity = GetComponent<Light> ();
		if (EmissiveObjects.Length > 0) {
			startColor = EmissiveObjects [0].material.GetColor ("_EmissionColor");
			objectsAdded = true;
		} else {
			objectsAdded = false;
		}
    }

    void Start () {
        StartCoroutine (LightAnim ());
    }

    IEnumerator LightAnim () {
		StartIntensity = LightIntensity.intensity;

        while (true) {
			float lastIntensity = LightIntensity.intensity;
			float randomIntensity = StartIntensity - Random.Range (0, _randomIntensity);


            float time = 0f;

            while (time < 1f) {
				
				LightIntensity.intensity = Mathf.Lerp (lastIntensity, randomIntensity, time);

				float emission = Mathf.Lerp (lastIntensity, randomIntensity, time);
				Color finalColor = startColor * Mathf.LinearToGammaSpace (emission * intensityMultiplier);

				if(objectsAdded){
					foreach (MeshRenderer _renderer in EmissiveObjects) {
						_renderer.material.SetColor ("_EmissionColor", finalColor);
					}
				}

                time += Time.deltaTime * _speed;
                yield return null;
            }

            yield return null;
        }
    }
}
