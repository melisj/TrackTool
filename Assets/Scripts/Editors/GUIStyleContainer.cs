using UnityEngine;
using UnityEditor;

// Enum
[System.Serializable]
public enum WarningStatus
{
    None,
    Warning,
    Error
}

public class GUIStyleContainer
{
    // Styles for every component
    public GUIStyle basicStyle;
    public GUIStyle basicMessageStyle;

    public GUIStyle titleStyle; // Style for titles for groups of variables
    public GUIStyle toggleStyle; // Style for toggle components
    public GUIStyle buttonStyle;
    public GUIStyle smallButtonStyle;
    public GUIStyle infoStyle; // Style warning titles
    public GUIStyle warningStyle; // Style warning titles
    public GUIStyle errorStyle; // Style error titles
    public GUIStyle textStyle; // Style for explanation text
    public GUIStyle smallErrorImage;
   

    private Texture2D backgroundTexture;

    // Used textures
    private Texture2D toggleOn;
    private Texture2D toggleOff;
    private Texture2D infoSymbol;
    private Texture2D warningSymbol;
    private Texture2D errorSymbol;

    private Texture2D buttonNormal;
    private Texture2D buttonClicked;
    private Texture2D buttonActive;

    // Color scheme
    private Color32 backgroundColorTitles;
    private Color32 backgroundColorInfo;
    private Color32 backgroundColorWarning;
    private Color32 backgroundColorError;
    private Color32 titleColor;

    // Fonts
    private Font font;

    public GUIStyleContainer()
    {
        RefreshStyle();
        InitSkin();
    }

    public void RefreshStyle()
    {
        toggleOn = (Texture2D)Resources.Load("CustomTextures/Toggle On", typeof(Texture2D));
        toggleOff = (Texture2D)Resources.Load("CustomTextures/Toggle Off", typeof(Texture2D));
        errorSymbol = (Texture2D)Resources.Load("CustomTextures/Error Symbol", typeof(Texture2D));
        warningSymbol = (Texture2D)Resources.Load("CustomTextures/Warning Symbol", typeof(Texture2D));
        infoSymbol = (Texture2D)Resources.Load("CustomTextures/Info Symbol", typeof(Texture2D));

        buttonNormal = (Texture2D)Resources.Load("CustomTextures/Button Normal", typeof(Texture2D));
        buttonClicked = (Texture2D)Resources.Load("CustomTextures/Button Click", typeof(Texture2D));
        buttonActive = (Texture2D)Resources.Load("CustomTextures/Button Active", typeof(Texture2D));

        font = (Font)Resources.Load("Fonts/PCapTerminalItalic", typeof(Font));

        backgroundColorTitles = new Color32(66, 171, 208, 255);
        backgroundColorInfo = new Color32(255, 255, 255, 255);
        backgroundColorWarning = new Color32(205, 171, 0, 255);
        backgroundColorError = new Color32(200, 0, 0, 255);

        titleColor = new Color32(0, 0, 0, 255);
    }

    #region Init Skins

    private void InitSkin()
    {
        SetTextureColor(Color.grey, out backgroundTexture);

        basicStyle = new GUIStyle();
        InitBasicStyle();

        basicMessageStyle = new GUIStyle(basicStyle);
        InitBasicMessageStyle();

        titleStyle = new GUIStyle(basicStyle);
        buttonStyle = new GUIStyle(basicStyle);
        toggleStyle = new GUIStyle(basicStyle);
        textStyle = new GUIStyle(basicStyle);
        smallErrorImage = new GUIStyle();

        InitTitleStyle();
        InitButtonStyle();
        InitToggleStyle();
        InitTextStyle();

        infoStyle = new GUIStyle(basicMessageStyle);
        warningStyle = new GUIStyle(basicMessageStyle);
        errorStyle = new GUIStyle(basicMessageStyle);

        InitWarningStyles();
    }

    private void InitBasicStyle()
    {
        basicStyle.fontSize = 12;
        basicStyle.active.textColor = Color.white;
        basicStyle.wordWrap = true;
        //basicStyle.font = font;

        basicStyle.alignment = TextAnchor.MiddleLeft;
    }

    private void InitBasicMessageStyle()
    {
        basicStyle.fontSize = 12;
        basicMessageStyle.fontStyle = FontStyle.BoldAndItalic;
        basicMessageStyle.margin = new RectOffset(5, 5, 10, 10);
    }

    private void InitWarningStyles()
    {
        // Info style
        SetTextureColor(backgroundColorInfo, out backgroundTexture);
        infoStyle.normal.background = backgroundTexture;

        // Warning style
        SetTextureColor(backgroundColorWarning, out backgroundTexture);
        warningStyle.normal.background = backgroundTexture;

        // Error style
        SetTextureColor(backgroundColorError, out backgroundTexture);
        errorStyle.normal.background = backgroundTexture;

        // Small error image style
        smallErrorImage.fixedHeight = 20;
        smallErrorImage.fixedWidth = 20;
    }

    private void InitButtonStyle()
    {
        // Button Style
        buttonStyle.normal.background = buttonNormal;
        buttonStyle.active.background = buttonClicked;
        buttonStyle.onActive.background = buttonClicked;
        buttonStyle.onNormal.background = buttonActive;

        buttonStyle.onActive.textColor = Color.white;
        buttonStyle.onNormal.textColor = titleColor;

        buttonStyle.alignment = TextAnchor.MiddleCenter;
        buttonStyle.fontStyle = FontStyle.Bold;

        buttonStyle.fixedWidth = 100;
        buttonStyle.fixedHeight = 40;

        // Small button style
        smallButtonStyle = new GUIStyle(buttonStyle);
        smallButtonStyle.fixedWidth = 60;
        smallButtonStyle.fixedHeight = 25;
        smallButtonStyle.margin = new RectOffset(10, 10, 0, 0);
    }

    private void InitTitleStyle()
    {
        // Title Style
        SetTextureColor(backgroundColorTitles, out backgroundTexture);
        titleStyle.normal.background = backgroundTexture;
        titleStyle.fontStyle = FontStyle.Bold;

        titleStyle.padding = new RectOffset(5, 5, 5, 5);
        titleStyle.margin = new RectOffset(0, 0, 0, 5);

        titleStyle.normal.textColor = titleColor;
    }

    private void InitToggleStyle()
    {
        // Toggle variable Style
        toggleStyle.onNormal.background = toggleOn;
        toggleStyle.normal.background = toggleOff;

        toggleStyle.fixedWidth = 15;
        toggleStyle.fixedHeight = 15;   
    }

    private void InitTextStyle()
    {
        // Explanation text Style
        titleStyle.padding = new RectOffset(5, 5, 5, 5);
        textStyle.margin = new RectOffset(10,10,0,0);

        SetTextureColor(backgroundColorTitles, out backgroundTexture);
        textStyle.normal.background = backgroundTexture;
    }

    #endregion

    #region DrawTools

    public void DrawError(string message)
    {
        GUILayout.BeginHorizontal(errorStyle);

        GUILayout.Label(errorSymbol, errorStyle);
        GUILayout.Label(message, errorStyle);
       
        GUILayout.EndHorizontal();
    }

    public void DrawSmallError(Rect rect, WarningStatus error = WarningStatus.Error)
    {
        if(rect.size != Vector2.zero)
            GUILayout.BeginArea(rect);

        GUILayout.Label(error == WarningStatus.Error ? errorSymbol : warningSymbol, smallErrorImage);

        if (rect.size != Vector2.zero)
            GUILayout.EndArea();
    }

    public void DrawWarning(string message)
    {
        GUILayout.BeginHorizontal(warningStyle);

        GUILayout.Label(warningSymbol, warningStyle);
        GUILayout.Label(message, warningStyle);

        GUILayout.EndHorizontal();
    }

    public void DrawInfo(string message)
    {
        GUILayout.BeginHorizontal(infoStyle);

        GUILayout.Label(infoSymbol, infoStyle);
        GUILayout.Label(message, infoStyle);

        GUILayout.EndHorizontal();
    }

    public void DrawToggle(string name, ref bool output)
    {
        output = EditorGUILayout.Toggle(name, output, toggleStyle);
    }

    public void DrawObjectField<T>(string name, ref T output, bool editable = true) where T : Object
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(name);
        if (editable)
            output = EditorGUILayout.ObjectField(output, typeof(T), true) as T;
        else
            EditorGUILayout.ObjectField(output, typeof(T), true);

        GUILayout.EndHorizontal();
    }

    public bool DrawButton(string name, string explanation, System.Action function = null, GUIStyle styleOverride = null)
    {
        bool returnValue = false;

        // Draw a simple box for text display
        if (explanation != "")
            GUILayout.BeginHorizontal(titleStyle);

        // Draw the button
        if (GUILayout.Button(name, styleOverride ?? buttonStyle))
        {
            function.Invoke();
            returnValue = true;
        }

        if (explanation != "")
        {
            GUILayout.Label(explanation, textStyle);
            GUILayout.EndHorizontal();
        }

        return returnValue;
    }

    public void DrawIntField(string name, ref int output, int min = 0, int max = int.MaxValue)
    {
        output = EditorGUILayout.IntField(name, output);
        if (output < min)
            output = min;
        else if (output > max)
            output = max;
    }

    public void DrawFloatField(string name, ref float output, float min = 0, float max = float.MaxValue)
    {
        output = EditorGUILayout.FloatField(name, output);

        if (output < min)
            output = min;
        else if (output > max)
            output = max;
    }

    #endregion

    #region Tools

    private void SetTextureColor(Color newColor, out Texture2D backgroundTexture)
    {
        Texture2D tempTexture = new Texture2D(1, 1);
        tempTexture.SetPixel(0, 0, newColor);
        tempTexture.Apply();
        backgroundTexture = tempTexture;
    }

    #endregion

}
