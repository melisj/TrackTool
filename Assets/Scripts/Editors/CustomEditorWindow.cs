using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CustomEditorWindow : EditorWindow
{
    // Rectangles for the editors to use
    public Rect settingsMenu;

    // Message for function
    protected string actionMessage = "";
    protected WarningStatus actionStatus;
    protected float timer;

    // Reference from the global style manager
    protected GUIStyleContainer style;

    public bool IsEditorDirty { get; set; }

    protected virtual void OnEnable()
    {
        // Get the gui style
        style = GUIStyleManager.style;
    }

    protected virtual void OnDisable()
    {
        IsEditorDirty = false;
    }

    // Keep track of a timer for the warning message
    protected virtual void OnInspectorUpdate()
    {
        timer++;
    }

    protected virtual void OnGUI()
    {
        // Set this editor as actively drawing to the screen
        GUIStyleManager.SetCurrentEditor(this);
        GUILayout.BeginArea(Screen.safeArea, style.basicStyle);
    }

    // Send a request for a message in the layout
    protected virtual void SendMessageRequest(string message, WarningStatus status)
    {
        if (status == WarningStatus.Warning)
            style.DrawWarning(message);
        else if (status == WarningStatus.Error)
            style.DrawError(message);
        else
            style.DrawInfo(message);
    }

    // Recieve a message from an action being completed for feedback
    public virtual void RecieveMessage(string message, WarningStatus status)
    {
        actionMessage = message;
        actionStatus = status;
        timer = 0;

        Debug.Log(status.ToString() + " - " + message);
    }
}
