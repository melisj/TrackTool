using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshTools
{
    private MeshSettingsContainer m; // Currently set container
    private int indexOffsetForSymmetry; // Calculate the offset for when the mesh uses symmetry mode
    private int amountOfCurvePoints;
    private List<NodeBehaviour.CurvePoint> curvePointInstance;

    public void InitData(MeshSettingsContainer m, NodeBehaviour node) {
        this.m = m;
        indexOffsetForSymmetry = m.vertices.Length / 2;
        curvePointInstance = node.curvePoints.ToList();

        if (node.nextNode && !m.isSeperateObj)
            curvePointInstance.Add(node.nextNode.curvePoints[0]);

        amountOfCurvePoints = curvePointInstance.Count;
    }

    #region Creation Tools

    public void SetVertices()
    {
        // Every vertice of the used mesh will be placed down before it starts with a new piece
        for (int i = 0; i < amountOfCurvePoints; i++)
        {
            Vector3 straight = curvePointInstance[i].direction;
            Vector3 left = curvePointInstance[i].perpendicular;
            Vector3 right = -left;

            for (int meshIndex = 0; meshIndex < m.amountVertex; meshIndex++)
            {
                CreateVertex(true, meshIndex, i, left, straight);

                // Add extra vertices on the opposite side of the curve
                if (m.symmetry) 
                    CreateVertex(false, meshIndex, i, right, straight);
            }
        }
    }

    private void CreateVertex(bool left, int meshIndex, int pointIndex, Vector3 direction, Vector3 straightOn)
    {
        // The local position of the vertex is determined by the given mesh input
        // Calculate the local height
        Vector3 vertexLocalPosY = new Vector3(0, GetReorderdVertices(meshIndex).y, 0);

        // Calculate the local position on the X (invert position for right)
        float vertexLocalPosX = ((left) ? 1 : -1) * GetReorderdVertices(meshIndex).x;

        // Set the local postion of the vertex (multiply with the size of the mesh) 
        Vector3 vertexPos =
            (vertexLocalPosY * m.localSizeOfMeshY +                                 // Add Y scaling
            direction * vertexLocalPosX * m.localSizeOfMeshX +                      // Add X scaling
            straightOn * GetReorderdVertices(meshIndex).z * m.localSizeOfMeshZ);   // Add Z scaling

        // Add offset from the curve
        vertexPos += (direction * m.offsetFromCurveX) + new Vector3(0, m.offsetFromCurveY, 0);

        // Index for the generated mesh array of vertices
        int newIndex = meshIndex + (pointIndex * m.amountVertex);

        // Add index offset for the symmetry part of the mesh
        if (!left)
            newIndex += indexOffsetForSymmetry;

        // Set the global position of the vertex in the array
        m.vertices[newIndex] = vertexPos + curvePointInstance[pointIndex].position; 
    }

    public void SetTriangles()
    {
        for (int iVert = 0, iTriangle = 0; iVert < m.vertices.Length - m.amountVertex; iVert++)
        {
            // Prevent the two ends of the symmetry mode connecting, creating a wierd mesh (the last m.amountVertex of the first part of the symmetry)
            if (iVert >= indexOffsetForSymmetry - m.amountVertex && iVert < indexOffsetForSymmetry && m.symmetry)
                continue;

            if ((iVert + 1) % m.amountVertex == 0)
                continue;

            // Reversed the index to flip the normals
            m.triangles[iTriangle + 5] = iVert;
            m.triangles[iTriangle + 4] = iVert + 1;                   // Next point
            m.triangles[iTriangle + 3] = iVert + m.amountVertex;      // Other meshpart with same index
            m.triangles[iTriangle + 2] = iVert + m.amountVertex + 1;  // Next point on other meshpart with same index
            m.triangles[iTriangle + 1] = iVert + m.amountVertex;      // Other meshpart with same index
            m.triangles[iTriangle + 0] = iVert + 1;                   // Next point

            iTriangle += 6; // Add 6 extra to the index;
        }

        if (m.flipNormals)
            FlipTriangles();
    }

    public void SetUVs()
    {
        // Get the circumference of the mesh 
        float totalLength = CalculateCircumference(); 

        // For each curve point
        for (int iPoint = 0, iUV = 0; iPoint < amountOfCurvePoints; iPoint++)
        {
            // Tracks the distance the vertex is from the first vertex
            float vertDistance = 0;

            // For each vertex in the used mesh
            for (int iLocal = 0; iLocal < m.amountVertex; iLocal++, iUV++)
            {
                // Value should be between 0 and 1 on the x axis of the UV map
                float xValue = vertDistance / totalLength; 
                // Scale the y value to that of the length and the size of the mesh. (value exceeds 1)
                float yValue = iPoint / totalLength / m.localSizeOfMeshX / DataTools.NodeSetting.curveResolution; 

                // Locate the UV's in the array
                m.UVs[iUV] = new Vector2(xValue, yValue);
                if (m.symmetry)
                    m.UVs[iUV + indexOffsetForSymmetry] = new Vector2(xValue, yValue);

                // Add the distance to the next vert
                if (iLocal < m.amountVertex - 1) // Do not do this at the last index
                    vertDistance += Vector3.Distance(GetReorderdVertices(iLocal), GetReorderdVertices(iLocal + 1)); // Distance this vertex is from next vertex
            }
        }
    }

    // Function to copy paste the pre determined vertices, triangles, normals and UV's in the new mesh.
    // This will be used when the mesh given not only has vertices.
    public void SeperateMeshPlacement()
    {
        // Every vertice of the used mesh will be placed down before it starts with a new piece (Do not place last one)
        for (int i = 0; i < amountOfCurvePoints; i++)
        {
            Vector3 straight = curvePointInstance[i].direction;
            Vector3 left = curvePointInstance[i].perpendicular; // Get the direction the vertex should go

            for (int meshIndex = 0; meshIndex < m.amountVertex; meshIndex++)
                CreateVertex(true, meshIndex, i, left, straight);

            for (int iTriangle = 0; iTriangle < m.amountTriangle; iTriangle++)
                m.triangles[m.amountTriangle * i + iTriangle] = m.localTriangleData[iTriangle] + (i * m.amountVertex);

            for (int iNormal = 0; iNormal < m.amountVertex; iNormal++)
                m.normals[m.amountVertex * i + iNormal] = m.localNormalData[iNormal];

            // UV's cannot be mapped
            if (m.localUVData.Length == 0)
                MeshEditor.editor.RecieveMessage("There were no available UV's to map onto: " + m.usedMesh.name +" Aborting!", WarningStatus.Error);

            for (int iUV = 0; iUV < m.amountVertex; iUV++)
                m.UVs[m.amountVertex * i + iUV] = m.localUVData[iUV];
        }

        if (!m.flipNormals) // Flip the normals as default, because triangles are inverted when they are imported
            FlipTriangles();
    }

    public void FlipTriangles()
    {
        for (int iTriangle = 0; iTriangle < m.triangles.Length; iTriangle += 3)
        {
            int temp = m.triangles[iTriangle];
            m.triangles[iTriangle] = m.triangles[iTriangle + 1];
            m.triangles[iTriangle + 1] = temp;
        }
    }

    #endregion

    #region Calculation Tools

    // Calculate the length of the mesh
    private float CalculateCircumference()
    {
        float totalLength = 0;
        for(int iVert = 0; iVert < m.newVerticeOrder.Length - 1; iVert++)
        {
            totalLength += Vector3.Distance(GetReorderdVertices(iVert), GetReorderdVertices(iVert + 1));
        }
        return totalLength;
    }

    private Vector3 GetReorderdVertices(int index)
    {
        return m.newVerticeOrder[index % m.newVerticeOrder.Length];
    }

    #endregion

    #region Clean Tool

    /// <summary>
    /// Function to clean up a used mesh by trying to index the vertices correctly
    /// This will assume that the vertices are placed ontop of each other in the y-axis
    /// It will also assume that when the vertices need to come down it will be in the negative x-axis
    /// </summary>
    public void CleanMesh(MeshSettingsContainer mesh)
    {
        // Reset local mesh data
        mesh.localVerticeData = null;
        mesh.localTriangleData = null;
        mesh.localUVData = null;
        mesh.localNormalData = null;

        // The code will use the reordered vertices. If those are not available because the mesh already has faces and a order, it will set this reference
        if (mesh.isSeperateObj) { mesh.newVerticeOrder = mesh.localVerticeData; return; }

        mesh.newVerticeOrder = new Vector3[mesh.amountVertex];


        for (int iVert = 0; iVert < mesh.amountVertex; iVert++)
        {
            // Start at the highest index possible
            int newIndex = (mesh.localVerticeData[iVert].x < 0) ? mesh.amountVertex / 2 : mesh.amountVertex / 2 - 1;

            for (int otherVert = 0; otherVert < mesh.amountVertex; otherVert++)
            {
                if (otherVert == iVert) // Skip if same
                    continue;

                float vertDataX = mesh.localVerticeData[iVert].x;
                float vertDataY = mesh.localVerticeData[iVert].y;
                float otherVertDataX = mesh.localVerticeData[otherVert].x;
                float otherVertDataY = mesh.localVerticeData[otherVert].y;

                // Vertices need to be ordered by going up on the positive side and down the negative side
                if (vertDataX < 0 && otherVertDataX < 0) // Check if both vertices are on the negative sides
                {
                    if (vertDataY < otherVertDataY) // Increase index when the otherVert is higher
                        newIndex++;
                }
                else if (vertDataX > 0 && otherVertDataX > 0) // Check if both vertices are on the positive sides
                {
                    if (vertDataY < otherVertDataY) // Lower index when the otherVert is higher
                        newIndex--;
                }
            }

            mesh.newVerticeOrder[newIndex] = mesh.localVerticeData[iVert];
        }
    }

    #endregion
}
