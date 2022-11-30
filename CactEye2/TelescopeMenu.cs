using System;
using System.Collections.Generic;
using UnityEngine;
using KSP.UI.Dialogs;
using ClickThroughFix;
using ToolbarControl_NS;
using static CactEye2.InitialSetup;

namespace CactEye2
{
    class TelescopeMenu : MonoBehaviour
    {
        internal static TelescopeMenu Instance;

        public float scienceMultiplier;

        private bool isSmallOptics = false;
        private bool isScopeOpen = false;

#if false
        private ModuleAnimateGeneric aperature = null;
#endif

        //Position and size of the window
        private Rect WindowPosition;

        //Unique ID and window title for the GUI.
        private int WindowId;
        private string WindowTitle;

        //Flag that detects if the GUI is enabled or not.
        public bool IsGUIVisible = false;

        //Gui is 80% of screen resolution.
        //private float ScreenToGUIRatio = 0.8f;

        //Field of view of the scope camera.
        internal float FieldOfView = 0f;
        private float GyroSensitivity = 1f;

        //Control variable for enabling the Gyro.
        private bool GyroEnabled = false;

        private Rect ScopeRect;
        private Rect ControlRect;

        internal CactEyeCamera CameraModule;

        //Textures
        static private Texture2D PreviewTexture = new Texture2D(2, 2);
        static private Texture2D CrosshairTexture = new Texture2D(2, 2);
        static private Texture2D TargetPointerTexture = new Texture2D(2, 2);
        static private Texture2D SaveScreenshotTexture = new Texture2D(2, 2);
        static private Texture2D Atom6Icon = new Texture2D(2, 2);
        static private Texture2D Back9Icon = new Texture2D(2, 2);
        static private Texture2D Forward9Icon = new Texture2D(2, 2);


        private List<CactEyeProcessor> Processors = new List<CactEyeProcessor>();
        private CactEyeOptics cactEyeOptics;
        private int CurrentProcessorIndex = 0;
        private List<CactEyeGyro> ReactionWheels = new List<CactEyeGyro>();
        private List<float> ReactionWheelPitchTorques = new List<float>();
        private List<float> ReactionWheelYawTorques = new List<float>();
        private List<float> ReactionWheelRollTorques = new List<float>();

        internal static GUIStyle upperCenter, middleRight, middleCenter, upperLeft, middleLeft, buttonText, labelBoldText;


        //Status message for player
        private string Notification = "";
        static private double timer = 6f;
        private double storedTime = 0f;
        private bool timedExperimentInProgress = false;

        static internal void InitStatics()
        {
            ToolbarControl.LoadImageFromFile(ref PreviewTexture, KSPUtil.ApplicationRootPath + "GameData/CactEye/PluginData/Icons/preview");
            PreviewTexture.filterMode = FilterMode.Point;
            ToolbarControl.LoadImageFromFile(ref CrosshairTexture, KSPUtil.ApplicationRootPath + "GameData/CactEye/PluginData/Icons/crosshair");
            ToolbarControl.LoadImageFromFile(ref TargetPointerTexture, KSPUtil.ApplicationRootPath + "GameData/CactEye/PluginData/Icons/target");
            ToolbarControl.LoadImageFromFile(ref SaveScreenshotTexture, KSPUtil.ApplicationRootPath + "GameData/CactEye/PluginData/Icons/save");
            ToolbarControl.LoadImageFromFile(ref Atom6Icon, KSPUtil.ApplicationRootPath + "GameData/CactEye/PluginData/Icons/atom6");
            ToolbarControl.LoadImageFromFile(ref Back9Icon, KSPUtil.ApplicationRootPath + "GameData/CactEye/PluginData/Icons/back19");
            ToolbarControl.LoadImageFromFile(ref Forward9Icon, KSPUtil.ApplicationRootPath + "GameData/CactEye/PluginData/Icons/forward19");

            upperCenter = new GUIStyle(GUI.skin.label);
            upperCenter.alignment = TextAnchor.UpperCenter;

            middleRight = new GUIStyle(GUI.skin.label);
            middleRight.alignment = TextAnchor.MiddleRight;

            middleCenter = new GUIStyle(GUI.skin.label);
            middleCenter.alignment = TextAnchor.MiddleCenter;

            upperLeft = new GUIStyle(GUI.skin.label);
            upperLeft.alignment = TextAnchor.UpperLeft;

            middleLeft = new GUIStyle(GUI.skin.label);
            middleLeft.alignment = TextAnchor.UpperLeft;

            buttonText = new GUIStyle(GUI.skin.button);
            buttonText.fontStyle = FontStyle.Bold;
            buttonText.fontSize = 18;

            labelBoldText = new GUIStyle(GUI.skin.label);
            labelBoldText.fontStyle = FontStyle.Bold;
            labelBoldText.fontSize = 18;
            labelBoldText.alignment = TextAnchor.MiddleCenter;

        }


        public TelescopeMenu(Transform Position)
        {
            Instance = this;

            //unique id for the gui window.
            this.WindowTitle = "CactEye Telescope Control System";
            this.WindowId = WindowTitle.GetHashCode() + new System.Random().Next(65536);

            //Create the window rectangle object
            float StartXPosition = Screen.width * 0.1f;
            float StartYPosition = Screen.height * 0.1f;
            float WindowWidth = 700f;
            float WindowHeight = 500f;
            WindowPosition = new Rect(StartXPosition, StartYPosition, WindowWidth, WindowHeight);

            //Attempt to create the Telescope camera object.
            try
            {
                CameraModule = new CactEyeCamera(Position);
            }
            catch (Exception E)
            {
                Log.Error("Exception 2: Was not able to create the camera object.");
                Log.Error(E.ToString());
            }
        }

        public void SetSmallOptics(bool sSmallOptics)
        {
            isSmallOptics = sSmallOptics;
        }

        public void SetScopeOpen(bool open)
        {
            isScopeOpen = open;
        }

#if false
        public void SetAperature(ModuleAnimateGeneric mag)
        {
            aperature = mag;
        }
#else
        private PartWrapper.PartWrapper sdwrapper;
        public void SetSDWrapper(PartWrapper.PartWrapper wrapper)
        {
            sdwrapper = wrapper;
        }
#endif

        //Might take a look at using lazy initialization for enabling/disabling the menu object
        public void Toggle(CactEyeOptics cactEyeOptics)
        {
            if (!IsGUIVisible)
            {
                this.cactEyeOptics = cactEyeOptics;
                //Moved to here from the constructor; this should get a new list of gyros
                //every time the player enables the menu to account for part changes due to
                //docking/undocking operations.
                try
                {
                    //Grab Reaction Wheels
                    GetReactionWheels();
                    GetProcessors();

                    //if (ActiveProcessor.GetProcessorType().Contains("Wide Field"))
                    //{
                    //    ActiveProcessor.ActivateProcessor();
                    //}
                    if (cactEyeOptics.ProcessorNeeded)
                    {
                        if (cactEyeOptics.ActiveProcessor != null)
                            cactEyeOptics.ActiveProcessor.ActivateProcessor();
                        else
                            Log.Error("Exception 3: No active Processors found.");
                    }
                }
                catch (Exception E)
                {
                    Log.Error("Exception 3: Was not able to get a list of Reaction Wheels or Processors.");
                    Log.Error(E.ToString());
                }
            }
            else
            {
                if (cactEyeOptics != null && cactEyeOptics.ActiveProcessor != null)
                {
                    if (cactEyeOptics.ActiveProcessor.GetProcessorType().Contains("Wide Field"))
                    {
                        cactEyeOptics.ActiveProcessor.DeactivateProcessor();
                    }
                    cactEyeOptics.ActiveProcessor = null;
                }
            }
            IsGUIVisible = !IsGUIVisible;
        }
        //static Texture2D blackBackground = new Texture2D(1, 1);
        //private static Texture2D[] _textureWhiteNoise;
        internal static Texture2D[] TextureNoSignal;

        int TextureNoSignalId = 0;


        int pauseFlag = 0;
        internal void UpdateWhiteNoise()
        {
            pauseFlag++;
            if (pauseFlag < 4) return;
            pauseFlag = 0;

            TextureNoSignalId++;

            if (TextureNoSignalId >= TextureNoSignal.Length)
            {
                TextureNoSignalId = 0;
            }
        }


        void DisplayOccultationInfo(CactEyeProcessor ActiveProcessor)
        {
            CactEyeOccultationProcessor processor = ActiveProcessor as CactEyeOccultationProcessor;

            GUI.Label(new Rect(425f, 188f, 250, 48), "Current Target: " + ((CactEyeOccultationProcessor)ActiveProcessor).targetBody, labelBoldText);
            if (processor.timeUntilStart > 0)
            {
                GUI.Label(new Rect(425f, 230, 250, 24), "Waiting for Timed Start", middleCenter);
                GUI.Label(new Rect(425f, 250, 250, 24), "Time until start: " + processor.timeUntilStart.ToString("F0"), middleCenter);
            }
            else
            {
                GUI.Label(new Rect(425f, 230, 250, 24), "Timed Observation in Progress", middleCenter);
                GUI.Label(new Rect(425f, 250, 250, 24), "Remaining Time:: " + processor.timeUntilCompletion.ToString("F0"), middleCenter);
            }
        }

        string lastTarget = "";
        private void MainTelescopeWindowGUI(int WindowID)
        {
            GUI.enabled = !OccultationScienceEventWindow.Instance.selectionWindowVisible;
            timer += Planetarium.GetUniversalTime() - storedTime;
            storedTime = Planetarium.GetUniversalTime();
            GUILayout.BeginHorizontal();
            //Top right hand corner button that exits the window.
            if (GUI.Button(new Rect(WindowPosition.width - 18, 2, 16, 16), ""))
            {

                Toggle(null);
                return;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            //What you see looking through the telescope.
            ScopeRect = GUILayoutUtility.GetRect(400f, 400f);
            Texture2D ScopeScreen;
            if (!cactEyeOptics.ProcessorNeeded || cactEyeOptics.ActiveProcessor)
                ScopeScreen = CameraModule.UpdateTexture(cactEyeOptics.ActiveProcessor);
            else
            {
                ScopeScreen = TextureNoSignal[TextureNoSignalId];
            }
            GUI.DrawTexture(ScopeRect, ScopeScreen);


            //Draw the preview texture
            GUI.DrawTexture(new Rect(ScopeRect.xMin, ScopeRect.yMax - 32f, 128f, 32f), PreviewTexture);
            //Draw the crosshair texture
            GUI.DrawTexture(new Rect(ScopeRect.xMin + (0.5f * ScopeRect.width) - 64, ScopeRect.yMin + (0.5f * ScopeRect.height) - 64, 128, 128), CrosshairTexture);
            //Draw the notification label
            if (timer > 5f)
            {
                Notification = "";
            }
            GUI.Label(new Rect(ScopeRect.xMin + 16, ScopeRect.yMin + 16, 600, 32), new GUIContent(Notification));

            ControlRect = GUILayoutUtility.GetRect(300f, 20f);
            if (!timedExperimentInProgress)
            {
                if (cactEyeOptics.ProcessorNeeded && Processors.Count > 1)
                {
                    //Previous button
                    if (GUI.Button(new Rect(433f, 72f, 32, 32), Back9Icon))
                    {
                        cactEyeOptics.ActiveProcessor.Active = false;
                        cactEyeOptics.ActiveProcessor = GetPrevious(Processors, cactEyeOptics.ActiveProcessor);
                        cactEyeOptics.ActiveProcessor.Active = true;
                        CactEyeAsteroidSpawner.instance.UpdateSpawnRate();
                    }
                    if (GUI.Button(new Rect(635f, 72f, 32, 32), Forward9Icon))
                    {
                        cactEyeOptics.ActiveProcessor.Active = false;
                        cactEyeOptics.ActiveProcessor = GetNext(Processors, cactEyeOptics.ActiveProcessor);
                        cactEyeOptics.ActiveProcessor.Active = true;
                        CactEyeAsteroidSpawner.instance.UpdateSpawnRate();
                    }
                }
            }
            //GUI.skin.GetStyle("Label").alignment = TextAnchor.UpperCenter;
            GUI.Label(new Rect(475f, 40f, 150, 32), "Active Processor", upperCenter);
            if (cactEyeOptics.ProcessorNeeded && cactEyeOptics.ActiveProcessor != null)
                GUI.Label(new Rect(475f, 72f, 150, 32), cactEyeOptics.ActiveProcessor.GetProcessorType(), upperCenter);

            float offset = 0;
            if (!timedExperimentInProgress)
            {

                if (!isSmallOptics)
                {
                    if (cactEyeOptics.ActiveProcessor != null && cactEyeOptics.ActiveProcessor.GetProcessorType() == "Occultation")
                    {
                        string t = "Select\nTarget";
                        if (((CactEyeOccultationProcessor)cactEyeOptics.ActiveProcessor).vessel != null)
                            t = "Change\nTarget";
                        if (GUI.Button(new Rect(475f, 124f, 150, 48), t, buttonText))
                        {
                            ((CactEyeOccultationProcessor)cactEyeOptics.ActiveProcessor).Toggle();
                        }
                        offset = 140f;
                    }
                    //else
                    {
                        if (GUI.Button(new Rect(475f, offset + 124f, 150, 48), "Toggle\nAperature", buttonText))
                        {
#if false
                    aperature.Toggle();
#else
                            sdwrapper.Toggle();
#endif
                        }
#if true
                        if (cactEyeOptics.ProcessorNeeded && cactEyeOptics.ActiveProcessor.GetProcessorType() == "Occultation")
                        {
                            ((CactEyeOccultationProcessor)cactEyeOptics.ActiveProcessor).solarFilter =
                                GUI.Toggle(new Rect(475f, offset + 124f - 30, 150, 48), ((CactEyeOccultationProcessor)cactEyeOptics.ActiveProcessor).solarFilter, "Enable Solar Filter");
                        }
#endif
                    }
                }

                string target = "";
                if (FlightGlobals.fetch.VesselTarget != null && cactEyeOptics.ProcessorNeeded && cactEyeOptics.ActiveProcessor != null && cactEyeOptics.ActiveProcessor.GetProcessorType().Contains("Wide Field"))
                {
                    //GUI.skin.GetStyle("Label").alignment = TextAnchor.MiddleRight;
                    GUI.Label(new Rect(425f, 188f, 150, 32), "Store Image:", middleRight);
                    if (GUI.Button(new Rect(585f, 188f, 32, 32), SaveScreenshotTexture))
                    {
                        //DisplayText("Saved screenshot to " + opticsModule.GetTex(true, targetName));
                        Notification = " Screenshot saved to " + WriteTextureToDrive(CameraModule.TakeScreenshot(cactEyeOptics.ActiveProcessor));
                        timer = 0f;
                    }
                }
                else
                {
                    if (cactEyeOptics.ProcessorNeeded && cactEyeOptics.ActiveProcessor != null && cactEyeOptics.ActiveProcessor.GetProcessorType() == "Occultation")
                    {
                        var r = cactEyeOptics.ActiveProcessor as CactEyeOccultationProcessor;
                        Vessel v = null;

                        if (r.occAstCom != Guid.Empty)
                        {
                            v = FlightGlobals.FindVessel(r.occAstCom);
                            target = v.GetDisplayName();
                        }
                        else
                            target = r.targetBody;
                        if (target != "")
                        {
                            GUI.Label(new Rect(425f, 188f, 250, 48), "Current Target: " + target, labelBoldText);
                            if (target != lastTarget)
                            {
                                lastTarget = target;
                                if (r.occAstCom != Guid.Empty)
                                {
                                    v = FlightGlobals.FindVessel(r.occAstCom);
                                    if (v != null)
                                    {
                                        FlightGlobals.fetch.SetVesselTarget(v);
                                    }
                                    else
                                        Log.Error("Unable to find current target in Vessel list, guid: " + r.occAstCom);
                                }
                                else
                                {
                                    CelestialBody t = null;
                                    foreach (var t1 in FlightGlobals.Bodies)
                                    {
                                        string name = t1.GetDisplayName();
                                        name = name.Substring(0, name.Length - 2);
                                        if (name == r.targetBody)
                                        {
                                            t = t1;
                                            break;
                                        }
                                    }
                                    if (t == null)
                                        Log.Error("Unable to find body in list: " + name);
                                    else
                                        FlightGlobals.fetch.SetVesselTarget(t);
                                }
                            }
                        }
                        else
                            GUI.Label(new Rect(425f, 188f, 250, 48), "No target set", labelBoldText);
                    }
                    else
                        GUI.Label(new Rect(475f, 188f, 150, 48), "Imaging not available.", labelBoldText);
                }

                if (FlightGlobals.fetch.VesselTarget != null && cactEyeOptics.ProcessorNeeded && cactEyeOptics.ActiveProcessor != null && HighLogic.CurrentGame.Mode != Game.Modes.SANDBOX)
                {
                    bool canProcessData = true;
                    if (cactEyeOptics.ActiveProcessor.GetProcessorType() == "Occultation")
                    {
                        if (target != "")
                        {
                            if (sdwrapper.IsOpen)
                            {
                                double diff = ((CactEyeOccultationProcessor)cactEyeOptics.ActiveProcessor).occTime - Planetarium.GetUniversalTime();
                                if (diff > 0)
                                    GUI.Label(new Rect(425f, offset / 2 + 252f, 150, 32), "Start Observation:", middleRight);
                                else canProcessData = false;
                            }
                            else
                            {
                                GUI.Label(new Rect(400f, offset / 2 + 252f, 300, 32), "Observation not possible with closed Aperature", middleRight);
                                canProcessData = false;
                            }
                        }
                        else
                            canProcessData = false;
                    }
                    else
                    {
                        GUI.Label(new Rect(425f, offset / 2 + 252f, 150, 32), "Process Data:", middleRight);
                    }
                    if (canProcessData && GUI.Button(new Rect(585f, offset / 2 + 252f, 32, 32), Atom6Icon))
                    {
                        if (cactEyeOptics.ActiveProcessor.GetProcessorType() == "Occultation")
                            timedExperimentInProgress = true;

                        //DisplayText("Saved screenshot to " + opticsModule.GetTex(true, targetName));
                        //ActiveProcessor.GenerateScienceReport(TakeScreenshot(ActiveProcessor.GetType()));
                        try
                        {
                            Notification = cactEyeOptics.ActiveProcessor.DoScience(GetTargetPos(FlightGlobals.fetch.VesselTarget.GetTransform().position, 500f), scienceMultiplier, CameraModule.FieldOfView, CameraModule.TakeScreenshot(cactEyeOptics.ActiveProcessor), cactEyeOptics.ActiveProcessor);

                            if (timedExperimentInProgress && Notification != "Waiting for start time")
                            {
                                timedExperimentInProgress = false;
                            }
                        }
                        catch (Exception e)
                        {
                            Notification = "An error occurred. Please post that you're having this error on the official CactEye 2 thread on the Kerbal Forums.";
                            Log.Error("CactEye 2a: Exception 4: An error occurred producing a science report!");
                            Log.Error(e.ToString());
                        }
                        timer = 0f;
                    }
                }
                else
                {
                    GUI.skin.GetStyle("Label").alignment = TextAnchor.MiddleCenter;
                    GUI.Label(new Rect(425f, offset / 2 + 252f, 250, 32), "Data processing not available.");
                }
            }
            else
            {
                DisplayOccultationInfo(cactEyeOptics.ActiveProcessor);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            //Draw Processor controls in bottom center of display, with observation and screenshot buttons in center.
            DrawTargetPointer();
            if (!timedExperimentInProgress || ((CactEyeOccultationProcessor)cactEyeOptics.ActiveProcessor).timeUntilStart > 0)
            {
                if (!cactEyeOptics.ProcessorNeeded || cactEyeOptics.ActiveProcessor)
                {
                    //Zoom Feedback Label.
                    string LabelZoom = "Zoom/Magnification: x";
                    if (CameraModule.FieldOfView > 0.0064)
                    {
                        LabelZoom += string.Format("{0:####0.0}", 64 / CameraModule.FieldOfView);
                    }
                    else
                    {
                        LabelZoom += string.Format("{0:0.00E+0}", (64 / CameraModule.FieldOfView));
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    //GUI.skin.GetStyle("Label").alignment = TextAnchor.UpperLeft;
                    GUILayout.Label(LabelZoom, upperLeft);
                    GUILayout.EndHorizontal();

                    //Zoom Slider Controls.
                    GUILayout.BeginHorizontal();
                    if (!cactEyeOptics.ProcessorNeeded || cactEyeOptics.ProcessorNeeded && cactEyeOptics.ActiveProcessor)
                    {
                        FieldOfView = GUILayout.HorizontalSlider(FieldOfView, 0f, 1f);
                        if (!cactEyeOptics.ProcessorNeeded)
                            CameraModule.FieldOfView = 0.5f * Mathf.Pow(4f - FieldOfView * (4f - Mathf.Pow(0.5f, (1f / 3f))), 3);
                        else
                            CameraModule.FieldOfView = 0.5f * Mathf.Pow(4f - FieldOfView * (4f - Mathf.Pow(cactEyeOptics.ActiveProcessor.GetMinimumFOV(), (1f / 3f))), 3);
                    }
#if DEBUG
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Up.x:" + cactEyeOptics.cameraUp.x.ToString("F0"));
                    if (GUILayout.Button("^")) cactEyeOptics.cameraUp.x++;
                    if (GUILayout.Button("v")) cactEyeOptics.cameraUp.x--;
                    GUILayout.Label("  Up.y:" + cactEyeOptics.cameraUp.y.ToString("F0"));
                    if (GUILayout.Button("^")) cactEyeOptics.cameraUp.y++;
                    if (GUILayout.Button("v")) cactEyeOptics.cameraUp.y--;
                    GUILayout.Label("  Up.z:" + cactEyeOptics.cameraUp.z.ToString("F0"));
                    if (GUILayout.Button("^")) cactEyeOptics.cameraUp.z++;
                    if (GUILayout.Button("v")) cactEyeOptics.cameraUp.z--;
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Forward.x:" + cactEyeOptics.cameraForward.x.ToString("F0"));
                    if (GUILayout.Button("^")) cactEyeOptics.cameraForward.x++;
                    if (GUILayout.Button("v")) cactEyeOptics.cameraForward.x--;
                    GUILayout.Label("  Forward.y:" + cactEyeOptics.cameraForward.y.ToString("F0"));
                    if (GUILayout.Button("^")) cactEyeOptics.cameraForward.y++;
                    if (GUILayout.Button("v")) cactEyeOptics.cameraForward.y--;
                    GUILayout.Label("  Forward.z:" + cactEyeOptics.cameraForward.z.ToString("F0"));
                    if (GUILayout.Button("^")) cactEyeOptics.cameraForward.z++;
                    if (GUILayout.Button("v")) cactEyeOptics.cameraForward.z--;

                        Log.Info("Setting camera position");
                        cactEyeOptics.localCameratransform.localPosition = cactEyeOptics.cameraPosition;
                        cactEyeOptics.localCameratransform.localRotation = Quaternion.LookRotation(cactEyeOptics.cameraForward, cactEyeOptics.cameraUp);

                    UpdatePosition(cactEyeOptics.transform);
#endif
                    //GUILayout.EndHorizontal();

                    //Log spam
                    //Debug.Log("CactEye 2: MinimumFOV = " + ActiveProcessor.GetMinimumFOV().ToString());
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    // GUI.skin.GetStyle("Label").alignment = TextAnchor.UpperLeft;
                    GUILayout.Label("Processor not installed; optics module cannot function without an image processor.", upperLeft);
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndHorizontal();

            //Gyro GUI. Active only if the craft has an active gyro
            if (GyroEnabled)
            {
                //Gyro Slider Label
                GUILayout.BeginHorizontal();
                //GUI.skin.GetStyle("Label").alignment = TextAnchor.UpperLeft;
                GUILayout.Label("Gyro Sensitivity:  " + GyroSensitivity.ToString("P") + " + minimum gyroscopic torgue.", upperLeft, GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

                //Gyro Slider Controls.
                GUILayout.BeginHorizontal();
                GyroSensitivity = GUILayout.HorizontalSlider(GyroSensitivity, 0f, 1f, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();

                SetTorgue();
            }

            //Make the window draggable by the top bar only.
            //GUI.DragWindow(new Rect(0, 0, WindowPosition.width, 16));
            GUI.enabled = true;
            GUI.DragWindow();
        }

        internal void EndTimedExperiment()
        {
            timedExperimentInProgress = false;
        }

        public void DrawGUI()
        {
            if (!PauseMenu.isOpen && !FlightResultsDialog.isDisplaying && !MapView.MapIsEnabled)
            {
                WindowPosition = ClickThruBlocker.GUILayoutWindow(WindowId, WindowPosition, MainTelescopeWindowGUI, WindowTitle); //, InitialSetup.windowStyle);
            }
        }

        internal void FixedUpdate(Part part, CactEyeOptics optics)
        {
            //UpdatePosition(part.FindModelTransform(CameraTransformName));
            //UpdatePosition(optics.transform);
            UpdateWhiteNoise();
            //Close window down if we run out of power
            if (cactEyeOptics.ProcessorNeeded && !cactEyeOptics.ActiveProcessor.IsActive())
            {
                cactEyeOptics.ActiveProcessor = null;
                Notification = "Image Processor is out of power. Please restore power to telescope";
                ScreenMessages.PostScreenMessage(Notification, 5f, ScreenMessageStyle.UPPER_CENTER);
                timer = 0f;
                Toggle(null);
                timedExperimentInProgress = false;
            }

        }

        public void UpdatePosition(Transform Position)
        {
            CameraModule.UpdatePosition(Position);
        }

        public void ToggleGyro()
        {
            GyroEnabled = !GyroEnabled;
        }

        public Vector3 GetTargetPos(Vector3 worldPos, float width)
        {
            //Camera c = cameras.Find(n => n.name.Contains("00"));
            //            Camera c = CameraModule.GetCamera(2);
            Camera c = CameraModule.GetOverlayCamera();
            Vector3 vec = c.WorldToScreenPoint(worldPos);

            if (Vector3.Dot(CameraModule.CameraTransform.forward, worldPos) > 0)
            {
                if (vec.x > 0 && vec.y > 0 && vec.x < c.pixelWidth && vec.y < c.pixelHeight)
                {
                    vec.y = c.pixelHeight - vec.y;
                    vec *= (width / c.pixelWidth);
                    return vec;
                }
            }

            return new Vector3(-1, -1, 0);
        }

        public float GetGyroSensitivty()
        {
            return GyroSensitivity;
        }

        public bool IsMenuEnabled()
        {
            return IsGUIVisible;
        }

        public float GetFOV()
        {
            return CameraModule.FieldOfView;
        }

        /* ************************************************************************************************
         * Function Name: DrawTargetPointer
         * Input: None
         * Output: None
         * Purpose: This function will draw the pink target recticle in the scope's view. This works
         * rather well, so don't touch this unless it's absolutely neccesary, as there's a lot of moving
         * parts here.
         * ************************************************************************************************/
        private void DrawTargetPointer()
        {

            if (FlightGlobals.fetch.VesselTarget != null)
            {
                string targetName = FlightGlobals.fetch.VesselTarget.GetName();
                Vector2 vec = GetTargetPos(FlightGlobals.fetch.VesselTarget.GetTransform().transform.position, ScopeRect.width);

                if (vec.x > 16 && vec.y > 16 && vec.x < ScopeRect.width - 16 && vec.y < ScopeRect.height - 16)
                {
                    //                    GUI.DrawTexture(new Rect(vec.x + ScopeRect.xMin - 16, vec.y + ScopeRect.yMin - 16, 32, 32), TargetPointerTexture);
                    GUI.DrawTexture(new Rect(vec.x + ScopeRect.xMin - 16, vec.y + ScopeRect.yMin - 32, 32, 32), TargetPointerTexture);
                    Vector2 size = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(targetName));
                    if (vec.x > 0.5 * size.x && vec.x < ScopeRect.width - (0.5 * size.x) && vec.y < ScopeRect.height - 16 - size.y)
                    {
                        //GUI.skin.GetStyle("Label").alignment = TextAnchor.UpperCenter;
                        GUI.Label(new Rect(vec.x + ScopeRect.xMin - (0.5f * size.x), vec.y + ScopeRect.yMin + 20, size.x, size.y), targetName, upperCenter);
                    }
                }
            }
        }
#if false
        /* ************************************************************************************************
         * Function Name: DrawProcessorControls
         * Input: None
         * Output: None
         * Purpose: This function will draw the four main processor control objects: the screenshot
         * button, the science button, and the next/previous processor buttons.
         * The screenshot button will only display and be available if the telescope has a valid
         * processor installed on the scope.
         * The science button will only appear if a target is selected, if there is a valid processor
         * installed, and if the game is not a sandbox game. It will generate a science report based
         * on the selected target.
         * The next/previous buttons will only appear if the scope has more than one processor installed,
         * and will allow the player to cycle through the different processors.
         * ************************************************************************************************/
        private void DrawProcessorControls()
        {
            //if (!ActiveProcessor.IsActive())
            //{
            //    //Craft is out of power.
            //    Notification = "Image processor is out of power; shutting down processor.";
            //    timer = 0f;
            //    Processors.Remove(ActiveProcessor);
            //}

            //Draw save icon
            if (FlightGlobals.fetch.VesselTarget != null && ActiveProcessor && ActiveProcessor.GetProcessorType().Contains("Wide Field"))
            {
                if (GUI.Button(new Rect(ScopeRect.xMin + ((0.5f * ScopeRect.width) + 20), ScopeRect.yMin + (ScopeRect.height - 48f), 32, 32), SaveScreenshotTexture))
                {
                    Notification = " Screenshot saved to " + WriteTextureToDrive(CameraModule.TakeScreenshot(ActiveProcessor));
                    timer = 0f;
                }
            }

            //Draw gather science icon
            //Atom6 icon from Freepik
            //<div>Icons made by Freepik from <a href="http://www.flaticon.com" title="Flaticon">www.flaticon.com</a>         is licensed by <a href="http://creativecommons.org/licenses/by/3.0/" title="Creative Commons BY 3.0">CC BY 3.0</a></div>
            if (FlightGlobals.fetch.VesselTarget != null && ActiveProcessor && HighLogic.CurrentGame.Mode != Game.Modes.SANDBOX)
            {
                if (GUI.Button(new Rect(ScopeRect.xMin + ((0.5f * ScopeRect.width) - 20), ScopeRect.yMin + (ScopeRect.height - 48f), 32, 32), Atom6Icon))
                {
                    //DisplayText("Saved screenshot to " + opticsModule.GetTex(true, targetName));
                    //ActiveProcessor.GenerateScienceReport(TakeScreenshot(ActiveProcessor.GetType()));
                    try
                    {
                        Notification = ActiveProcessor.DoScience(GetTargetPos(FlightGlobals.fetch.VesselTarget.GetTransform().position, 500f), scienceMultiplier, CameraModule.FieldOfView, CameraModule.TakeScreenshot(ActiveProcessor), ActiveProcessor);
                    }
                    catch (Exception e)
                    {
                        Notification = "An error occurred. Please post that you're having this error on the official CactEye 2 thread on the Kerbal Forums.";
                        Log.Error("Exception 4: An error occurred producing a science report!");
                        Log.Error(e.ToString());
                    }

                    timer = 0f;
                }
            }

            //Got an off-by-one error in the list somewhere
            //Previous/Next buttons
            if (Processors.Count > 1)
            {
                //Previous button
                if (GUI.Button(new Rect(ScopeRect.xMin + ((0.5f * ScopeRect.width) - 72), ScopeRect.yMin + (ScopeRect.height - 48f), 32, 32), Back9Icon))
                {
                    ActiveProcessor.Active = false;
                    ActiveProcessor = GetPrevious(Processors, ActiveProcessor);
                    ActiveProcessor.Active = true;
                }

                //Next Button
                if (GUI.Button(new Rect(ScopeRect.xMin + ((0.5f * ScopeRect.width) + 72), ScopeRect.yMin + (ScopeRect.height - 48f), 32, 32), Forward9Icon))
                {
                    ActiveProcessor.Active = false;
                    ActiveProcessor = GetNext(Processors, ActiveProcessor);
                    ActiveProcessor.Active = true;
                }
            }
        }
#endif

        /* ************************************************************************************************
         * Function Name: GetReactionWheels
         * Input: none
         * Output: None
         * Purpose: This function will grab a list of gyroscopes installed on the scope's craft. The name
         * is leftover from a previous functionality, of which the function use to return a list of all
         * reactionwheels, including command modules and gyroscopes. 
         * 
         * This was heavily refactored on 11/3/2014 by Raven.
         * ************************************************************************************************/
        private void GetReactionWheels()
        {
            ReactionWheels.Clear();

            for (int i = 0; i < FlightGlobals.ActiveVessel.Parts.Count; i++)
            {
                Part p = FlightGlobals.ActiveVessel.Parts[i];

                //foreach (Part p in FlightGlobals.ActiveVessel.Parts)
                //{
                CactEyeGyro mrw = p.GetComponent<CactEyeGyro>();
                if (mrw != null)
                {
                    if (!ReactionWheels.Contains(mrw))
                    {
                        ReactionWheels.Add(mrw);
                        GyroSensitivity = mrw.GyroSensitivity;
                    }
                }
            }

            Log.Info("Found " + ReactionWheels.Count.ToString() + " Gyro units.");

            if (ReactionWheels.Count > 0)
            {
                GyroEnabled = true;
                ReactionWheelPitchTorques = new List<float>();
                ReactionWheelRollTorques = new List<float>();
                ReactionWheelYawTorques = new List<float>();

                for (int i = 0; i < ReactionWheels.Count; i++)
                {
                    ReactionWheelPitchTorques.Add(ReactionWheels[i].PitchTorque);
                    ReactionWheelRollTorques.Add(ReactionWheels[i].RollTorque);
                    ReactionWheelYawTorques.Add(ReactionWheels[i].YawTorque);
                }

            }
            else
            {
                GyroEnabled = false;
            }
        }

        /* ************************************************************************************************
         * Function Name: GetProcessors
         * Input: None
         * Output: None
         * Purpose: This function will generate a list of image processors installed on the telescope
         * craft.
         * ************************************************************************************************/
        private void GetProcessors()
        {
            Processors.Clear();
            //for (int i = 0; i <  cactEyeOptics.part.children.Count; i++)

            for (int i = 0; i < FlightGlobals.ActiveVessel.Parts.Count; i++)
            {
                Part p = FlightGlobals.ActiveVessel.Parts[i];
                //  if (p.parent == cactEyeOptics.part || cactEyeOptics.part.parent == p)
                {

                    //foreach (Part p in FlightGlobals.ActiveVessel.Parts)
                    //{
                    CactEyeProcessor cpu = p.GetComponent<CactEyeProcessor>();
                    if (cpu != null)
                    {
                        if (!Processors.Contains(cpu))
                        {
                            Processors.Add(cpu);
                        }
                    }
                }
            }
            Log.Info("Found " + Processors.Count.ToString() + " Processors.");

            if (Processors.Count > 0)
            {
                cactEyeOptics.ActiveProcessor = Processors[0];
                CurrentProcessorIndex = 0;

                //if (ActiveProcessor.GetProcessorType().Contains("Wide Field"))
                //{
                //    ActiveProcessor.Active = true;
                //}
            }
            else
                cactEyeOptics.ActiveProcessor = null;
        }

        /* ************************************************************************************************
         * Function Name: SetTorgue
         * Input: None
         * Output: None
         * Purpose: This function will modify the torgue rating of all gyroscopes installed on the telescope
         * craft. This is tied directly with the gryoscope sensitivity control slider.
         * ************************************************************************************************/
        private void SetTorgue()
        {

            for (int i = 0; i < ReactionWheels.Count; i++)
            {
                ReactionWheels[i].GyroSensitivity = GyroSensitivity;
            }
        }


        private CactEyeProcessor GetNext(IEnumerable<CactEyeProcessor> list, CactEyeProcessor current)
        {
            try
            {
                //return list.SkipWhile(x => !x.Equals(current)).Skip(1).First();
                //lastAgentIDAarhus = agents[ index == -1 ? 0 : index % ( agents.Count - 1 ) ];
                if ((CurrentProcessorIndex + 1) < Processors.Count)
                {
                    CurrentProcessorIndex++;
                }
                else
                {
                    CurrentProcessorIndex = 0;
                }

                //return Processors[CurrentProcessorIndex == -1 ? 0 : CurrentProcessorIndex % (Processors.Count - 1)];
                if (HighLogic.CurrentGame.Parameters.CustomParams<CactiSettings>().DebugMode)
                {
                    Log.Info("CurrentProcessorIndex: " + CurrentProcessorIndex.ToString());
                }

                return Processors[CurrentProcessorIndex];
            }
            catch (Exception e)
            {
                Log.Error("Exception 6: Was not able to find the next processor, even though there is one.");
                Log.Error(e.ToString());

                return Processors[0];
            }
        }

        private CactEyeProcessor GetPrevious(IEnumerable<CactEyeProcessor> list, CactEyeProcessor current)
        {
            try
            {
                //return list.SkipWhile(x => !x.Equals(current)).Skip(1).First();
                //lastAgentIDAarhus = agents[ index == -1 ? 0 : index % ( agents.Count - 1 ) ];
                if (CurrentProcessorIndex == 0)
                {
                    CurrentProcessorIndex = Processors.Count - 1;
                }
                else
                {
                    CurrentProcessorIndex--;
                }

                //return Processors[CurrentProcessorIndex == -1 ? 0 : CurrentProcessorIndex % (Processors.Count - 1)];
                Log.Info("CurrentProcessorIndex: " + CurrentProcessorIndex.ToString());
                return Processors[CurrentProcessorIndex];
            }
            catch (Exception e)
            {
                Log.Error("Exception #: Was not able to find the next processor, even though there is one.");
                Log.Error(e.ToString());

                return Processors[0];
            }
        }

        /* ************************************************************************************************
         * Function Name: WriteTextureToDrive
         * Input: The texture object that will be written to the hard drive.
         * Output: None
         * Purpose: This function will take an input texture and then convert it to a png file in the 
         * CactEye subfolder of the Screenshot folder.
         * This currently has some bugs.
         * If Linux users complain about screenshots not saving to the disk, then this is the first place
         * to look.
         * ************************************************************************************************/
        private string WriteTextureToDrive(Texture2D Input)
        {
            byte[] Bytes = Input.EncodeToPNG();
            string ScreeshotFolderPath = KSPUtil.ApplicationRootPath.Replace("\\", "/") + "Screenshots/CactEye/";
            //string TargetName = FlightGlobals.activeTarget.ToString();
            string TargetName = FlightGlobals.fetch.VesselTarget.GetName().ToString();
            string ScreenshotFilename = "";



            //Create CactEye screenshot folder if it doesn't exist
            if (!System.IO.Directory.Exists(ScreeshotFolderPath))
            {
                System.IO.Directory.CreateDirectory(ScreeshotFolderPath);
            }

            ScreenshotFilename = TargetName + CactEyeAPI.Time() + ".png";
            System.IO.File.WriteAllBytes(ScreeshotFolderPath + ScreenshotFilename, Bytes);
            return ScreenshotFilename;
        }
    }
}
