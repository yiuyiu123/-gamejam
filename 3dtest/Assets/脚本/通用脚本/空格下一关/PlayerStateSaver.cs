using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerStateSaver
{
    [System.Serializable]
    public class PlayerState
    {
        public string playerName;
        public Vector3 position;
        public Quaternion rotation;
        public int health;
        public int score;
        // 添加其他需要保存的状态...
    }

    public static Dictionary<string, PlayerState> playerStates = new Dictionary<string, PlayerState>();

    public static void SavePlayerStates()
    {
        playerStates.Clear();

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            PlayerState state = new PlayerState
            {
                playerName = player.name,
                position = player.transform.position,
                rotation = player.transform.rotation,
                health = 100, // 从玩家组件获取实际值
                score = 0     // 从玩家组件获取实际值
            };

            playerStates[player.name] = state;
        }

        Debug.Log($"已保存 {players.Length} 名玩家状态");
    }

    public static void LoadPlayerStates()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            if (playerStates.ContainsKey(player.name))
            {
                PlayerState state = playerStates[player.name];
                player.transform.position = state.position;
                player.transform.rotation = state.rotation;
                // 恢复其他状态...
            }
        }

        Debug.Log($"已恢复 {players.Length} 名玩家状态");
    }
}

