using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;
using ToolbarControl_NS;
using ClickThroughFix;

using KSP_Log;

namespace CactEye2
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    class CactEyeConfigMenu: MonoBehaviour
    {
        internal static Log Log;

        //Position and size of the window
        private Rect WindowPosition;

        //Unique ID and window title for the GUI.
        private int WindowId;
        private string WindowTitle;

        //Flag that detects if the GUI is enabled or not.
        private bool IsGUIVisible = false;

        //private static ApplicationLauncherButton appLauncherButton = null;
        //private Texture2D Icon = null;

        private bool DebugMode = false;
        private bool SunDamage = false;
        private bool GyroDecay = false;
        private bool AsteroidSpawner = false;

        internal const string MODID = "CactiEye_ns";
        internal const string MODNAME = "CactEye Optics";

        //Set once application launcher button has been installed
        //private bool AppLauncher = false;
        ToolbarControl toolbarControl;

        /* ************************************************************************************************
         * Function Name: CactEyeConfigMenu
         * Input: N/A
         * Output: N/A
         * Purpose: Default constructor for the configuration menu. This will set several different 
         * initial values for the config menu GUI.
         * ************************************************************************************************/
        void CactEyeConfigMenuInit()
        {
            if (Log == null)
                Log = new Log("CactEyeOptics", Log.LEVEL.INFO);

            //unique id for the gui window.
            this.WindowTitle = "CactEye 2 Configuration Menu";
            this.WindowId = WindowTitle.GetHashCode() + new System.Random().Next(65536);

            //Create the window rectangle object
            float StartXPosition = Screen.width * 0.1f;
            float StartYPosition = Screen.height * 0.1f;
            float WindowWidth = 200;
            float WindowHeight = 125;
            WindowPosition = new Rect(StartXPosition, StartYPosition, WindowWidth, WindowHeight);
        }

        /* ************************************************************************************************
         * Function Name: Awake
         * Input: N/A
         * Output: N/A
         * Purpose: Awake is a function that Unity looks for when instantiating a MonoBehavior or derived
         * class. Awake should fire at the start of a scene, typically directly after a scene change.
         * ************************************************************************************************/
        public void Awake() 
        {
            CactEyeConfigMenuInit();
            InitToolbarButton();
        }


        /* ************************************************************************************************
         * Function Name: OnDestroy
         * Input: N/A
         * Output: N/A
         * Purpose: OnDestroy is a function that Unity look for when destroying a MonoBehavior or derived
         * class. Think of it as a destructor; it's called on the destruction of an object, and in Unity
         * allows the programmer to clean things up. In this case, OnDestroy will remove the application
         * launcher button so we don't get duplicate buttons in the SpaceCenter.
         * ************************************************************************************************/
        public void OnDestroy()
        {
            if (toolbarControl != null)
            {

                toolbarControl.OnDestroy();
                Destroy(toolbarControl);

                if (CactEyeConfig.DebugMode)
                {
                    Debug.Log("CactEye 2: Debug: ToolbarControl Button destroyed!");
                }
            }
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
                toolbarControl.AddToAllToolbars(OnButtonTrue,
                    OnButtonFalse,
                    ApplicationLauncher.AppScenes.SPACECENTER,
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

        /* ************************************************************************************************
         * Function Name: OnAppLauncherTrue
         * Input: N/A
         * Output: N/A
         * Purpose: This function is called when a user clicks the application launcher button and the 
         * configuration menu is not being displayed. Essentially it brings up the configuration menu.
         * ************************************************************************************************/
        void OnButtonTrue()
        {
            Toggle();

            if (CactEyeConfig.DebugMode)
            {
                Log.Info(" Debug: OnButtonTrue() fired!");
            }

        }

        /* ************************************************************************************************
         * Function Name: OnAppLauncherFalse
         * Input: N/A
         * Output: N/A
         * Purpose: This function is called when a user clicks the application launcher button and the 
         * configuration menu is not being displayed. Essentially it hides the configuration menu.
         * ************************************************************************************************/
        void OnButtonFalse()
        {
            Toggle();

            if (CactEyeConfig.DebugMode)
            {
                Log.Info(" Debug: OnButtonFalse() fired!");
            }
        }


        /* ************************************************************************************************
         * Function Name: Toggle
         * Input: N/A
         * Output: N/A
         * Purpose: This function will show or hide the configuration menu, depending on whether or not 
         * the configuration menu is already up.
         * ************************************************************************************************/
        public void Toggle()
        {
            if (!IsGUIVisible)
            {

                CactEyeConfig.ReadSettings();
                DebugMode = CactEyeConfig.DebugMode;
                Log.SetLevel(DebugMode? Log.LEVEL.INFO:Log.LEVEL.ERROR);
                
                SunDamage = CactEyeConfig.SunDamage;
                GyroDecay = CactEyeConfig.GyroDecay;
                AsteroidSpawner = CactEyeConfig.AsteroidSpawner;
                if (CactEyeConfig.DebugMode)
                {
                    Log.Info(" Debug: CactEyeConfigMenu enabled!");
                }
            }

            else
            {

                CactEyeConfig.DebugMode = DebugMode;
                Log.SetLevel(DebugMode ? Log.LEVEL.INFO : Log.LEVEL.ERROR);
                CactEyeConfig.SunDamage = SunDamage;
                CactEyeConfig.GyroDecay = GyroDecay;
                CactEyeConfig.AsteroidSpawner = AsteroidSpawner;
                CactEyeConfig.ApplySettings();

                if (CactEyeConfig.DebugMode)
                {
                    Log.Info(" Debug: CactEyeConfigMenu disabled!");
                }
            }

            IsGUIVisible = !IsGUIVisible;
        }

        /* ************************************************************************************************
         * Function Name: MainGUI
         * Input: N/A
         * Output: N/A
         * Purpose: This function will draw the configuration menu GUI, and define the individual controls
         * on that GUI.
         * ************************************************************************************************/
        private void MainGUI(int WindowID)
        {

            if (CactEyeConfig.DebugMode)
            {
                Log.Info(" Debug: CactEyeConfigMenu.MainGUI called!");
            }

            //Top right hand corner button that exits the window.
            if (GUI.Button(new Rect(WindowPosition.width - 18, 2, 16, 16), ""))
            {
                Toggle();
            }

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            DebugMode = GUILayout.Toggle(DebugMode, "Enable Debug Mode.");
            Log.SetLevel(DebugMode ? Log.LEVEL.INFO : Log.LEVEL.ERROR);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            SunDamage = GUILayout.Toggle(SunDamage, "Enable Sun Damage to Telescopes.");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GyroDecay = GUILayout.Toggle(GyroDecay, "Enable Gyroscope decay over time.");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            AsteroidSpawner = GUILayout.Toggle(AsteroidSpawner, "Enable Asteroid spawning.");
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            //Make the window draggable by the top bar only.
            GUI.DragWindow(new Rect(0, 0, WindowPosition.width, 16));
        }

        public void OnGUI()
        {
            if (IsGUIVisible)
                DrawGUI();
        }

        /* ************************************************************************************************
         * Function Name: DrawGUI
         * Input: N/A
         * Output: N/A
         * Purpose: This function is called when the Toggle function is called. This will define the 
         * configuration menu window and then call MainGUI.
         * ************************************************************************************************/
        private void DrawGUI()
        {

            if (CactEyeConfig.DebugMode)
            {
                Log.Info(" Debug: Callback to DrawGUI occurred!");
            }

            WindowPosition = ClickThruBlocker.GUILayoutWindow(WindowId, WindowPosition, MainGUI, WindowTitle);
        }
    }
}
