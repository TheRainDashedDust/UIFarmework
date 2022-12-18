using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;

public interface IUIWidget
{
    void OnLoad(params object[] args);
}
public class UIManager : MonoBehaviour
{
    public delegate void OnOpenUIDelegate(bool bSuccess, object param);
    public delegate void OnLoadUIItemDelegate(GameObject resItem, object param);

    private Transform BaseUIRoot;       // 位于UI最底层，常驻场景，基础交互
    private Transform PopUIRoot;        // 位于UI上层，弹出式，互斥
    private Transform StoryUIRoot;      // 故事背景层
    private Transform TipUIRoot;        // 位于UI顶层，弹出重要提示信息等
    private Transform MenuPopUIRoot;    // 菜单根节点弹出的UI
    private Transform MessageUIRoot;    // 信息UI
    private Transform DeathUIRoot;      // 死亡UI

    private Dictionary<string, GameObject> m_dicTipUI = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> m_dicBaseUI = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> m_dicPopUI = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> m_dicStoryUI = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> m_dicMenuPopUI = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> m_dicMessageUI = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> m_dicDeathUI = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> m_dicCacheUI = new Dictionary<string, GameObject>();

    private Dictionary<string, int> m_dicWaitLoad = new Dictionary<string, int>();

    private static UIManager m_instance;
    public static UIManager Instance
    {
        get
        {
            return m_instance;
        }
    }

    private const int GCCollectTime = 1;

    private void Awake()
    {
        m_dicTipUI.Clear();
        m_dicBaseUI.Clear();
        m_dicPopUI.Clear();
        m_dicStoryUI.Clear();
        m_dicMenuPopUI.Clear();
        m_dicMessageUI.Clear();
        m_dicDeathUI.Clear();
        m_dicCacheUI.Clear();
        m_instance = this;

        BaseUIRoot = transform.Find("BaseUIRoot");
        if (null == BaseUIRoot)
        {
            BaseUIRoot = AddObjToRoot("BaseUIRoot").transform;
        }

        PopUIRoot = transform.Find("PopUIRoot");
        if (null == PopUIRoot)
        {
            PopUIRoot = AddObjToRoot("PopUIRoot").transform;
        }

        StoryUIRoot = transform.Find("StoryUIRoot");
        if (null == StoryUIRoot)
        {
            StoryUIRoot = AddObjToRoot("StoryUIRoot").transform;
        }

        TipUIRoot = transform.Find("TipUIRoot");
        if (null == TipUIRoot)
        {
            TipUIRoot = AddObjToRoot("TipUIRoot").transform;
        }

        MenuPopUIRoot = transform.Find("MenuPopUIRoot");
        if (null == MenuPopUIRoot)
        {
            MenuPopUIRoot = AddObjToRoot("MenuPopUIRoot").transform;
        }

        MessageUIRoot = transform.Find("MessageUIRoot");
        if (null == MessageUIRoot)
        {
            MessageUIRoot = AddObjToRoot("MessageUIRoot").transform;
        }

        DeathUIRoot = transform.Find("DeathUIRoot");
        if (null == DeathUIRoot)
        {
            DeathUIRoot = AddObjToRoot("DeathUIRoot").transform;
        }

        BaseUIRoot.gameObject.SetActive(true);
        TipUIRoot.gameObject.SetActive(true);
        PopUIRoot.gameObject.SetActive(true);
        StoryUIRoot.gameObject.SetActive(true);
        MenuPopUIRoot.gameObject.SetActive(true);
        MessageUIRoot.gameObject.SetActive(true);
        DeathUIRoot.gameObject.SetActive(true);
    }
    private GameObject AddObjToRoot(string name)
    {
        GameObject obj = new GameObject();
        obj.transform.parent = transform;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;
        obj.name = name;
        return obj;
    }
    private void OnDestroy()
    {
        m_instance= null;
    }
    public static bool LoadItem(UIPathData pathData, OnLoadUIItemDelegate delLoadItem, object param = null)
    {
        if (null == m_instance)
        {
           
            return false;
        }

        m_instance.LoadUIItem(pathData, delLoadItem, param);
        return true;
    }
    public static bool ShowUI(UIPathData pathData, OnOpenUIDelegate delOpenUI = null, object param = null)
    {
        if (null == m_instance)
        {
            
            return false;
        }

        m_instance.AddLoadDicRefCount(pathData.name);
        Dictionary<string, GameObject> curDic = null;
        switch (pathData.uiType)
        {
            case UIPathData.UIType.TYPE_BASE:
                curDic = m_instance.m_dicBaseUI;
                break;
            case UIPathData.UIType.TYPE_POP:
                curDic = m_instance.m_dicPopUI;
                break;
            case UIPathData.UIType.TYPE_STORY:
                curDic = m_instance.m_dicStoryUI;
                break;
            case UIPathData.UIType.TYPE_TIP:
                curDic = m_instance.m_dicTipUI;
                break;
            case UIPathData.UIType.TYPE_MENUPOP:
                curDic = m_instance.m_dicMenuPopUI;
                break;
            case UIPathData.UIType.TYPE_MESSAGE:
                curDic = m_instance.m_dicMessageUI;
                break;
            case UIPathData.UIType.TYPE_DEATH:
                curDic = m_instance.m_dicDeathUI;

                break;
            default:
                return false;
        }

        if (null == curDic)
        {
            return false;
        }

        if (m_instance.m_dicCacheUI.ContainsKey(pathData.name))
        {
            if (!curDic.ContainsKey(pathData.name))
            {
                curDic.Add(pathData.name, m_instance.m_dicCacheUI[pathData.name]);
            }

            m_instance.m_dicCacheUI.Remove(pathData.name);
        }

        if (curDic.ContainsKey(pathData.name))
        {
            curDic[pathData.name].SetActive(true);
            m_instance.DoAddUI(pathData, curDic[pathData.name], delOpenUI, param);
            return true;
        }

        m_instance.LoadUI(pathData, delOpenUI, param);

        return true;
    }
    // 读表展示UI，
    public static bool ShowUIByID(int tableID, OnOpenUIDelegate delOpenUI = null, object param = null)
    {
        if (null == m_instance)
        {
            
            return false;
        }

        /*Tab_UIPath curTabPath = TableManager.GetUIPathByID(tableID, 0);
        if (null == curTabPath)
        {
            LogModule.ErrorLog("cur ui is not set in table" + tableID);
            return false;
        }

        if (!UIPathData.m_DicUIInfo.ContainsKey(curTabPath.Path))
        {
            LogModule.ErrorLog("cur ui is not set in table" + curTabPath.Path);
            return false;
        }*/

        UIPathData curData = UIPathData.m_DicUIInfo["Path"];
        return UIManager.ShowUI(curData, delOpenUI, param);

    }

    public static void CloseUIByID(int tableID)
    {
        

    }

    // 关闭UI，根据类型不同，触发不同行为
    public static void CloseUI(UIPathData pathData)
    {
        
    }


    void DoAddUI(UIPathData uiData, GameObject curWindow, object fun, object param)
    {

    }
    public void HideAllUILayer()
    {
        BaseUIRoot.gameObject.SetActive(false);
        TipUIRoot.gameObject.SetActive(false);
        PopUIRoot.gameObject.SetActive(false);
        MenuPopUIRoot.gameObject.SetActive(false);
        MessageUIRoot.gameObject.SetActive(false);
        StoryUIRoot.gameObject.SetActive(false);
    }

    public void ShowAllUILayer()
    {
        BaseUIRoot.gameObject.SetActive(true);
        TipUIRoot.gameObject.SetActive(true);
        PopUIRoot.gameObject.SetActive(true);
        MenuPopUIRoot.gameObject.SetActive(true);
        MessageUIRoot.gameObject.SetActive(true);
        StoryUIRoot.gameObject.SetActive(true);
    }
    IEnumerator GCAfterOneSceond()
    {
        yield return new WaitForSeconds(1);

        // Resources.UnloadUnusedAssets();
        //System.GC.Collect();
    }
    void DoLoadUIItem(UIPathData uiData, GameObject curItem, object fun, object param)
    {
        if (null != fun)
        {
            OnLoadUIItemDelegate delLoadItem = fun as OnLoadUIItemDelegate;
            delLoadItem(curItem, param);
        }
    }
    void ClosePopUI(string name)
    {
        OnClosePopUI(m_dicPopUI, name);
    }
    void CloseStoryUI(string name)
    {
        if (TryDestroyUI(m_dicStoryUI, name))
        {
            BaseUIRoot.gameObject.SetActive(true);
            TipUIRoot.gameObject.SetActive(true);
            PopUIRoot.gameObject.SetActive(true);
            MenuPopUIRoot.gameObject.SetActive(true);
            MessageUIRoot.gameObject.SetActive(true);
            StoryUIRoot.gameObject.SetActive(true);
        }
    }

    void CloseBaseUI(string name)
    {
        if (m_dicBaseUI.ContainsKey(name))
        {
            m_dicBaseUI[name].SetActive(false);
        }
    }

    void CloseTipUI(string name)
    {
        TryDestroyUI(m_dicTipUI, name);
    }

    void CloseMenuPopUI(string name)
    {
        OnClosePopUI(m_dicMenuPopUI, name);
    }

    void CloseMessageUI(string name)
    {
        TryDestroyUI(m_dicMessageUI, name);
    }

    void CloseDeathUI(string name)
    {
        if (TryDestroyUI(m_dicDeathUI, name))
        {
            // 关闭复活界面时 恢复节点的显示
            m_instance.PopUIRoot.gameObject.SetActive(true);
            m_instance.MenuPopUIRoot.gameObject.SetActive(true);
            m_instance.TipUIRoot.gameObject.SetActive(true);
            m_instance.BaseUIRoot.gameObject.SetActive(true);
        }
    }
    void LoadUI(UIPathData uiData, OnOpenUIDelegate delOpenUI = null, object param1 = null)
    {
        
    }

    void LoadUIItem(UIPathData uiData, OnLoadUIItemDelegate delLoadItem, object param = null)
    {
        
    }

    static void LoadMenuSubUIShield(GameObject newWindow)
    {
       
    }
    static void LoadPopUIShield(GameObject newWindow)
    {
        
    }
    bool SubUIShow()
    {
        
		 //防止下面问题出现直接返回false
		 //任务寻路到NPC的过程中，点击小聊天框内装备链接，弹出装备Tips，任务寻路结束后，弹出NPC对话面板，结束对话后，方向操控按钮失效
		 
        //if (m_dicPopUI.Count + m_dicStoryUI.Count + m_dicTipUI.Count + m_dicMenuPopUI.Count > 0)
        //{
        //    return true;
        //}
        //else
        //{
        return false;
        //}
    }
    public static bool IsSubUIShow()
    {
        if (m_instance != null)
        {
            return m_instance.SubUIShow();
        }
        return false;
    }

    static void ReliveCloseOtherSubUI()
    {
        // 关闭所有UI
        
    }

    static public void NewPlayerGuideCloseSubUI()
    {
        // 关闭所有PopUI
        foreach (KeyValuePair<string, GameObject> pair in m_instance.m_dicPopUI)
        {
            m_instance.ClosePopUI(pair.Key);
            break;
        }
        // 关闭所有MenuPopUI
        foreach (KeyValuePair<string, GameObject> pair in m_instance.m_dicMenuPopUI)
        {
            m_instance.CloseMenuPopUI(pair.Key);
            break;
        }
        // 关闭所有TipUI
        foreach (KeyValuePair<string, GameObject> pair in m_instance.m_dicTipUI)
        {
            m_instance.CloseTipUI(pair.Key);
            break;
        }
        // 关闭所有MessageUI
        //         foreach (KeyValuePair<string, GameObject> pair in m_instance.m_dicMessageUI)
        //         {
        //             m_instance.CloseMessageUI(pair.Key);
        //             break;
        //         }
    }

    void AddLoadDicRefCount(string pathName)
    {
        if (m_dicWaitLoad.ContainsKey(pathName))
        {
            m_dicWaitLoad[pathName]++;
        }
        else
        {
            m_dicWaitLoad.Add(pathName, 1);
        }
    }

    bool RemoveLoadDicRefCount(string pathName)
    {
        if (!m_dicWaitLoad.ContainsKey(pathName))
        {
            return false;
        }

        m_dicWaitLoad[pathName]--;
        if (m_dicWaitLoad[pathName] <= 0)
        {
            m_dicWaitLoad.Remove(pathName);
        }

        return true;
    }

    void DestroyUI(string name, GameObject obj)
    {
        Destroy(obj);
        
    }

    private void OnLoadNewPopUI(Dictionary<string, GameObject> curList, string curName)
    {
        if (curList == null)
        {
            return;
        }

        List<string> objToRemove = new List<string>();

        if (curList.Count > 0)
        {
            objToRemove.Clear();
            foreach (KeyValuePair<string, GameObject> objs in curList)
            {
                if (curName == objs.Key)
                {
                    continue;
                }
                objs.Value.SetActive(false);
                objToRemove.Add(objs.Key);
                if (UIPathData.m_DicUIName.ContainsKey(objs.Key) && UIPathData.m_DicUIName[objs.Key].isDestroyOnUnload)
                {
                    DestroyUI(objs.Key, objs.Value);
                }
                else
                {
                    m_dicCacheUI.Add(objs.Key, objs.Value);
                }
            }

            for (int i = 0; i < objToRemove.Count; i++)
            {
                if (curList.ContainsKey(objToRemove[i]))
                {
                    curList.Remove(objToRemove[i]);
                }
            }
        }
    }
    private void OnClosePopUI(Dictionary<string, GameObject> curList, string curName)
    {
        if (TryDestroyUI(curList, curName))
        {
            
        }
    }

    private bool TryDestroyUI(Dictionary<string, GameObject> curList, string curName)
    {
        if (curList == null)
        {
            return false;
        }

        if (!curList.ContainsKey(curName))
        {
            return false;
        }

        //#if UNITY_ANDROID

        // < 768M UI不进行缓存
        if (SystemInfo.systemMemorySize < 768)
        {
            DestroyUI(curName, curList[curName]);
            curList.Remove(curName);

            Resources.UnloadUnusedAssets();
            GC.Collect();
            return true;
        }

        //#endif

        if (UIPathData.m_DicUIName.ContainsKey(curName) && !UIPathData.m_DicUIName[curName].isDestroyOnUnload)
        {
            curList[curName].SetActive(false);
            m_dicCacheUI.Add(curName, curList[curName]);
        }
        else
        {
            DestroyUI(curName, curList[curName]);
        }

        curList.Remove(curName);

        return true;
    }
#if UNITY_ANDROID
    void Update() 
    {
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            PlatformHelper.ClickEsc();
        }
    }
#endif
}
