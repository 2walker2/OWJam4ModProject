using NewHorizons.Utility;
using System.Collections;
using UnityEngine;

namespace OWJam4ModProject
{
    internal class ShuttleFlightController : MonoBehaviour
    {
        [Tooltip("The name of the transform to target")]
        [SerializeField] string landingTargetName;
        [Tooltip("The radius away from it's landing target at which the ship orbits")]
        [SerializeField] float orbitRadius;
        [Tooltip("How fast the shuttle moves")]
        [SerializeField] float flightSpeed;
        [Tooltip("How fast the shuttle rotates to face towards the planet as it flies there")]
        [SerializeField] float flightAlignmentSpeed;
        [Tooltip("The light sensor that activates flight")]
        [SerializeField] SingleLightSensor lightSensor;
        [Tooltip("The shuttle's OWRigidbody")]
        [SerializeField] OWRigidbody body;

        Transform landingTarget;

        void Start()
        {
            lightSensor.OnDetectLight += StartFlight;

            landingTarget = SearchUtilities.Find(landingTargetName).transform;
            // terrible. please do this in unity
            var b = landingTarget.gameObject.AddComponent<OWRigidbody>();
            b.MakeKinematic();
            b.EnableKinematicSimulation();
        }

        void OnDestroy()
        {
            lightSensor.OnDetectLight -= StartFlight;
        }

        void StartFlight()
        {
            OWJam4ModProject.instance.ModHelper.Console.WriteLine("Starting shuttle flight", OWML.Common.MessageType.Success);

            StartCoroutine(FlyToPlanet());
        }

        IEnumerator FlyToPlanet()
        {
            // Start moving towards planet
            Vector3 towardsPlanet = (landingTarget.position - body.transform.position).normalized;
            Vector3 velocity = towardsPlanet * flightSpeed;
            body.SetVelocity(velocity);

            // start aligning with body
            // should probably put this on the prefab lol
            var align = body.gameObject.AddComponent<AlignWithTargetBody>();
            align.SetLocalAlignmentAxis(Vector3.up);
            align.SetUsePhysicsToRotate(true);
            align.SetTargetBody(landingTarget.GetAttachedOWRigidbody());
            align._interpolationMode = AlignWithDirection.InterpolationMode.Linear; // change the mode to make it nicer. idk what ship uses for landing cam
            align._interpolationRate = 10;

            yield return null;
        }
    }
}
