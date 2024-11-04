using NewHorizons.Utility.OWML;
using UnityEngine;

namespace OWJam4ModProject;

public class MorseCodeBeep : MonoBehaviour
{
	public SingleLightSensor LightSensor;
	public OWAudioSource AudioSource;

	private void Start()
	{
		LightSensor.OnDetectLight += OnDetectLight;
		LightSensor.OnDetectDarkness += OnDetectDarkness;

		// owaudiosource is being bad so dont use it
		AudioSource._audioSource.volume = 0;
		AudioSource._audioSource.Play();
	}

	void OnDestroy()
	{
		LightSensor.OnDetectLight -= OnDetectLight;
		LightSensor.OnDetectDarkness -= OnDetectDarkness;
	}

	private void OnDetectLight()
	{
		AudioSource._audioSource.volume = .5f;
	}

	private void OnDetectDarkness()
	{
		AudioSource._audioSource.volume = 0;
	}
}
