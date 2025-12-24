using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUIPanel : MonoBehaviour
{
    // 面板对应的类型
    public UIPanelType panelType;
    // 是否默认隐藏
    public bool isHideByDefault = true;

    protected virtual void Awake()
    {
        if (isHideByDefault)
        {
            Hide();
        }
    }

    /// <summary>
    /// 显示面板（可重写添加动画等逻辑）
    /// </summary>
    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 隐藏面板（可重写添加动画等逻辑）
    /// </summary>
    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }
}
