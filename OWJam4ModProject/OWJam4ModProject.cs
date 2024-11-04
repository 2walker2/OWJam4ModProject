using HarmonyLib;
using NewHorizons;
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
        public const bool DEBUG = false;

        public static ModBehaviour instance;

        private void Awake()
        {
            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.

            // Initialize singleton
            instance = this;

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        private void Start()
        {
            // Starting here, you'll have access to OWML's mod helper.
            Log($"My mod {nameof(OWJam4ModProject)} is loaded!", MessageType.Success);

            // Get the New Horizons API and load configs
            var newHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            newHorizons.LoadConfigs(this);

            newHorizons.GetStarSystemLoadedEvent().AddListener(OnStarSystemLoaded);

            GlobalMessenger.AddListener("EnterDreamWorld", InitDreamWorld);
        }

        private void OnStarSystemLoaded(string system)
        {
            if (system != "SolarSystem") return;
            Log("Loaded into solar system!", MessageType.Success);

            Ernesto.Attach();

            // make zone1 sector guy huge
            // can see blue atmo from other zone but idc
            var zone1shape = GameObject.Find("DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_1").GetComponent<CylinderShape>();
            zone1shape.height = 9999;
            zone1shape.radius = 9999;

            // add sector to things that need it
            foreach (var tessSphereSectorToggle in GameObject.Find("DreamWorld_Body").GetComponentsInChildren<TessSphereSectorToggle>())
            {
                // somehow locator works here???? even tho this is called in OnCompleteSceneLoad
                tessSphereSectorToggle._sector = Locator.GetDreamWorldController()._dreamWorldSector;
                Log($"set sector for {tessSphereSectorToggle}");
            }

            //Geswaldo sector
            GameObject geswaldo = SearchUtilities.Find("DreamGeswaldo");
            if (geswaldo != null)
            {
                GameObject zone2SectorGO = GameObject.Find("DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_2");
                if (zone2SectorGO != null)
                {
                    geswaldo.GetComponent<SectorCullGroup>().SetSector(zone2SectorGO.GetComponent<Sector>());
                }
                else
                {
                    Log("Sector nooooooooooooooooooooooooooooooo!", MessageType.Error);
                }
            }
            else
                Log("Geswaldo nooooooooooooooooooooooooooooooo!", MessageType.Error);

            //Vision torch parent
            GameObject visionTorchParent = SearchUtilities.Find("MorseVisionTorchSocket");
            GameObject visionTorch = SearchUtilities.Find("MorseVisionTorch");
            visionTorch.transform.parent = visionTorchParent.transform;
        }

        private void InitDreamWorld()
        {
            Log("INIT DREAMWORLD");

            Locator.GetPlayerCamera().postProcessingSettings.ambientOcclusionAvailable = true; // we have ambient light so we want this back

            FindObjectOfType<ShuttleFlightController>().ResetShuttle();

            // nh startlit seems to just not work, so we have to do this ourselves
            var projector = SearchUtilities.Find("TotemPlatform").GetComponentInChildren<DreamObjectProjector>();
            projector.SetLit(true);
            projector.SetLit(false);

            Delay.FireInNUpdates(() =>
            {
                // turn visible planet "ambient light" down
                var light = GameObject.Find("DreamWorld_Body/Sector_DreamWorld/Atmosphere_Dreamworld/Prefab_IP_VisiblePlanet/AmbientLight_IP");
                light.GetComponent<Light>().intensity = .3f;
                Log("GO DARK YOU STUPID LIGHT");

                // unity explorer script
                // Locator.GetDreamWorldController()._dreamBody._transform.Find("Sector_DreamWorld/Atmosphere_Dreamworld/Prefab_IP_VisiblePlanet/AmbientLight_IP").GetComponent<Light>().intensity
            }, 100);

            //Open the zone 1 raft door
            SearchUtilities.Find("DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_1/Interactibles_DreamZone_1/Tunnel/Prefab_IP_DreamObjectProjector (2)").GetComponent<DreamObjectProjector>().SetLit(false);
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        public static void Log(string line, MessageType type = MessageType.Message)
        {
            if (!DEBUG) return;
            instance.ModHelper.Console.WriteLine(line, type);
        }
    }

}
