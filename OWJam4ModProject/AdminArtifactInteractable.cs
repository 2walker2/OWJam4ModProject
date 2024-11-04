using UnityEngine;

namespace OWJam4ModProject;

public class AdminArtifactInteractable : MonoBehaviour
{
	public InteractReceiver InteractReceiver;

	[Tooltip("The gameobject to disable when picked up")]
	[SerializeField] GameObject objectToDisable;
	[Tooltip("The pickup audio source")]
	[SerializeField] OWAudioSource pickupAudio;

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
		pickupAudio.Play();

		AdminArtifact.AttachToPlayerLantern();

		InteractReceiver.DisableInteraction();
	}
}
