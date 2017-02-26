using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CactEye2
{
    class CactEyePlanetaryProcessor : CactEyeProcessor
    {
        public override string DoScience(float FOV)
        {
            CelestialBody Target = FlightGlobals.ActiveVessel.mainBody;
            if (Target == FlightGlobals.Bodies[0])
            {
                return "Not a valid observation target";
            }
            else if (!CheckAim(FlightGlobals.fetch.activeVessel.transform.forward, FlightGlobals.fetch.activeVessel.transform.position, Target, FOV))
            {
                return "Camera not pointed at target";
            }
            else if (CheckFOV(FlightGlobals.fetch.activeVessel.transform.position, Target, FOV, MinimumView) == 0)
            {
                return "Camera not close enought to target";
            }
            else if (CheckFOV(FlightGlobals.fetch.activeVessel.transform.position, Target, FOV, MinimumView) == 2)
            {
                return "Camera to far from target";
            }
            try
            {
                ScienceExperiment PlanetaryCameraExperiment = ResearchAndDevelopment.GetExperiment(ExperimentID);
                ScienceSubject PlanetaryCameraSubject = ResearchAndDevelopment.GetExperimentSubject(PlanetaryCameraExperiment, ExperimentSituations.InSpaceHigh, Target, "PlanetaryObservation" + Target.name);
                PlanetaryCameraSubject.title = "Planetary Camera Observation of " + Target.GetName();

                float sciencePoints = PlanetaryCameraExperiment.baseValue * PlanetaryCameraExperiment.dataScale;
                ScienceData Data = new ScienceData(sciencePoints, 1f, 0f, PlanetaryCameraSubject.id, PlanetaryCameraSubject.title);
                StoredData.Add(Data);
                ReviewData(Data);
            }
            catch (Exception e)
            {
                Debug.Log("CactEye 2: Excpetion 5: Was not able to find Experiment with ExperimentID: " + ExperimentID.ToString());
                Debug.Log(e.ToString());

                return "An error occurred. Please post on the Official CactEye 2 thread on the Kerbal Forums.";
            }


            return "";
        }
    }
}
