using UnityEngine;

[System.Serializable]
public class CurveAdjustmentNode : MonoBehaviour
{
    [SerializeField] private NodeBehaviour parentNode;

    private void OnDrawGizmos()
    {
        if (NodeEditor.settings != null)
            if (NodeEditor.settings.renderCurveAdjustmentNodes)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(transform.position, NodeEditor.settings.adjustmentNodeSize);

                if (parentNode)
                    Gizmos.DrawLine(transform.position, parentNode.transform.position);
            }
    }

    public void AssignParentNode(NodeBehaviour parent)
    {
        parentNode = parent;
    }
}
