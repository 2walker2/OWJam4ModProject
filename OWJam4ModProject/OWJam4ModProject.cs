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

        public static ModBehaviour instance;

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

            newHorizons.GetStarSystemLoadedEvent().AddListener(OnStarSystemLoaded);

            // Initialize singleton
            instance = this;

            GlobalMessenger.AddListener("EnterDreamWorld", InitDreamWorld);
        }

        private void OnStarSystemLoaded(string system)
        {
            if (system != "SolarSystem") return;
            ModHelper.Console.WriteLine("Loaded into solar system!", MessageType.Success);

            // make zone1 sector guy huge
            // can see blue atmo from other zone but idc
            var zone1shape = GameObject.Find("DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_1").GetComponent<CylinderShape>();
            zone1shape.height = 9999;
            zone1shape.radius = 9999;

            // add sector to things that need it
            foreach (var tessSphereSectorToggle in GameObject.Find("DreamWorld_Body").GetComponentsInChildren<TessSphereSectorToggle>())
            {
                tessSphereSectorToggle._sector = Locator.GetDreamWorldController()._dreamWorldSector;
                ModHelper.Console.WriteLine($"set sector for {tessSphereSectorToggle}");
            }
        }

        private void InitDreamWorld()
        {
            ModHelper.Console.WriteLine("INIT DREAMWORLD");

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
                ModHelper.Console.WriteLine("GO DARK YOU STUPID LIGHT");

                // unity explorer script
                // Locator.GetDreamWorldController()._dreamBody._transform.Find("Sector_DreamWorld/Atmosphere_Dreamworld/Prefab_IP_VisiblePlanet/AmbientLight_IP").GetComponent<Light>().intensity
            }, 100);

            //Attach admin artifact
            AdminArtifact.AttachToPlayerLantern();
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }
    }

}
