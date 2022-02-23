using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Camera cam;

    public float smooth = 0.5f;
    public float limitDist = 20.0f;

    private void FixedUpdate()
    {
        if(cam == null)
        {
            cam = GetComponentInChildren<Camera>();
            return;
        }

        Follow();
    }

    private void Follow()
    {
        float distance = Vector3.Distance(transform.position, cam.transform.position);
        if (distance > limitDist)
            cam.transform.position = Vector3.Lerp(cam.transform.position, transform.position, Time.deltaTime * smooth);
    }
}
