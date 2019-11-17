using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObject : MonoBehaviour
{
    NodeBehaviour currentNode;
    public float speed;
    public float currentPosOnCurve;
    public float timeAlongCurve;

    private void Start()
    {
        currentNode = DataTools.allNodes[0];
    }

    public void UpdateObj()
    {
        // Update the speed and position of this object
        currentPosOnCurve += speed;
        if (currentPosOnCurve > currentNode.curveLength)
            ChangeNode();

        // Calculate the time along the curve
        timeAlongCurve = currentPosOnCurve / currentNode.curveLength;
        
        // Set the rotation and the posistion of the object
        transform.position = currentNode.GetCurvePoint(timeAlongCurve);
        transform.rotation = Quaternion.LookRotation(currentNode.GetCurveDirection(timeAlongCurve));
    }

    private void ChangeNode()
    {
        if (currentNode.curveLength != 0) // Set the new node so this object will keep on following the next node
        {
            // Set the new pos on the new curve with the overshot distance of the prev curve
            currentPosOnCurve %= currentNode.curveLength;

            // Change to the nextnode
            if (currentNode.nextNode)
                currentNode = currentNode.nextNode;
        }
        else // If the end of the line has been reached, respawn to the start
        {
            currentNode = DataTools.allNodes[0];
            currentPosOnCurve %= currentNode.curveLength;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
}
