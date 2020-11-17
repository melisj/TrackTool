using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataTools
{
    #region Node Info

    public static float totalLength;

    public static CurveAdjustmentNode[] GetCurveAdjustmentNodes(NodeBehaviour parent) {
        return parent.transform.GetComponentsInChildren<CurveAdjustmentNode>();
    }

    #endregion

    #region FileManagement
    public static T LoadData<T>(string dataName) {
        T tempSettings = (T)System.Convert.ChangeType(Resources.Load("EditorData/" + dataName), typeof(T));

        if (tempSettings == null)
            tempSettings = (T)System.Convert.ChangeType(ScriptableObject.CreateInstance(typeof(T)), typeof(T));

        return tempSettings;
    }

    public static void SaveData(Object settings, string dataName) {
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
                _allNodes = NodeParent.GetComponentsInChildren<NodeBehaviour>();
            return _allNodes;
        }
        set
        {
            _allNodes = value;
        }
    }

    private static Transform _nodeParent;
    public static Transform NodeParent
    {
        get
        {
            if (_nodeParent == null)
                _nodeParent = GameObject.FindGameObjectWithTag("NodeManager")?.transform;

            return _nodeParent ?? new GameObject { tag = "NodeManager", name = "NodeManager" }.transform;
        }
    }

    private static Transform _meshParent;
    public static Transform MeshParent
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
    public static GameObject LoadAdjustmentNode() {
        return adjustmentNodePrefab = adjustmentNodePrefab ?? Resources.Load<GameObject>("AdjustmentNode");
    }

    #endregion

    #region Settings

    // Load and store the settings for the node editor
    private static NodeSettings _nodeSetting;
    public static NodeSettings NodeSetting
    {
        get
        {
            if (_nodeSetting == null)
                _nodeSetting = LoadData<NodeSettings>("NodeSettings");
            return _nodeSetting;
        }
    }

    // Load and store the settings for the mesh editor
    private static MeshSettings _meshSetting;
    public static MeshSettings MeshSetting
    {
        get
        {
            if (_meshSetting == null)
                _meshSetting = LoadData<MeshSettings>("MeshSettings");
            return _meshSetting;
        }
    }

    #endregion
}
