using Assets.g3UnityUtils;
using g3;
using UnityEngine;

namespace Assets
{
    public static class DebugGizmos
    {
        public static void ArrowForGizmo(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Gizmos.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
        }

        public static void ArrowForGizmo(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Gizmos.color = color;
            Gizmos.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
        }

        public static void ArrowForDebug(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Debug.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Debug.DrawRay(pos + direction, right * arrowHeadLength);
            Debug.DrawRay(pos + direction, left * arrowHeadLength);
        }
        public static void ArrowForDebug(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Debug.DrawRay(pos, direction, color);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Debug.DrawRay(pos + direction, right * arrowHeadLength, color);
            Debug.DrawRay(pos + direction, left * arrowHeadLength, color);
        }
        public static void LineForDebug(Vector3 pos, Vector3 end, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            arrowHeadLength = (end - pos).magnitude * arrowHeadLength;
            Debug.DrawLine(pos, end, color, 5, false);

            Vector3 right = Quaternion.LookRotation(end-pos) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(end-pos) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Debug.DrawLine(end, end +right * arrowHeadLength, color, 5, false);
            Debug.DrawLine(end, end+left * arrowHeadLength, color, 5, false);
        }

        public static void DrawBoundingBox(AxisAlignedBox3d bounds, Transform transform)
        {
            var min = bounds.Min.toVector3();
            var max = bounds.Max.toVector3();
            var bottomRightBack = new Vector3(max.x, min.y,min.z);
            var bottomRightFront = new Vector3(max.x, max.y, min.z);
            var bottomLeftFront = new Vector3(min.x, max.y, min.z);
            var topRightBack = new Vector3(max.x, min.y, max.z);
            var topLeftBack = new Vector3(min.x, min.y, max.z);
            var topLeftFront = new Vector3(min.x, max.y, max.z);
            DrawLine(min, topLeftBack, transform);
            DrawLine(min, bottomLeftFront, transform);
            DrawLine(min, bottomRightBack, transform);
            DrawLine(max, bottomRightFront, transform);
            DrawLine(max, topLeftFront, transform);
            DrawLine(max, topRightBack, transform);
            DrawLine(topLeftBack, topRightBack, transform);
            DrawLine(bottomLeftFront, bottomRightFront, transform);
            DrawLine(topLeftFront, bottomLeftFront, transform);
            DrawLine(topRightBack, bottomRightBack, transform);
            DrawLine(topLeftBack, topLeftFront, transform);
            DrawLine(bottomRightBack, bottomRightFront, transform);
        }

        private static void DrawLine(Vector3 start, Vector3 end, Transform transform)
        {
            Debug.DrawLine(transform.TransformPoint(start), transform.TransformPoint(end), Color.red, 5, false);
        }
    }
}