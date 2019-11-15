using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIStyleManager
{
    private static GUIStyleContainer _style;
    public static GUIStyleContainer style
    {
        get
        {
            if (_style == null)
                _style = new GUIStyleContainer();
            return _style;
        }
    }

}
