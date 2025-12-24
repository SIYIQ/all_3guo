using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI面板类型（与实际面板预制体名称对应）
/// </summary>
public enum UIPanelType
{
    StartPanel,   // 开始界面
    LevelUIPanel, // 关卡内UI（血量、分数等）
    PausePanel,   // 暂停界面
    PassPanel,    // 过关界面
    CompletePanel // 通关界面
}
