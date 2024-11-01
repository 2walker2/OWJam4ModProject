using UnityEngine;

namespace OWJam4ModProject
{
    internal class ShuttleLightController : MonoBehaviour
    {
        [Tooltip("The light sensor used to turn on the light")]
        [SerializeField] SingleLightSensor lightSensor;
        [Tooltip("The objects to toggle based on whether the light is on")]
        [SerializeField] GameObject[] objectsToToggle;
        [Tooltip("The audio to play when the light is activated")]
        [SerializeField] OWAudioSource lightActivatedAudio;
        [Tooltip("The audio to play when the light is deactivated")]
        [SerializeField] OWAudioSource lightDeactivatedAudio;

        void Start ()
        {
            lightSensor.OnDetectLight += TurnOnLight;
            lightSensor.OnDetectDarkness += TurnOffLight;
        }

        void OnDestroy()
        {
            lightSensor.OnDetectLight -= TurnOnLight;
            lightSensor.OnDetectDarkness -= TurnOffLight;
        }

        void TurnOnLight()
        {
            foreach (GameObject g in objectsToToggle)
                g.SetActive(true);

            lightActivatedAudio.PlayOneShot();
        }

        void TurnOffLight()
        {
            foreach (GameObject g in objectsToToggle)
                g.SetActive(false);

            lightDeactivatedAudio.PlayOneShot();
        }
    }
}
