using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using static UIPathData;

public class UIPathData
{
    public enum UIType
    {
        TYPE_ITEM,          // 只是用资源路径
        TYPE_BASE,          // 常驻场景的UI，Close不销毁 一级UI
        TYPE_POP,           // 弹出式UI，互斥，当前只能有一个弹出界面 二级弹出 在一级之上 阻止移动 无法操作后面UI
        TYPE_STORY,         // 故事界面，故事界面出现，所有UI消失，故事界面关闭，其他界面恢复
        TYPE_TIP,           // 三级弹出 在二级之上 不互斥 阻止移动 无法操作后面UI
        TYPE_MENUPOP,       // TYPE_POP的一个分支 由主菜单MenuBar打开的二级UI 主要用于动态加载特殊屏蔽区域 其他和POPUI完全一致
        TYPE_MESSAGE,       // 消息提示UI 在三级之上 一般是最高层级 不互斥 不阻止移动 可操作后面UI
        TYPE_DEATH,         // 死亡UI 目前只有复活界面 用于添加复活特殊规则
    };

    public string path;
    public string name;
    public UIType uiType;
    public string uiGroupName;
    public bool isMainAsset;            // 是否是主资源，如果主资源UI被关闭
    public bool isDestroyOnUnload;

    public static Dictionary<string, UIPathData> m_DicUIInfo = new Dictionary<string, UIPathData>();
    public static Dictionary<string, UIPathData> m_DicUIName = new Dictionary<string, UIPathData>();
    public UIPathData(string _path, UIType _uiType, string groupName = null, bool bMainAsset = false, bool bDestroyOnUnload = true)
    {
        path = _path;
        uiType = _uiType;
        int lastPathPos = _path.LastIndexOf('/');
        if (lastPathPos > 0)
        {
            name = path.Substring(lastPathPos + 1);
        }
        else
        {
            name = path;
        }

        uiGroupName = groupName;
        isMainAsset = bMainAsset;

        isDestroyOnUnload = bDestroyOnUnload;

#if UNITY_ANDROID

        // < 768M UI不进行缓存
        if (SystemInfo.systemMemorySize < 768)
        {
            isDestroyOnUnload = true;
        }

#endif

        m_DicUIInfo.Add(_path, this);
        m_DicUIName.Add(name, this);
    }

}
/// <summary>
/// UI路径及对外接口
/// </summary>
public class UIInfo
{

}
