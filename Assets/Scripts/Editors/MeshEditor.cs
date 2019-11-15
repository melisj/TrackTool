using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MeshEditor : EditorWindow
{
    // Rectangles for the editors to use
    public Rect settingsMenu;

    private static string[] tabList = new string[] { "Functions", "Settings", "Meta Settings" };
    private static Vector2 windowSize { get { return new Vector2(tabList.Length * 100 + 100, 400); } }

    // Scrollbar for the mesh setting window
    private Vector2 scrollPos;

    // Message for function button
    private string actionMessage = "";
    private WarningStatus actionStatus;
    private float timer;

    // Refereces to the Settings and the Manager (used by every script)
    private static MeshSettings _settings;
    public static MeshSettings settings
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

    public static MeshEditor editor { get { return (MeshEditor)GetWindow(typeof(MeshEditor)); } }

    // Reference from the global style manager
    private GUIStyleContainer style;

    private void OnEnable()
    {
        // Init the foldoutgroups
        if(settings.foldoutGroups.Count == 0)
            for (int i = 0; i < settings.container.Count; i++)
                settings.foldoutGroups.Add(false);

        // Init the tabstatus
        if (settings.tabStatus == null || settings.tabStatus.Count != settings.container.Count)
        {
            settings.tabStatus = new List<WarningStatus>();
            for (int i = 0; i < settings.container.Count; i++)
                settings.tabStatus.Add(WarningStatus.None);
        }

        // Get the gui style
        GUIStyleManager.style.RefreshStyle();
        style = GUIStyleManager.style;

        settingsMenu = new Rect(new Vector2(10, 10), windowSize);
    }

    private void OnDisable()
    {
        SaveData();
    }

    [MenuItem("Track Editor/Mesh Editor #w")]
    static void OpenWindow()
    {
        MeshEditor window = (MeshEditor)GetWindow(typeof(MeshEditor));
        window.titleContent.text = "Mesh Editor";
        window.minSize = windowSize + new Vector2(20, 0);
        window.Show();
    }

    private void OnGUI()
    {
            GUILayout.BeginArea(settingsMenu);

            settings.selectedTab = GUILayout.Toolbar(settings.selectedTab, tabList, style.buttonStyle);
            GUILayout.Space(10);

            switch (settings.selectedTab)
            {
                case 0:
                    DrawButtons();
                    break;
                case 1:
                    DrawSettings();
                    break;
                case 2:
                    DrawMetaSettings();
                    break;
            }

            GUILayout.EndArea();
    }

    #region DrawSettings

    private void DrawSettings()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        style.DrawButton("Add", "Add an extra mesh", () => AddExtraMesh(), style.smallButtonStyle);

        // Draw every mesh setting container
        foreach (MeshSettingsContainer mesh in settings.container)
        {
            int meshIndex = settings.container.IndexOf(mesh);

            // Draw the header for the mesh settings with buttons
            GUILayout.BeginHorizontal(style.textStyle);
            settings.foldoutGroups[meshIndex] = EditorGUILayout.BeginFoldoutHeaderGroup(settings.foldoutGroups[meshIndex], (meshIndex + 1) + " mesh", style.smallButtonStyle);
            if (style.DrawButton("Remove", "", () => RemoveMesh(meshIndex), style.smallButtonStyle))
                break;

            if (mesh.usedMesh != null)
                GUILayout.Label(mesh.usedMesh.name, style.titleStyle);

            DrawErrorMeshSetting(meshIndex);
            GUILayout.EndHorizontal();

            // Draw all the settings
            if (settings.foldoutGroups[meshIndex]) 
            {
                settings.tabStatus[meshIndex] = WarningStatus.None; // Reset tab errors

                DrawMeshSettings(mesh, meshIndex);

                GUILayout.Space(20);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            GUILayout.Space(10);
        }

        GUILayout.Space(20);

        EditorGUILayout.EndScrollView();
    }

    // Draw for each mesh setting container the settings
    private void DrawMeshSettings(MeshSettingsContainer mesh, int meshIndex)
    {
        GUILayout.Space(10);
        GUILayout.Label("Settings", style.titleStyle);

        // Mesh reference
        style.DrawObjectField("Mesh Object", ref mesh.usedMesh);

        if (mesh.usedMesh)
        {
            // Draw this mesh y/n
            style.DrawToggle("Create Mesh", ref mesh.createMesh);

            if (mesh.createMesh)
            {
                // Draw settings for object without a triangle in the input mesh
                if (!mesh.isSeperateObj)
                {
                    // Draw symmetry mode
                    GUILayout.Label("Mesh Settings", style.textStyle);
                    style.DrawToggle("Symmetry Mode", ref mesh.symmetry);

                    // Draw flip normal setting
                    style.DrawToggle("Flip Normals", ref mesh.flipNormals);

                    // Draw loop mesh setting
                    style.DrawToggle("Loop Mesh Around", ref mesh.loopMesh);
                }
                // Settings unavailable for mesh with triangles
                else if (settings.showInfo)
                {
                    mesh.symmetry = false;
                    mesh.loopMesh = false;
                    style.DrawInfo("Loop Mesh and symmetry is not available when the mesh has faces.");
                }

                // Draw offset settings
                GUILayout.Label("Offset of the mesh", style.textStyle);
                style.DrawFloatField("Offset Mesh X", ref mesh.offsetFromCurveX, -100, 100);
                style.DrawFloatField("Offset Mesh Y", ref mesh.offsetFromCurveY, -100, 100);

                // Draw size settings
                GUILayout.Label("Size of the mesh", style.textStyle);
                style.DrawFloatField("Mesh Size X", ref mesh.localSizeOfMeshX, 0.01f);
                style.DrawFloatField("Mesh Size Y", ref mesh.localSizeOfMeshY, 0.01f);

                if (mesh.isSeperateObj)
                    style.DrawFloatField("Mesh Size Z", ref mesh.localSizeOfMeshZ, 0.01f);
                else if (settings.showInfo)
                {
                    mesh.localSizeOfMeshZ = 0;
                    style.DrawInfo("The size of the mesh in the Z axis is unavailable when the mesh has faces.");
                }

                // Check if mesh size is not too small
                if (mesh.localSizeOfMeshX < 0.1f || mesh.localSizeOfMeshY < 0.1f || (mesh.isSeperateObj && mesh.localSizeOfMeshZ < 0.1f))
                    SendMessageRequest("Size of the mesh is really small! Mesh might not be visible.", meshIndex, WarningStatus.Warning);
            }
            else if (settings.showInfo)
                style.DrawInfo("This mesh will not be generated.");
        }
        else
            SendMessageRequest("The settings need a mesh to operate!", meshIndex, WarningStatus.Error);
    }

    private void DrawButtons()
    {
        GUILayout.Label("Functions for the mesh generation", style.titleStyle);
        GUIStyleManager.style.DrawButton("Create mesh", "Generate a mesh", () => TrackManager.generator.StartGenerating());

        DrawMeshGeneratorInfo();
        DrawActionError();
        }

    private void DrawMeshGeneratorInfo()
    {
        GUILayout.BeginVertical(style.textStyle);

        GUILayout.Label("Meshes Generated: | " + settings.meshInfo.meshesGenerated, style.titleStyle);
        GUILayout.Label("Completion Time: \t | " + settings.meshInfo.completionTime / 1000 + " Sec");
        GUILayout.Label("Vertices: \t\t | " + settings.meshInfo.vertexCount);
        GUILayout.Label("Triangles: \t\t | " + settings.meshInfo.triangleCount);

        GUILayout.EndVertical();
    }

    // Meta settings tab
    private void DrawMetaSettings()
    {
        GUILayout.Label("Settings for info within the editor", style.titleStyle);

        style.DrawToggle("Show Info", ref settings.showInfo);
    }

    // Draw an error image next to the mesh button
    private void DrawErrorMeshSetting(int index)
    {
        if (settings.tabStatus[index] != WarningStatus.None)
            style.DrawSmallError(Rect.zero, settings.tabStatus[index]);
    }

    // Draw action error for the buttons for a small period of time
    private void DrawActionError()
    {
        if (actionMessage != "")
        {
            SendMessageRequest(actionMessage, -1, actionStatus);
            if (timer >= 50) // Remove message when it is over the time limit 
                actionMessage = "";
        }
    }

    private void OnInspectorUpdate()
    {
        timer++;
    }

    #endregion

    // Send a request for a message in the layout
    private void SendMessageRequest(string message, int index, WarningStatus status)
    {
        if (status == WarningStatus.Warning)
            style.DrawWarning(message);
        else if (status == WarningStatus.Error)
            style.DrawError(message);
        else
            style.DrawInfo(message);

        if (index != -1)
            settings.tabStatus[index] = status;
    }

    // Recieve a message from an action being completed for feedback
    public void RecieveMessage(string message, WarningStatus status)
    {
        actionMessage = message;
        actionStatus = status;
        timer = 0;

        if (status == WarningStatus.Error)
            settings.meshInfo = new GeneratedMeshInfo();

        Debug.Log(status.ToString() + " - " + message);
    }

    #region FileManagement
    private static MeshSettings LoadData()
    {
        MeshSettings tempSettings;
        tempSettings = (MeshSettings)Resources.Load("EditorData/MeshSettings");

        if (tempSettings == null)
            tempSettings = (MeshSettings)CreateInstance(typeof(MeshSettings));

        return tempSettings;
    }

    private void SaveData()
    {
        if (!AssetDatabase.Contains(settings))
            AssetDatabase.CreateAsset(settings, "Assets/Resources/EditorData/MeshSettings.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    // Add an extra mesh to the array
    private void AddExtraMesh()
    {
        settings.container.Add(new MeshSettingsContainer());
        settings.foldoutGroups.Add(true);
        settings.tabStatus.Add(WarningStatus.Error);
    }

    // Remove an mesh from the array
    private void RemoveMesh(int index)
    {
        settings.container.RemoveAt(index);
        settings.foldoutGroups.RemoveAt(index);
        settings.tabStatus.RemoveAt(index);
    }
    #endregion
}