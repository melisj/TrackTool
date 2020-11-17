using UnityEngine;

/// <summary>
/// The mesh is incrementaly created by adding the seperate meshes together at each cycle of the generation.
/// Each mesh setting container will be read when told to (setting "Create mesh") and used as on the curve generated.
/// </summary>
public class MeshGenerator
{
    // <Program Use Data>
    // Keep track of the time actions take and send them to the editor
    System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
    private int totalAmountVertex = 0;
    private int totalAmountTriangle = 0;
    private int meshCount = 0;

    // </Program Use Data>

    public void StartGenerating()
    {
        StartupMeshGeneration();

        if (DataTools.allNodes.Length != 0)
        {
            foreach (NodeBehaviour node in DataTools.allNodes) {
                InitGeneratedMeshData(node);

                LoopThroughMeshes(false, node); // First the meshes without any face will be generated
                LoopThroughMeshes(true, node); // Then with faces to keep the RecalculateNormals function from interupting the normal copy pasting.

                FinalizeGeneration(node);
            }
        }
        else
            MeshEditor.editor.RecieveMessage("There was no mesh generated, curve data is missing!", WarningStatus.Error);

        timer.Stop();
        DataTools.MeshSetting.meshInfo = new GeneratedMeshInfo((int)timer.ElapsedMilliseconds, totalAmountVertex, totalAmountTriangle, meshCount);
        timer.Reset();

        if (!DataTools.MeshSetting.renderRealtime)
            MeshEditor.editor.RecieveMessage("Mesh succesfully generated!", WarningStatus.None);
    }

    private void LoopThroughMeshes(bool generateObjects, NodeBehaviour node)
    {
        int meshIndex = 0;
        foreach (MeshSettingsContainer mesh in DataTools.MeshSetting.container)
        {
            if (mesh.isAllowedToGenerate && (generateObjects == mesh.isSeperateObj) && node.createMeshForNode[meshIndex])
            {
                TrackManager.meshTools.InitData(mesh, node);
                CreateMesh(mesh);
            }
            meshIndex++;
        }
    }

    private void CreateMesh(MeshSettingsContainer mesh)
    {
        if (!mesh.isSeperateObj)
        {
            TrackManager.meshTools.SetVertices();
            TrackManager.meshTools.SetTriangles();
            TrackManager.meshTools.SetUVs();
        }
        else
            TrackManager.meshTools.SeperateMeshPlacement();
    }

    #region Initialization

    private void StartupMeshGeneration() {
        timer.Start();

        totalAmountVertex = 0;
        totalAmountTriangle = 0;
        meshCount = 0;

        // Clear children from parent
        for (int iChild = DataTools.MeshParent.childCount - 1; iChild >= 0; iChild--) {
            Object.DestroyImmediate(DataTools.MeshParent.GetChild(iChild).gameObject);
        }
    }

    // Init the main array's that are going to be used for the main mesh
    private void InitGeneratedMeshData(NodeBehaviour node)
    {
        int meshIndex = 0;
        foreach (MeshSettingsContainer mesh in DataTools.MeshSetting.container)
        {
            if (mesh.isAllowedToGenerate && node.createMeshForNode[meshIndex]) {
                CalculateArrayLengths(mesh, node);

                totalAmountVertex += mesh.vertices.Length;
                totalAmountTriangle += mesh.triangles.Length;
            }
            meshIndex++;
        }
    }

    // Calculate the length of the array's
    private void CalculateArrayLengths(MeshSettingsContainer mesh, NodeBehaviour node)
    {
        int amountVertices, amountTriangles;

        if (!mesh.isSeperateObj)
        {
            amountVertices = node.curvePoints.Length * mesh.amountVertex + mesh.amountVertex;

            int trianglesPerMesh = (mesh.amountVertex * 6) - 6; // Triangles used for to connect two pieces together
            amountTriangles = node.curvePoints.Length * trianglesPerMesh;

            // Add extra space in the arrays for symmetry mode
            if (mesh.symmetry) 
            {
                amountVertices *= 2;
                amountTriangles *= 2;
            }
        }
        else
        {
            amountVertices = node.curvePoints.Length * mesh.amountVertex;
            amountTriangles = node.curvePoints.Length * mesh.amountTriangle;
        }

        // Init the array's
        mesh.vertices = new Vector3[amountVertices];
        mesh.triangles = new int[amountTriangles];
        mesh.normals = new Vector3[amountVertices];
        mesh.UVs = new Vector2[amountVertices];
    }

    #endregion

    #region Finalization

    private void FinalizeGeneration(NodeBehaviour node)
    {
        // Save the meshes in individual components
        int meshIndex = 0;
        foreach (MeshSettingsContainer mesh in DataTools.MeshSetting.container)
        {
            if (mesh.isAllowedToGenerate && node.createMeshForNode[meshIndex])
            {
                GameObject newMeshObj = new GameObject(mesh.usedMesh.name, typeof(MeshFilter), typeof(MeshRenderer));
                MeshFilter filter = newMeshObj.GetComponent<MeshFilter>();
                MeshRenderer renderer = newMeshObj.GetComponent<MeshRenderer>();

                newMeshObj.transform.parent = DataTools.MeshParent;

                Mesh generatedMesh = new Mesh
                {
                    name = mesh.usedMesh.name,

                    vertices = mesh.vertices,
                    triangles = mesh.triangles,
                    normals = mesh.normals,
                    uv = mesh.UVs
                };

                generatedMesh.RecalculateNormals();
                filter.mesh = generatedMesh;
                renderer.material = mesh.materialInput;

                meshCount++;
            }
            meshIndex++;
        }
    }

    #endregion
}
