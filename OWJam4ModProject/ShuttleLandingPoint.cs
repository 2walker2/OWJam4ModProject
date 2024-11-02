using UnityEngine;

namespace OWJam4ModProject;

public class ShuttleLandingPoint : MonoBehaviour
{
	public MorseCodeSensor CodeSensor;

	private void Awake()
	{
		CodeSensor.OnEnterCode += OnCode;
	}

	private void OnDestroy()
	{
		CodeSensor.OnEnterCode -= OnCode;
	}

	private void OnCode()
	{
		OWJam4ModProject.instance.ModHelper.Console.WriteLine("ding dong");

		FindObjectOfType<ShuttleFlightController>().StartLanding(this);
	}
}
