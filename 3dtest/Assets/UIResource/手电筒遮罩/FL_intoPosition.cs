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
            // 将玩家世界坐标写入 shader
            rend.material.SetVector("_Playerpos", player.position);

            // 可选：控制是否启用（性能优化）
            float dist = Vector3.Distance(player.position, transform.position);
            if (dist < 50f)
                rend.enabled = true;
            else
                rend.enabled = false;
        }
    }
}