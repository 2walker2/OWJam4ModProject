using NewHorizons.Utility;
using UnityEngine;

namespace OWJam4ModProject;

public class Ernesto : MonoBehaviour
{
	private Vector3 startPos, endPos;

	public static void Attach()
	{
		SearchUtilities.Find("Ernesto").AddComponent<Ernesto>();
	}

	private void Start()
	{
		var doohicky = SearchUtilities.Find("Doohicky").transform;
		transform.parent = doohicky;
		transform.position = doohicky.position + doohicky.up * 2.5f;
		transform.rotation = doohicky.rotation;

		endPos = transform.localPosition;
		startPos = endPos + transform.up * 50;
		transform.localPosition = endPos;

		// doohicky.GetComponentInChildren<MultiInteractReceiver>().OnReleaseInteract += GoTime;

		gameObject.SetActive(false);
	}

	private void GoTime(IInputCommands inputcommand)
	{

	}
}
