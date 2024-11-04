using UnityEngine;

namespace OWJam4ModProject;

/// <summary>
/// turn off water renderer gameobjects so you dont have fog and stencil bullshit
/// </summary>
[RequireComponent(typeof(OWTriggerVolume))]
public class Number1UnderwaterFogHater : MonoBehaviour
{
	private OWTriggerVolume _triggerVolume;
	public GameObject[] GameObjectsToDisable;

	private void Awake()
	{
		_triggerVolume = GetComponent<OWTriggerVolume>();

		_triggerVolume.OnEntry += OnEntry;
		_triggerVolume.OnExit += OnExit;
	}

	private void OnDestroy()
	{
		_triggerVolume.OnEntry -= OnEntry;
		_triggerVolume.OnExit -= OnExit;
	}

	private void OnEntry(GameObject hitobj)
	{
		foreach (var go in GameObjectsToDisable) go.SetActive(false);
	}

	private void OnExit(GameObject hitobj)
	{
		foreach (var go in GameObjectsToDisable) go.SetActive(true);
	}
}
