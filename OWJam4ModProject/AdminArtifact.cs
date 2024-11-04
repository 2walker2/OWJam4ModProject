using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using OWML.Common;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OWJam4ModProject;

public class AdminArtifact : MonoBehaviour
{
	public MorseCodeSensor[] CodeSensors;
	[Tooltip("The particles to disable outside the dreamworld")]
	[SerializeField] GameObject particles;

	public static void AttachToPlayerLantern()
	{
		var playerLantern = Locator.GetDreamWorldController().GetPlayerLantern();
		if (playerLantern.GetComponentInChildren<AdminArtifact>()) return; // already attached

		OWJam4ModProject.Log("elevating privileges");

		// detail builder is not working here so we must do it ourselves teehee
		var prefab = AssetBundleUtilities.AssetBundles["bundle"].bundle.LoadAsset<GameObject>("Assets/_Bundle/Prefabs/AdminArtifact.prefab");
		prefab.SetActive(false);
		AssetBundleUtilities.ReplaceShaders(prefab);
		var instance = Instantiate(prefab);
		instance.transform.SetParent(playerLantern.transform, false);
		instance.SetActive(true);
	}

	private void Update()
	{
		if (!OWJam4ModProject.DEBUG) return;

		if (!Keyboard.current[Key.LeftCtrl].isPressed) return;
		// copied names from unity
		if (Keyboard.current[Key.Digit1].wasPressedThisFrame) WarpToSpawnPoint("Spawn_DreamZone_1");
		if (Keyboard.current[Key.Digit2].wasPressedThisFrame) WarpToSpawnPoint("Spawn_DreamZone_2");
		if (Keyboard.current[Key.Digit3].wasPressedThisFrame) WarpToSpawnPoint("Spawn_DreamZone_3");
		if (Keyboard.current[Key.Digit4].wasPressedThisFrame) WarpToSpawnPoint("Spawn_DreamZone_4_Dock");
		if (Keyboard.current[Key.Digit5].wasPressedThisFrame) WarpToSpawnPoint("Spawn_DreamZone_2_LighthouseUpstairs");
		if (Keyboard.current[Key.Digit6].wasPressedThisFrame) WarpToSpawnPoint("Spawn_Underground_CodeTotems");
		if (Keyboard.current[Key.Digit7].wasPressedThisFrame) WarpToSpawnPoint("AdminZoneSpawnPoint");

		particles.SetActive(Locator.GetDreamWorldController().IsInDream());
	}

	private void Awake()
	{
		OWJam4ModProject.Log("hello i am admin now hooray", MessageType.Success);

		foreach (var sensor in CodeSensors)
		{
			sensor.OnEnterCode += () => OnEnterCode(sensor);
			// yes this means we cant remove this event. tiny memory leak. oh well
		}
	}

	private void OnEnterCode(MorseCodeSensor sensor)
	{
		OWJam4ModProject.Log($"sensor {sensor} activate", MessageType.Success);

		WarpToSpawnPoint(sensor.name);

		foreach (MorseCodeSensor codeSensor in CodeSensors)
			codeSensor.ClearCode();
	}

	void WarpToSpawnPoint(string spawnPointName)
	{
        SpawnPoint warpSpawnPoint = SearchUtilities.Find(spawnPointName).GetComponent<SpawnPoint>();
        Locator.GetPlayerBody().WarpToPositionRotation(warpSpawnPoint.transform.position, warpSpawnPoint.transform.rotation);
        Locator.GetPlayerBody().SetVelocity(warpSpawnPoint.GetPointVelocity());
    }
}
