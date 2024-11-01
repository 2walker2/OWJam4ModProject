using NewHorizons.Utility;
using System.Collections;
using UnityEngine;

namespace OWJam4ModProject
{
    /// <summary>
    /// handles takeoff, landing, and flighting orbit around the planet
    /// </summary>
    internal class ShuttleFlightController : MonoBehaviour
    {
        [Tooltip("The shuttle's takeoff controller")]
        [SerializeField] ShuttleTakeoffController takeoffController;
        [Tooltip("The name of the transform to target")]
        [SerializeField] string landingTargetName;
        [Tooltip("The radius away from it's landing target at which the ship orbits")]
        [SerializeField] float orbitRadius;
        [Tooltip("How fast the shuttle moves")]
        [SerializeField] float flightSpeed;
        [Tooltip("The light sensor that activates flight")]
        [SerializeField] SingleLightSensor lightSensor;
        [Tooltip("The shuttle's OWRigidbody")]
        [SerializeField] OWRigidbody body;

        Transform landingTarget;

        void Start()
        {
            lightSensor.OnDetectLight += StartFlight;

            landingTarget = SearchUtilities.Find(landingTargetName).transform;
        }

        void OnDestroy()
        {
            lightSensor.OnDetectLight -= StartFlight;
        }

        void StartFlight()
        {
            OWJam4ModProject.instance.ModHelper.Console.WriteLine("Starting shuttle flight", OWML.Common.MessageType.Success);

            // Don't take off if the takeoff puzzle isn't solved
            if (!takeoffController.CanTakeOff())
                return;

            StartCoroutine(FlyToPlanet());
        }

        IEnumerator FlyToPlanet()
        {
            // Start moving towards planet
            Vector3 towardsPlanet = (landingTarget.position - body.transform.position).normalized;
            Vector3 velocity = towardsPlanet * flightSpeed;
            body.SetVelocity(velocity);

            // start aligning with body
            var align = body.GetComponent<AlignWithTargetBody>();
            align.SetTargetBody(landingTarget.GetAttachedOWRigidbody());
            align.enabled = true;

            // wait until landed
            while (Vector3.Distance(landingTarget.position, body.transform.position) > orbitRadius)
            {
                yield return null;
            }

            body.SetVelocity(Vector3.zero); // do smooth stop later
            Locator.GetPlayerBody().AddVelocityChange(-velocity);

            yield return DoFlightControlsLoop();
        }

        private IEnumerator DoFlightControlsLoop()
        {
            while (true)
            {
                yield return null;
            }
        }
    }
}
