using NewHorizons.Utility;
using OWML.Common;
using System.Collections;
using System.Diagnostics;
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
        [SerializeField] OWRigidbody body;
        [Tooltip("The audio source for the takeoff controller")]
        [SerializeField] OWAudioSource takeoffAudio;

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

            var shipFluidDetector = (ConstantFluidDetector)body.GetAttachedFluidDetector();
            var planetFluidVolume = landingTarget.Find("AirVolume").GetComponent<SimpleFluidVolume>();
            shipFluidDetector.SetDetectableFluid(planetFluidVolume);

            body.GetAttachedFluidDetector().GetComponent<ForceApplier>().enabled = false; // dont apply fluids FOR NOW
        }

        void OnDestroy()
        {
            lightSensor.OnDetectLight -= StartFlight;
        }

        #region flight

        void Update()
        {
            if (!OWJam4ModProject.DEBUG) return;

            if (Keyboard.current[Key.Home].wasPressedThisFrame)
            {
                takeoffAudio.Play();


                StopAllCoroutines();
                StartCoroutine(FlyToPlanet());
            }

            if (Keyboard.current[Key.End].wasPressedThisFrame)
            {
                AdminArtifact.AttachToPlayerLantern();
            }
        }

        void StartFlight()
        {
            OWJam4ModProject.Log("Starting shuttle flight", MessageType.Success);

            // Don't take off if the takeoff puzzle isn't solved
            if (!takeoffController.CanTakeOff())
                return;

            takeoffAudio.Play();


            StopAllCoroutines();
            StartCoroutine(FlyToPlanet());
        }

        IEnumerator FlyToPlanet()
        {
            var belowOrbit = Vector3.Distance(landingTarget.position, transform.position) < orbitRadius;

            // start aligning with body
            StartCoroutine(DoAlign(Vector3.up));

            // Start moving towards planet
            Vector3 towardsPlanet = (landingTarget.position - transform.position).normalized;
            Vector3 velocity = towardsPlanet * flightSpeed;
            if (belowOrbit)
                velocity = -towardsPlanet * LandingSpeed;
            body.SetVelocity(velocity);

            // wait until in orbit
            if (!belowOrbit)
                while (Vector3.Distance(landingTarget.position, transform.position) > orbitRadius)
                    yield return null;
            else
                while (Vector3.Distance(landingTarget.position, transform.position) < orbitRadius)
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
            OWJam4ModProject.Log("weve stopped. its time to fly");

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
                    var diff = Vector3.Distance(landingTarget.position, transform.position) - orbitRadius;
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

            // flip around
            StartCoroutine(DoAlign(Vector3.down));

            // move to landing spot
            Vector3 towardsLanding = (landingPoint.transform.position - transform.position).normalized;
            Vector3 velocity = towardsLanding * LandingSpeed;
            body.SetVelocity(velocity);

            // get height to land on
            Physics.Raycast(transform.position + towardsLanding * 20, towardsLanding, out var hit, CLOUD_RADIUS, OWLayerMask.physicalMask);
            var height = Vector3.Distance(landingTarget.position, hit.point);
            OWJam4ModProject.Log($"height to land at is {height}. current distance is {Vector3.Distance(landingTarget.position, transform.position)}");

            while (Vector3.Distance(landingTarget.position, transform.position) > height)
            {
                yield return null;
            }

            // "collided" with ground. stop instantly
            Locator.GetPlayerBody().AddVelocityChange(-body.GetVelocity());
            body.SetVelocity(Vector3.zero);

            OWJam4ModProject.Log("landed");
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
                joe.position = transform.position;
                joe.rotation = transform.rotation;
            }
            else
            {
                OWJam4ModProject.Log($"resetting shuttle to {joe.position} {joe.rotation}");

                transform.position = joe.position;
                transform.rotation = joe.rotation;
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
