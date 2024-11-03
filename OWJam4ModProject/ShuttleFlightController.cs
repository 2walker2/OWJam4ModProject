using NewHorizons.Utility;
using OWML.Common;
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

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
        public PlayerAttachPoint PlayerAttachPoint;

        private const int CLOUD_RADIUS = 450;
        private const int PLANET_RADIUS = 50;

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

        #region flight

        void Update()
        {
            if (Keyboard.current[Key.Home].wasPressedThisFrame) StartFlight();
        }

        void StartFlight()
        {
            OWJam4ModProject.instance.ModHelper.Console.WriteLine("Starting shuttle flight", MessageType.Success);

            // Don't take off if the takeoff puzzle isn't solved
            // if (!takeoffController.CanTakeOff())
                // return;

            StopAllCoroutines();
            StartCoroutine(FlyToPlanet());
        }

        IEnumerator FlyToPlanet()
        {
            var belowOrbit = Vector3.Distance(landingTarget.position, body.GetPosition()) < orbitRadius;

            // start aligning with body
            StartCoroutine(DoAlign(Vector3.up));

            // Start moving towards planet
            Vector3 towardsPlanet = (landingTarget.position - body.GetPosition()).normalized;
            Vector3 velocity = towardsPlanet * flightSpeed;
            if (belowOrbit)
                velocity = -towardsPlanet * LandingSpeed;
            body.SetVelocity(velocity);

            // wait until in orbit
            if (!belowOrbit)
                while (Vector3.Distance(landingTarget.position, body.GetPosition()) > orbitRadius)
                    yield return null;
            else
                while (Vector3.Distance(landingTarget.position, body.GetPosition()) < orbitRadius)
                    yield return null;


            // instantly stop. its easier
            Locator.GetPlayerBody().AddVelocityChange(-body.GetVelocity());
            body.SetVelocity(Vector3.zero);

            yield return DoFlightControlsLoop();
        }

        #endregion




        #region orbit controls

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
                direction *= 40;

                // stay in the orbit radius
                {
                    var diff = Vector3.Distance(landingTarget.position, body.GetPosition()) - orbitRadius;
                    direction += Vector3.up * diff / 10;
                }

                direction = transform.TransformDirection(direction);

                body.AddAcceleration(direction);

                yield return null;
            }
        }

        #endregion




        #region landing

        public void StartLanding(ShuttleLandingPoint landingPoint)
        {
            // stop the flight thing
            StopAllCoroutines();
            StartCoroutine(DoLanding(landingPoint));
        }

        private IEnumerator DoLanding(ShuttleLandingPoint landingPoint)
        {
            body.GetAttachedFluidDetector().GetComponent<ForceApplier>().enabled = false;

            // move to landing spot
            Vector3 towardsLanding = (landingPoint.transform.position - body.GetPosition()).normalized;
            Vector3 velocity = towardsLanding * LandingSpeed;
            body.SetVelocity(velocity);

            var hits = Physics.RaycastAll(body.GetPosition(), towardsLanding, PLANET_RADIUS, OWLayerMask.physicalMask, QueryTriggerInteraction.Ignore);
            var hit = hits.Single(x => x.collider.GetComponentInParent<ShuttleLandingPoint>() == landingPoint);

            // get height to land on
            var height = Vector3.Distance(landingTarget.position, hit.transform.position);
            OWJam4ModProject.instance.ModHelper.Console.WriteLine($"height to land at is {height}. current distance is {Vector3.Distance(landingTarget.position, body.GetPosition())}");

            while (Vector3.Distance(landingTarget.position, body.GetPosition()) > height)
            {
                yield return null;
            }

            // "collided" with ground. stop instantly
            Locator.GetPlayerBody().AddVelocityChange(-body.GetVelocity());
            body.SetVelocity(Vector3.zero);

            OWJam4ModProject.instance.ModHelper.Console.WriteLine("landed");
        }

        #endregion


        private IEnumerator DoAlign(Vector3 direction)
        {
            var align = body.GetComponent<AlignWithTargetBody>();
            align.SetUsePhysicsToRotate(true);
            align.SetTargetBody(landingTarget.GetAttachedOWRigidbody());
            align.SetLocalAlignmentAxis(direction);
            align.enabled = true;

            // hack: have to turn off use physics to rotate, so we attach player temporarily to make it not as awful
            align.SetUsePhysicsToRotate(false); // for some reason it doesnt flip unless i do this

            PlayerAttachPoint.transform.position = Locator.GetPlayerTransform().position;
            PlayerAttachPoint.transform.rotation = Locator.GetPlayerTransform().rotation;
            PlayerAttachPoint.AttachPlayer();
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < 100)
            {
                yield return null;
            }
            sw.Stop();
            PlayerAttachPoint.DetachPlayer();

            align.SetUsePhysicsToRotate(true);
        }





        #region reset

        private Transform joe;

        public void ResetShuttle()
        {
            StopAllCoroutines();

            if (!joe)
            {
                // first time in dreamworld. just grab current location
                joe = new GameObject("joe").transform;
                joe.parent = body.GetOrigParent();
                joe.position = body.GetPosition();
                joe.rotation = body.GetRotation();
            }
            else
            {
                OWJam4ModProject.instance.ModHelper.Console.WriteLine($"resetting shuttle to {joe.position} {joe.rotation}");

                body.SetPosition(joe.position);
                body.SetRotation(joe.rotation);
                body.SetVelocity(Vector3.zero);
                body.SetAngularVelocity(Vector3.zero);

                body.GetComponent<AlignWithTargetBody>().enabled = false;
                body.GetAttachedFluidDetector().GetComponent<ForceApplier>().enabled = false;
                body.GetComponentInChildren<ShuttleDoorController>().StartOpenDoors();
            }
        }

        #endregion
    }
}
