using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using KSP.Localization;



namespace CactEye2
{
    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings
    // HighLogic.CurrentGame.Parameters.CustomParams<CactiSettings>().
    public class CactiSettings : GameParameters.CustomParameterNode
    {
        public override string Title { get { return ""; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "CactiOptics Refocused"; } }
        public override string DisplaySection { get { return "CactiOptics Refocused"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return false; } }

        //private bool DebugMode = false;
        //private bool SunDamage = false;
        //private bool GyroDecay = false;
        //private bool AsteroidSpawner = false;

        [GameParameters.CustomParameterUI("DebugMode",
            toolTip = "Enable some additional logging")]
        public bool DebugMode = false;

        [GameParameters.CustomParameterUI("SunDamage",
            toolTip = "Allow the sun to damage the optics")]
        public bool SunDamage = true;

        [GameParameters.CustomParameterUI("GyroDecay",
            toolTip = "")]
        public bool GyroDecay = true;

        [GameParameters.CustomParameterUI("AsteroidSpawner",
            toolTip = "Spawn Asteroids")]
        public bool AsteroidSpawner = true;

        [GameParameters.CustomIntParameterUI("Alarm advance time", minValue = 1, maxValue = 600,
            toolTip = "How far in advance of the beginning of the event should the alarm be set")]
        public int alarmTimeInAdvance = 300;


        public override void SetDifficultyPreset(GameParameters.Preset preset)
        { }

        public override bool Enabled(MemberInfo member, GameParameters parameters)
        { 
            if (member.Name == "alarmTimeInAdvance")
            {
                return KACWrapper.AssemblyExists;
            }
            return true; 
        }

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        { return true; }

        public override IList ValidValues(MemberInfo member)
        { return null; }
    }

}
