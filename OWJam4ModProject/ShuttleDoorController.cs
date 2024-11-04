using System.Collections;
using UnityEngine;

namespace OWJam4ModProject
{
    internal class ShuttleDoorController : MonoBehaviour
    {
        [Tooltip("The grapple sensor the player uses to get into the ship")]
        [SerializeField] SingleLightSensor grappleSensor;
        [Tooltip("The light sensor on the door")]
        [SerializeField] SingleLightSensor doorLightSensor;
        [Tooltip("The grapple totem to disable when the player is inside the ship")]
        [SerializeField] GameObject grappleTotem;
        [Tooltip("The delay after grappling before closing the doors")]
        [SerializeField] float grappleDoorCloseDelay;
        [Tooltip("The door transforms to move")]
        [SerializeField] Transform[] doorTransforms;
        [Tooltip("The transforms that mark the door's open positions")]
        [SerializeField] Transform[] doorOpenTransforms;
        [Tooltip("The transforms that mark the door's closed positions")]
        [SerializeField] Transform[] doorClosedTransforms;
        [Tooltip("How long the doors take to open or close")]
        [SerializeField] float doorDuration;
        [Tooltip("The door collider to enable")]
        [SerializeField] Collider doorCollider;
        [Tooltip("The audio to play when the door opens or closes")]
        [SerializeField] OWAudioSource doorAudio;

        bool doorsClosed = false;
        bool doorsAnimating = false;

        void Start()
        {
            grappleSensor.OnDetectLight += GrappleActivated;
            doorLightSensor.OnDetectLight += DoorActivated;
        }

        void Update()
        {
            // Make sure the projection totem doesn't mess with the doors (yes I know this is hacky)
            doorCollider.enabled = doorsClosed;
        }

        void OnDestroy()
        {
            grappleSensor.OnDetectLight -= GrappleActivated;
            doorLightSensor.OnDetectLight -= DoorActivated;
        }

        void GrappleActivated()
        {
            Invoke(nameof(StartCloseDoors), grappleDoorCloseDelay);
        }

        void DoorActivated()
        {
            if (!doorsClosed)
                StartCloseDoors();
            else
                StartOpenDoors();
        }

        public void StartCloseDoors()
        {
            if (!doorsClosed && !doorsAnimating)
                StartCoroutine(CloseDoors());
        }

        public void StartOpenDoors()
        {
            if (doorsClosed && !doorsAnimating)
                StartCoroutine(OpenDoors());
        }

        IEnumerator CloseDoors()
        {
            //OWJam4ModProject.instance.ModHelper.Console.WriteLine("Started closing doors", OWML.Common.MessageType.Success);

            doorsAnimating = true;

            // Audio
            doorAudio.PlayOneShot();

            // Animate the doors closed
            float startTime = Time.time;
            while (Time.time - startTime <= doorDuration)
            {
                float t = (Time.time - startTime) / doorDuration;

                for (int i = 0; i < doorTransforms.Length; i++)
                {
                    doorTransforms[i].localPosition = Vector3.Lerp(doorOpenTransforms[i].localPosition, doorClosedTransforms[i].localPosition, t);
                }

                yield return new WaitForEndOfFrame();
            }

            // Make sure the doors end exactly where they should
            for (int i = 0; i < doorTransforms.Length; i++)
            {
                doorTransforms[i].localPosition = doorClosedTransforms[i].localPosition;
            }

            doorCollider.enabled = true;
            grappleTotem.SetActive(false);

            doorsAnimating = false;
            doorsClosed = true;

            //OWJam4ModProject.instance.ModHelper.Console.WriteLine("Finished closing doors", OWML.Common.MessageType.Success);
        }

        IEnumerator OpenDoors()
        {
            //OWJam4ModProject.instance.ModHelper.Console.WriteLine("Started opening doors", OWML.Common.MessageType.Success);

            doorsAnimating = true;

            // Audio
            doorAudio.PlayOneShot();

            // Animate the doors open
            float startTime = Time.time;
            while (Time.time - startTime <= doorDuration)
            {
                float t = (Time.time - startTime) / doorDuration;

                for (int i = 0; i < doorTransforms.Length; i++)
                {
                    doorTransforms[i].localPosition = Vector3.Lerp(doorClosedTransforms[i].localPosition, doorOpenTransforms[i].localPosition, t);
                }

                yield return new WaitForEndOfFrame();
            }

            // Make sure the doors end exactly where they should
            for (int i = 0; i < doorTransforms.Length; i++)
            {
                doorTransforms[i].localPosition = doorOpenTransforms[i].localPosition;
            }

            doorCollider.enabled = false;
            grappleTotem.SetActive(true);

            doorsAnimating = false;
            doorsClosed = false;

            //OWJam4ModProject.instance.ModHelper.Console.WriteLine("Finished opening doors", OWML.Common.MessageType.Success);
        }
    }
}
