using UnityEngine;
using static CactEye2.CactEyeConfigMenu;

namespace CactEye2
{
    /* ************************************************************************************************
    * Class Name: CactEyeAsteroidSpawner_Flight
    * Purpose: This will allow CactEye's custom asteroid spawner to run in multiple specific scenes. 
    * Shamelessly stolen from Starstrider42, who shamelessly stole it from Trigger Au.
    * Thank you!
    * ************************************************************************************************/
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    internal class CactEyeConfig_Flight : CactEyeConfig
    {
    }

    /* ************************************************************************************************
    * Class Name: CactEyeAsteroidSpawner_SpaceCentre
    * Purpose: This will allow CactEye's custom asteroid spawner to run in multiple specific scenes. 
    * Shamelessly stolen from Starstrider42, who shamelessly stole it from Trigger Au.
    * Thank you!
    * ************************************************************************************************/
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    internal class CactEyeConfig_SpaceCentre : CactEyeConfig
    {
    }

    /* ************************************************************************************************
    * Class Name: CactEyeAsteroidSpawner_TrackingStation
    * Purpose: This will allow CactEye's custom asteroid spawner to run in multiple specific scenes. 
    * Shamelessly stolen from Starstrider42, who shamelessly stole it from Trigger Au.
    * Thank you!
    * ************************************************************************************************/
    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    internal class CactEyeConfig_TrackStation : CactEyeConfig
    {
    }

    internal class CactEyeConfig : MonoBehaviour
    {

        //Path to the configuration file.
        private static string ConfigFilePath = null;

        //Specifies whether CactEye should operate in debug mode or not.
        public static bool DebugMode = true;

        //Specifies whether telescopes should blow up when pointed at the sun.
        public static bool SunDamage = true;

        //Specifies whether telescopes should blow up when pointed at the sun.
        public static bool GyroDecay = true;

        //Specifies whether the Asteroid spawner should be active
        public static bool AsteroidSpawner = false;

        public void Start()
        {
            ConfigFilePath = KSPUtil.ApplicationRootPath + "GameData/CactEye/Resources/settings.cfg";
            ReadSettings();
        }

        public static void ReadSettings()
        {
            ConfigNode Settings = ConfigNode.Load(ConfigFilePath);

            if (Settings != null)
            {
                if (Settings.HasNode("CactEye2"))
                {
                    ConfigNode CactEye2 = Settings.GetNode("CactEye2");

                    if (CactEye2.HasValue("DebugMode"))
                    {
                        DebugMode = bool.Parse(CactEye2.GetValue("DebugMode"));

                        Log.SetLevel(DebugMode ? KSP_Log.Log.LEVEL.INFO : KSP_Log.Log.LEVEL.ERROR);
                    }
                    if (CactEye2.HasValue("SunDamage"))
                    {
                        SunDamage = bool.Parse(CactEye2.GetValue("SunDamage"));
                    }
                    if (CactEye2.HasValue("GyroDecay"))
                    {
                        GyroDecay = bool.Parse(CactEye2.GetValue("GyroDecay"));
                    }
                    if(CactEye2.HasValue("AsteroidSpawner"))
                    {
                        AsteroidSpawner = bool.Parse(CactEye2.GetValue("AsteroidSpawner"));
                    }
                }
                else
                {
                    Log.Error("Logical Error: Error loaded configuration file. Was not able to find CactEye2 node in configuration file.");
                }

                //IsLoaded = true;
            }
            else
            {
                Log.Error("Logical Error: Was not able to load the CactEye configuration file.");
            }

        }

        public static void ApplySettings()
        {

            Log.Info(" Settings saved to " + ConfigFilePath);

            ConfigNode Settings = new ConfigNode();
            ConfigNode CactEye2 = Settings.AddNode("CactEye2");
            CactEye2.AddValue("DebugMode", DebugMode);
            Log.Info(" DebugMode = " + DebugMode.ToString());
            CactEye2.AddValue("SunDamage", SunDamage);
            Log.Info(" SunDamage = " + SunDamage.ToString());
            CactEye2.AddValue("GyroDecay", GyroDecay);
            Log.Info(" GyroDecay = " + GyroDecay.ToString());
            CactEye2.AddValue("AsteroidSpawner", AsteroidSpawner);
            Log.Info("Cacteye 2: AsteroidSpawner = " + AsteroidSpawner.ToString());
            Settings.Save(ConfigFilePath);
        }
    }
}
