using System.Diagnostics;
using UnityEngine;

public class NodeManager
{
    private NodeBehaviour[] _allNodes;
    public NodeBehaviour[] allNodes
    {
        get
        {
            if(_allNodes == null)
                _allNodes = nodeParent.GetComponentsInChildren<NodeBehaviour>();
            return _allNodes;
        }
        set
        {
            _allNodes = value;
        }
    }

    private static Transform _nodeParent;
    public static Transform nodeParent { get
        {
            if (_nodeParent == null)
                _nodeParent = GameObject.FindGameObjectWithTag("NodeManager").transform;

            return _nodeParent;
        }
    }

    // Keep track of the time actions take and send them to the editor
    Stopwatch timer = new Stopwatch();

    #region NetworkOptions

    public void SetupNetwork(bool hardReset = false)
    {
        // Check if the user really wants to do this
        if (hardReset && !UnityEditor.EditorUtility.DisplayDialog("Hard reset", "You are about to reset every node, regardless whether they are set to reset in their options", "Hard Reset", "I made a mistake"))
            return;

        timer.Start();

        GetAllNodes();

        foreach (NodeBehaviour node in allNodes)
        {
            if (node.endNode) // End nodes should not connect with any other node
                continue;

            if (hardReset) // Hard reset the curves nodes and curves
                node.resetCurves = hardReset;

            ConnectToOtherNode(node);

            if (!node.InitNode()) // Init the node after it has been connected and check if it connected
                return;

            node.resetCurves = false;
        }

        FinalizeConnecting(hardReset ? "Hard Reset" : "Connect Network", (int)timer.ElapsedMilliseconds);
    }

    public void BakeCurves()
    {
        timer.Start();

        foreach (NodeBehaviour node in allNodes)
        {
            if (!node.endNode)
                if (!node.CreateCurvePoints())
                    return;
        }

        FinalizeConnecting("Bake Curves", (int)timer.ElapsedMilliseconds);
    }

    #endregion

    #region Tools

    private void GetAllNodes()
    {
        allNodes = null;

        int index = 0;
        foreach (NodeBehaviour node in allNodes)
        {
            index++;
            node.name = "RailNode: " + index;

            node.nextNode = null;
            node.prevNode = null;

            node.SetType();
        }
    }

    private NodeBehaviour ConnectToOtherNode(NodeBehaviour currentNode)
    {
        float closestDistance = float.MaxValue;
        NodeBehaviour candidate = null;

        foreach (NodeBehaviour otherNode in allNodes)
        {
            if (otherNode.prevNode || otherNode.startNode)
                continue;
            if (currentNode.prevNode == otherNode)
                continue;

            if (currentNode != otherNode)
            {
                float distance = Vector3.Distance(currentNode.transform.position, otherNode.transform.position);

                if (distance < closestDistance)
                {
                    candidate = otherNode;
                    closestDistance = distance;
                }
            }
        }

        currentNode.nextNode = candidate;
        candidate.prevNode = currentNode;

        return candidate;
    }

    private void FinalizeConnecting(string lastAction, int ExecutionTime)
    {
        DataTools.SetAllDataPoint();

        timer.Stop();
        NodeEditor.settings.nodeInfo = new GeneratedNodeSystem(lastAction, ExecutionTime, allNodes.Length, DataTools.curvePoints.Count, DataTools.totalLength);
        timer.Reset();

        NodeEditor.editor.RecieveMessage("Generation was succesful!", WarningStatus.None);
    }

    public Vector3 CalculateCurvePoint(float timeAlongCurve, Vector3 P0, Vector3 P1, Vector3 P2, Vector3 P3)
    {
        float t = timeAlongCurve;
        float tInv = 1 - t;
        Vector3 point = Vector3.zero;

        // (1-t)^3 * P0 +
        // 3 * (1-t)^2 * t * P1 +
        // 3 * (1-t) * t^2 * P2 +
        // t^3 * P3
        point += Mathf.Pow(tInv, 3) * P0;
        point += 3 * Mathf.Pow(tInv, 2) * t * P1;
        point += 3 * tInv * (t * t) * P2;
        point += (t * t * t) * P3;

        return point;
    }

    public Vector3 CalculateDirectionOnCurve(float timeAlongCurve, Vector3 P0, Vector3 P1, Vector3 P2, Vector3 P3)
    {
        float t = timeAlongCurve;
        float tInv = 1 - t; // Inverse t
        Vector3 direction = Vector3.zero;

        // 3 * (1-t)^2 * (P1 - P0) +
        // 6 * (1-t) * t * (P2 - P1) +
        // 3t^2 * (P3 - P2)
        direction += 3 * Mathf.Pow(tInv, 2) * (P1 - P0);
        direction += 6 * tInv * t * (P2 - P1);
        direction += 3 * Mathf.Pow(t, 2) * (P3 - P2);

        return direction.normalized;
    }

    #endregion
}
