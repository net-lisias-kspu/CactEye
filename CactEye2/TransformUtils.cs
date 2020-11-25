using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if false
namespace CactEye2
{
    public struct TransformData
    {
        public Vector3 position;
        public Quaternion rotation;

        public Vector3 localPosition;
        public Vector3 localScale;
        public Quaternion localRotation;

        public Transform parent;
    }

    public static class TransformUtils
    {

        public static TransformData Clone(this Transform transform)
        {
            TransformData td = new TransformData();

            td.position = transform.position;
            td.localPosition = transform.localPosition;

            td.rotation = transform.rotation;
            td.localRotation = transform.localRotation;

            td.localScale = transform.localScale;

            td.parent = transform.parent;

            return td;
        }

    }
}
#endif