using UnityEngine;

namespace OWJam4ModProject
{
    internal class ShuttleTakeoffController : MonoBehaviour
    {
        [Tooltip("The code controller the player needs to input the launch code into")]
        [SerializeField] EclipseCodeController4 codeController;
        [Tooltip("The dream candle the player needs to light to ignite the engines")]
        [SerializeField] DreamCandle furnace;
        [Tooltip("The engine start audio source")]
        [SerializeField] OWAudioSource engineStartSource;
        [Tooltip("The engine hum audio source")]
        [SerializeField] OWAudioSource engineHumSource;

        bool codeCorrect = false;
        bool furnaceLit = false;

        void Start()
        {
            codeController.OnOpen += CodeCorrect;
            codeController.OnClose += CodeIncorrect;

            furnace.OnLitStateChanged += FurnaceLitStateChanged;
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

        void FurnaceLitStateChanged()
        {
            furnaceLit = furnace.IsLit();

            if (furnaceLit)
            {
                if (codeCorrect)
                {
                    engineStartSource.Play();
                    engineHumSource.Play();
                }
                else
                {
                    furnace.SetLit(false);
                }
            }
        }

        public bool CanTakeOff()
        {
            return codeCorrect && furnaceLit;
        }
    }
}
