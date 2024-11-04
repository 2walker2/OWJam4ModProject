using UnityEngine;

namespace OWJam4ModProject;

public class MorseCodeBeep : MonoBehaviour
{
	public SingleLightSensor LightSensor;
	public OWAudioSource AudioSource;

	float originalVolume;

	private void Start()
	{
		LightSensor.OnDetectLight -= OnDetectLight;
		LightSensor.OnDetectDarkness -= OnDetectDarkness;
		LightSensor.OnDetectLight += OnDetectLight;
		LightSensor.OnDetectDarkness += OnDetectDarkness;

		// owaudiosource is being bad so dont use it
		originalVolume = AudioSource.volume;
		AudioSource._audioSource.volume = 0;
		AudioSource._audioSource.Play();
	}

	void OnDestroy()
	{
		LightSensor.OnDetectLight -= OnDetectLight;
		LightSensor.OnDetectDarkness -= OnDetectDarkness;
	}

	// work if you turn the script on and off
	private void OnEnable()
	{
		LightSensor.OnDetectLight -= OnDetectLight;
		LightSensor.OnDetectDarkness -= OnDetectDarkness;
		LightSensor.OnDetectLight += OnDetectLight;
		LightSensor.OnDetectDarkness += OnDetectDarkness;

		if (LightSensor.IsIlluminated()) OnDetectLight();
	}

	private void OnDisable()
	{
		LightSensor.OnDetectLight -= OnDetectLight;
		LightSensor.OnDetectDarkness -= OnDetectDarkness;

		OnDetectDarkness();
	}

	private void OnDetectLight()
	{
		AudioSource._audioSource.volume = originalVolume;
	}

	private void OnDetectDarkness()
	{
		AudioSource._audioSource.volume = 0;
	}
}
