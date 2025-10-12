using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneInitializer : MonoBehaviour
{
    void Start()
    {
        // 场景加载后恢复玩家状态
        PlayerStateSaver.LoadPlayerStates();

        // 启用所有玩家的输入
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
