using HarmonyLib;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;
using UnityEngine;

namespace OWJam4ModProject
{
    public class OWJam4ModProject : ModBehaviour
    {

        public static ModBehaviour instance;

        private static Vector3? _shuttleStartingPosition;

        private void Awake()
        {
            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        private void Start()
        {
            // Starting here, you'll have access to OWML's mod helper.
            ModHelper.Console.WriteLine($"My mod {nameof(OWJam4ModProject)} is loaded!", MessageType.Success);

            // Get the New Horizons API and load configs
            var newHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            newHorizons.LoadConfigs(this);

            // Example of accessing game code.
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (loadScene != OWScene.SolarSystem) return;
                ModHelper.Console.WriteLine("Loaded into solar system!", MessageType.Success);

                _shuttleStartingPosition = null; // this needs to get init'd again
            };

            // Initialize singleton
            instance = this;

            // nh startlit seems to just not work, so we have to do this ourselves
            GlobalMessenger.AddListener("EnterDreamWorld", () =>
            {
                Locator.GetPlayerCamera().postProcessingSettings.ambientOcclusionAvailable = true; // we have ambient light so we want this back

                FindObjectOfType<ShuttleFlightController>().ResetShuttle();

                // make zone1 sector guy huge
                // can see blue atmo from other zone but idc
                var zone1shape = GameObject.Find("DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_1").GetComponent<CylinderShape>();
                zone1shape.height = 9999;
                zone1shape.radius = 9999;

                return;
                SearchUtilities.Find("TotemPlatform").GetComponentInChildren<DreamObjectProjector>().SetLit(false);
                ModHelper.Console.WriteLine("TURN OFF THE THING PLEASE");
            });
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        /// <summary>
        /// turn visible planet "ambient light" down
        /// </summary>
        public static void MakeTheStupidLightDark()
        {
            var dw = Locator.GetDreamWorldController()._dreamBody._transform;
            var light = dw.Find("Sector_DreamWorld/Atmosphere_Dreamworld/Prefab_IP_VisiblePlanet/AmbientLight_IP");
            light.GetComponent<Light>().intensity = .3f;
            instance.ModHelper.Console.WriteLine("GO DARK YOU STUPID LIGHT");
        }
    }

}
