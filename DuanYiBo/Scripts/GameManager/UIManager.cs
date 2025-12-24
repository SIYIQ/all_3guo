using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    // 单例
    public static UIManager Instance;

    // 存储所有UI面板
    private Dictionary<UIPanelType, BaseUIPanel> panelDict = new Dictionary<UIPanelType, BaseUIPanel>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 跨场景保留
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 自动获取所有子面板（也可手动赋值）
        BaseUIPanel[] allPanels = GetComponentsInChildren<BaseUIPanel>(true);
        foreach (var panel in allPanels)
        {
            if (!panelDict.ContainsKey(panel.panelType))
            {
                panelDict.Add(panel.panelType, panel);
            }
            else
            {
                Debug.LogError($"重复的UI面板类型：{panel.panelType}");
            }
        }
    }

    /// <summary>
    /// 显示指定面板
    /// </summary>
    public void ShowPanel(UIPanelType type)
    {
        if (panelDict.TryGetValue(type, out BaseUIPanel panel))
        {
            panel.Show();
        }
        else
        {
            Debug.LogError($"未找到UI面板：{type}");
        }
    }

    /// <summary>
    /// 隐藏指定面板
    /// </summary>
    public void HidePanel(UIPanelType type)
    {
        if (panelDict.TryGetValue(type, out BaseUIPanel panel))
        {
            panel.Hide();
        }
        else
        {
            Debug.LogError($"未找到UI面板：{type}");
        }
    }

    /// <summary>
    /// 隐藏所有面板
    /// </summary>
    public void HideAllPanels()
    {
        foreach (var panel in panelDict.Values)
        {
            panel.Hide();
        }
    }

    /// <summary>
    /// 获取指定面板（用于自定义逻辑）
    /// </summary>
    public T GetPanel<T>(UIPanelType type) where T : BaseUIPanel
    {
        if (panelDict.TryGetValue(type, out BaseUIPanel panel))
        {
            return panel as T;
        }
        return null;
    }
}
