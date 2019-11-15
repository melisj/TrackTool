using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackManager
{
    private static NodeManager _nodeManager;
    public static NodeManager nodeManager
    {
        get
        {
            return (_nodeManager == null) ? _nodeManager = new NodeManager() : _nodeManager;
        }
    }

    private static MeshTools _meshTools;
    public static MeshTools meshTools
    {
        get
        {
            return (_meshTools == null) ? _meshTools = new MeshTools() : _meshTools;
        }
    }

    private static DataTools _dataTools;
    public static DataTools dataTools
    {
        get
        {
            return (_dataTools == null) ? _dataTools = new DataTools() : _dataTools;
        }
    }

    private static MeshGenerator _generator;
    public static MeshGenerator generator
    {
        get
        {
            return (_generator == null) ? _generator = new MeshGenerator() : _generator;
        }
    }
}
