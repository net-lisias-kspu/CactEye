using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ClickThroughFix;
using static CactEye2.OccultationScienceEventWindow;
using System.Text.RegularExpressions;
using KSPPluginFramework;
using static CactEye2.InitialSetup;

//Currently incomplete
namespace CactEye2
{
    public class CactEyeOccultationProcessor : CactEyeProcessor
    {
        public enum OccultationTypes { None = 0, Asteroid = 1, Planet = 2, Moon = 3, Comet = 4 };

        [KSPField(isPersistant = true)]
        public string occType = "None";

        [KSPField(isPersistant = true)]
        public int occTypeInt = 0;

        [KSPField(isPersistant = true)]
        public double occTime = 0;

        [KSPField(isPersistant = true)]
        public string occBody;

        [KSPField(isPersistant = true)]
        public string refBody;

        [KSPField(isPersistant = true)]
        public Guid occAstCom; // Asteroid or Comet

        [KSPField(isPersistant = true, guiActive = true, guiName = "Target")]
        internal string targetBody = "";

        internal ITargetable target;

        [KSPField(isPersistant = true)]
        internal bool solarFilter = false;

        public string OccType { get { return ((OccultationTypes)occTypeInt).ToString(); } }
        public OccultationTypes OccTypeEnum
        {
            get
            {
                foreach (OccultationTypes e in (OccultationTypes[])Enum.GetValues(typeof(OccultationTypes)))
                    if (e.ToString() == occType)
                        return e;
                return OccultationTypes.None;
            }
            set { occTypeInt = (int)value; occType = OccType; }
        }

        public static OccultationTypes OccTypeStringToEnum(string str)
        {
            foreach (OccultationTypes e in (OccultationTypes[])Enum.GetValues(typeof(OccultationTypes)))
                if (e.ToString() == str)
                    return e;
            return OccultationTypes.None;            
        }

        internal bool TargetSet { get { return targetBody != ""; } }


        [KSPEvent(guiName = "Select Target", guiActive = true, guiActiveEditor = false)]
        public void SetTarget()
        {
            Toggle();
        }

        public void Toggle()
        {
            OccultationScienceEventWindow_TrackStation.Instance.ToggleSelectionWindow(this);

            Events["SetTarget"].guiName = (OccultationScienceEventWindow_TrackStation.Instance.SelectionWindowVisible ? "Close Selection Window" : "Select Target");
        }

        //Position and size of the window
        private Rect WindowPosition;
        private int WindowId;
        private string WindowTitle;
        new void Start()
        {
            base.Start();
            WindowTitle = "Target Selection";
            //Create the window rectangle object
            float StartXPosition = Screen.width * 0.1f;
            float StartYPosition = Screen.height * 0.1f;
            float WindowWidth = 300;
            float WindowHeight = 400;
            WindowPosition = new Rect(StartXPosition, StartYPosition, WindowWidth, WindowHeight);
            WindowId = WindowTitle.GetHashCode() + new System.Random().Next(65536);
        }


        [KSPEvent(guiName = "Clear Target", guiActive = true, guiActiveEditor = false)]
        public void ClearTarget()
        {
            Log.Info("CactEyeOccultationProcessor.ClearTarget, SetVesselTarget(null)");
            FlightGlobals.fetch.SetVesselTarget(null);
            this.vessel.targetObject = null;
            targetBody = "";
            OccTypeEnum = OccultationTypes.None;
            occTypeInt = (int)OccTypeEnum;
            occBody = "";
            refBody = "";
            occAstCom = Guid.Empty;
            target = null;
            ScreenMessages.PostScreenMessage("Target Body Cleared", 5);
        }

#if false
        private string OccultationExperiment_Asteroid(Vector3 TargetPosition, float scienceMultiplier, float FOV, Texture2D Screenshot)
        {
            return "";
        }

        private string OccultationExperiment_Planetary(Vector3 TargetPosition, float scienceMultiplier, float FOV, Texture2D Screenshot)
        {
            return "";
        }
#endif
#if false
        public override string DoScience(Vector3 TargetPosition, float scienceMultiplier, float FOV, Texture2D Screenshot, CactEyeProcessor ActiveProcessor)
        {
            if (occType == "None")
            {
                return Type + ": Occultation experiment not available";
            }

            else if (occType == "Asteroid")
            {
                return OccultationExperiment_Asteroid(TargetPosition, scienceMultiplier, FOV, Screenshot);
            }

            else
            {
                return OccultationExperiment_Planetary(TargetPosition, scienceMultiplier, FOV, Screenshot);
            }
        }
#endif

        public override Texture2D ApplyFilter(string Filter, Texture2D InputTexture)
        {
            if (solarFilter)
            {
                Color[] Colors = InputTexture.GetPixels();

                for (int i = 0; i < Colors.Length; i++)
                {
                    float GrayscaleValue = Colors[i].grayscale / 2;
                    Colors[i] = new Color(GrayscaleValue, GrayscaleValue, GrayscaleValue, Colors[i].a);
                }

                InputTexture.SetPixels(Colors);
            }
            return InputTexture;
        }

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

        public override string DoScience(Vector3 TargetPosition, float scienceMultiplier, float FOV, Texture2D Screenshot, CactEyeProcessor ActiveProcessor)
        {
            Target = FlightGlobals.Bodies.Find(n => n.GetName() == FlightGlobals.fetch.VesselTarget.GetName());
            CelestialBody Home = this.vessel.mainBody;

            occProcessor = ActiveProcessor as CactEyeOccultationProcessor;
            if (Target.bodyName != occProcessor.occBody)
            {
                Log.Error("Vessel target not matching occBody, Target.name: " + Target.bodyName + ", occbody: " + occProcessor.occBody);
                return Type + ": Vessel target not matching experiment target";
            }

            if (TargetPosition == new Vector3(-1, -1, 0))
            {
                //target not in scope
                return Type + ": Target not in scope field of view.";
            }
            //This has a tendency to be rather tempermental. If a player is getting false "Scope not zoomed in far enough" errors,
            //then the values here will need to be adjusted.
            else if (FOV > CactEyeAPI.bodyInfo[Target].bodySize * 4f)
            {
                //Scope not zoomed in far enough
                Log.Info("Occultation Camera: Scope not zoomed in far enough.");
                Log.Info("Occultation Camera: " + FOV.ToString());
                Log.Info("Occultation Camera: " + (CactEyeAPI.bodyInfo[Target].bodySize * 50f).ToString());
                return Type + ": Scope not zoomed in far enough.";
            }
            //Check to see if target is blocked.
            else if (CactEyeAPI.CheckOccult(Target) != "")
            {
                if (HighLogic.CurrentGame.Parameters.CustomParams<CactiSettings>().DebugMode)
                {
                    Log.Info("Target is occulted by another body.");
                }
                return Type + ": Target is occulted by another body.";
            }

            this.TargetPosition = TargetPosition;
            this.FOV = FOV;
            this.Screenshot = Screenshot;
            this.scienceMultiplier = scienceMultiplier;
            timeToReturn = false;
            StartCoroutine(WaitToReturn());
            StartCoroutine(WaitForStartTime());
            return "Waiting for start time";
        }

        bool timeToReturn = false;
        internal bool showMessage = false;
        string returnMsg;

        float FOV;
        float scienceMultiplier;
        CelestialBody Target;
        Vector3 TargetPosition;
        CactEyeOccultationProcessor occProcessor;
        Texture2D Screenshot;
        string lastMessage = "";
        double lastMessageTime = 0;
        void ShowMessage(string Notification, float time = 5f)
        {
            if (Notification != lastMessage || Planetarium.GetUniversalTime() - lastMessageTime > time)
            {
                ScreenMessages.PostScreenMessage(Notification, time, ScreenMessageStyle.UPPER_CENTER);
                lastMessage = Notification;
                lastMessageTime = Planetarium.GetUniversalTime();
            }
        }
        IEnumerator WaitToReturn()
        {
            while (!timeToReturn)
            {
                yield return new WaitForSeconds(1);
            }
            TelescopeMenu.Instance.EndTimedExperiment();
        }

        internal double timeUntilStart;
        internal double timeUntilCompletion;
        IEnumerator WaitForStartTime()
        {
            double endTime = occProcessor.occTime + 60;

            while (endTime > Planetarium.GetUniversalTime())
            {

                bool waiting = occProcessor.occTime > Planetarium.GetUniversalTime();

                if (waiting)
                {
                    FOV = TelescopeMenu.Instance.CameraModule.FieldOfView;
                    timeUntilStart = occProcessor.occTime - Planetarium.GetUniversalTime();
                }
                else
                {
                    timeUntilStart = -1;
                    timeUntilCompletion = endTime - Planetarium.GetUniversalTime();
                }

                //Sandbox or Career mode logic handled by gui.
                //if (HighLogic.CurrentGame.Mode == Game.Modes.SANDBOX)
                //{
                //    //CactEyeGUI.DisplayText("Science experiments unavailable in sandbox mode!");
                //    return;
                //}
                TargetPosition = TelescopeMenu.Instance.GetTargetPos(FlightGlobals.fetch.VesselTarget.GetTransform().position, 500f);
                if (TargetPosition == new Vector3(-1, -1, 0))
                {
                    //target not in scope
                    returnMsg = Type + ": Target not in scope field of view.";
                    if (!waiting)
                    {
                        timeToReturn = true;
                        break;
                    }
                    else
                    {
                        showMessage = true;
                        ShowMessage(returnMsg, 1);
                    }
                }
                else if (FOV > CactEyeAPI.bodyInfo[Target].bodySize * 4f)
                {
                    //Scope not zoomed in far enough
                    Log.Info("Occultation Camera: Scope not zoomed in far enough.");
                    Log.Info("Occultation Camera: FOV: " + FOV.ToString());
                    Log.Info("Occultation Camera: " + (CactEyeAPI.bodyInfo[Target].bodySize * 8f).ToString());
                    returnMsg = Type + ": Scope not zoomed in far enough.";
                    if (!waiting)
                    {
                        timeToReturn = true;
                        break;
                    }
                    else
                    {
                        showMessage = true;
                        ShowMessage(returnMsg, 1);
                    }
                }
                else if (FOV < CactEyeAPI.bodyInfo[Target].bodySize * 2f)
                {
                    //Scope zoomed in too far
                    Log.Info("Occultation Camera: Scope zoomed in too far.");
                    Log.Info("Occultation Camera: FOV: " + FOV.ToString());
                    Log.Info("Occultation Camera: " + (CactEyeAPI.bodyInfo[Target].bodySize * 2f).ToString());
                    returnMsg = Type + ": Scope zoomed in too far.";
                    if (!waiting)
                    {
                        timeToReturn = true;
                        break;
                    }
                    else
                    {
                        showMessage = true;
                        ShowMessage(returnMsg, 1);
                    }
                }
                //Check to see if target is blocked.
                else if (CactEyeAPI.CheckOccult(Target) != "")
                {
                    if (HighLogic.CurrentGame.Parameters.CustomParams<CactiSettings>().DebugMode)
                    {
                        Log.Info("Target is occulted by another body.");
                    }
                    returnMsg = Type + ": Target is occulted by another body.";
                    timeToReturn = true;
                    break;
                }

                double science = (2 * CactEyeAPI.bodyInfo[Target].bodySize - FOV) / FOV +1;

                Log.Info("FOV: " + FOV  + ", base bodySize: " + (2*CactEyeAPI.bodyInfo[Target].bodySize).ToString() +
                    ", science: " + science.ToString("F4"));

                yield return new WaitForSeconds(0.25f);
            }
            if (!timeToReturn)
            {
                FinalizeScience();
                timeToReturn = true;
            }
        }


        void FinalizeScience()
        {
            float SciencePoints = 0f;
            float ScienceAdjustedCap = 0f;
            float ScienceAvailableCap = 0f;
            string TargetName = Target.name;
            ScienceExperiment OccultationExperiment;
            ScienceSubject OccultationSubject;

            bool withParent;
            CelestialBody parentBody;

            double scienceFOVadjustement = (2 * CactEyeAPI.bodyInfo[Target].bodySize - FOV) / FOV + 1;

            ExperimentID = "CactEyeOccultation";
            try
            {
                OccultationExperiment = ResearchAndDevelopment.GetExperiment(ExperimentID);
                OccultationSubject = ResearchAndDevelopment.GetExperimentSubject(OccultationExperiment, ExperimentSituations.InSpaceHigh, Target, "VisualObservation" + Target.name, "");
                OccultationSubject.title = "CactEye Visual Planetary Observation of " + Target.name;
                SciencePoints = OccultationExperiment.baseValue * OccultationExperiment.dataScale * maxScience * scienceMultiplier * (float)scienceFOVadjustement;
                if (solarFilter)
                    SciencePoints /= 2;
                if (HighLogic.CurrentGame.Parameters.CustomParams<CactiSettings>().DebugMode)
                {
                    Log.Info("Cacteye 2: SciencePoints: " + SciencePoints);
                    Log.Info("Cacteye 2: Current Science: " + OccultationSubject.science);
                    Log.Info("Cacteye 2: Current Cap: " + OccultationSubject.scienceCap);
                    Log.Info("Cacteye 2: ScienceValue: " + OccultationSubject.scientificValue);
                    Log.Info("Cacteye 2: SubjectValue: " + ResearchAndDevelopment.GetSubjectValue(SciencePoints, OccultationSubject));
                    Log.Info("Cacteye 2: RnDScienceValue: " + ResearchAndDevelopment.GetScienceValue(SciencePoints, OccultationSubject, 1.0f));
                    Log.Info("Cacteye 2: RnDReferenceDataValue: " + ResearchAndDevelopment.GetReferenceDataValue(SciencePoints, OccultationSubject));

                }
                //Modify Science cap and points gathered based on telescope and processor
                ScienceAdjustedCap = OccultationExperiment.scienceCap * OccultationExperiment.dataScale * maxScience * scienceMultiplier;

                //Since it's not clear how KSP figures science points, reverse engineer based off of what this will return.
                ScienceAvailableCap = ScienceAdjustedCap - ((SciencePoints / ResearchAndDevelopment.GetScienceValue(SciencePoints, OccultationSubject, 1.0f)) * OccultationSubject.science);
                if (HighLogic.CurrentGame.Parameters.CustomParams<CactiSettings>().DebugMode)
                {
                    Log.Info("Cacteye 2: Adjusted Cap: " + ScienceAdjustedCap);
                    Log.Info("Cacteye 2: Available Cap: " + ScienceAvailableCap);
                }
                if (ScienceAvailableCap < 0)
                {
                    ScienceAvailableCap = 0;
                }
                if (SciencePoints > ScienceAvailableCap)
                {
                    SciencePoints = ScienceAvailableCap;
                }


                if (HighLogic.CurrentGame.Parameters.CustomParams<CactiSettings>().DebugMode)
                {
                    Log.Info("SciencePoints: " + SciencePoints.ToString());
                }


                ScienceData Data = new ScienceData(SciencePoints, 1f, 0f, OccultationSubject.id, OccultationSubject.title);
                StoredData.Add(Data);
                ReviewData(Data, Screenshot);
                if (RBWrapper.APIRBReady)
                {
                    Log.Info("Wrapper ready");

                    RBWrapper.CelestialBodyInfo cbi;

                    RBWrapper.RBactualAPI.CelestialBodies.TryGetValue(Target, out cbi);
                    if (!cbi.isResearched)
                    {
                        int RBFoundScience = (int)(8f * OccultationExperiment.dataScale);
                        RBWrapper.RBactualAPI.FoundBody(RBFoundScience, Target, out withParent, out parentBody);
                    }
                    else
                    {
                        RBWrapper.RBactualAPI.Research(Target, InitialSetup.Random.Next(1, 11));
                    }


                }
                else
                {
                    Log.Info("Wrapper not ready");
                }
            }

            catch (Exception e)
            {
                Log.Error("Exception 5: Was not able to find Experiment with ExperimentID: " + ExperimentID.ToString());
                Log.Error(e.ToString());

                returnMsg = "An error occurred. Please post on the Official CactEye 2 thread on the Kerbal Forums.";
            }

            returnMsg = "";
        }
    }
}
