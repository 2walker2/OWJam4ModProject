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

        [Header("Orbit sensors")]
        public SingleLightSensor ForwardSensor;
        public SingleLightSensor BackSensor;
        public SingleLightSensor LeftSensor;
        public SingleLightSensor RightSensor;

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
            // if (!takeoffController.CanTakeOff())
                // return;

            lightSensor.OnDetectLight -= StartFlight;

            StartCoroutine(FlyToPlanet());
        }

        IEnumerator FlyToPlanet()
        {
            // Start moving towards planet
            Vector3 towardsPlanet = (landingTarget.position - body.GetPosition()).normalized;
            Vector3 velocity = towardsPlanet * flightSpeed;
            body.SetVelocity(velocity);

            // start aligning with body
            var align = body.GetComponent<AlignWithTargetBody>();
            align.SetTargetBody(landingTarget.GetAttachedOWRigidbody());
            align.enabled = true;

            // wait until in planet
            while (Vector3.Distance(landingTarget.position, body.GetPosition()) > 450/*cloud radius*/)
            {
                yield return null;
            }

            // Locator.GetPlayerBody().AddVelocityChange(-body.GetVelocity());
            // body.SetVelocity(Vector3.zero); // do smooth stop later

            yield return DoFlightControlsLoop();
        }

        private IEnumerator DoFlightControlsLoop()
        {
            OWJam4ModProject.instance.ModHelper.Console.WriteLine("weve stopped. its time to fly");

            while (true)
            {
                var direction = Vector3.zero;
                if (ForwardSensor.IsIlluminated()) direction += Vector3.forward;
                if (BackSensor.IsIlluminated()) direction += Vector3.back;
                if (LeftSensor.IsIlluminated()) direction += Vector3.left;
                if (RightSensor.IsIlluminated()) direction += Vector3.right;
                direction *= 10;

                // stay in the orbit radius
                {
                    var diff = Vector3.Distance(landingTarget.position, body.transform.position) - orbitRadius;
                    direction += Vector3.up * diff / 10f;
                }

                direction = transform.TransformDirection(direction);

                body.AddAcceleration(direction);
                // TODO: make it stay in the orbit radius

                yield return null;
            }
        }
    }
}
