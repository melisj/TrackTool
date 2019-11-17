using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLookAt : MonoBehaviour
{

    public GameObject obj;

    void Update()
    {
        Vector3 diff = obj.transform.position - Camera.main.transform.position;

       Camera.main.transform.rotation = Quaternion.LookRotation(diff);
    }
}
