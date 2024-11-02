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
        [SerializeField] internal OWRigidbody body;

        [Header("Orbit sensors")]
        public SingleLightSensor ForwardSensor;
        public SingleLightSensor BackSensor;
        public SingleLightSensor LeftSensor;
        public SingleLightSensor RightSensor;
        [Space]
        public float LandingSpeed = 10f;

        Transform landingTarget;

        void Start()
        {
            lightSensor.OnDetectLight += StartFlight;

            landingTarget = SearchUtilities.Find(landingTargetName).transform;

            body.GetAttachedFluidDetector().GetComponent<ForceApplier>().enabled = false; // dont apply fluids FOR NOW
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
            align.SetUsePhysicsToRotate(true);
            align.SetTargetBody(landingTarget.GetAttachedOWRigidbody());
            align.enabled = true;

            // wait until in orbit
            const int CLOUD_RADIUS = 450;
            while (Vector3.Distance(landingTarget.position, body.GetPosition()) > orbitRadius)
            {
                yield return null;
            }

            // instantly stop. its easier
            Locator.GetPlayerBody().AddVelocityChange(-body.GetVelocity());
            body.SetVelocity(Vector3.zero);

            yield return DoFlightControlsLoop();
        }

        private IEnumerator DoFlightControlsLoop()
        {
            OWJam4ModProject.instance.ModHelper.Console.WriteLine("weve stopped. its time to fly");

            body.GetAttachedFluidDetector().GetComponent<ForceApplier>().enabled = true; // we need drag now

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
                    var diff = Vector3.Distance(landingTarget.position, body.GetPosition()) - orbitRadius;
                    direction += Vector3.up * diff / 10f;
                }

                direction = transform.TransformDirection(direction);

                body.AddAcceleration(direction);

                yield return null;
            }
        }

        public void StartLanding(ShuttleLandingPoint landingPoint)
        {
            // stop the flight thing
            StopAllCoroutines();
            StartCoroutine(DoLanding(landingPoint));
        }

        private IEnumerator DoLanding(ShuttleLandingPoint landingPoint)
        {
            // flip around
            var align = body.GetComponent<AlignWithTargetBody>();
            align.SetUsePhysicsToRotate(false); // for some reason it doesnt flip unless i do this
            align.SetLocalAlignmentAxis(Vector3.down);

            Vector3 towardsPlanet = (landingTarget.position - body.GetPosition()).normalized;
            Vector3 velocity = towardsPlanet * LandingSpeed;
            body.SetVelocity(velocity);

            const int PLANET_RADIUS = 50;
            while (Vector3.Distance(landingTarget.position, body.GetPosition()) > PLANET_RADIUS)
            {
                yield return null;
            }

            // "collided" with ground. stop instantly
            Locator.GetPlayerBody().AddVelocityChange(-body.GetVelocity());
            body.SetVelocity(Vector3.zero);
        }
    }
}
