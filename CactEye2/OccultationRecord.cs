using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CactEye2.InitialSetup;


namespace CactEye2
{
    internal class OccultationRecord
    {
        internal double time;
        internal string type;
        internal CactEyeOccultationProcessor.OccultationTypes occType;
        internal CelestialBody referencebody;
        internal CelestialBody body;
        internal Vessel vessel;
        internal Guid assignedToGuid;
        //internal Vessel assignedToVessel;

        internal Guid AssignTo { set { assignedToGuid = value; } }
        //internal Vessel Assignto { set { assignedToVessel = value; } }
        internal Guid AssignedTo { get { return assignedToGuid; } }
        internal OccultationRecord(CelestialBody refBody, double time, string type, Vessel vessel)
        {
            Log.Info("New OccultationRecord, refBody: " + refBody.bodyName + ", time: " + time + ", type: " + type + ", vessel: " + vessel.name);
            this.body = null;
            this.referencebody = refBody;
            this.time = time;
            this.type = type;
            occType = CactEyeOccultationProcessor.OccTypeStringToEnum(type);
            this.vessel = vessel;
            //assignedToVessel = null;
            assignedToGuid = Guid.Empty;
        }
        internal OccultationRecord(CelestialBody refBody, double time, string type, CelestialBody body)
        {
            Log.Info("New OccultationRecord, refBody: " + refBody.bodyName + ", time: " + time + ", type: " + type + ", CelestialBody: " + body.bodyName);
            this.body = body;
            this.referencebody = refBody;
            this.time = time;
            this.type = type;
            occType = CactEyeOccultationProcessor.OccTypeStringToEnum(type);
            this.vessel = null;
            //assignedToVessel = null;
            assignedToGuid = Guid.Empty;
        }

        internal OccultationRecord(double time, string type, CactEyeOccultationProcessor.OccultationTypes occType,
            CelestialBody referenceBody, CelestialBody body, Vessel vessel, Vessel assignedToVessel)
        {
            string refBody = "n/a";
            if (referenceBody != null)
                refBody = referenceBody.bodyName;

            string bodyname = "n/a";
            if (body != null)
                bodyname = body.bodyName;
            string vname = "n/a";
            if (vessel != null)
                vname = vessel.name;
            string avname = "n/a";
            if (assignedToVessel != null)
                avname = assignedToVessel.name;
            Log.Info("ScenarioLoad OccultationRecord: " + time + ", type: " + type +
                ", occType: " + occType + ", referenceBody: " + refBody + ", body: " + bodyname +
                ", vessel: " + vname + ", assignedToVessel: " + avname);

            this.time = time;
            this.type = type;
            this.occType = occType;
            this.referencebody = referenceBody;
            this.body = body;
            this.vessel = vessel;
            //this.assignedToVessel = assignedToVessel;
            this.assignedToGuid = assignedToVessel.protoVessel.vesselID;
        }

    }
}
