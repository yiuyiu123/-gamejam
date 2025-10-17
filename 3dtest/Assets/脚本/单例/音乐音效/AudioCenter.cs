using UnityEngine;

public class AudioCenter : MonoBehaviour
{
    public Transform player1;
    public Transform player2;
    public float smoothFollow = 5f;

    void LateUpdate()
    {
        if (player1 && player2)
        {
            // 让 AudioListener 永远位于两玩家中点
            Vector3 midpoint = (player1.position + player2.position) / 2f;
            transform.position = Vector3.Lerp(transform.position, midpoint, Time.deltaTime * smoothFollow);
        }
    }
}
