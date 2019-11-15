using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataTools
{
    #region Node Info

    private static List<NodeBehaviour.CurvePoint> _curvePoints;
    public static List<NodeBehaviour.CurvePoint> curvePoints { get { return (_curvePoints == null) ? SetAllDataPoint() : _curvePoints; } private set { _curvePoints = value; } } // Current Set of curve data points

    public static float totalLength;

    public static List<NodeBehaviour.CurvePoint> SetAllDataPoint()
    {
        List<NodeBehaviour.CurvePoint> tempPoints = new List<NodeBehaviour.CurvePoint>();
        NodeBehaviour currentNode = TrackManager.nodeManager.allNodes[0]; // Get the first node

        totalLength = 0;

        // Get all the connected nodes with all the data points in them
        while (currentNode.nextNode != null) // Check if there is a nextnode connected
        {
            if (currentNode.curvePoints != null) // Check if the curves have been baked
            {
                foreach (NodeBehaviour.CurvePoint point in currentNode.curvePoints) // Get all the curve data
                {
                    tempPoints.Add(point); // Store data
                }

                totalLength += currentNode.curveLength; // Calculate the total length
            }

            currentNode = currentNode.nextNode; // Check next node
        }

        curvePoints = tempPoints;
        return tempPoints;
    }
    #endregion
}
