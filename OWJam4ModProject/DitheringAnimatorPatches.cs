using HarmonyLib;
using UnityEngine;

namespace OWJam4ModProject;

[HarmonyPatch(typeof(DitheringAnimator))]
public static class DitheringAnimatorPatches
{
	[HarmonyPostfix, HarmonyPatch(nameof(DitheringAnimator.Awake))]
	private static void DitheringAnimator_Awake(DitheringAnimator __instance)
	{
		// CHANGED: include inactive
		Renderer[] componentsInChildren = __instance.GetComponentsInChildren<Renderer>(true);
		__instance._renderers = new OWRenderer[componentsInChildren.Length];
		for (int i = 0; i < __instance._renderers.Length; i++)
		{
			__instance._renderers[i] = componentsInChildren[i].GetComponent<OWRenderer>();
			if (__instance._renderers[i] == null)
			{
				__instance._renderers[i] = componentsInChildren[i].gameObject.AddComponent<OWRenderer>();
			}
		}
	}

	[HarmonyPostfix, HarmonyPatch(nameof(DitheringAnimator.UpdateDithering))]
	private static void DitheringAnimator_UpdateDithering(DitheringAnimator __instance)
	{
		// some things dont have dither shader variable on them, so we have to do this
		foreach (var renderer in __instance._renderers)
		{
			if (!renderer) continue;
			renderer._renderer.enabled = __instance._visibleFraction != 0;
		}
	}
}
