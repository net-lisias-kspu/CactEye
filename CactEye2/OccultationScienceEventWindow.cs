using System;
using System.Linq;
using UnityEngine;
using KSP.UI.Screens;
using static CactEye2.InitialSetup;
using KSPPluginFramework;
using ToolbarControl_NS;
using ClickThroughFix;


// Break 'occultation' down into sounds: [OK] + [UL] + [TAY] + [SHUHN] 

namespace CactEye2
{
    /* ************************************************************************************************
    * Class Name: OccultationScienceEventWindow_Flight
    * Purpose: This will allow CactEye's Occultation Science Event Window to run in multiple specific scenes. 
    * Shamelessly stolen from Starstrider42, who shamelessly stole it from Trigger Au.
    * Thank you!
    * ************************************************************************************************/
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    internal class OccultationScienceEventWindow_Flight : OccultationScienceEventWindow
    {
    }

    /* ************************************************************************************************
    * Class Name: OccultationScienceEventWindow_SpaceCentre
    * Purpose: This will allow CactEye's CactEye's Occultation Science Event Window to run in multiple specific scenes. 
    * Shamelessly stolen from Starstrider42, who shamelessly stole it from Trigger Au.
    * Thank you!
    * ************************************************************************************************/
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    internal class OccultationScienceEventWindow_SpaceCentre : OccultationScienceEventWindow
    {
    }

    /* ************************************************************************************************
    * Class Name: OccultationScienceEventWindow_TrackStation
    * Purpose: This will allow CactEye's CactEye's Occultation Science Event Window to run in multiple specific scenes. 
    * Shamelessly stolen from Starstrider42, who shamelessly stole it from Trigger Au.
    * Thank you!
    * ************************************************************************************************/
    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    internal class OccultationScienceEventWindow_TrackStation : OccultationScienceEventWindow
    {
    }


    internal class OccultationScienceEventWindow : MonoBehaviour
    {
        internal static OccultationScienceEventWindow Instance;


        internal bool SelectionWindowVisible { get { return selectionWindowVisible; } }
        internal bool selectionWindowVisible = false;

        CactEyeOccultationProcessor occultationProcessor = null;
         bool selectionActive = false;

        //Position and size of the window
        private Rect WindowPosition;
        private int WindowId;
        private string WindowTitle;
        Vector2 scrollPos;

        internal const string MODID = "CactiEye_ns";
        internal const string MODNAME = "CactEye Optics";
        static ToolbarControl toolbarControl;

        void Start()
        {
            Instance = this;
            WindowTitle = "Occultation Science Event Window";
            //Create the window rectangle object
            float StartXPosition = Screen.width * 0.1f;
            float StartYPosition = Screen.height * 0.1f;
            float WindowWidth = 300;
            float WindowHeight = 250;
            WindowPosition = new Rect(StartXPosition, StartYPosition, WindowWidth, WindowHeight);
            WindowId = WindowTitle.GetHashCode() + new System.Random().Next(65536);
            InitToolbarButton();
        }

        /* ************************************************************************************************
         * Function Name: InitializeApplicationButton
         * Input: N/A
         * Output: A reference to the newly created button.
         * Purpose: This function will initialize the application launcher button for CactEye, and specify
         * which functions to call when a user clicks the button.
         * ************************************************************************************************/
        void InitToolbarButton()
        {
            if (toolbarControl == null)
            {
                toolbarControl = gameObject.AddComponent<ToolbarControl>();
                toolbarControl.AddToAllToolbars(ToggleScienceEventWindow,
                    ToggleScienceEventWindow,
                    ApplicationLauncher.AppScenes.SPACECENTER |
                    ApplicationLauncher.AppScenes.FLIGHT |
                    ApplicationLauncher.AppScenes.TRACKSTATION,
                    MODID,
                    "cacteyeButton",
                    "CactEye/PluginData/Icons/CactEyeOptics-38",
                    "CactEye/PluginData/Icons/CactEyeOptics_disabled-38",
                    "CactEye/PluginData/Icons/toolbar-24",
                    "CactEye/PluginData/Icons/toolbar_disabled-24",
                    MODNAME
                );
            }

        }

        internal void ToggleSelectionWindow(CactEyeOccultationProcessor ceop)
        {
            occultationProcessor = ceop;
            selectionActive = true;
            WindowTitle = "Target Selection Window";
            selectionWindowVisible = !selectionWindowVisible;
        }

        internal void ToggleScienceEventWindow()
        {
            selectionActive = false;
            WindowTitle = "Occultation Science Event Window";
            selectionWindowVisible = !selectionWindowVisible;
        }

        void OnGUI()
        {
            if (selectionWindowVisible && !FlightDriver.Pause)
            {
                WindowPosition = ClickThruBlocker.GUILayoutWindow(WindowId, WindowPosition, MainGUI, WindowTitle); //, InitialSetup.windowStyle);                
                DumpTargetData(4);
            }
        }

        void Toggle()
        {
            selectionWindowVisible = !selectionWindowVisible;
        }

        KSPDateTime dt = new KSPDateTime(0f);
        KSPTimeSpan ts = new KSPTimeSpan(0f);
        string SecondsToDate(double UT) //, String format)
        {
            dt.UT = UT;
            ts.UT = UT;
            if (showDateOf)
                return dt.ToStringStandard(DateStringFormatsEnum.DateTimeFormat);
            else
                return ts.ToStringStandard(TimeSpanStringFormatsEnum.IntervalLongTrimYears);
        }

        string FromTo(string from, string to) { return from + RIGHTARROW + to; }

        bool showDateOf = false;
        float[] colWidths = new float[10];
        void GetFieldWidth(string str, int col)
        {
            Vector2 x = GUI.skin.label.CalcSize(new GUIContent(str));
            var w = x.x + 5f;

            colWidths[col] = Math.Max(colWidths[col], w + 10);
        }

        /// <summary>
        /// Calculate the maximum widths of each column
        /// </summary>
        void CalcColumns()
        {
            string str = "";
            GetFieldWidth("SOI" + RIGHTARROW + "Target", 0);
            GetFieldWidth("Assigned To", 1);
            GetFieldWidth(showDateOf ? "Date of" : "Time Until", 2);
            if (KACWrapper.APIReady)
                GetFieldWidth("Add KAC Alarm", 3);

            foreach (var r in CactEyeAPI.occExpAstCometDict.Values)
            {
                int cnt = 0;
                string from;
                if (r.referencebody != null)
                    from = r.referencebody.bodyName;
                else
                    from = "NoRefBody";
                switch (r.occType)
                {
                    case CactEyeOccultationProcessor.OccultationTypes.Moon:
                    case CactEyeOccultationProcessor.OccultationTypes.Planet:
                        str = r.body.displayName;
                        str = str.Substring(0, str.Length - 2);
                        break;

                    case CactEyeOccultationProcessor.OccultationTypes.Comet:
                    case CactEyeOccultationProcessor.OccultationTypes.Asteroid:
                        str = r.vessel.vesselName;
                        str = str.Substring(0, str.Length - 2);
                        break;

                    default:
                        Log.Error("No recognized type found");
                        break;
                }
                GetFieldWidth(FromTo(from, str), cnt++);
                GetFieldWidth((r.AssignedTo != Guid.Empty) ? VesselFromGuid(r.assignedToGuid) : "--", cnt++);
                str = showDateOf ? SecondsToDate(r.time) : SecondsToDate(r.time - Planetarium.GetUniversalTime());
                GetFieldWidth(str, cnt++);

                cnt++;
                WindowPosition.width = 10;
                for (int i = 0; i < cnt; i++)
                    WindowPosition.width += colWidths[i] + 15f;
                calcDone = true;
            }
        }

        uint IdFromGuid(Guid guid)
        {
            var v = FlightGlobals.FindVessel(guid);
            if (v == null)
                return 0;

            return v.persistentId;

        }
        string VesselFromGuid(Guid guid)
        {
            uint id = IdFromGuid(guid);
            if (!FlightGlobals.PersistentVesselIds.ContainsKey(id))
                return "";
            var v = FlightGlobals.FindVessel(guid);
            if (v == null)
                return "";
            return v.GetName();
        }

        const string RIGHTARROW = " ===> ";
        bool calcDone = false;
        internal void RedoColCals() { calcDone = false; }

        void MainGUI(int id)
        {
            if (!calcDone)
                CalcColumns();

            int cnt = 0;
            //Top right hand corner button that exits the window.
            if (GUI.Button(new Rect(WindowPosition.width - 18, 2, 16, 16), ""))
            {
                Toggle();
            }
            GUILayout.BeginVertical();
            GUILayout.Label("Select the observation to be assigned to the selected processor.  Your vessel will need to be in the SOI listed in order to observe the Occultation");

            GUILayout.BeginHorizontal();
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            GUILayout.BeginHorizontal();
            GUILayout.Label("SOI" + RIGHTARROW + "Target", GUILayout.Width(colWidths[cnt++]));
            GUILayout.Label("Assigned To", GUILayout.Width(colWidths[cnt++]));
            if (GUILayout.Button(showDateOf ? "Date of" : "Time Until", GUILayout.Width(colWidths[cnt++])))
            {
                showDateOf = !showDateOf;
                CalcColumns();
            }
            if (KACWrapper.APIReady)
                GUILayout.Label("", GUILayout.Width(colWidths[cnt++]));
            GUILayout.EndHorizontal();
            string str;

            foreach (var r in CactEyeAPI.occExpAstCometDict.Values)
            {
                cnt = 0;
                GUILayout.BeginHorizontal();

                string from;
                if (r.referencebody != null)
                    from = r.referencebody.bodyName;
                else
                    from = "NoRefBody";
                switch (r.occType)
                {
                    case CactEyeOccultationProcessor.OccultationTypes.Moon:
                    case CactEyeOccultationProcessor.OccultationTypes.Planet:
                        str = r.body.displayName;
                        str = str.Substring(0, str.Length - 2);
                        if (selectionActive)
                        {
                            if (GUILayout.Button(FromTo(from, str), GUILayout.Width(colWidths[cnt++])))
                            {
                                ScreenMessages.PostScreenMessage("Target Body set to: " + str, 5);
                                ScreenMessages.PostScreenMessage("Target Body is: " + r.type, 5);
                                SetTarget(str, r, r.body);
                            }
                        }
                        else
                            GUILayout.Label(FromTo(from, str), GUILayout.Width(colWidths[cnt++]));
                        break;

                    case CactEyeOccultationProcessor.OccultationTypes.Comet:
                    case CactEyeOccultationProcessor.OccultationTypes.Asteroid:
                        str = r.vessel.vesselName;
                        str = str.Substring(0, str.Length - 2);
                        if (selectionActive)
                        {
                            if (GUILayout.Button(FromTo(from, str), GUILayout.Width(colWidths[cnt++])))
                            {
                                ScreenMessages.PostScreenMessage("Target Body set to: " + str, 5);
                                ScreenMessages.PostScreenMessage("Target Body is: " + r.type, 5);
                                SetTarget(str, r, r.vessel);

                            }
                        }
                        else
                        {
                            GUILayout.Label(FromTo(from, str), GUILayout.Width(colWidths[cnt++]));
                        }
                        break;

                    default:
                        Log.Error("No recognized type found");
                        break;
                }

                GUILayout.Label((r.assignedToGuid != Guid.Empty) ? VesselFromGuid(r.assignedToGuid) : "--", GUILayout.Width(colWidths[cnt++]));
                GUILayout.Label(str = showDateOf ? SecondsToDate(r.time) : SecondsToDate(r.time - Planetarium.GetUniversalTime()), TelescopeMenu.middleLeft, GUILayout.Width(colWidths[cnt++]));

#if true
                if (r.AssignedTo != Guid.Empty && KACWrapper.APIReady)
                {
                    string existingAlarm = null;
                    if (KACWrapper.KAC.Alarms.Count() > 0)
                        existingAlarm = KACWrapper.KAC.Alarms.First(z => z.VesselID == IdFromGuid(r.AssignedTo).ToString()).ID;
                    if (existingAlarm == null)
                    {
                        if (GUILayout.Button("Add KAC Alarm", GUILayout.Width(colWidths[cnt++])))
                        {
                            var aID = KACWrapper.KAC.CreateAlarm(KACWrapper.KACAPI.AlarmTypeEnum.Raw, "Occultation Experiment", r.time - HighLogic.CurrentGame.Parameters.CustomParams<CactiSettings>().alarmTimeInAdvance);
                            var alarm = KACWrapper.KAC.Alarms.First(z => z.ID == aID);
                            alarm.Notes = "Aim telescope at target and keep it in the target for the duratio";
                            alarm.AlarmMargin = 60;
                            alarm.VesselID = IdFromGuid(r.AssignedTo).ToString();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Delete Alarm", GUILayout.Width(colWidths[cnt++])))
                        {
                            KACWrapper.KAC.DeleteAlarm(existingAlarm);
                        }
                    }
                }
#endif
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

            }
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Close"))
                Toggle();
            GUILayout.EndVertical();
            GUI.DragWindow();
            DumpTargetData(3);
        }


        [KSPEvent(guiName = "Clear Target", guiActive = true, guiActiveEditor = false)]
        public void ClearTarget()
        {
            Log.Info("OccultationScienceEventWindow.ClearTarget, SetVesselTarget(null)");
            FlightGlobals.fetch.SetVesselTarget(null);
            occultationProcessor.vessel.targetObject = null;
            occultationProcessor.targetBody = "";
            occultationProcessor.OccTypeEnum = CactEyeOccultationProcessor.OccultationTypes.None;
            occultationProcessor.occTypeInt = (int)occultationProcessor.OccTypeEnum;
            occultationProcessor.occBody = "";
            occultationProcessor.refBody = "";
            occultationProcessor.occTime = 0;
            occultationProcessor.occAstCom = Guid.Empty;
            ScreenMessages.PostScreenMessage("Target Body Cleared", 5);
        }

        void SetTarget(string target, OccultationRecord r, CelestialBody targetedBody)
        {
            occultationProcessor.targetBody = target;

            Log.Info("CactEyeOccultationProcessor.SetTarget, SetVesselTarget(CelestialBody targetedBody)");

            FlightGlobals.fetch.SetVesselTarget(targetedBody, true);

            occultationProcessor.occType = r.type;
            occultationProcessor.occTypeInt = (int)occultationProcessor.OccTypeEnum;
            occultationProcessor.occBody = targetedBody.bodyName;
            occultationProcessor.refBody = FlightGlobals.ActiveVessel.mainBody.bodyName;
            occultationProcessor.occTime = r.time;
            occultationProcessor.occAstCom = Guid.Empty;
            occultationProcessor.target = targetedBody;
            DumpTargetData(1);

            AssignTo(r, occultationProcessor.vessel.protoVessel.vesselID);
        }

        void SetTarget(string target, OccultationRecord r, Vessel v)
        {
            Log.Info("CactEyeOccultationProcessor.SetTarget, SetVesselTarget(Vessel v)");

            occultationProcessor.targetBody = target;

            // For some reason, the Vessel "v" which is passed in is not valid for the SetVesselTarget, so the loop  
            // below will find the correct vessel
            foreach (var f in FlightGlobals.Vessels)
                if (f.GetDisplayName() == v.GetDisplayName())
                {
                    FlightGlobals.fetch.SetVesselTarget(f);
                }

            occultationProcessor.occType = r.type;
            occultationProcessor.occTypeInt = (int)occultationProcessor.OccTypeEnum;
            occultationProcessor.occBody = "";
            occultationProcessor.refBody = r.referencebody.bodyName;
            occultationProcessor.occTime = r.time;
            occultationProcessor.occAstCom = v.protoVessel.vesselID;
            occultationProcessor.target = v;
            DumpTargetData(2);
            
            AssignTo(r,  occultationProcessor.vessel.protoVessel.vesselID);
        }

        void AssignTo(OccultationRecord r, Guid guid)
        {

            foreach (var r1 in CactEyeAPI.occExpAstCometDict)
                if (r1.Value.assignedToGuid == occultationProcessor.vessel.protoVessel.vesselID)
                    r1.Value.AssignTo = Guid.Empty;

            r.AssignTo = guid;
            calcDone = false;
        }
        void DumpTargetData(int i1)
        {
            return;
#if DEBUG
            if (occultationProcessor != null)
            {
                Log.Info("DumpTargetData: " + i1);
                Vessel vTarget = new Vessel();
                vTarget.vesselName = "NoTargetVessel";
                for (int i = 0; i < FlightGlobals.Vessels.Count; i++)
                {
                    if (FlightGlobals.Vessels[i].protoVessel.vesselID == occultationProcessor.occAstCom)
                    {
                        vTarget = FlightGlobals.Vessels[i];
                        break;
                    }
                }
                if (occultationProcessor.occAstCom != Guid.Empty)
                {
                    string str = "DumpTargetData: targetBody: ";
                    if (occultationProcessor.targetBody != null)
                        str += occultationProcessor.targetBody;
                    else
                        str += "null";

                    str += ", occType: ";
                    if (occultationProcessor.occType != null)
                        str += occultationProcessor.occType;
                    else
                        str += "null";
                    str += ", OccTypeEnum: " + occultationProcessor.OccTypeEnum;
                    str += ", occBody: ";
                    if (occultationProcessor.occBody != null)
                        str += occultationProcessor.occBody;
                    else
                        str += "null";
                    str += ", refBody: ";
                    if (occultationProcessor.refBody != null)
                        str += occultationProcessor.refBody;
                    else
                        str += "null";
                    str += ", occAstCom: " + occultationProcessor.occAstCom;
                    str += ", vTarget: ";
                    if (vTarget != null)
                        str += vTarget.vesselName;
                    else
                        str += "null";
                    Log.Info(str);

                }
            }
#endif
        }


        private string OccultationExperiment_Asteroid(Vector3 TargetPosition, float scienceMultiplier, float FOV, Texture2D Screenshot)
        {
            return "";
        }

        private string OccultationExperiment_Planetary(Vector3 TargetPosition, float scienceMultiplier, float FOV, Texture2D Screenshot)
        {
            return "";
        }
    }
}
