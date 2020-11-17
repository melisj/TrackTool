using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObject : MonoBehaviour
{
    NodeBehaviour currentNode;
    public float currentPosOnCurve;

    float factorUp = 1.1f;
    float factorDown = 0.9f;
    public int accuracy;

    private void Start() {
        currentNode = DataTools.allNodes[0];
        OnUpdate(0);
    }

    public void OnUpdate(float distance) {
        currentPosOnCurve = CalculateDistanceOnCurve(distance, out NodeBehaviour newNode);
        currentNode = newNode;

        // Set the rotation and the posistion of the object
        transform.position = currentNode.GetCurvePoint(currentPosOnCurve);
        transform.rotation = Quaternion.LookRotation(currentNode.GetCurveDirection(currentPosOnCurve));
    }

    public float CalculateDistanceOnCurve(float distance, out NodeBehaviour node) {
        float targetDistance = distance; 
        float timeStepCurve = targetDistance / currentNode.curveLength;
        float newTimeCurve = currentPosOnCurve + timeStepCurve;
        float currentDistance = 0;
        NodeBehaviour checkNode = currentNode;
        accuracy = 0;
        factorUp = 1.1f;
        factorDown = 0.9f;

        // Adjust distance by incrmenting the time on curve
        int i = 0;
        do {
            // Check if the train moves over a node
            checkNode = GetNode(ref newTimeCurve, checkNode);

            // Check distance between current pos and the new curve pos
            currentDistance = Vector3.Distance(transform.position, checkNode.GetCurvePoint(newTimeCurve));

            // Add new timestep to check for
            newTimeCurve -= timeStepCurve;
            timeStepCurve = AdjustTimeOnCurve(timeStepCurve, currentDistance, targetDistance, ref accuracy);
            newTimeCurve += timeStepCurve;

            i++;
        } while (i < 400);

        node = checkNode;
        return newTimeCurve;
    }

    // Change the node if neccessary
    private NodeBehaviour GetNode(ref float posOnCurve, NodeBehaviour checkNode) {
        if (posOnCurve > 1 || posOnCurve < 0) {
            NodeBehaviour newNode = TrackManager.nodeManager.tools.GetAdjacentNode(posOnCurve, checkNode);
            posOnCurve += (posOnCurve > 1) ? -1 : 1;
            return newNode;
        }

        return checkNode;
    }

    private float AdjustTimeOnCurve(float timeOnCurve, float currentDistance, float targetDistance, ref int accuracy) {
        if (currentDistance != 0) {
            if (targetDistance > 0) {
                timeOnCurve *= (currentDistance < targetDistance) ? factorUp : factorDown;
            } else {
                currentDistance *= -1;
                timeOnCurve *= (currentDistance > targetDistance) ? factorUp : factorDown;
            }

            // Increase accuracy of calculation 
            if (Mathf.Abs(targetDistance - currentDistance) < Mathf.Abs(targetDistance) / Mathf.Pow(10, accuracy)) {
                accuracy++;
                factorDown += 9 / Mathf.Pow(10, accuracy + 1);
                factorUp -= 9 / Mathf.Pow(10, accuracy + 1);
            }
        }
        return timeOnCurve;
    }

    private void OnDrawGizmos() {
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
}
