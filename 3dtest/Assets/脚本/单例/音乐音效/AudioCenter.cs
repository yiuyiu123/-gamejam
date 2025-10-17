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
            // �� AudioListener ��Զλ��������е�
            Vector3 midpoint = (player1.position + player2.position) / 2f;
            transform.position = Vector3.Lerp(transform.position, midpoint, Time.deltaTime * smoothFollow);
        }
    }
}
