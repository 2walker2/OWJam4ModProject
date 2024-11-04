using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using OWML.Common;
using System;
using UnityEngine;

namespace OWJam4ModProject;

public class AdminArtifact : MonoBehaviour
{
	public MorseCodeSensor[] CodeSensors;

	public static void AttachToPlayerLantern()
	{
		var playerLantern = Locator.GetDreamWorldController().GetPlayerLantern();
		if (playerLantern.GetComponentInChildren<AdminArtifact>()) return; // already attached

		OWJam4ModProject.instance.ModHelper.Console.WriteLine("elevating privileges");

		// detail builder is not working here so we must do it ourselves teehee
		var prefab = AssetBundleUtilities.AssetBundles["bundle"].bundle.LoadAsset<GameObject>("Assets/_Bundle/Prefabs/AdminArtifact.prefab");
		prefab.SetActive(false);
		AssetBundleUtilities.ReplaceShaders(prefab);
		var instance = Instantiate(prefab);
		instance.transform.SetParent(playerLantern.transform, false);
		instance.SetActive(true);
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
		OWJam4ModProject.instance.ModHelper.Console.WriteLine($"sensor {sensor} activate", MessageType.Success);

		WarpToSpawnPoint(sensor.name);
	}

	void WarpToSpawnPoint(string spawnPointName)
	{
        SpawnPoint warpSpawnPoint = SearchUtilities.Find(spawnPointName).GetComponent<SpawnPoint>();
        Locator.GetPlayerBody().WarpToPositionRotation(warpSpawnPoint.transform.position, warpSpawnPoint.transform.rotation);
        Locator.GetPlayerBody().SetVelocity(warpSpawnPoint.GetPointVelocity());
    }
}
