using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.GraphicsBuffer;

[CustomEditor(typeof(RouteMaker))]
public class RouteMakerEditor : Editor
{
    private RouteMaker.RoutePoint selectedPoint;
    private RouteMaker.Route selectedRoute;

    private RouteMaker routeMaker;
    private RaycastHit mouseHit;

    private List<bool> routesFoldout = new List<bool>();
    private SerializedProperty routes;

    private void OnEnable()
    {
        routeMaker = target as RouteMaker;

        routes = serializedObject.FindProperty("routes");

        for (int i = 0; i < routes.arraySize; i++)
        {
            routesFoldout.Add(false);
        }
    }

    public override void OnInspectorGUI()
    {
        GUILayout.Label("Settings", EditorStyles.centeredGreyMiniLabel);

        DrawDefaultInspector();

        GUILayout.Space(20);

        GUILayout.Label("Route Making", EditorStyles.centeredGreyMiniLabel);

        if (GUILayout.Button("New Route"))
        {
            routeMaker.routes.Add(new RouteMaker.Route());
        }

        serializedObject.Update();

        for (int i = 0; i < routes.arraySize; i++)
        {
            if (routeMaker.routes.Count == 0)
                return;

            var route = routes.GetArrayElementAtIndex(i);
            var routeName = route.FindPropertyRelative("name");

            routesFoldout[i] = EditorGUILayout.Foldout(routesFoldout[i], new GUIContent(routeName.stringValue));

            if (!routesFoldout[i])
                continue;

            EditorGUILayout.PropertyField(routeName);

            if(GUILayout.Button("Remove"))
            {
                routeMaker.routes.RemoveAt(i);
            }

            if (GUILayout.Button("Edit"))
            {
                selectedRoute = routeMaker.routes[i];
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    public void OnSceneGUI()
    {
        if (routeMaker.routes.Count == 0)
            return;

        //Disable selection
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        if (!Physics.Raycast(worldRay, out mouseHit, 10000, routeMaker.wallLayer))
            return;

        if (selectedRoute.anchors.Count > 0)
            OnEdit();

        if (Event.current.keyCode == KeyCode.C && Event.current.type == EventType.KeyUp)
            OnAdd();

        Handles.color = Color.blue;
        Handles.DrawWireDisc(mouseHit.point, mouseHit.normal, 1.0f);
    }

    public void OnAdd ()
    {
        RouteMaker.RoutePoint newRoutePoint = new RouteMaker.RoutePoint();

        newRoutePoint.position = mouseHit.point;
        newRoutePoint.normal = mouseHit.normal;
        selectedRoute.anchors.Add(newRoutePoint);
    }

    public void OnEdit ()
    {
        if (selectedPoint != null)
        {
            Vector3 change = Handles.PositionHandle(selectedPoint.position, Quaternion.LookRotation(selectedPoint.normal));

            Vector2 screenPos = HandleUtility.WorldToGUIPoint(change);
            Ray rayTest = HandleUtility.GUIPointToWorldRay(screenPos);
            RaycastHit hitTest;

            if (Physics.Raycast(rayTest, out hitTest, 10000, routeMaker.wallLayer))
            {
                selectedPoint.position = hitTest.point;
                selectedPoint.normal = hitTest.normal;
            }

            Handles.color = Color.red;
            Handles.DrawWireDisc(selectedPoint.position, selectedPoint.normal, 0.5f);
        }

        //Set new selected point
        if (Event.current.type == EventType.MouseDown)
        {
            selectedPoint = getClosestPointTo(mouseHit.point);
        }
    }

    public RouteMaker.RoutePoint getClosestPointTo(Vector3 p)
    {
        var t = (target as RouteMaker);

        RouteMaker.RoutePoint closestPoint = null;
        float smallestDistance = float.MaxValue;

        foreach (RouteMaker.RoutePoint point in selectedRoute.anchors)
        {
            float dist = Vector3.Distance(p,point.position);

            if (dist < smallestDistance)
            {
                smallestDistance = dist;
                closestPoint = point;
            }
        }

        return closestPoint;
    }
}