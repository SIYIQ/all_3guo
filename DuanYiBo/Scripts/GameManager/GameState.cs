using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏核心状态
/// </summary>
public enum GameState
{
    StartMenu,    // 开始界面
    LevelPlaying, // 关卡运行中
    Paused,       // 暂停状态
    LevelPass,    // 关卡过关
    LevelComplete // 关卡通关
}
