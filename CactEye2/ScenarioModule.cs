using System;
using static CactEye2.InitialSetup;

#if true
namespace CactEye2
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.FLIGHT, GameScenes.TRACKSTATION, GameScenes.SPACECENTER)]
    class Scenario : ScenarioModule
    {
        public override void OnLoad(ConfigNode parentNode)
        {
            Log.Info("ScenarioModule.OnLoad");
            base.OnLoad(parentNode);
            ConfigNode node = parentNode.GetNode("CactEyeOptics");
            if (node != null)
            {
                ConfigNode[] nodes = node.GetNodes("OccultationRecord");

                foreach (var n in nodes)
                {
                    CelestialBody referencebody = null;
                    CelestialBody body = null; 

                    double time = n.SafeLoad("time", 0d);
                    string type = n.SafeLoad("type", "");
                    int i = n.SafeLoad("occType", 0);
                    CactEyeOccultationProcessor.OccultationTypes occType = (CactEyeOccultationProcessor.OccultationTypes)i;

                    string refBodyName = n.SafeLoad("referencebody", "");
                    if (refBodyName != "")
                        referencebody = FlightGlobals.Bodies.Find(b => b.bodyName == refBodyName);

                    string bodyName = n.SafeLoad("body", "");
                    if (bodyName != "")
                        body = FlightGlobals.Bodies.Find(b => b.bodyName == bodyName);
                    
                    Guid id = n.SafeLoad("vessel", Guid.Empty);
                    Vessel vessel = FlightGlobals.fetch.vessels.Find(v => v.protoVessel.vesselID == id);
                    Guid id1 = n.SafeLoad("assignedToVessel", Guid.Empty);
                    Vessel assignedToVessel = FlightGlobals.fetch.vessels.Find(v => v.protoVessel.vesselID == id1);
                    if (id == Guid.Empty || id1 == Guid.Empty) continue;
                    OccultationRecord oc = new OccultationRecord(time, type, occType, referencebody, body, vessel, assignedToVessel);
                }
            }
        }


        public override void OnSave(ConfigNode parentNode)
        {
            Log.Info("ScenarioModule.OnSave");
            ConfigNode node = new ConfigNode("CactEyeOptics");
            foreach (var a in CactEyeAPI.occExpAstCometDict.Values)
            {
                ConfigNode n = new ConfigNode("OccultationRecord");
                n.AddValue("time", a.time);
                n.AddValue("type", a.type);
                n.AddValue("occType", (int)a.occType);
                if (a.referencebody != null)
                    n.AddValue("referencebody", a.referencebody.bodyName);
                if (a.body != null)
                    n.AddValue("body", a.body.bodyName);
                if (a.vessel != null)
                    n.AddValue("vessel", a.vessel.protoVessel.vesselID);
                //if (a.assignedToVessel != null)
                //    n.AddValue("assignedToVessel", a.assignedToVessel.protoVessel.vesselID);
                if (a.assignedToGuid != Guid.Empty)
                    n.AddValue("assignedToGuid", a.assignedToGuid);
                node.AddNode(n);
            }
            parentNode.AddNode(node);
            base.OnSave(parentNode);

        }
    }
}
#endif