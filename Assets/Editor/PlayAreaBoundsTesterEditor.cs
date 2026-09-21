#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayAreaBoundsTester))]
public class PlayAreaBoundsTesterEditor : Editor
{
    private void OnSceneGUI()
    {
        PlayAreaBoundsTester tester = (PlayAreaBoundsTester)target;
        float z = tester.transform.position.z;

        Vector3 minHandlePos = new Vector3(tester.boundsMin.x, tester.boundsMin.y, z);
        Vector3 maxHandlePos = new Vector3(tester.boundsMax.x, tester.boundsMax.y, z);

        float handleSize = HandleUtility.GetHandleSize(minHandlePos) * 0.12f;

        EditorGUI.BeginChangeCheck();

        Handles.color = Color.magenta;
        Vector3 newMin = Handles.FreeMoveHandle(minHandlePos, handleSize, Vector3.zero, Handles.SphereHandleCap);
        Vector3 newMax = Handles.FreeMoveHandle(maxHandlePos, handleSize, Vector3.zero, Handles.SphereHandleCap);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(tester, "Ajustar Play Area Bounds");
            tester.boundsMin = new Vector2(newMin.x, newMin.y);
            tester.boundsMax = new Vector2(newMax.x, newMax.y);
            EditorUtility.SetDirty(tester);
        }

        Handles.Label(minHandlePos, $"Min ({tester.boundsMin.x:F2}, {tester.boundsMin.y:F2})");
        Handles.Label(maxHandlePos, $"Max ({tester.boundsMax.x:F2}, {tester.boundsMax.y:F2})");
    }
}
#endif