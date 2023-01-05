using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RouteMaker))]
public class RouteMakerEditor : Editor
{
    private RouteMaker.RoutePoint selectedPoint;
    private RouteMaker.Route selectedRoute;

    private RouteMaker routeMaker;
    private RaycastHit mouseHit;
    bool hit;

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

        selectedRoute = null;
        selectedPoint = null;
    }

    public override void OnInspectorGUI()
    {
        GUILayout.Label("Settings", EditorStyles.centeredGreyMiniLabel);

        DrawDefaultInspector();

        GUILayout.Space(20);

        GUILayout.Label("Route Making", EditorStyles.centeredGreyMiniLabel);

        if (GUILayout.Button("New Route"))
        {
            RouteMaker.Route newRoute = new RouteMaker.Route();
            routeMaker.routes.Add(newRoute);
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

            if (GUILayout.Button("Remove"))
            {
                if (selectedRoute == routeMaker.routes[i])
                {
                    selectedRoute = null;
                    selectedPoint = null;
                }

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
        Draw();
    }

    public void Draw()
    {
        if (routeMaker.routes.Count == 0)
            return;

        //Disable selection
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        hit = Physics.Raycast(worldRay, out mouseHit, 10000, routeMaker.wallLayer);

        if (selectedRoute == null)
            return;

        if (selectedRoute.anchors.Count > 0)
            OnEdit();

        if (Event.current.keyCode == KeyCode.C && Event.current.type == EventType.KeyUp)
            OnAdd();

        DrawBezierCurve();

        Handles.color = UnityEngine.Color.white;
        Handles.DrawWireDisc(mouseHit.point, mouseHit.normal, .5f);
    }

    public void OnAdd ()
    {
        if (!hit)
            return;

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
        if (Event.current.type == EventType.MouseDown && hit )
        {
            selectedPoint = getClosestPointTo(mouseHit.point);
        }
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