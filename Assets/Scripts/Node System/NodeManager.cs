using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class NodeManager
{
    // Keep track of the time actions take and send them to the editor
    Stopwatch timer = new Stopwatch();

    public NodeTools tools = new NodeTools();

    #region Settings

    public float restrictConnectionRange = 20;
    public float connectionRange = 300;

    public float restrictJunctionRange = 20;
    public float junctionRange = 50;

    #endregion

    #region NetworkOptions

    public void SetupNetwork(bool hardReset = false, bool skipDialogue = false) {
        // Check if the user really wants to do this
        if (!skipDialogue)
            if (hardReset && !UnityEditor.EditorUtility.DisplayDialog("Hard reset", "You are about to reset every node, regardless whether they are set to reset in their options", "Hard Reset", "I made a mistake"))
                return;

        timer.Start();

        if (!GetAllNodes())
            return;

        CheckFromNode(hardReset);

        FinalizeConnecting(hardReset ? "Hard Reset" : "Connect Network", (int)timer.ElapsedMilliseconds);
    }

    public void CheckFromNode(bool hardReset) {
        List<NodeBehaviour> availableNodes = DataTools.allNodes.ToList();
        NodeBehaviour currentNode = availableNodes[0];
        NodeBehaviour nextNode = null;

        //currentNode.connected = true; // First one is the starting node and should not be connected with again.

        int time = 0;
        int error = 100;
        while (availableNodes.Count > 0) {
            if (time >= error) {
                NodeEditor.editor.RecieveMessage("conecting failed", WarningStatus.Error);
                return;
            }
            time++;

            // End nodes should not connect with any other node
            if (currentNode.thisNodeType == NodeType.end)
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

            if (nextNode.thisNodeType == NodeType.connection) {
                return;
            }
        }
    }

    public void BakeCurves() {
        timer.Start();

        foreach (NodeBehaviour node in DataTools.allNodes) {
            if (node.thisNodeType != NodeType.end)
                if (!node.CreateCurvePoints())
                    return;
        }

        if (DataTools.allNodes.Length < 2) {
            NodeEditor.editor.RecieveMessage("There are no available nodes in the node manager, please assign at least two in the node parent", WarningStatus.Error);
            return;
        }

        FinalizeConnecting("Bake Curves", (int)timer.ElapsedMilliseconds);
    }

    #endregion

    #region Tools
    private NodeBehaviour ConnectToOtherNode(NodeBehaviour currentNode) {
        float closestDistance = float.MaxValue;
        NodeBehaviour candidate = null;

        foreach (NodeBehaviour otherNode in DataTools.allNodes) {
            if (otherNode.connected)
                continue;

            if (currentNode != otherNode) {
                float distance = Vector3.Distance(currentNode.transform.position, otherNode.transform.position);

                if (distance < closestDistance && distance > restrictConnectionRange && distance < connectionRange) {
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

    //private NodeBehaviour ConnectToJunctionNodes(NodeBehaviour currentNode) {

    //}

    private bool GetAllNodes() {
        DataTools.allNodes = null;

        // Send error when there are not enough nodes available
        if (DataTools.allNodes.Length < 2) {
            NodeEditor.editor.RecieveMessage("There are no available nodes in the node manager, please assign at least two in the node parent", WarningStatus.Error);
            return false;
        }

        // Set all nodes
        int index = 0;
        foreach (NodeBehaviour node in DataTools.allNodes) {
            index++;
            node.name = "RailNode: " + index;

            node.nextNode = null;
            node.prevNode = null;
            node.connected = false;

            node.SetType();
        }

        return true;
    }

    private void FinalizeConnecting(string lastAction, int ExecutionTime) {
        // Save all the points on the curve
        timer.Stop();
        DataTools.NodeSetting.nodeInfo = new GeneratedNodeSystem
            (
            lastAction,
            ExecutionTime,
            DataTools.allNodes.Length,
            DataTools.totalLength
            );
        timer.Reset();

        if (!DataTools.MeshSetting.renderRealtime)
            NodeEditor.editor.RecieveMessage("Generation was succesful!", WarningStatus.None);

        UnityEngine.Debug.Log(DataTools.NodeParent);
        UnityEditor.EditorUtility.SetDirty(DataTools.NodeParent);
    }
    #endregion
}
