using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{

    [Tooltip("建议值：30-100")]
    public float rotationSpeed = 50f; // 默认值避免为0 

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // 增加Time.deltaTime 保证帧率无关 
        transform.Rotate(
            v * rotationSpeed * Time.deltaTime,
            h * rotationSpeed * Time.deltaTime,
            0,
            Space.World // 明确坐标系 
        );
    }
}
