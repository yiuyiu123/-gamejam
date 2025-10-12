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
        // ���������Ҫ�����״̬...
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
                health = 100, // ����������ȡʵ��ֵ
                score = 0     // ����������ȡʵ��ֵ
            };

            playerStates[player.name] = state;
        }

        Debug.Log($"�ѱ��� {players.Length} �����״̬");
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
                // �ָ�����״̬...
            }
        }

        Debug.Log($"�ѻָ� {players.Length} �����״̬");
    }
}

