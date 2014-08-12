using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UnityMadeAwesome.BlenderInUnity
{
    public class Util
    {
        private const int CAMERA_NEAR_PLANE = 4;
        private static Color DEFAULT_FILL = new Color(1, 1, 1, 0.2f);
        private static Color DEFAULT_BORDER = new Color(0, 0, 0, 1);
        private const float DEFAULT_DISTANCE = 50;

        public static void DrawPlane(Plane plane)
        {
            DrawPlane(plane, DEFAULT_FILL, DEFAULT_BORDER);
        }

        public static void DrawPlane(Plane plane, Color faceColor, Color outlineColor, float distance = DEFAULT_DISTANCE)
        {
            // We have to get the four vertices from the plane. Grab the normal.
            Vector3 planeNormal = plane.normal;
            Vector3 vecOnPlane;

            Vector3[] vecs = new Vector3[4];

            if (planeNormal != Vector3.forward)
            {
                vecOnPlane = Vector3.Cross(planeNormal, Vector3.forward).normalized * distance;
            }
            else
            {
                vecOnPlane = Vector3.Cross(planeNormal, Vector3.up).normalized * distance;
            }

            Quaternion rotation = Quaternion.AngleAxis(90, planeNormal);

            vecs[0] = planeNormal * plane.distance + vecOnPlane;

            vecOnPlane = rotation * vecOnPlane;
            vecs[1] = planeNormal * plane.distance + vecOnPlane;

            vecOnPlane = rotation * vecOnPlane;
            vecs[2] = planeNormal * plane.distance + vecOnPlane;

            vecOnPlane = rotation * vecOnPlane;
            vecs[3] = planeNormal * plane.distance + vecOnPlane;

            Handles.DrawSolidRectangleWithOutline(vecs, faceColor, outlineColor);
        }

        public static Vector3 GetCamViewPlaneNormal(Camera cam)
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);

            return planes[CAMERA_NEAR_PLANE].normal;
        }
    }
}