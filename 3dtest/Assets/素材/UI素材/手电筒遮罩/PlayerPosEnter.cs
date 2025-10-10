using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PlayerPosEnter : MonoBehaviour
{
    public Transform playeTrans;
    private Transform objectTrans;
    private Renderer rend;
    private Vector3 trans;
    Vector3 dir;
    public float distance;
    
    void Start()
    {
        rend = GetComponent<Renderer>();
        objectTrans = GetComponent<Transform>();
    }

    void Update()
    {

        if (rend != null && playeTrans != null)
        {
            dir = (playeTrans.transform.position - objectTrans.position);
            trans = playeTrans.position + dir.normalized * distance;
            // �������������д�� shader
            rend.material.SetVector("_Playerpos", trans);
            

            // ��ѡ�������Ƿ����ã������Ż���
            float dist = Vector3.Distance(playeTrans.position, objectTrans.position);
            if (dist < 50f)
                rend.enabled = true;
            else
                rend.enabled = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, playeTrans.position);
    }
}
