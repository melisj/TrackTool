using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataTools
{
    #region Node Info

    private static List<NodeBehaviour.CurvePoint> _curvePoints;
    public static List<NodeBehaviour.CurvePoint> curvePoints {
        get { return (_curvePoints == null) ? SetAllDataPoint() : _curvePoints; }
        private set { _curvePoints = value; } } // Current Set of curve data points

    public static float totalLength;

    public static List<NodeBehaviour.CurvePoint> SetAllDataPoint()
    {
        // Return when there are no nodes
        if (allNodes.Length == 0) return null;

        List<NodeBehaviour.CurvePoint> tempPoints = new List<NodeBehaviour.CurvePoint>();
        NodeBehaviour currentNode = allNodes[0]; // Get the first node

        totalLength = 0;

        // Get all the connected nodes with all the data points in them
        while (currentNode.nextNode != null) // Check if there is a nextnode connected
        {
            if (currentNode.curvePoints != null) // Check if the curves have been baked
            {
                foreach (NodeBehaviour.CurvePoint point in currentNode.curvePoints) // Get all the curve data
                {
                    tempPoints.Add(point); // Store data
                }

                totalLength += currentNode.curveLength; // Calculate the total length
            }

            // Check next node
            currentNode = currentNode.nextNode;

            // Stop the loop when it looped around the whole circle
            if (currentNode == allNodes[0])
                break;
        }

        curvePoints = tempPoints;
        return tempPoints;
    }

    public static CurveAdjustmentNode[] GetCurveAdjustmentNodes(NodeBehaviour parent)
    {
        return parent.transform.GetComponentsInChildren<CurveAdjustmentNode>();
    }

    #endregion

    #region FileManagement
    public static T LoadData<T>(string dataName)
    {
        T tempSettings = (T)System.Convert.ChangeType(Resources.Load("EditorData/" + dataName), typeof(T));

        if (tempSettings == null)
            tempSettings = (T)System.Convert.ChangeType(ScriptableObject.CreateInstance(typeof(T)), typeof(T));

        return tempSettings;
    }

    public static void SaveData(Object settings, string dataName)
    {
        if (!UnityEditor.AssetDatabase.Contains(settings))
            UnityEditor.AssetDatabase.CreateAsset(settings, "Assets/Resources/EditorData/" + dataName);

        UnityEditor.EditorUtility.SetDirty(settings);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
    }
    #endregion

    #region Scene Info

    private static NodeBehaviour[] _allNodes;
    public static NodeBehaviour[] allNodes
    {
        get
        {
            if (_allNodes == null)
                _allNodes = nodeParent.GetComponentsInChildren<NodeBehaviour>();
            return _allNodes;
        }
        set
        {
            _allNodes = value;
        }
    }

    private static Transform _nodeParent;
    public static Transform nodeParent
    {
        get
        {
            if (_nodeParent == null)
                _nodeParent = GameObject.FindGameObjectWithTag("NodeManager")?.transform;

            return _nodeParent ?? new GameObject { tag = "NodeManager", name = "NodeManager" }.transform;
        }
    }

    private static Transform _meshParent;
    public static Transform meshParent
    {
        get
        {
            if (_meshParent == null)
                _meshParent = GameObject.FindGameObjectWithTag("Mesh Storage")?.transform;

            return _meshParent ?? new GameObject { tag = "Mesh Storage", name = "Mesh Storage" }.transform;
        }
    }

    private static RealtimeManager _realtimeManager;
    public static RealtimeManager realtimeManager
    {
        get
        {
            if (_realtimeManager == null)
                _realtimeManager = new RealtimeManager();

            return _realtimeManager;
        }
    }

    #endregion

    #region ResourceManagement

    private static GameObject adjustmentNodePrefab;
    public static GameObject LoadAdjustmentNode()
    {
        return adjustmentNodePrefab = adjustmentNodePrefab ?? Resources.Load<GameObject>("AdjustmentNode");
    }

    #endregion
}
