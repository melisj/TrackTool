using UnityEngine;

[System.Serializable]
public class CurveAdjustmentNode : MonoBehaviour
{
    public NodeBehaviour parentNode;

    private void OnDrawGizmos()
    {
        if (DataTools.NodeSetting != null)
            if (DataTools.NodeSetting.renderCurveAdjustmentNodes)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(transform.position, DataTools.NodeSetting.adjustmentNodeSize);

                if (parentNode)
                    Gizmos.DrawLine(transform.position, parentNode.transform.position);
            }
    }

    public void AssignParentNode(NodeBehaviour parent)
    {
        parentNode = parent;
    }
}
