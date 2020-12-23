using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CactEye2
{
    internal class BodyInfo
    {
        internal double bodyDist;
        internal double bodySize;
        internal Vector3d bodyAngle;

        internal BodyInfo(double bodyDist, double bodySize, Vector3d bodyAngle)
        {
            this.bodyDist = bodyDist;
            this.bodySize = bodySize;
            this.bodyAngle = bodyAngle;
        }
    }

}
