using NewHorizons.Builder.Props;
using NewHorizons.External.Modules.Props;
using UnityEngine;

namespace OWJam4ModProject;

public class AdminArtifact : MonoBehaviour
{
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
}
