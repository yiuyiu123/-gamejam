using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class L_flahLight : MonoBehaviour
{
    public float rotateStep = 15f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            transform.Rotate(Vector3.forward * rotateStep);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            transform.Rotate(Vector3.forward * -rotateStep);
        }
    }
}
