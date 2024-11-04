using UnityEngine;

namespace OWJam4ModProject;

public class AdminArtifactInteractable : MonoBehaviour
{
	public InteractReceiver InteractReceiver;

	[Tooltip("The gameobject to disable when picked up")]
	[SerializeField] GameObject objectToDisable;

	private void Start()
	{
		InteractReceiver.ChangePrompt("Elevate Privileges");
		InteractReceiver.OnPressInteract += OnPressInteract;
	}

	private void OnDestroy()
	{
		InteractReceiver.OnPressInteract -= OnPressInteract;
	}

	private void OnPressInteract()
	{
		objectToDisable.SetActive(false);
		AdminArtifact.AttachToPlayerLantern();
	}
}
