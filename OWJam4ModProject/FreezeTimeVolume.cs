using System;
using UnityEngine;

namespace OWJam4ModProject
{
    internal class FreezeTimeVolume : MonoBehaviour
    {
        [Tooltip("The trigger volume to freeze time within")]
        [SerializeField] OWTriggerVolume triggerVolume;

        bool inTrigger = false;
        float secondsRemainingWhenEntered;

        void Start ()
        {
            triggerVolume.OnEntry += EnteredTrigger;
            triggerVolume.OnExit += ExitedTrigger;
        }

        void Update ()
        {
            if (inTrigger)
            {
                TimeLoop.SetSecondsRemaining(secondsRemainingWhenEntered);
            }
        }

        private void EnteredTrigger(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector"))
            {
                inTrigger = true;
                secondsRemainingWhenEntered = TimeLoop.GetSecondsRemaining();
            }
        }

        private void ExitedTrigger(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector"))
            {
                inTrigger = false;
            }
        }
    }
}
