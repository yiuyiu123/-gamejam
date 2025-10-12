using UnityEngine;
using System;

public enum GameState
{
    Anima,   // 被动播放动画，玩家禁止跳过
    Video,   // 被动播放动画，玩家可以跳过
    Playing, // 正常游戏
    Paused,  // 暂停
    GameOver,// 关卡结束
    Victory  // 关卡胜利
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.Anima;
    public event Action<GameState> OnGameStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 状态切换
    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"GameState changed to: {newState}");

        // 触发事件
        OnGameStateChanged?.Invoke(CurrentState);

        // 根据状态执行不同逻辑
        switch (CurrentState)
        {
            case GameState.Anima:
                // 禁止玩家操作
                Debug.Log("播放被动画控制的场景，禁止跳过");
                break;

            case GameState.Video:
                // 可以跳过的动画或视频
                Debug.Log("播放可跳过的视频/动画");
                break;

            case GameState.Playing:
                // 开始正常游戏
                Debug.Log("玩家可以操作，游戏进行中");
                break;

            case GameState.Paused:
                // 暂停游戏
                Debug.Log("游戏暂停");
                Time.timeScale = 0f; // 暂停游戏时间
                break;

            case GameState.GameOver:
                // 关卡结束，显示失败面板
                Debug.Log("游戏结束");
                UI_Manager.Instance?.ShowLose();
                break;

            case GameState.Victory:
                // 关卡胜利，显示胜利面板
                Debug.Log("关卡胜利");
                UI_Manager.Instance?.ShowVictory();
                break;

            default:
                Debug.LogWarning("未处理的游戏状态");
                break;
        }

        // 恢复时间流速（避免暂停状态持续影响其他状态）
        if (CurrentState != GameState.Paused)
        {
            Time.timeScale = 1f;
        }
    }

    public void StartGame() => ChangeState(GameState.Playing);
    public void PauseGame() => ChangeState(GameState.Paused);
    public void GameOver() => ChangeState(GameState.GameOver);
    public void WinGame() => ChangeState(GameState.Victory);
}
