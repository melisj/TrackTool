using UnityEngine;
using UnityEditor;

public class NodeEditor : EditorWindow
{
    // Rectangles for the editors to use
    public Rect settingsMenu;

    // Tabs in the settings to organise them
    private static string[] tabList = new string[] { "Functions", "Rendering", "Curves", "Visuals" };
    private static Vector2 windowSize { get { return new Vector2(tabList.Length * 100, 400); } }

    // Tab rectangles
    private Rect[] tabErrors = new Rect[tabList.Length];

    // Message for function
    private string actionMessage = "";
    private WarningStatus actionStatus;
    private float timer;

    // Refereces to the Settings and the Manager (used by every script)
    private static NodeSettings _settings;
    public static NodeSettings settings
    {
        get
        {
            if (_settings == null)
                _settings = LoadData();
            return _settings;
        }
        set
        {
            _settings = value;
        }
    }

    public static NodeEditor editor { get { return (NodeEditor)GetWindow(typeof(NodeEditor)); } }

    // Reference from the global style manager
    private GUIStyleContainer style; 

    private void OnEnable()
    {
        LoadData();

        GUIStyleManager.style.RefreshStyle();

        // Copy the global reference
        style = GUIStyleManager.style;

        for (int iRect = 0; iRect < tabErrors.Length; iRect++)
            tabErrors[iRect] = new Rect(80 + iRect * 100, 0,20,20);

        if(settings.tabStatus == null || settings.tabStatus.Length != tabList.Length)
            settings.tabStatus = new WarningStatus[tabList.Length];

        settingsMenu = new Rect(new Vector2(10, 10), windowSize);
    }

    private void OnDisable()
    {
        SaveData();
    }

    [MenuItem("Track Editor/Node Editor #q")]
    static void OpenWindow()
    {
        NodeEditor window = (NodeEditor)GetWindow(typeof(NodeEditor));
        window.titleContent.text = "Node Editor";
        window.minSize = windowSize + new Vector2(20, 0);
        window.Show();
    }

    private void OnGUI()
    {
            // Draw all the settings
            GUILayout.BeginArea(Screen.safeArea, style.basicStyle);
            DrawSettings();
            GUILayout.EndArea();
    }

    private void OnInspectorUpdate()
    {
        timer++;
    }


    #region DrawSettings

    private void DrawSettings()
    {
        // Draw a toolbar with all the options given by the tablist array
        GUILayout.BeginArea(settingsMenu);
        settings.selectedTab = GUILayout.Toolbar(settings.selectedTab, tabList, style.buttonStyle);
        GUILayout.Space(10);

        settings.currentTabStatus = WarningStatus.None; // Reset tab errors

        // Draw the selected tab
        switch (settings.selectedTab)
        {
            case 0:
                DrawButtons();
                break;
            case 1:
                DrawRenderSettings();
                break;
            case 2:
                DrawCurveSettings();
                break;
            case 3:
                DrawVisualSettings();
                break;
        }

        DrawErrors(); // Draw errors under each tab

        GUILayout.EndArea();
    }

    // Draw toggles
    private void DrawRenderSettings()
    {
        GUILayout.Label("Render Settings", style.titleStyle);

        style.DrawToggle("Nodes", ref settings.renderNodes);
        style.DrawToggle("Lines", ref settings.renderLines);
        style.DrawToggle("Curves", ref settings.renderCurves);
        style.DrawToggle("Adjustment Nodes", ref settings.renderCurveAdjustmentNodes);

        if (!settings.renderNodes && !settings.renderLines && !settings.renderCurves && !settings.renderCurveAdjustmentNodes)
            SendMessageRequest("Nothing is being displayed, please select an option!", WarningStatus.Warning);
    }

    // Draw extra visual settings to make the visuals of the node system dynamic
    private void DrawVisualSettings()
    {
        GUILayout.Label("References", style.titleStyle);

        style.DrawObjectField("Curve Point Mesh", ref settings.curvePointMesh);
        if (settings.curvePointMesh == null)
            SendMessageRequest("Reference is empty, curve points will not be visible!", WarningStatus.Warning);

        GUILayout.Space(10);
        GUILayout.Label("Node Sizes", style.titleStyle);

        style.DrawFloatField("Node", ref settings.nodeSize);
        style.DrawFloatField("Adjustment Node", ref settings.adjustmentNodeSize);
    }

    // Draw settings for the curves
    private void DrawCurveSettings()
    {
        GUILayout.Label("Curve Settings", style.titleStyle);

        GUILayout.Label("Curve accuracy should be around 2000.", style.textStyle);
        style.DrawIntField("Curve Accuracy", ref settings.curveAccuracy, 1, 100000);
        if (settings.curveAccuracy < 1000)
            SendMessageRequest("It is advised to not use less than 1000. Recommended amount: 2000", WarningStatus.Warning);

        GUILayout.Label("X amount of points placed per meter.", style.textStyle);
        style.DrawFloatField("Curve Resolution", ref settings.curveResolution, 0.1f, 50);

        GUILayout.Label("Amount of segments for the Preview line.", style.textStyle);
        style.DrawIntField("Curve Preview Resolution", ref settings.curvePreviewResolution, 1, 1000);
    }

    // Draw the options in the functions tab
    private void DrawButtons()
    {
        GUILayout.Label("Function for the node system", style.titleStyle);

        style.DrawButton("Connect\nNetwork", 
            "Connect network will connect all the nodes with eachother and bake the curves. Will reset curves on nodes with a reset flag",
            () => TrackManager.nodeManager.SetupNetwork());

        style.DrawButton("Bake\nCurves",
            "Option will only bake curves",
            () => TrackManager.nodeManager.BakeCurves());

        style.DrawButton("Hard\nReset",
            "This will reset all the curves and nodes regardless of their status",
            () => TrackManager.nodeManager.SetupNetwork(true));


        DrawNodeSystemInfo();
        DrawActionMessage();
    }

    // Draw info about the last generation of the node system
    private void DrawNodeSystemInfo()
    {
        GUILayout.BeginVertical(style.textStyle);

        GUILayout.Label("Last Action: \t | " + settings.nodeInfo.lastAction, style.titleStyle);
        GUILayout.Label("Completion Time: \t | " + settings.nodeInfo.completionTime / 1000 + " Sec");
        GUILayout.Label("Nodes: \t\t | " + settings.nodeInfo.nodeCount);
        GUILayout.Label("Curve Points: \t | " + settings.nodeInfo.pointCount);
        GUILayout.Label("Curve Length: \t | " + settings.nodeInfo.totalLength + " m");

        GUILayout.EndVertical();
    }

    // Draw errors images for each tab in the toolbar
    private void DrawErrors()
    {
        for (int iRect = 0; iRect < tabErrors.Length; iRect++)
        {
            if(settings.tabStatus[iRect] != WarningStatus.None)
                style.DrawSmallError(tabErrors[iRect], settings.tabStatus[iRect]);
        }
    }

    // Draw button action message for the buttons for a small period of time
    private void DrawActionMessage()
    {
        if (actionMessage != "")
        {
            SendMessageRequest(actionMessage, actionStatus);
            if (timer >= 50) // Remove message when it is over the time limit 
                actionMessage = "";
        }
    }

    #endregion

    // Send a request for a message in the layout
    private void SendMessageRequest(string message, WarningStatus status)
    {
        if (status == WarningStatus.Warning)
            style.DrawWarning(message);
        else if (status == WarningStatus.Error)
            style.DrawError(message);
        else
            style.DrawInfo(message);

        if(status != WarningStatus.None)
            settings.currentTabStatus = status;
    }

    // Recieve a message from an action being completed for feedback
    public void RecieveMessage(string message, WarningStatus status)
    {
        actionMessage = message;
        actionStatus = status;
        timer = 0;

        if (status == WarningStatus.Error)
            settings.nodeInfo = new GeneratedNodeSystem();

        Debug.Log(status.ToString() + " - " + message);
    }

    #region FileManagement
    private static NodeSettings LoadData()
    {
        NodeSettings tempSettings;
        tempSettings = (NodeSettings)Resources.Load("EditorData/NodeSettings");

        if (tempSettings == null)
            tempSettings = (NodeSettings)CreateInstance(typeof(NodeSettings));

        return tempSettings;
    }

    private void SaveData()
    {
        if (!AssetDatabase.Contains(settings))
            AssetDatabase.CreateAsset(settings, "Assets/Resources/EditorData/NodeSettings.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    #endregion
}
