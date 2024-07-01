using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace EvroDev.LevelEditorTool.Tools
{
    public static class RaycastTools
    {
        public static float snapValue = 1;

        public static Vector3 GetNormalOfRaycast(Vector2 mousePosition)
        {
            // Convert mousePosition to world position
            // Remember to implement snapping logic here if needed
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100, LayerMask.GetMask(new string[] { "Default", "Static" }), QueryTriggerInteraction.Ignore))
            {
                return hit.normal;
            }
            // Fallback to a default plane if no object was hit
            return Vector3.zero; //GetPlaneIntersectionWithRay(mousePosition);

        }

        public static Vector3 GetWorldPosition(Vector2 mousePosition, bool shouldRound = true)
        {
            // Convert mousePosition to world position
            // Remember to implement snapping logic here if needed
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100, LayerMask.GetMask(new string[] { "Default", "StaticLW" }), QueryTriggerInteraction.Ignore))
            {
                // Snap to hit object or adjust based on your snapping logic
                if (shouldRound) return hit.point.RoundToNearest(snapValue);
                else return hit.point;
            }
            // Fallback to a default plane if no object was hit
            return Vector3.zero; //GetPlaneIntersectionWithRay(mousePosition);

        }

        public static Vector3 GetWorldPositionBoxCast(Bounds bounds, Vector2 mousePosition, bool shouldRound = true)
        {
            // Convert mousePosition to world position
            // Remember to implement snapping logic here if needed
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            if (Physics.BoxCast(ray.origin, bounds.extents, ray.direction, out RaycastHit hit, Quaternion.identity, 100, LayerMask.GetMask(new string[] { "Default", "StaticLW" }), QueryTriggerInteraction.Ignore))
            {
                // Snap to hit object or adjust based on your snapping logic
                if (shouldRound) return hit.point.RoundToNearest(snapValue);
                else return hit.point;
            }
            // Fallback to a default plane if no object was hit
            return Vector3.zero; //GetPlaneIntersectionWithRay(mousePosition);

        }

        public static Vector3 GetPlaneIntersectionWithRay(Vector2 mousePosition, Vector3 planeUp, Vector3 planePoint, bool round = true)
        {
            // Convert the mouse position to a ray
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            float enter = 0.0f;
            Plane plane = new(planeUp, planePoint);

            // Check if the ray hits the plane
            if (plane.Raycast(ray, out enter))
            {
                Vector3 hitPoint = round ? ray.GetPoint(enter).RoundToNearest(snapValue) : ray.GetPoint(enter);
                return hitPoint;
            }

            // Fallback, should not happen but just in case
            return planePoint;
        }

        public static Vector3 GetCardinalDirectionTowardsSceneViewCamera()
        {
            // Get the current Scene View camera
            Camera sceneViewCamera = SceneView.lastActiveSceneView.camera;

            if (sceneViewCamera == null)
            {
                Debug.LogWarning("Scene View Camera is null. Defaulting to Vector3.forward.");
                return Vector3.forward;
            }

            Vector3 cameraForward = sceneViewCamera.transform.forward;
            // Flatten the forward vector to ignore pitch.
            cameraForward.y = 0;
            cameraForward.Normalize();

            // Determine the angle in degrees between the forward vector and North (world's Z axis).
            float angle = Vector3.SignedAngle(Vector3.forward, cameraForward, Vector3.up);

            // Normalize the angle to be within 0-360 degrees.
            if (angle < 0) angle += 360;

            // Determine the closest cardinal direction.
            int cardinalIndex = Mathf.RoundToInt(angle / 90f) % 4; // Result is 0, 1, 2, or 3

            // Convert cardinal index to a direction vector.
            Vector3 cardinalDirection;
            switch (cardinalIndex)
            {
                case 0: // North
                    cardinalDirection = Vector3.forward;
                    break;
                case 1: // East
                    cardinalDirection = Vector3.right;
                    break;
                case 2: // South
                    cardinalDirection = Vector3.back;
                    break;
                case 3: // West
                    cardinalDirection = Vector3.left;
                    break;
                default:
                    cardinalDirection = Vector3.forward;
                    break;
            }

            return cardinalDirection;
        }

        public static Vector3 GetNearestPointOnLine(Vector2 mousePosition, Vector3 linePoint, Vector3 lineDirection, bool round = true)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

            Vector3 lineToPoint = ray.origin - linePoint;

            // Project this vector onto the line direction
            float t = Vector3.Dot(lineToPoint, lineDirection) / Vector3.Dot(lineDirection, lineDirection);

            // Calculate and return the closest point
            Vector3 closestPoint = linePoint + t * lineDirection;
            return round ? closestPoint.RoundToNearest(snapValue) : closestPoint;
        }

        public static RaycastFillOutput EvaluateRaycastFill(Vector3 point)
        {
            if (Physics.Raycast(point, Vector3.up, out var vertHit, 100, LayerMask.GetMask(new string[] { "Default", "Static" }), QueryTriggerInteraction.Ignore))
            {
                Vector3 newCenter = point + Vector3.up * vertHit.distance/2;

                float rightAmount = 0;
                float leftAmount = 0;
                float forwardAmount = 0;
                float backAmount = 0;

                if(Physics.Raycast(newCenter, Vector3.right, out var xHit, 100, LayerMask.GetMask(new string[] { "Default", "Static" }), QueryTriggerInteraction.Ignore))
                {
                    rightAmount = xHit.distance;
                }
                if (Physics.Raycast(newCenter, Vector3.left, out var lHit, 100, LayerMask.GetMask(new string[] { "Default", "Static" }), QueryTriggerInteraction.Ignore))
                {
                    leftAmount = lHit.distance;
                }
                if (Physics.Raycast(newCenter, Vector3.forward, out var fHit, 100, LayerMask.GetMask(new string[] { "Default", "Static" }), QueryTriggerInteraction.Ignore))
                {
                    forwardAmount = fHit.distance;
                }
                if (Physics.Raycast(newCenter, Vector3.back, out var bHit, 100, LayerMask.GetMask(new string[] { "Default", "Static" }), QueryTriggerInteraction.Ignore))
                {
                    backAmount = bHit.distance;
                }

                float xLen = rightAmount - leftAmount;
                float zLen = forwardAmount - backAmount;


                newCenter = newCenter + Vector3.forward * zLen / 2;
                newCenter = newCenter + Vector3.right * xLen / 2;

                return new RaycastFillOutput(newCenter, new Vector3(rightAmount + leftAmount, vertHit.distance, forwardAmount + backAmount));
            }
            else
            {
                return new RaycastFillOutput(point, Vector3.zero);
            }
        }
    }

    public struct RaycastFillOutput
    {
        public Vector3 center;
        public Vector3 bounds;

        public RaycastFillOutput(Vector3 center, Vector3 bounds)
        {
            this.center = center;
            this.bounds = bounds;
        }
    }
}
