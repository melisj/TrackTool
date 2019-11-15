using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NodeBehaviour))]
public class NodeBehaviourEditor : Editor
{
    public override void OnInspectorGUI()
    {
        NodeBehaviour node = target as NodeBehaviour;

        GUILayout.BeginVertical(GUIStyleManager.style.basicStyle);

        GUILayout.Label("References (non adjustable)", GUIStyleManager.style.titleStyle);

        GUIStyleManager.style.DrawObjectField("Next node", ref node.nextNode, false);
        GUIStyleManager.style.DrawObjectField("Previous node", ref node.prevNode, false);

        GUILayout.Space(10);
        GUILayout.Label("Node options", GUIStyleManager.style.titleStyle);

        GUIStyleManager.style.DrawToggle("Start node", ref node.startNode);
        GUIStyleManager.style.DrawToggle("End node", ref node.endNode);
        GUIStyleManager.style.DrawToggle("Reset curve", ref node.resetCurves);

        if (node.resetCurves)
            GUIStyleManager.style.DrawWarning("Curve of this node will be reset when the next reconnect will be made");

        GUILayout.EndVertical();
    }
}
