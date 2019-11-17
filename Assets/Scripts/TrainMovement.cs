using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainMovement : MonoBehaviour
{
    public FollowObject front, rear;

    private void Awake()
    {
        front.currentPosOnCurve = Vector3.Distance(front.transform.position, rear.transform.position);
    }

    void Update()
    {
        // Update the wheels
        front.UpdateObj();
        rear.UpdateObj();

        // Calculate rotation
        Vector3 difference = front.transform.position - rear.transform.position;
        transform.rotation = Quaternion.LookRotation(difference);

        // Calculate position
        transform.position = front.transform.position - difference / 2;
    }
}
