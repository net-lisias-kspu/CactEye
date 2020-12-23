using System;
using UnityEngine;
using static CactEye2.OccultationScienceEventWindow;
using SpaceTuxUtility;

#if false
namespace CactEye2
{


    /* ************************************************************************************************
    * Class Name: CactEyeCometSpawner_Flight
    * Purpose: This will allow CactEye's custom comet spawner to run in multiple specific scenes. 
    * Shamelessly stolen from Starstrider42, who shamelessly stole it from Trigger Au.
    * Thank you!
    * ************************************************************************************************/
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    internal class CactEyeCometSpawner_Flight : CactEyeCometSpawner
    {
    }

    /* ************************************************************************************************
    * Class Name: CactEyeCometSpawner_SpaceCentre
    * Purpose: This will allow CactEye's custom comet spawner to run in multiple specific scenes. 
    * Shamelessly stolen from Starstrider42, who shamelessly stole it from Trigger Au.
    * Thank you!
    * ************************************************************************************************/
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    internal class CactEyeCometSpawner_SpaceCentre : CactEyeCometSpawner
    {
    }

    /* ************************************************************************************************
    * Class Name: CactEyeCometSpawner_TrackingStation
    * Purpose: This will allow CactEye's custom comet spawner to run in multiple specific scenes. 
    * Shamelessly stolen from Starstrider42, who shamelessly stole it from Trigger Au.
    * Thank you!
    * ************************************************************************************************/
    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    internal class CactEyeCometSpawner_TrackStation : CactEyeCometSpawner
    {
    }

    internal class CactEyeCometSpawner : MonoBehaviour
    {
        internal static CactEyeCometSpawner instance;

        private int GlobalDiscoveryRate;

        /* ************************************************************************************************
         * Function Name: Start
         * Input: N/A
         * Output: N/A
         * Purpose: This function will run at the start of the loaded scene. It will check to verify it is
         * loaded in the correct scene and then call a coroutine, "DelayedStart()."
         * ************************************************************************************************/
        public void Start()
        {
            instance = this;
            Log.Info("CactEyeCometSpawner.Start, CactEyeConfig.CometSpawner: " + CactEyeConfig.CometSpawner);
            if (CactEyeConfig.CometSpawner)
            {
                StartCoroutine(DelayedStart());
            }
        }

        /* ************************************************************************************************
         * Function Name: DelayedStart
         * Input: N/A
         * Output: N/A
         * Purpose: This function will run directly after the start of the loaded scene. It will calculate
         * the discovery rate, and then adjust the spawn rate of comets if it can.
         * ************************************************************************************************/
        public System.Collections.IEnumerator DelayedStart()
        {
            while (HighLogic.CurrentGame.scenarios[0].moduleRef == null)
            {
                yield return new WaitForSecondsRealtime(0.1f);
            }

            UpdateSpawnRate();
        }

        /* ************************************************************************************************
         * Function Name: CalculateGlobalDiscoveryRate
         * Input: N/A
         * Output: N/A
         * Purpose: This function will scan for any active telescopes that are equipped with an comet
         * processor that is active and functioning correctly. It will then calculate what the total 
         * spawn rate should be for the comet spawner based on available telescopes.
         * ************************************************************************************************/
        private void CalculateGlobalDiscoveryRate()
        {

            GlobalDiscoveryRate = 100;

            foreach (Vessel vessel in FlightGlobals.Vessels)
            {

                if (vessel.loaded)
                {
                    Log.Info("Vessel loaded: " + vessel.name);
                    foreach (var p in vessel.Parts)
                    {
                        foreach (var m in p.Modules)
                        {
                            if (m.moduleName == "CactEyeCometProcessor")
                            {
                                CactEyeCometProcessor ceap = m as CactEyeCometProcessor;
                                if (ceap.Active)
                                {
                                    GlobalDiscoveryRate -= ceap.DiscoveryRate;
                                    Log.Info("ceap.Active is true, DiscoveryRate: " + ceap.DiscoveryRate);
                                }
                                break;
                            }
                        }
                    }

                }
                else
                {
                    Log.Info("Vessel not loaded: " + vessel.name);
                    foreach (ProtoPartSnapshot part in vessel.protoVessel.protoPartSnapshots)
                    {
                        ProtoPartModuleSnapshot cpu = part.modules.Find(n => n.moduleName == "CactEyeCometProcessor");
                        if (cpu != null && bool.Parse(cpu.moduleValues.GetValue("Active")))
                        {
                            int PartDiscoveryRate = 0;
                            try
                            {
                                PartDiscoveryRate = int.Parse(cpu.moduleValues.GetValue("DiscoveryRate"));
                            }
                            catch (Exception e)
                            {
                                Log.Error("Comet Spawner: Was not able to retrieve a discovery rate from an active telescope");
                            }
                            //int.TryParse(cpu.moduleValues.GetValue("DiscoveryRate"), out PartDiscoveryRate);
                            Log.Info("ceap.Active is true, PartDiscoveryRate: " + PartDiscoveryRate);

                            GlobalDiscoveryRate -= PartDiscoveryRate;

                            if (CactEyeConfig.DebugMode)
                            {
                                Log.Error("Comet Spawner: Comet Processor Found!");
                                Log.Error("Comet Spawner: Discovery rate: " + PartDiscoveryRate.ToString());
                            }
                        }
                    }
                }

                //foreach (Part part in vessel.Parts)
                //{
                //    CactEyeCometProcessor cpu = part.GetComponent<CactEyeCometProcessor>();
                //    if (cpu != null && cpu.Active)
                //    {
                //        GlobalDiscoveryRate -= cpu.DiscoveryRate;
                //        Debug.Log("CactEye 2: Comet Spawner: Comet Processor Found!");
                //    }
                //}

                Log.Info("GlobalDescoveryRate: " + GlobalDiscoveryRate);
            }
        }

        /* ************************************************************************************************
         * Function Name: CheckForIncompatibleMods
         * Input: N/A
         * Output: N/A
         * Purpose: This function will check to see if certain mods that are imcompatible with the 
         * Comet spawner are installed. If they are, CactEye will not override the spawning of comets.
         * This is primarily to prevent CactEye from interfering and conflicting with CustomComets by
         * Starstrider42.
         * ************************************************************************************************/
        private bool CheckForIncompatibleMods()
        {
            Log.Info("CustomComets: " + HasMod.hasMod("CustomComets"));
            return HasMod.hasMod("CustomComets");
        }

        /* ************************************************************************************************
         * Function Name: AdjustSpawnRate
         * Input: N/A
         * Output: N/A
         * Purpose: This function will adjust the spawn rate of the stock comet spawner to CactEye's 
         * own calculated value, of which is based on the avialability of comet telescopes.
         * This function will not run if CustomComets is installed at this time. Perhaps in the future,
         * this can be modified to allow CactEye and CustomComets to work together, but this will require
         * some changes in CustomComets first.
         * ************************************************************************************************/
        private void AdjustSpawnRate()
        {
            if (!CheckForIncompatibleMods())
            {

                try
                {
                    ScenarioDiscoverableObjects CometSpawner = HighLogic.CurrentGame.scenarios.Find(scenario => scenario.moduleRef is ScenarioDiscoverableObjects).moduleRef as ScenarioDiscoverableObjects;

                    Log.Info("CometSpawner.spawnOddsAgainst: " + CometSpawner.spawnOddsAgainst + ", GlobalDiscoveryRate: " + GlobalDiscoveryRate);
                    CometSpawner.spawnOddsAgainst = GlobalDiscoveryRate;

                    if (CactEyeConfig.DebugMode)
                    {
                        Log.Error("Comet Spawner: spawnOddsAgainst = " + CometSpawner.spawnOddsAgainst.ToString());
                    }
                }
                catch (Exception e)
                {
                    Log.Error("Comet Spawner: Was not able to adjust spawn rate; CometSpawner object is null!");
                    Log.Error(e.ToString());
                }
            }

            else
            {
                Log.Error("An incompatible mod (most likely Custom Comets) was detected. CactEye will not adjust the comet spawn rate.");
            }
        }


        /* ************************************************************************************************
         * Function Name: UpdateSpawnRate
         * Input: N/A
         * Output: N/A
         * Purpose: This function will recalculate and readjust the spawn rate of comets. This is to
         * allow for dynamic changes in the spawn rate as telescopes are enabled/disabled.
         * ************************************************************************************************/
        public void UpdateSpawnRate()
        {
            CalculateGlobalDiscoveryRate();
            AdjustSpawnRate();
        }
    }
}

#endif