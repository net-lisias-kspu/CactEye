using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using SpaceTuxUtility;
using SpaceDust;
using KSP.Localization;
using UnityEngine;

namespace SpaceDustWrapper
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class SDWrapperInit : MonoBehaviour
    {

        internal static bool HasSpaceDust = false;
        private void Start()
        {
            HasSpaceDust = HasMod.hasMod("SpaceDust");
        }
    }

    public class SDWrapper
    {
        private ModuleAnimateGeneric aperature = null;
        ModuleSpaceDustTelescope scope = null;
        public void InitPartWrapper(Part p)
        {
            if (SDWrapperInit.HasSpaceDust)
                GetModuleSpaceDustTelescope(p);
            else
                aperature = p.GetComponent<ModuleAnimateGeneric>();
        }

        void GetModuleSpaceDustTelescope(Part p) { scope = p.GetComponent<ModuleSpaceDustTelescope>(); }
        public void SetAperature(ModuleAnimateGeneric mag) { aperature = mag; }

        public float animTime { get { return SDWrapperInit.HasSpaceDust ? GetScopeEnabled() : aperature.animTime; } }

        float GetScopeEnabled() { return scope.Enabled ? 1 : 0; }

        public void Toggle()
        {
            if (SDWrapperInit.HasSpaceDust)
                ToggleSpaceDustScope();
            else
                aperature.Toggle();
        }

        void ToggleSpaceDustScope()
        {
            if (scope.Enabled)
                scope.DisableTelescope();
            else
                scope.EnableTelescope();
        }
    }
}
