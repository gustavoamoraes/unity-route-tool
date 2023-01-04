using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

[ExecuteInEditMode]
public class RouteMaker : MonoBehaviour
{
    [Serializable]
    public class RoutePoint
    {
        public Vector3 position;
        public Vector3 normal;
    }

    [Serializable]
    public class Route
    {
        public List<RoutePoint> anchors;
        public string name;
    }

    [HideInInspector]
    public List<Route> routes = new List<Route>();

    public LayerMask wallLayer;
    public float distanceFromTheWall;

    void OnDrawGizmos()
    {
#if UNITY_EDITOR

        foreach(Route route in routes)
        {
            for (int i = 0; i < route.anchors.Count; i++)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(route.anchors[i].position, 0.1f);

                if (i == route.anchors.Count - 1)
                    continue;

                Gizmos.color = Color.yellow;

                Vector3 offsetStart = (route.anchors[i].normal * distanceFromTheWall);
                Vector3 offsetEnd = (route.anchors[i+1].normal * distanceFromTheWall);

                Gizmos.DrawLine(route.anchors[i].position + offsetStart, route.anchors[i + 1].position + offsetEnd);
            }
        }
#endif
    }
}
