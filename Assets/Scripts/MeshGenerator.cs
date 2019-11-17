using UnityEngine;

/// <summary>
/// The mesh is incrementaly created by adding the seperate meshes together at each cycle of the generation.
/// Each mesh setting container will be read when told to (setting "Create mesh") and used as on the curve generated.
/// </summary>
public class MeshGenerator
{
    // <Settings>   
    private MeshSettingsContainer m; // Current mesh being generated with, all its settings
    // </Settings>   

    // <Program Use Data>
    // Indeces to keep track of where the generated mesh is in the array's with data
    // private int vertexIndex;[Old]
    // private int triangleIndex;[Old]
    // private int normalIndex;[Old]

    // Keep track of the time actions take and send them to the editor
    System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
    private int totalAmountVertex = 0;
    private int totalAmountTriangle = 0;

    // </Program Use Data>

    // <Output>   
    // private Vector3[] newVertices;[Old]
    // private int[] newTriangles;[Old]
    // private Vector3[] newNormals;[Old]

    // private Mesh generatedMesh;[Old]
    // </Output>   

    public void StartGenerating()
    {
        timer.Start();

        if (DataTools.curvePoints.Count != 0)
        {
            InitGeneratedMeshData();

            LoopThroughMeshes(false); // First the meshes without any face will be generated
            LoopThroughMeshes(true); // Then with faces to keep the RecalculateNormals function from interupting the normal copy pasting.

            FinalizeGeneration();
        }
        else
            MeshEditor.editor.RecieveMessage("There was no mesh generated, curve data is missing!", WarningStatus.Error);
    }

    private void LoopThroughMeshes(bool doSeperateObj)
    {
        foreach (MeshSettingsContainer mesh in MeshEditor.settings.container)
        {
            if (mesh.isAllowedToGenerate && (doSeperateObj == mesh.isSeperateObj))
            {
                m = mesh; // Set current mesh
                TrackManager.meshTools.InitData(m);
                CreateMesh();
            }
        }
    }

    private void CreateMesh()
    {
        if (!m.isSeperateObj)
        {
            TrackManager.meshTools.SetVertices();
            TrackManager.meshTools.SetTriangles();
            TrackManager.meshTools.SetUVs();
        }
        else
            TrackManager.meshTools.SeperateMeshPlacement();

        //UpdateMesh(); [Old]
    }

    #region Initialization

    // Init the main array's that are going to be used for the main mesh
    private void InitGeneratedMeshData()
    {
        //generatedMesh = new Mesh();[Old]
        //generatedMesh.name = "Generated Mesh";[Old]

        //vertexIndex = 0;[Old]
        //triangleIndex = 0;[Old]
        //normalIndex = 0;[Old]

        totalAmountVertex = 0;
        totalAmountTriangle = 0;

        foreach (MeshSettingsContainer mesh in MeshEditor.settings.container)
        {
            if (mesh.isAllowedToGenerate)
            {
                CalculateArrayLengths(mesh);

                totalAmountVertex += mesh.vertices.Length;
                totalAmountTriangle += mesh.triangles.Length;
            }
        }

        //newVertices = new Vector3[totalAmountVertex];[Old]
        //newTriangles = new int[totalAmountTriangle];[Old]
        //newNormals = new Vector3[totalAmountVertex];[Old]
    }

    // Calculate the length of the array's
    private void CalculateArrayLengths(MeshSettingsContainer mesh)
    {
        int amountVertices, amountTriangles;

        if (!mesh.isSeperateObj)
        {
            amountVertices = DataTools.curvePoints.Count * mesh.amountVertex;

            int trianglesPerMesh = (mesh.amountVertex * 6) - (mesh.loopMesh ? 0 : 6); // Triangles used for to connect two pieces together
            amountTriangles = (DataTools.curvePoints.Count * trianglesPerMesh - trianglesPerMesh);

            if (mesh.symmetry) // Add extra space in the arrays for symmetry mode
            {
                amountVertices *= 2;
                amountTriangles *= 2;
            }
        }
        else
        {
            amountVertices = DataTools.curvePoints.Count * mesh.amountVertex;
            amountTriangles = DataTools.curvePoints.Count * mesh.amountTriangle;
        }

        // Init the array's
        mesh.vertices = new Vector3[amountVertices];
        mesh.triangles = new int[amountTriangles];
        mesh.normals = new Vector3[amountVertices];
        mesh.UVs = new Vector2[amountVertices];
    }

    #endregion

    #region Finalization

    private void FinalizeGeneration()
    {
        // Clear children from parent
        for (int iChild = DataTools.meshParent.childCount - 1; iChild >= 0; iChild--)
        {
            Object.DestroyImmediate(DataTools.meshParent.GetChild(iChild).gameObject);
        }

        int meshCount = 0;
        // Save the meshes in individual components
        foreach (MeshSettingsContainer mesh in MeshEditor.settings.container)
        {
            if (mesh.isAllowedToGenerate)
            {
                GameObject newMeshObj = new GameObject(mesh.usedMesh.name, typeof(MeshFilter), typeof(MeshRenderer));
                MeshFilter filter = newMeshObj.GetComponent<MeshFilter>();
                MeshRenderer renderer = newMeshObj.GetComponent<MeshRenderer>();

                newMeshObj.transform.parent = DataTools.meshParent;

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
        }
       
        timer.Stop();
        MeshEditor.settings.meshInfo = new GeneratedMeshInfo((int)timer.ElapsedMilliseconds, totalAmountVertex, totalAmountTriangle, meshCount);
        timer.Reset();

        if (!MeshEditor.settings.renderRealtime)
            MeshEditor.editor.RecieveMessage("Mesh succesfully generated!", WarningStatus.None);
    }

    #endregion

    #region Old Code

    // This was needed for adding the newly generated mesh to the one mesh
    /*
    #region Generation Tools

    // Set references correct for the generated mesh
    private void UpdateMesh()
    {
        for (int iTriangle = 0; iTriangle < m.triangles.Length; iTriangle++, triangleIndex++) // Add the new triangles
            // Set the vertex reference straight by adding the length of the vertices
            newTriangles[triangleIndex] = m.triangles[iTriangle] + vertexIndex;

        for (int iVertex = 0; iVertex < m.vertices.Length; iVertex++, vertexIndex++) // Add the new verteces
            newVertices[vertexIndex] = m.vertices[iVertex];


        if (!m.isSeperateObj)
        {
            generatedMesh.vertices = newVertices;
            generatedMesh.triangles = newTriangles;

            generatedMesh.RecalculateNormals();
        }

        // The normals of the generated mesh will be added when the mesh has no normals, else it will add them from the used mesh
        for (int iNormal = 0; iNormal < m.normals.Length; iNormal++, normalIndex++) // Add the new normals
            newNormals[normalIndex] = (m.isSeperateObj) ? m.normals[iNormal] : generatedMesh.normals[normalIndex];
    }

    #endregion
    */

    // This did the assigning of the vertices to the one mesh
    /*
    private void FinalizeGeneration()
    {
        generatedMesh.vertices = newVertices;
        generatedMesh.triangles = newTriangles;
        generatedMesh.normals = newNormals;

    }
    */

    #endregion
}
