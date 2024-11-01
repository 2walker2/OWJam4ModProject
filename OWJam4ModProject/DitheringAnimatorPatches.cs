using HarmonyLib;

namespace OWJam4ModProject;

// some things dont have dither shader variable on them, so we have to do this
[HarmonyPatch(typeof(DitheringAnimator))]
public static class DitheringAnimatorPatches
{
	[HarmonyPostfix, HarmonyPatch(nameof(DitheringAnimator.UpdateDithering))]
	private static void DitheringAnimator_UpdateDithering(DitheringAnimator __instance)
	{
		foreach (var renderer in __instance._renderers)
		{
			if (!renderer) continue;
			renderer._renderer.enabled = __instance._visibleFraction != 0;
		}
	}
}
