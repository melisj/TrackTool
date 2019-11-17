using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MeshSettings : ScriptableObject
{
    public List<MeshSettingsContainer> container = new List<MeshSettingsContainer>();

    public bool showInfo = true;
    public bool renderRealtime = false;

    // Mesh editor settings
    [Space(20)]
    public int selectedTab;

    public List<bool> foldoutGroups = new List<bool>();

    public List<WarningStatus> tabStatus;

    public GeneratedMeshInfo meshInfo;
}

[System.Serializable]
public class MeshSettingsContainer
{
    // <User Input>   
    public Mesh usedMesh; // Mesh as input used as layout for the generated mesh

    public bool loopMesh; // Connect the ends of the mesh with each other // Not available when seperate obj
    public bool symmetry; // Symmetry mode // Not available when seperate obj
    public bool createMesh = true; // Should this mesh be created
    public bool flipNormals;

    public float offsetFromCurveX;
    public float offsetFromCurveY;

    public float localSizeOfMeshX = 1;
    public float localSizeOfMeshY = 1;
    public float localSizeOfMeshZ = 1; // Only available when seperate obj

    public Material materialInput;
    // </User Input>   

    // <Generated Data>  
    public Vector3[] newVerticeOrder; // Order of the mesh's vertice when it is recalculated (user input mesh)

    public Vector3[] vertices; // All vertices of the generated mesh
    public int[] triangles; // All the triangles of the generated mesh
    public Vector3[] normals; // All the normals of the generated mesh
    public Vector2[] UVs; // All the UV's of the generated mesh
    // </Generated Data> 

    // <Program Use Data>
    private Vector3[] _localVertices;
    public Vector3[] localVerticeData // Used mesh vertice data stored locally
    {
        get { return (_localVertices == null || _localVertices.Length == 0) ? _localVertices = usedMesh.vertices : _localVertices; }
        set { _localVertices = value; }
    }

    public int amountVertex { get { return usedMesh.vertexCount; } } // Return amount of verteces in the used mesh

    public bool isSeperateObj { get { return usedMesh.triangles.Length != 0; } } // Returns whether this obj has faces in it

    public bool isAllowedToGenerate { get { return usedMesh && createMesh; } }

    // Only available when seperate obj
    private int[] _localTriangleData;
    public int[] localTriangleData {
        get { return (_localTriangleData == null || _localTriangleData.Length == 0) ? _localTriangleData = usedMesh.triangles : _localTriangleData; }
        set { _localTriangleData = value; }
    }

    // Only available when seperate obj
    private Vector3[] _localNormalData;
    public Vector3[] localNormalData {
        get { return (_localNormalData == null || _localNormalData.Length == 0) ? _localNormalData = usedMesh.normals : _localNormalData; }
        set { _localNormalData = value; }
    }

    // Only available when seperate obj
    private Vector2[] _localUVData;
    public Vector2[] localUVData {
        get { return (_localUVData == null || _localUVData.Length == 0) ? _localUVData = usedMesh.uv : _localUVData; }
        set { _localUVData = value; }
    }

    // Only available when seperate obj
    public int amountTriangle { get { return usedMesh.triangles.Length; } }
    // </Program Use Data>
}

[System.Serializable]
public struct GeneratedMeshInfo
{
    public float completionTime; // miliseconds
    public int vertexCount;
    public int triangleCount;
    public int meshesGenerated;

    public GeneratedMeshInfo(float time, int vertex, int triangle, int meshCount)
    {
        completionTime = time;
        vertexCount = vertex;
        triangleCount = triangle / 3;
        meshesGenerated = meshCount;
    }
}
