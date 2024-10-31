using System.Collections;
using UnityEngine;

namespace OWJam4ModProject
{
    internal class ShuttleDoorController : MonoBehaviour
    {
        [Tooltip("The single light sensor to use to determine when to close the doors")]
        [SerializeField] SingleLightSensor doorCloseSensor;
        [Tooltip("The delay after the light sensor is activated to wait before closing the doors")]
        [SerializeField] float doorCloseDelay;
        [Tooltip("The door gameobjects to enable")]
        [SerializeField] GameObject[] doorGameObjects;
        [Tooltip("The door collider to enable")]
        [SerializeField] Collider doorCollider;

        bool doorsClosed = false;

        void Start()
        {
            doorCloseSensor.OnDetectLight += LightSensorActivated;
        }

        void OnEnable()
        {
            // Make sure door starts open
            Invoke(nameof(OpenDoors), 0.1f);
        }

        void OnDestroy()
        {
            doorCloseSensor.OnDetectLight -= LightSensorActivated;
        }

        void LightSensorActivated()
        {
            Invoke(nameof(CloseDoors), doorCloseDelay);
        }

        void CloseDoors()
        {
            doorsClosed = true;

            foreach (GameObject g in doorGameObjects)
            {
                g.SetActive(true);
            }
            doorCollider.enabled = true;
        }

        void OpenDoors()
        {
            foreach (GameObject g in doorGameObjects)
            {
                g.SetActive(false);
            }
            doorCollider.enabled = false;
        }
    }
}
