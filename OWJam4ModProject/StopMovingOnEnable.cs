using UnityEngine;

namespace OWJam4ModProject;

public class StopMovingOnEnable : MonoBehaviour
{
	private void OnEnable()
	{
		this.GetAttachedOWRigidbody().SetVelocity(Vector3.zero);
	}
}
