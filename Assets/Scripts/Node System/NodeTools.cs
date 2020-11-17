using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeTools
{
    public Vector3 CalculateCurvePoint(float timeAlongCurve, Vector3 P0, Vector3 P1, Vector3 P2, Vector3 P3) {
        float t = timeAlongCurve;
        float tInv = 1 - t;
        Vector3 point = Vector3.zero;

        // (1-t)^3 * P0 +
        // 3 * (1-t)^2 * t * P1 +
        // 3 * (1-t) * t^2 * P2 +
        // t^3 * P3
        point += Mathf.Pow(tInv, 3) * P0;
        point += 3 * Mathf.Pow(tInv, 2) * t * P1;
        point += 3 * tInv * (t * t) * P2;
        point += (t * t * t) * P3;

        return point;
    }

    public Vector3 CalculateDirectionOnCurve(float timeAlongCurve, Vector3 P0, Vector3 P1, Vector3 P2, Vector3 P3) {
        float t = timeAlongCurve;
        float tInv = 1 - t; // Inverse t
        Vector3 direction = Vector3.zero;

        // 3 * (1-t)^2 * (P1 - P0) +
        // 6 * (1-t) * t * (P2 - P1) +
        // 3t^2 * (P3 - P2)
        direction += 3 * Mathf.Pow(tInv, 2) * (P1 - P0);
        direction += 6 * tInv * t * (P2 - P1);
        direction += 3 * Mathf.Pow(t, 2) * (P3 - P2);

        return direction.normalized;
    }

    public NodeBehaviour GetAdjacentNode(float timeOnCurve, NodeBehaviour fromNode) {
        if (timeOnCurve > 1) {
            // Change to the nextnode
            return (fromNode.nextNode != null) ? fromNode.nextNode : DataTools.allNodes[0];
        } else {
            // Change to the prevnode
            return (fromNode.prevNode != null) ? fromNode.prevNode : DataTools.allNodes[DataTools.allNodes.Length - 1];
        }
    }
}
