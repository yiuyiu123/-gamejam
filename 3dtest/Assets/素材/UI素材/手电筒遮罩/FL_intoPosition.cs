using UnityEngine;

public class FL_intoPosition : MonoBehaviour
{
    public Transform player;
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        if (rend != null && player != null)
        {
            // �������������д�� shader
            rend.material.SetVector("_Playerpos", player.position);

            // ��ѡ�������Ƿ����ã������Ż���
            float dist = Vector3.Distance(player.position, transform.position);
            if (dist < 50f)
                rend.enabled = true;
            else
                rend.enabled = false;
        }
    }
}