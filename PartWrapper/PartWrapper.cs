using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using SpaceTuxUtility;
using SpaceDust;
using SCANsat;
using KSP_Log;

namespace PartWrapper
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class PartWrapperInit : MonoBehaviour
    {
        internal static bool HasSpaceDust = false;
        internal static bool HasScanSat = false;

        private void Start()
        {
            //HasSpaceDust = HasMod.hasMod("SpaceDust");
            HasScanSat = HasMod.hasMod("SCANsat");
            PartWrapper.Log = new Log("CactEye2.PartWrapper", Log.LEVEL.INFO);
        }
    }

    public class PartWrapper
    {
        const string SPACEDUST = "SpaceDust";
        const string SCANSAT = "SCANsat";
        internal static Log Log;

        private ModuleAnimateGeneric aperature = null;
        System.Object scope = null;
        string partOwner;
        public void InitPartWrapper(Part p, string partOwner)
        {
            this.partOwner = partOwner;
            if (PartWrapperInit.HasSpaceDust && partOwner == SPACEDUST)
                GetModuleSpaceDustTelescope(p);
            else
                if (PartWrapperInit.HasScanSat && partOwner == SCANSAT)
                GetModuleSCANsatTelescope(p);
            else
                aperature = p.GetComponent<ModuleAnimateGeneric>();
        }

        void GetModuleSpaceDustTelescope(Part p) { scope = p.GetComponent<ModuleSpaceDustTelescope>() as System.Object; }
        void GetModuleSCANsatTelescope(Part p) { scope = p.GetComponent<SCANsat.SCAN_PartModules.SCANsat>() as System.Object; }
        public void SetAperature(ModuleAnimateGeneric mag)
        {
            aperature = mag;
        }

        public float animTime
        {
            get
            {
                if (PartWrapperInit.HasSpaceDust && partOwner == SPACEDUST)
                    return GetScopeEnabled();
                if (PartWrapperInit.HasScanSat && partOwner == SCANSAT)
                    return GetScanSatEnabled();
                return (aperature != null ? aperature.animTime : 0);
            }
        }

        float GetScopeEnabled() { return ((ModuleSpaceDustTelescope)scope).Enabled ? 1 : 0; }
        float GetScanSatEnabled() { return ((SCANsat.SCAN_PartModules.SCANsat)scope).scanningNow ? 1 : 0;  }

        bool open = false;
        public bool IsOpen {  get { return open; } }
        public void Toggle()
        {
            if (PartWrapperInit.HasSpaceDust && partOwner == SPACEDUST)
            {
                ToggleSpaceDustScope();
                open = !open;
            }
            else if (PartWrapperInit.HasScanSat && partOwner == SCANSAT)
            {
                ToggleScanSat();
                open = !open;
            }
            else
            {
                if (aperature != null)
                {
                    aperature.Toggle();
                    open = !open;
                }
            }
        }

        void ToggleSpaceDustScope()
        {
            if (((ModuleSpaceDustTelescope)scope).Enabled)
                ((ModuleSpaceDustTelescope)scope).DisableTelescope();
            else
                ((ModuleSpaceDustTelescope)scope).EnableTelescope();
        }

        void ToggleScanSat()
        {
            Log.Info("ToggleScanSat");
            ((SCANsat.SCAN_PartModules.SCANsat)scope).toggleScanAction(new KSPActionParam(KSPActionGroup.Custom01, KSPActionType.Activate));
        }
    }
}
