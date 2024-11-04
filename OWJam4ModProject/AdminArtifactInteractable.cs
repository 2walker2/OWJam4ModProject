using UnityEngine;

namespace OWJam4ModProject;

public class AdminArtifactInteractable : MonoBehaviour
{
	public InteractReceiver InteractReceiver;

	private void Awake()
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
		gameObject.SetActive(false);
		AdminArtifact.AttachToPlayerLantern();
	}
}
