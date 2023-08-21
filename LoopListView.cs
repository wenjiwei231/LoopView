using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoopListView : MonoBehaviour
{
    //单例
    private static LoopListView _instance;
    public static LoopListView Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<LoopListView>();
            }
            return _instance;
        }
    }

    public Transform parent;
    public GameObject prefabAI;
    public GameObject prefabPreson;
    public Vector2 spacing;
    public ScrollRect scrollRect;

    private RectTransform mRectTransform;
    private List<DialogInfoData> dialogInfoDatas;
    private ItemPool poolAI;
    private ItemPool poolPerson;
    private Dictionary<int,DynamicRect> mDict_dRect;
    private Vector2 mMasksize;
    private Rect mRectMask;


    private void Start()
    {
        dialogInfoDatas = new List<DialogInfoData>();
        poolAI = new ItemPool(prefabAI,parent);
        poolPerson = new ItemPool(prefabPreson,parent);
        mRectTransform = parent.GetComponent<RectTransform>();
        mMasksize = mRectTransform.parent.GetComponent<RectTransform>().sizeDelta;
    }


    public void AddDialog(string info, DialogType type)
    {
        DialogInfoData dialogInfoData = new DialogInfoData(info, type);
        DialogItem item;
        switch (type)
        {

            case DialogType.Ai:

                item=  poolAI.GetItem(dialogInfoData);
                item.ImageSize(scrollRect, dialogInfoData);

                dialogInfoDatas.Add(dialogInfoData);

                UpdateDynmicRects(dialogInfoDatas);
                item.DRect = dialogInfoDatas[dialogInfoDatas.Count - 1].rect;

                UpdateChildTransformPos(item.gameObject, dialogInfoDatas[dialogInfoDatas.Count - 1].rect.Index);


                break;
            case DialogType.person:
                item= poolPerson.GetItem(dialogInfoData);
                item.ImageSize(scrollRect, dialogInfoData);
                dialogInfoDatas.Add(dialogInfoData);

                UpdateDynmicRects(dialogInfoDatas);
                item.DRect = dialogInfoDatas[dialogInfoDatas.Count - 1].rect;
                UpdateChildTransformPos(item.gameObject, dialogInfoDatas[dialogInfoDatas.Count-1].rect.Index);

                break;
            default:
                break;
        }

       
        RefreshRate();
        scrollRect.verticalNormalizedPosition = 0;
    }

    private void RefreshRate()
    {
        if (dialogInfoDatas.Count <= 0)
            throw new Exception("数据为空请检查数据");

       
        SetListRenderSize(dialogInfoDatas);

    }
    private float contentHeight;
    private void SetListRenderSize(List<DialogInfoData> datas)
    {
        contentHeight = 0;
        for (int i = 0; i < datas.Count; i++)
        {
            contentHeight += GetBlockSizeY(datas[i]);
        }
        mRectTransform.sizeDelta = new Vector2(mRectTransform.sizeDelta.x,contentHeight);
        mRectMask = new Rect(0,-mMasksize.y,mMasksize.x,mMasksize.y);
        scrollRect.vertical = mRectTransform.sizeDelta.y > mMasksize.y;


    }
    float height;
    private void UpdateDynmicRects(List<DialogInfoData> datas)
    {
        mDict_dRect=new Dictionary<int, DynamicRect>();
        for (int i = 0,len = datas.Count; i < len; i++)
        {
            //if (i > 0)
            //{
            //    for (int j = 0; j <i; j++)
            //    {
            //        height += datas[j].cellSize.y;
            //    }
            //}
            //else
            //{
            //    height = 0;
            //}

            if (i > 0)
            {
                for (int j   = 0;  j <= i; j++)
                {
                   height+= GetBlockSizeY(dialogInfoDatas[j]);
                }
            }
            else
            {
                height= GetBlockSizeY(dialogInfoDatas[0]);
            }
            // DynamicRect dRect = new DynamicRect(column * GetBlockSizeX(), -row * GetBlockSizeY() - CellSize.y, CellSize.x, CellSize.y, i);

            DynamicRect dRect =new DynamicRect(0,-height,datas[i].cellSize.x,datas[i].cellSize.y,i);
            mDict_dRect[i]=dRect;
            datas[i].rect = dRect;
            height = 0;
        }
       
    }


    private float GetBlockSizeX(DialogInfoData data ) {return data.cellSize.x+spacing.x ;}
    private float GetBlockSizeY(DialogInfoData data ) {return data.cellSize.y+spacing.y ;}

    public Texture texture;
    public Texture texture2;
    public Texture texture3;

    private void OnDrawGizmos()
    {

        Gizmos.DrawGUITexture(mRectMask, texture);

        if (poolAI.inPool.Count <= 0) return;
        for (int i = 0; i < poolAI.inPool.Count; i++)
        {
            DialogItem item = poolAI.inPool[i];
            if (item.DRect != null)
                Gizmos.DrawGUITexture(item.DRect.mRect, texture2);
        }
        if (poolPerson.inPool.Count <= 0) return;
        for (int i = 0; i < poolPerson.inPool.Count; i++)
        {
            DialogItem item = poolPerson.inPool[i];
            if (item.DRect != null)
                Gizmos.DrawGUITexture(item.DRect.mRect, texture3);
        }
    }

    private void Update()
    {
       if(dialogInfoDatas.Count>0)
            UndateRender();
    }
    private void UndateRender()
    {
        mRectMask.y = -mMasksize.y - mRectTransform.anchoredPosition.y;
        
        Dictionary<int, DynamicRect> inOverlaps = new Dictionary<int, DynamicRect>();
        foreach (DynamicRect dr in mDict_dRect.Values)
        {
            if (dr.Overlaps(mRectMask))
            {
                inOverlaps.Add(dr.Index,dr);
            }

        }
        for (int i = 0,len=poolAI.inPool.Count; i < len; i++)
        {
            DialogItem item = poolAI.inPool[i];
            if (item.DRect!=null&&!inOverlaps.ContainsKey(item.DRect.Index))
            {
                item.DRect = null;
                poolAI.RecycledItem(item);
            }

        }
        for (int i = 0, len = poolPerson.inPool.Count; i < len; i++)
        {
            DialogItem item = poolPerson.inPool[i];
            if (item.DRect != null && !inOverlaps.ContainsKey(item.DRect.Index))
            {
                item.DRect = null;
                poolPerson.RecycledItem(item);
            }

        }
        foreach (var dr in inOverlaps.Values)
        {

            if (GetDynmicItem(dr)==null)
            {
                DialogItem item = GetNullDynmicItem(dialogInfoDatas[dr.Index].dialogType);
                item.DRect = dr;
                UpdateChildTransformPos(item.gameObject, dr.Index);
                if (dialogInfoDatas!=null&&dr.Index<dialogInfoDatas.Count)
                {
                    item.ImageSize(scrollRect,dialogInfoDatas[dr.Index]);
                }
            }
        }

    }

    private void UpdateChildTransformPos(GameObject gameObject, int index)
    {
        Vector2 vPos = new Vector2();
        //if (index > 0)
        //{
        //    for (int i = 0; i < index; i++)
        //    {
        //        vPos.y += GetBlockSizeY(dialogInfoDatas[i]);
        //    }
        //}
        //else
        //{
        //    vPos.y = 0;
        //}
        if (index > 0)
        {
            for (int i = 0; i <=index; i++)
            {
                vPos.y += GetBlockSizeY(dialogInfoDatas[i]);
            }
        }
        else
        {
            vPos.y = GetBlockSizeY(dialogInfoDatas[0]);
        }
       ((RectTransform)gameObject.transform).anchoredPosition3D = Vector3.zero;
        ((RectTransform)gameObject.transform).anchoredPosition = -vPos;


    }

    private DialogItem GetNullDynmicItem(DialogType type)
    {

        switch (type)
        {
            case DialogType.Ai:
                for (int i = 0, len = poolAI.inPool.Count; i < len; i++)
                {
                    DialogItem item = poolAI.inPool[i];
                    if (item.DRect == null)
                    {
                        return item;
                    }
                }
                break;
            case DialogType.person:
                for (int i = 0, len = poolPerson.inPool.Count; i < len; i++)
                {
                    DialogItem item = poolPerson.inPool[i];
                    if (item.DRect == null)
                    {
                        return item;
                    }
                }
                break;
            default:
                break;
        }


        return null;
    }

    private object GetDynmicItem(DynamicRect dr)
    {
        for (int i = 0,len=poolPerson.inPool.Count; i < len; i++)
        {
            DialogItem item = poolPerson.inPool[i];
            if (item.DRect == null)
            {
                continue;
            }
            if (dr.Index == item.DRect.Index)
            {
                return item;
            }
            
        }
        for (int i = 0, len = poolAI.inPool.Count; i < len; i++)
        {
            DialogItem item = poolAI.inPool[i];
            if (item.DRect == null)
            {
                continue;
            }
            if (dr.Index == item.DRect.Index)
            {
                return item;
            }

        }
        return null;
    }
}
