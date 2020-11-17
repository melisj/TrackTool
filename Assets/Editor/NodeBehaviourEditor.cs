using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NodeBehaviour))]
[CanEditMultipleObjects]
public class NodeBehaviourEditor : Editor
{
    SerializedProperty nextNodeProp;
    SerializedProperty prevNodeProp;
    SerializedProperty nodeTypeProp;
    SerializedProperty resetCurvesProp;
    SerializedProperty createMeshForNodeProp;

    GUIContent nextNodeLabel = new GUIContent("Next node");
    GUIContent prevNodeLabel = new GUIContent("Prev node");
    GUIContent nodeTypeLabel = new GUIContent("Node type");
    GUIContent resetCurvesLabel = new GUIContent("Reset curve");

    public void OnEnable() {
        nextNodeProp = serializedObject.FindProperty("nextNode");
        prevNodeProp = serializedObject.FindProperty("prevNode");
        nodeTypeProp = serializedObject.FindProperty("thisNodeType");
        resetCurvesProp = serializedObject.FindProperty("resetCurves");
        createMeshForNodeProp = serializedObject.FindProperty("createMeshForNode");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        //EditorGUI.BeginChangeCheck();
        GUILayout.BeginVertical(GUIStyleManager.style.basicStyle);

        GUILayout.Label("References (non adjustable)", GUIStyleManager.style.titleStyle);

        EditorGUILayout.PropertyField(nextNodeProp, nextNodeLabel);
        EditorGUILayout.PropertyField(prevNodeProp, prevNodeLabel);

        GUILayout.Space(10);
        GUILayout.Label("Node options", GUIStyleManager.style.titleStyle);

        EditorGUILayout.PropertyField(nodeTypeProp, nodeTypeLabel);
        EditorGUILayout.PropertyField(resetCurvesProp, resetCurvesLabel);

        GUILayout.Space(10);
        GUILayout.Label("Mesh options", GUIStyleManager.style.titleStyle);

        for (int i = 0; i < createMeshForNodeProp.arraySize; i++) { 
            if(DataTools.MeshSetting.container[i].createMesh)
                EditorGUILayout.PropertyField(createMeshForNodeProp.GetArrayElementAtIndex(i), new GUIContent(DataTools.MeshSetting.container[i].usedMesh.name));
        }

        if (resetCurvesProp.boolValue)
            GUIStyleManager.style.DrawWarning("Curve of this node will be reset when the next reconnect will be made");

        GUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
}
