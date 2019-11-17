using UnityEngine;

[System.Serializable]
public class NodeSettings : ScriptableObject
{
    public bool renderNodes = true;
    public bool renderLines = true;
    public bool renderCurves = true;
    public bool renderCurveAdjustmentNodes = true;

    [Space(20)]
    public int curveAccuracy = 1000; // The amount of nodes created before the equal spacing
    public float curveResolution = 1; // Amount of nodes per meter
    public int curvePreviewResolution = 10; // Amount of segements for the preview
    public bool renderRealtime = false;

    [Space(20)]
    [SerializeField] public Mesh curvePointMesh;

    [Space(20)]
    public float nodeSize = 0.5f;
    public float adjustmentNodeSize = 0.2f;

    // Editor settings
    [Space(20)]
    public int selectedTab;

    public WarningStatus[] tabStatus;

    public WarningStatus currentTabStatus
    {
        get { return tabStatus[selectedTab]; }
        set { tabStatus[selectedTab] = value; }
    }

    public GeneratedNodeSystem nodeInfo;
}

[System.Serializable]
public struct GeneratedNodeSystem
{
    public string lastAction; // miliseconds
    public float completionTime;
    public int nodeCount;
    public int pointCount;
    public float totalLength;

    public GeneratedNodeSystem(string lastAction, float time, int nodeCount, int pointCount, float length)
    {
        completionTime = time;
        this.nodeCount = nodeCount;
        totalLength = length;
        this.pointCount = pointCount;
        this.lastAction = lastAction;
    }
}
