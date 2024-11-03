using UnityEngine;

namespace OWJam4ModProject;

public class ShuttleLandingPoint : MonoBehaviour
{
	[Header("hook up one or the other, or both")]
	public MorseCodeSensor CodeSensor;
	public SingleLightSensor LightSensor;

	private void Awake()
	{
		if (CodeSensor) CodeSensor.OnEnterCode += OnCode;
		if (LightSensor) LightSensor.OnDetectLight += OnCode;
	}

	private void OnDestroy()
	{
		if (CodeSensor) CodeSensor.OnEnterCode -= OnCode;
		if (LightSensor) LightSensor.OnDetectLight -= OnCode;
	}

	private void OnCode()
	{
		OWJam4ModProject.instance.ModHelper.Console.WriteLine("ding dong");

		FindObjectOfType<ShuttleFlightController>().StartLanding(this);
	}
}
