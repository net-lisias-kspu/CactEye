using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CactEye2
{
    class CactEyeWideField : CactEyeProcessor
    {

        /* ************************************************************************************************
         * Function Name: DoScience
         * Input: Position of the target, whether or not we're dealing with the FungEye or CactEye optics,
         *          the current field of view, and a screenshot.
         * Output: None
         * Purpose: This function will generate a science report based on the input parameters. This is an 
         * override of a function prototype. This will generate a science report based on the target 
         * celestial body. Science reports will only be generated if the target is a celestial body,
         * if the target is not the sun, if the target is visible in the scope, and if the telescope
         * is zoomed in far enough.
         * ************************************************************************************************/
        public override string DoScience(float FOV)
        {
            
            if (FlightGlobals.fetch.VesselTarget.GetType() == typeof(CelestialBody))
            {
                CelestialBody Target = FlightGlobals.GetBodyByName(FlightGlobals.fetch.VesselTarget.GetName());
                CelestialBody Home = this.vessel.mainBody;

 //               CheckAim(FlightGlobals.fetch.activeVessel.transform.forward, FlightGlobals.fetch.activeVessel.transform.position, Target, FOV);
   //             return "";

                if (Target == FlightGlobals.Bodies[0])
                {
                    //Cannot target the sun
                    if (CactEyeConfig.DebugMode)
                    {
                        Debug.Log("CactEye 2: Wide Field Camera: Cannot target the sun.");
                    }
                    return Type + ": Cannot target the sun.";
                }

                if (!CheckAim(FlightGlobals.fetch.activeVessel.transform.forward, FlightGlobals.fetch.activeVessel.transform.position, Target, FOV))
                {
                    //target not in scope
                    if (CactEyeConfig.DebugMode)
                    {
                        Debug.Log("CactEye 2: Wide Field Camera: Target not in scope.");
                    }
                    return Type + ": Target not in scope field of view.";
                }

                int cFOV = CheckFOV(FlightGlobals.ship_position, (CelestialBody)FlightGlobals.ActiveVessel.targetObject, FOV, MinimumView);
                if (cFOV == 0)
                {
                    if (CactEyeConfig.DebugMode)
                    {
                        Debug.Log("CactEye 2: Wide Field Camera: Scope not zoomed in far enough.");
                        Debug.Log("CactEye 2: Wide Field Camera: Field of view: " + FOV.ToString());
                        Debug.Log("CactEye 2: Wide Field Camera: Target arc size: " + CalcSize(Target.Radius, Target.GetAltitude(FlightGlobals.ship_position)));
                    }
                    return Type + ": Scope not zoomed in far enough.";
                }
                else if (cFOV == 2)
                {
                    if (CactEyeConfig.DebugMode)
                    {
                        Debug.Log("CactEye 2: Wide Field Camera: Scope zoomed in to close");
                        Debug.Log("CactEye 2: Wide Field Camera: " + FOV.ToString());
                        Debug.Log("CactEye 2: Wide Field Camera: " + CalcSize(Target.Radius, Target.GetAltitude(FlightGlobals.ship_position)));
                    }
                    return Type + ": Scope zoomed in to far.";
                }

                //Check to see if target is blocked.
                else if (CheckOccult(FlightGlobals.fetch.activeVessel.transform.position, Target))
                {
                    if (CactEyeConfig.DebugMode)
                    {
                        Debug.Log("CactEye 2: Target is occulted by another body.");
                    }
                    return Type + ": Target is occulted by another body.";
                }

                

                string TargetName = Target.name;
                ScienceExperiment WideFieldExperiment;
                ScienceSubject WideFieldSubject;
                
                try
                {
                    WideFieldExperiment = ResearchAndDevelopment.GetExperiment(ExperimentID);
                    WideFieldSubject = ResearchAndDevelopment.GetExperimentSubject(WideFieldExperiment, ExperimentSituations.InSpaceHigh, Target, "VisualObservation" + Target.name);
                    WideFieldSubject.title = "CactEye Visual Planetary Observation of " + Target.name;

                    float sciencePoints = WideFieldExperiment.baseValue * WideFieldExperiment.dataScale;
                    ScienceData Data = new ScienceData(sciencePoints, 1f, 0f, WideFieldSubject.id, WideFieldSubject.title);
                    StoredData.Add(Data);
                    ReviewData(Data);
                    if (RBWrapper.APIRBReady)
                    {
                        Debug.Log("CactEye 2: Wrapper ready");

                        RBWrapper.CelestialBodyInfo cbi;

                        RBWrapper.RBactualAPI.CelestialBodies.TryGetValue(Target, out cbi);
                        if (!cbi.isResearched)
                        {
                            int RBFoundScience = (int)(8f * WideFieldExperiment.dataScale);
                            //RBWrapper.RBactualAPI.FoundBody(RBFoundScience, Target, out withParent, out parentBody);
                        }
                        else
                        {
                            System.Random rnd = new System.Random();
                            RBWrapper.RBactualAPI.Research(Target, rnd.Next(1, 11));
                        }


                    }
                    else
                    {
                        Debug.Log("CactEye 2: Wrapper not ready");
                    }
                }

                catch (Exception e)
                {
                    Debug.Log("CactEye 2: Excpetion 5: Was not able to find Experiment with ExperimentID: " + ExperimentID.ToString());
                    Debug.Log(e.ToString());

                    return "An error occurred. Please post on the Official CactEye 2 thread on the Kerbal Forums.";
                }

                return "";
            }
            return "";

        }

    }
}