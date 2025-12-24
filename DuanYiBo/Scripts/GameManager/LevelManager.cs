using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    // 当前关卡编号
    public int CurrentLevel { get; private set; } = 1;
    // 最大关卡数
    public int MaxLevel = 10;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 加载指定关卡
    /// </summary>
    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 1 || levelIndex > MaxLevel)
        {
            Debug.LogError($"无效关卡编号：{levelIndex}");
            return;
        }

        CurrentLevel = levelIndex;
        // 假设关卡场景命名为 "Level1", "Level2"...
        SceneManager.LoadScene(CurrentLevel);

        // 通知GameManager切换状态
        GameManager.Instance.ChangeGameState(GameState.LevelPlaying);
    }

    /// <summary>
    /// 加载下一关
    /// </summary>
    public void LoadNextLevel()
    {
        if (CurrentLevel >= MaxLevel)
        {
            Debug.Log("已通关所有关卡！");
            // 可自定义“全部通关”逻辑
            return;
        }
        LoadLevel(CurrentLevel + 1);
    }

    /// <summary>
    /// 重新加载当前关卡
    /// </summary>
    public void ReloadCurrentLevel()
    {
        LoadLevel(CurrentLevel);
    }

    /// <summary>
    /// 判定关卡通关
    /// </summary>
    public void CompleteLevel()
    {
        GameManager.Instance.ChangeGameState(GameState.LevelComplete);
    }
}