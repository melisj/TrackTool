using UnityEngine;

public class NodeBehaviour : MonoBehaviour
{
    public NodeBehaviour nextNode, prevNode;
    public bool endNode;
    public bool startNode;
    public bool resetCurves = true;

    [SerializeField] private Color typeColor;
    [SerializeField] private GameObject adjustmentNodePrefab;

    // Curve info
    [SerializeField] public CurvePoint[] curvePoints;
    [SerializeField] public float curveLength;
    [SerializeField] private CurveAdjustmentNode[] adjustmentNodes;
    
    [System.Serializable]
    public struct CurvePoint
    {
        public Vector3 position;
        public Vector3 direction;
        public Vector3 perpendicular;

        public CurvePoint(Vector3 position, Vector3 direction)
        {
            this.position = position;
            this.direction = direction;

            perpendicular = Vector3.Cross(direction, Vector3.up);
        }
    }

    #region CreateConnection
    public bool InitNode()
    {
        SetAdjustmentNodes();
        return CreateCurvePoints();
    }
    
    // Set the color of this node
    public void SetType()
    {
        typeColor = startNode ? Color.green : Color.blue;
        if (endNode)
            typeColor = Color.red;
    }

    /// <summary>
    /// Function for adding and deleting the adjustment nodes for this nodes curves
    /// </summary>
    private void SetAdjustmentNodes()
    {
        adjustmentNodes = GetComponentsInChildren<CurveAdjustmentNode>();
        adjustmentNodePrefab = Resources.Load<GameObject>("AdjustmentNode");
        int amount = adjustmentNodes.Length; // Amount of nodes
        Transform thisTrans = transform; // Parent for new nodes

        if (!endNode && resetCurves)
        {
            if (amount != 2)
            {
                Debug.LogWarning(name + " Does not contain two adjustment nodes, nodes will be deleted or created");

                // Create and get the nodes
                if (amount < 2)
                {
                    adjustmentNodes = new CurveAdjustmentNode[2];
                    for (int i = 0; i < 2; i++) // Add if there are less than 2 nodes
                    {
                        GameObject adjustmentNode = Instantiate(adjustmentNodePrefab);
                        adjustmentNodes[i] = adjustmentNode.GetComponent<CurveAdjustmentNode>();

                        adjustmentNode.transform.SetParent(thisTrans);
                    }
                }
                // Delete node overflow
                else if (amount > 2)
                {
                    for (int i = 0; i < 2; i++) // Get the first two of the already present nodes
                        adjustmentNodes[i] = transform.GetChild(i).GetComponent<CurveAdjustmentNode>();
                    for (int i = transform.childCount - 1; i >= 2; i--) // Delete the remaining children
                        DestroyImmediate(transform.GetChild(i).gameObject);
                }
            }

            // Position the nodes
            for (int i = 0; i < 2; i++)
                adjustmentNodes[i].transform.position = Vector3.Lerp(transform.position, nextNode.transform.position, i == 0 ? 0.2f : 0.8f);
        }
        else if (endNode) // Delete the remaining children when this node is an end node
            for (int i = 0; i < transform.childCount; i++) // Delete the remaining children
                DestroyImmediate(transform.GetChild(i).gameObject);

        // Assign the line parents 
        for (int i = 0; i < adjustmentNodes.Length; i++)
            adjustmentNodes[i].AssignParentNode(i == 0 ? this : nextNode);
    }

    /// <summary>
    /// Function to calculate the curve of the track in more accuracy than the mesh is gonna be placed.
    /// This will calculate the length of the node to node curve.
    /// It will also calculate the points on the curve where the MakeEqualPoints function can use these points as placement
    /// </summary>
    public bool CreateCurvePoints()
    {
        // Error when there is no nextnode
        if (!nextNode)
        {
            NodeEditor.editor.RecieveMessage("Curve points could not be baked! There was no next node connected! Try connecting first!", WarningStatus.Warning);
            return false;
        }

        // Setup the temporary array's with the points and lengths of the segments
        Vector3[] tempPoints = new Vector3[NodeEditor.settings.curveAccuracy + 1];
        float[] tempSegment = new float[NodeEditor.settings.curveAccuracy + 1];
        curvePoints = null;
        curveLength = 0;

        Vector3 oldPoint = transform.position;
        Vector3 newPoint;
        for (int i = 0; i <= NodeEditor.settings.curveAccuracy; i++)
        {
            newPoint = GetCurvePoint(i / (float)NodeEditor.settings.curveAccuracy); // Get a new point on the curve between 0 and 1

            tempSegment[i] = Vector3.Distance(oldPoint, newPoint); // Calc distance
            tempPoints[i] = newPoint; // Set the new point in array

            curveLength += tempSegment[i]; // Add segment length to total length

            oldPoint = newPoint; // Do it for the next point
        }

        return MakeEqualPoints(tempPoints, tempSegment); // Return if it was succesfull
    }

    /// <summary>
    /// This function will use the points and segments lengths given to calculate which point it can use to make an accurate as possible equal curve.
    /// The function will use the curveLength to determine the distance between each equal point, depending on the amount of points per meter.
    /// The segmentLengths will determine which point the equal point will choose. 
    /// It will check for each segment if it is above the minimum distance it should be apart from the previous equal point, and use the unequal point as new equal point.
    /// </summary>
    /// <param name="unequalPoints"></param>
    /// <param name="segmentLengths"></param>
    private bool MakeEqualPoints(Vector3[] unequalPoints, float[] segmentLengths)
    {
        // The amount of points per meter * length in meters
        int curveRes = Mathf.FloorToInt(NodeEditor.settings.curveResolution * curveLength);
        curvePoints = new CurvePoint[curveRes];

        float segmentLength = curveLength / curveRes; // Mimimum distance between each equal point
        int pointIndex = 0; // Start index
        float currentLength = 0;

        for (int i = 0; i < curveRes; i++)
        {
            while ((i) * segmentLength > currentLength) // While the distance of this equal point is bigger than the currentLength of the segments
            {
                currentLength += segmentLengths[pointIndex]; // The length is not past the minimum length of distance between equal points (Add extra distance for the next point)
                pointIndex++; // Check for the next unequal point
            }

            // There was a critical error, regarding the amount of unequal points available
            if (pointIndex >= unequalPoints.Length)
            {
                NodeEditor.editor.RecieveMessage("There were insufficient points to calculate a curve, try bumping up the curve accuracy: " + transform.name, WarningStatus.Error);
                return false; // Return error
            }

            // Set the curve point in the array when the length was above the minimum distance
            curvePoints[i] = new CurvePoint(unequalPoints[pointIndex], 
                GetCurveDirection(pointIndex / (float)NodeEditor.settings.curveAccuracy)); // Set the direction of the point
        }

        return true; // Return if it was succesfull
    }
    #endregion

    #region Tools
    private Vector3 GetCurvePoint(float time)
    {
        if (nextNode)
        {
            try
            {
                return TrackManager.nodeManager.CalculateCurvePoint(time,
                    transform.position,
                    adjustmentNodes[0].transform.position,
                    adjustmentNodes[1].transform.position,
                    nextNode.transform.position);
            }
            catch
            {
                throw new System.NullReferenceException(("Node does not contain adjustment nodes: " + adjustmentNodes.Length + " nodes."));
            }
        }
        return Vector3.zero;
    }

    private Vector3 GetCurveDirection(float time)
    {
        if (nextNode)
        {
            try
            {
                return TrackManager.nodeManager.CalculateDirectionOnCurve(time,
                    transform.position,
                    adjustmentNodes[0].transform.position,
                    adjustmentNodes[1].transform.position,
                    nextNode.transform.position);
            }
            catch
            {
                throw new System.NullReferenceException(("Node does not contain adjustment nodes: " + adjustmentNodes.Length + " nodes."));
            }
        }
        return Vector3.zero;
    }

    #endregion

    #region DrawGizmos
    private void OnDrawGizmos()
    {
        Gizmos.color = typeColor;

        if (resetCurves)
            Gizmos.DrawIcon(transform.position + new Vector3(0,1,0), "Reset_Icon");

        if (NodeEditor.settings != null)
        {
            if (adjustmentNodes?.Length != 2 && !endNode) // Set the adjustmentnodes when they are not set yet
                SetAdjustmentNodes();

            if (NodeEditor.settings.renderNodes) // Draw the node
                Gizmos.DrawSphere(transform.position, NodeEditor.settings.nodeSize);
            if (nextNode && NodeEditor.settings.renderLines) // Draw the lines connected to the next node
                Gizmos.DrawLine(transform.position, nextNode.transform.position);

            if (NodeEditor.settings.renderCurves && !endNode) // Draw the two curves
            {
                DrawCurvePreview();
                DrawCurve();
            }
        }
    }

    private void DrawCurvePreview()
    {
        Vector3 oldPoint = transform.position;
        Vector3 newPoint = Vector3.zero;

        for (int i = 0; i <= NodeEditor.settings.curvePreviewResolution; i++)
        {
            newPoint = GetCurvePoint(i / (float)NodeEditor.settings.curvePreviewResolution);
            if (newPoint == Vector3.positiveInfinity)
                break;

            Gizmos.DrawLine(oldPoint, newPoint);

            oldPoint = newPoint;
        }
    }

    private void DrawCurve()
    {
        if (curvePoints != null)
            foreach (CurvePoint point in curvePoints)
                Gizmos.DrawMesh(NodeEditor.settings.curvePointMesh, point.position);
    }
    #endregion
}
