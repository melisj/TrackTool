using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class NodeManager
{
    // Keep track of the time actions take and send them to the editor
    Stopwatch timer = new Stopwatch();

    #region NetworkOptions

    public void SetupNetwork(bool hardReset = false, bool skipDialogue = false)
    {
        // Check if the user really wants to do this
        if(!skipDialogue)
            if (hardReset && !UnityEditor.EditorUtility.DisplayDialog("Hard reset", "You are about to reset every node, regardless whether they are set to reset in their options", "Hard Reset", "I made a mistake"))
                return;

        timer.Start();

        if (!GetAllNodes())
            return;

        List<NodeBehaviour> availableNodes = DataTools.allNodes.ToList();
        NodeBehaviour currentNode = availableNodes[0];
        NodeBehaviour nextNode = null;

        //currentNode.connected = true; // First one is the starting node and should not be connected with again.

        int time = 0;
        int error = 100;
        while(availableNodes.Count > 0)
        {
            if (time >= error)
            {
                NodeEditor.editor.RecieveMessage("conecting failed", WarningStatus.Error);
                return;
            }
            time++;

            // End nodes should not connect with any other node
            if (currentNode.endNode) 
                break;

            // Hard reset the curves nodes and curves
            if (hardReset) 
                currentNode.resetCurves = hardReset;

            // Connect to another node that is the closest to this one
            nextNode = ConnectToOtherNode(currentNode);
            if (!nextNode)
                return;

            // Init the node after it has been connected and check if it connected
            if (!currentNode.InitNode()) 
                return;

            // Set the new current node to be checked
            currentNode = nextNode;
            availableNodes.Remove(nextNode);
        }

        // Set the last node again when the tracks loops around
        if (!currentNode.endNode)
        {
            if (hardReset)
                currentNode.resetCurves = hardReset;
            currentNode.InitNode();
        }

        FinalizeConnecting(hardReset ? "Hard Reset" : "Connect Network", (int)timer.ElapsedMilliseconds);
    }

    public void BakeCurves()
    {
        timer.Start();

        foreach (NodeBehaviour node in DataTools.allNodes)
        {
            if (!node.endNode)
                if (!node.CreateCurvePoints())
                    return;
        }

        if (DataTools.allNodes.Length < 3)
        {
            NodeEditor.editor.RecieveMessage("There are no available nodes in the node manager, please assign at least three in the node parent", WarningStatus.Error);
            return;
        }

        FinalizeConnecting("Bake Curves", (int)timer.ElapsedMilliseconds);
    }

    #endregion

    #region Tools

    private bool GetAllNodes()
    {
        DataTools.allNodes = null;

        // Send error when there are not enough nodes available
        if (DataTools.allNodes.Length < 3)
        {
            NodeEditor.editor.RecieveMessage("There are no available nodes in the node manager, please assign at least three in the node parent", WarningStatus.Error);
            return false;
        }

        // Set all nodes
        int index = 0;
        foreach (NodeBehaviour node in DataTools.allNodes)
        {
            index++;
            node.name = "RailNode: " + index;

            node.nextNode = null;
            node.prevNode = null;
            node.connected = false;

            node.SetType();
        }

        return true;
    }

    private NodeBehaviour ConnectToOtherNode(NodeBehaviour currentNode)
    {
        float closestDistance = float.MaxValue;
        NodeBehaviour candidate = null;

        foreach (NodeBehaviour otherNode in DataTools.allNodes)
        {
            if (otherNode.connected)
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
        candidate.connected = true;

        return candidate;
    }

    private void FinalizeConnecting(string lastAction, int ExecutionTime)
    {
        // Save all the points on the curve
        if (DataTools.SetAllDataPoint() == null)
        {
            // Send error when there are no nodes available
            NodeEditor.editor.RecieveMessage("There are no available nodes in the node manager, please assign at least three in the node parent", WarningStatus.Error);
            return;
        }

        timer.Stop();
        NodeEditor.settings.nodeInfo = new GeneratedNodeSystem
            (
            lastAction,
            ExecutionTime,
            DataTools.allNodes.Length,
            DataTools.curvePoints.Count,
            DataTools.totalLength
            );
        timer.Reset();

        if (!MeshEditor.settings.renderRealtime)
            NodeEditor.editor.RecieveMessage("Generation was succesful!", WarningStatus.None);

        UnityEditor.EditorUtility.SetDirty(DataTools.nodeParent);
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
