//--------------ABOUT AND COPYRIGHT----------------------
// Copyright © 2013 SketchWork Productions Limited
//       support@sketchworkdev.com
//-------------------------------------------------------

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace _ThirdParty.SWP_HeartRateMonitor.Scripts
{
	/// <summary>
	/// This is the Heart Rate Monitor main script and controls every element of the control.
	/// </summary>
	public class SwpHeartRateMonitor : MonoBehaviour
	{
		private static readonly int TintColor = Shader.PropertyToID("_TintColor");

		private enum SoundType {HeartBeat1, HeartBeat2, Flatline};

		[FormerlySerializedAs("BeatsPerMinute")] public int beatsPerMinute = 90; // Beats per minute.
		[FormerlySerializedAs("FlatLine")] public bool flatLine = false; // Initialise a flat line.
	
		[FormerlySerializedAs("ShowBlip")] public bool showBlip = true; // Show the blip circle at the front of the monitor line.
		[FormerlySerializedAs("Blip")] public GameObject blip; // The blip game object.
		[FormerlySerializedAs("BlipSize")] public float blipSize = 1f; // The size of the blip circle at the front of the line.
		[FormerlySerializedAs("BlipTrailStartSize")] public float blipTrailStartSize = 0.2f; // The size of the monitor trail line nearest to the blip circle.
		[FormerlySerializedAs("BlipTrailEndSize")] public float blipTrailEndSize = 0.1f; // The size of the monitor line at the end before it fades out.
		[FormerlySerializedAs("BlipMonitorWidth")] public float blipMonitorWidth = 40f; // The actual width of the entire monitor control.
		[FormerlySerializedAs("BlipMonitorHeightModifier")] public float blipMonitorHeightModifier = 1f; // The actual height of the entire monitor control.
		
		[FormerlySerializedAs("EnableSound")] public bool enableSound = true;
		[FormerlySerializedAs("SoundVolume")] public float soundVolume = 1f;
		[FormerlySerializedAs("Heart1Sound")] public AudioClip heart1Sound;
		[FormerlySerializedAs("Heart2Sound")] public AudioClip heart2Sound;
		[FormerlySerializedAs("FlatlineSound")] public AudioClip flatlineSound;
		private bool _bFlatLinePlayed;

		private const float LineSpeed = 0.3f;
		private GameObject _newClone;
		private float _trailTime;
		private float _beatsPerSecond;
		private float _lastUpdate;
		private Vector3 _blipOffset = Vector3.zero;
		private float _displayXEnd;
	
		[FormerlySerializedAs("MainMaterial")] public Material mainMaterial;
	
		[FormerlySerializedAs("NormalColour")] public Color normalColour = new Color(0f, 1f, 0f, 1f);
		[FormerlySerializedAs("MediumColour")] public Color mediumColour = new Color(1f, 1f, 0f, 1f);	
		[FormerlySerializedAs("BadColour")] public Color badColour = new Color(1f, 0f, 0f, 1f);
		[FormerlySerializedAs("FlatlineColour")] public Color flatlineColour = new Color(1f, 0f, 0f, 1f); // Automatic when BeatsPerMinute is Zero
	
		// Use this for initialization
		private void Start()
		{
			_beatsPerSecond = 60f / beatsPerMinute;
			_blipOffset = new Vector3 (transform.position.x - (blipMonitorWidth / 2), transform.position.y, transform.position.z);
			_displayXEnd = _blipOffset.x + blipMonitorWidth;
			CreateClone();
			_trailTime = _newClone.GetComponentInChildren<TrailRenderer>().time;
		}
	
		// Update is called once per frame
		private void Update()
		{
			_beatsPerSecond = 60f / beatsPerMinute;
			_blipOffset = new Vector3 (transform.position.x - (blipMonitorWidth / 2), transform.position.y, transform.position.z);
			_displayXEnd = _blipOffset.x + blipMonitorWidth;
		
			if (_newClone.transform.position.x > _displayXEnd)
			{			
				if (_newClone != null)
				{
					var oldClone = _newClone;
					StartCoroutine(WaitThenDestroy(oldClone));
				}
			
				CreateClone();
			}
			else if (!flatLine)
				_newClone.transform.position += new Vector3(blipMonitorWidth * Time.deltaTime * LineSpeed, Random.Range(-0.05f, 0.05f), 0);
			else
			{
				_newClone.transform.position += new Vector3(blipMonitorWidth * Time.deltaTime * LineSpeed, 0, 0);
			
				if (!_bFlatLinePlayed)
				{
					PlayHeartSound(SoundType.Flatline, soundVolume);
					_bFlatLinePlayed = true;
				}
			}
		
			if (beatsPerMinute <= 0 || flatLine)
				_lastUpdate = Time.time;
			else if (Time.time - _lastUpdate >= _beatsPerSecond)
			{			
				_lastUpdate = Time.time;
				StartCoroutine(PerformBlip());
			}
		}

		private IEnumerator PerformBlip()
		{
			if (_bFlatLinePlayed)
				_bFlatLinePlayed = false;
		
			if (!_bFlatLinePlayed)
				PlayHeartSound(SoundType.HeartBeat1, soundVolume);
		
			_newClone.transform.position = new Vector3(_newClone.transform.position.x, (10f * blipMonitorHeightModifier) + Random.Range(0f, (2f * blipMonitorHeightModifier)) + _blipOffset.y, _blipOffset.z);
			yield return new WaitForSeconds(0.03f);		
			_newClone.transform.position = new Vector3(_newClone.transform.position.x, (-5f * blipMonitorHeightModifier) - Random.Range(0f, (3f * blipMonitorHeightModifier)) + _blipOffset.y, _blipOffset.z);
			yield return new WaitForSeconds(0.02f);		
			_newClone.transform.position = new Vector3(_newClone.transform.position.x, (3f * blipMonitorHeightModifier) + Random.Range(0f, (2f * blipMonitorHeightModifier)) + _blipOffset.y, _blipOffset.z);
			yield return new WaitForSeconds(0.02f);		
			_newClone.transform.position = new Vector3(_newClone.transform.position.x, (2f * blipMonitorHeightModifier) + Random.Range(0f, (1f * blipMonitorHeightModifier)) + _blipOffset.y, _blipOffset.z);
			yield return new WaitForSeconds(0.02f);		
		
			_newClone.transform.position = new Vector3(_newClone.transform.position.x, 0f + _blipOffset.y, _blipOffset.z);

			yield return new WaitForSeconds(0.2f);		
		
			if (!_bFlatLinePlayed)
				PlayHeartSound(SoundType.HeartBeat2, soundVolume);
		}

		private void CreateClone()
		{
			_newClone = Instantiate(blip, new Vector3(_blipOffset.x, _blipOffset.y, _blipOffset.z), Quaternion.identity) as GameObject;
			_newClone.transform.parent = gameObject.transform;
		
			_newClone.GetComponentInChildren<TrailRenderer>().startWidth = blipTrailStartSize;
			_newClone.GetComponentInChildren<TrailRenderer>().endWidth = blipTrailEndSize;
			_newClone.transform.localScale = new Vector3(blipSize,blipSize,blipSize);

			_newClone.GetComponent<MeshRenderer>().enabled = showBlip;
		}

		private IEnumerator WaitThenDestroy(GameObject oldClone)
		{
			oldClone.GetComponent<MeshRenderer>().enabled = false;
			yield return new WaitForSeconds(_trailTime);
			Destroy(oldClone);
		}

		private void PlayHeartSound(SoundType soundType, float fSoundVolume)
		{
			if (!enableSound)
				return;

			switch (soundType)
			{
				case SoundType.HeartBeat1:
					GetComponent<AudioSource>().PlayOneShot(heart1Sound, fSoundVolume);
					break;
				case SoundType.HeartBeat2:
					GetComponent<AudioSource>().PlayOneShot(heart2Sound, fSoundVolume);
					break;
				case SoundType.Flatline:
					GetComponent<AudioSource>().PlayOneShot(flatlineSound, fSoundVolume);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(soundType), soundType, null);
			}
		}
		
		public void SetHeartRateColour(Color newColour)
		{
			if (mainMaterial == null)
				throw new System.ArgumentException("You are trying to change the colour without having the 'MainMaterial' set in the control.  It must be set to the 'BlipMaterial' in order to use the colour changer.");
		
			mainMaterial.SetColor(TintColor, newColour);
		}
	}
}
