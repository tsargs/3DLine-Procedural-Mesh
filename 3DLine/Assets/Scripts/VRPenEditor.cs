using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VRPen))]
public class VRPenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();

        VRPen vrPen = (VRPen)target;

        EditorGUILayout.LabelField("---------------------------");

        EditorGUILayout.LabelField("Line Thickness:");

        vrPen.lineThickness = EditorGUILayout.Slider(vrPen.lineThickness, 0.005f, 0.007f);

        EditorGUILayout.LabelField("---------------------------");

        EditorGUILayout.LabelField("Vertical Segments:");

        vrPen.meshDetail_X = EditorGUILayout.IntSlider(vrPen.meshDetail_X, 10, 24);

        EditorGUILayout.LabelField("---------------------------");

        vrPen.newPrefabName = EditorGUILayout.TextField("File Name", vrPen.newPrefabName);

        if (GUILayout.Button("Save Drawing"))
        {
            vrPen.SaveLines();
        }

        EditorGUILayout.LabelField("---------------------------");

        vrPen.loadPrefabName = EditorGUILayout.TextField("File Name", vrPen.loadPrefabName);

        if (GUILayout.Button("Load Drawing"))
        {
            vrPen.LoadDrawingPrefab();
        }
    }
}
