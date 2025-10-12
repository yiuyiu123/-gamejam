using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{

    [Tooltip("����ֵ��30-100")]
    public float rotationSpeed = 50f; // Ĭ��ֵ����Ϊ0 

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // ����Time.deltaTime ��֤֡���޹� 
        transform.Rotate(
            v * rotationSpeed * Time.deltaTime,
            h * rotationSpeed * Time.deltaTime,
            0,
            Space.World // ��ȷ����ϵ 
        );
    }
}
