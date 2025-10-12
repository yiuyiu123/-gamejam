using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneInitializer : MonoBehaviour
{
    void Start()
    {
        // �������غ�ָ����״̬
        PlayerStateSaver.LoadPlayerStates();

        // ����������ҵ�����
        EnableAllPlayersInput();
    }

    void EnableAllPlayersInput()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // playerController.SetInputEnabled(true);
            }
        }
    }
}
