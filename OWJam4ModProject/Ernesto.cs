using NewHorizons.Utility;
using System.Collections;
using UnityEngine;

namespace OWJam4ModProject;

public class Ernesto : MonoBehaviour
{
	private Vector3 startLocalPos, endLocalPos;

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

		endLocalPos = transform.localPosition;
		startLocalPos = endLocalPos + Vector3.up * 10;
		transform.localPosition = endLocalPos;

		doohicky.GetComponentInChildren<MultiInteractReceiver>().OnPressInteract += GoTime;

		gameObject.SetActive(false);
	}

	private void GoTime(IInputCommands inputcommand)
	{
		OWJam4ModProject.Log("ernesto time");
		gameObject.SetActive(true);
		StopAllCoroutines();
		StartCoroutine(DoGoTime());
	}

	private IEnumerator DoGoTime()
	{
		var startTime = Time.time;
		var endTime = Time.time + 3;
		while (Time.time < endTime)
		{
			var t = Mathf.InverseLerp(startTime, endTime, Time.time);
			transform.localPosition = Vector3.Lerp(startLocalPos, endLocalPos, t);
			yield return null;
		}

		OWJam4ModProject.Log("ernesto is here");
	}
}
