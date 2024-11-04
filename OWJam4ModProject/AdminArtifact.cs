using NewHorizons.Builder.Props;
using NewHorizons.External.Modules.Props;
using OWML.Common;
using UnityEngine;

namespace OWJam4ModProject;

public class AdminArtifact : MonoBehaviour
{
	public MorseCodeSensor[] CodeSensors;

	public static void AttachToPlayerLantern()
	{
		var playerLantern = Locator.GetDreamWorldController().GetPlayerLantern();
		if (playerLantern.GetComponentInChildren<AdminArtifact>()) return; // already attached

		var adminStuff = DetailBuilder.Make(playerLantern.gameObject, null, new DetailInfo()
		{
			assetBundle = "assets/bundle",
			path = "Assets/_Bundle/Prefabs/AdminArtifact.prefab"
		});
	}

	private void Awake()
	{
		OWJam4ModProject.instance.ModHelper.Console.WriteLine("hello i am admin now hooray", MessageType.Success);

		foreach (var sensor in CodeSensors)
		{
			sensor.OnEnterCode += () => OnEnterCode(sensor);
			// yes this means we cant remove this event. tiny memory leak. oh well
		}
	}

	private void OnEnterCode(MorseCodeSensor sensor)
	{
		OWJam4ModProject.instance.ModHelper.Console.WriteLine($"sensor {sensor} activate");
	}
}
