using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RouteMaker))]
public class RouteMakerEditor : Editor
{
    private RouteMaker.RoutePoint selectedPoint;
    private RouteMaker.Route selectedRoute;
    private int selectedRouteIndex = -1;

    private RouteMaker routeMaker;
    private RaycastHit mouseHit;

    private List<bool> routesFoldout = new List<bool>();
    private SerializedProperty routes;

    private void OnEnable()
    {
        routeMaker = target as RouteMaker;
        routes = serializedObject.FindProperty("routes");

        for (int i = 0; i < routes.arraySize; i++)
            routesFoldout.Add(false);

        if(selectedRouteIndex != -1)
            selectedRoute = routeMaker.routes[selectedRouteIndex];

        SceneView.RepaintAll();
    }

    public override void OnInspectorGUI()
    {
        GUILayout.Label("Settings", EditorStyles.centeredGreyMiniLabel);

        DrawDefaultInspector();

        GUILayout.Space(10);

        GUILayout.Label("Route Making", EditorStyles.centeredGreyMiniLabel);

        if (GUILayout.Button("New Route"))
        {
            RouteMaker.Route newRoute = new RouteMaker.Route();
            routeMaker.routes.Add(newRoute);
            routesFoldout.Add(false);
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

            if (GUILayout.Button("Select"))
            {
                selectedRoute = routeMaker.routes[i];
                selectedRouteIndex = i;
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Remove"))
            {
                if(selectedRoute == routeMaker.routes[i])
                {
                    selectedRoute = null;
                    selectedRouteIndex = -1;
                }

                routeMaker.routes.RemoveAt(i);
                routesFoldout.RemoveAt(i);
                SceneView.RepaintAll();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    public void OnSceneGUI()
    {
        Draw();
    }

    public void Draw()
    {
        Handles.color = Color.white;
        Handles.DrawWireDisc(mouseHit.point, mouseHit.normal, .5f);

        if (selectedRoute == null)
            return;

        //Disable selection
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        if (selectedRoute.anchors.Count > 0)
        {
            if (selectedPoint != null)
                OnEdit();

            DrawBezierCurve();
        }

        Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        if (!Physics.Raycast(worldRay, out mouseHit, 10000, routeMaker.wallLayer))
            return;

        if (Event.current.type == EventType.MouseDown)
            selectedPoint = getClosestPointTo(mouseHit.point);

        if (Event.current.keyCode == KeyCode.C && Event.current.type == EventType.KeyUp)
            OnAdd();
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

    public void DrawBezierCurve()
    {
        List<RouteMaker.RoutePoint> points = selectedRoute.anchors;

        for (int i = 0; i < points.Count-2; i++)
        {
            Vector3 anchor0 = points[i].position + points[i].normal * routeMaker.distanceFromTheWall;
            Vector3 anchor1 = points[i+1].position + points[i + 1].normal * routeMaker.distanceFromTheWall;
            Vector3 anchor2 = points[i+2].position + points[i + 2].normal * routeMaker.distanceFromTheWall;

            Vector3 diff0 = (anchor0 - anchor1).normalized;
            Vector3 diff1 = (anchor2 - anchor1).normalized;
            Vector3 dir1 = (diff0 - diff1).normalized;
            float dist = (anchor0 - anchor1).magnitude;

            Vector3 control0 = anchor0 - points[i].dirToPrevius * dist/2.0f;
            Vector3 control1 = anchor1 + dir1 * dist/2.0f;

            points[i + 1].dirToPrevius = dir1;

            Handles.DrawBezier(anchor0, anchor1, control0, control1, Color.yellow, null, 3.0f);
        }
    }

    public RouteMaker.RoutePoint getClosestPointTo(Vector3 p)
    {
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