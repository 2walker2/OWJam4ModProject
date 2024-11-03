using UnityEngine;

namespace OWJam4ModProject
{
    internal class ShuttleTakeoffController : MonoBehaviour
    {
        [Tooltip("The code controller the player needs to input the launch code into")]
        [SerializeField] EclipseCodeController4 codeController;
        [Tooltip("The light sensor the player needs to activate to light the furnce")]
        [SerializeField] SingleLightSensor furnaceSensor;
        [Tooltip("The engine start audio source")]
        [SerializeField] OWAudioSource engineStartSource;
        [Tooltip("The engine hum audio source")]
        [SerializeField] OWAudioSource engineHumSource;
        [Tooltip("The gameobject to enable when the ship activates")]
        [SerializeField] GameObject enableWhenStarted;

        bool codeCorrect = false;
        bool furnaceLit = false;

        void Start()
        {
            codeController.OnOpen += CodeCorrect;
            codeController.OnClose += CodeIncorrect;

            furnaceSensor.OnDetectLight += FurnaceLit;
        }

        void CodeCorrect()
        {
            codeCorrect = true;

            OWJam4ModProject.instance.ModHelper.Console.WriteLine("Correct takeoff code entered", OWML.Common.MessageType.Success);
        }

        void CodeIncorrect()
        {
            codeCorrect = false;
        }

        void FurnaceLit()
        {
            if (!furnaceLit)
            {
                furnaceLit = true;
                enableWhenStarted.SetActive(true);
                engineStartSource.Play();
                engineHumSource.Play();
            }
        }

        public bool CanTakeOff()
        {
            return furnaceLit && codeCorrect;
        }
    }
}
