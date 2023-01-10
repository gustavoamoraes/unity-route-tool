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
        public Vector3 dirToPrevius;
    }

    [Serializable]
    public class Route
    {
        public List<RoutePoint> anchors = new List<RoutePoint>();
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
            }
        }
#endif
    }
}
