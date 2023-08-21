using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class DialogItem : MonoBehaviour
{
    public Text msg;
    public Image dialogImage;
    public LayoutElement LayoutElement; 
   // public ContentSizeFitter ContentSizeFitter;
    public DialogType dialogType;
    protected DynamicRect mDRect;
    /// <summary>
    /// 动态格子数据
    /// </summary>
    protected object mData;
    public DynamicRect DRect
    {
        set
        {
            mDRect = value;
            gameObject.SetActive(value != null);
        }
        get { return mDRect; }
    }


    private void Awake()
    {
        
    }
    public void ResetItem()
    {
        msg.text = string.Empty;
        LayoutElement.preferredWidth = -1;
      
    }

    private float width;
    public void ImageSize(ScrollRect scrollRect,DialogInfoData dialogInfoData)
    {
        msg.text = dialogInfoData.msg;
      
        if (msg.preferredWidth > 480)
        {
            LayoutElement.preferredWidth= 480;
            width = 480;
        }
        else
        {
            width=msg.preferredWidth;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(msg.GetComponent<RectTransform>());
        dialogImage.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,width+50);
        dialogImage.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
            msg.preferredHeight+50
            );
        transform.GetComponent<RectTransform>().sizeDelta=new Vector2(810.309f,msg.preferredHeight+80);
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
       // scrollRect.verticalNormalizedPosition = 0;

        dialogInfoData.cellSize=transform.GetComponent<RectTransform>().sizeDelta;
    }
}

public enum DialogType
{
    Ai,
    person
}

public class DialogInfoData
{
    public string msg;
    public DialogType dialogType;
    public Vector2 cellSize;
    public DynamicRect rect;
    public DialogInfoData(string msg,DialogType dialogType)
    {
        this.msg = msg;
        this.dialogType = dialogType;

    }
}
/// <summary>
/// 动态格子矩形
/// </summary>
public class DynamicRect
{
    /// <summary>
    /// 矩形数据
    /// </summary>
    public Rect mRect;
    /// <summary>
    /// 格子索引
    /// </summary>
    public int Index;
    public DynamicRect(float x, float y, float width, float height, int index)
    {
        this.Index = index;
        mRect = new Rect(x, y, width, height);
    }

    /// <summary>
    /// 是否相交
    /// </summary>
    /// <param name="otherRect"></param>
    /// <returns></returns>
    public bool Overlaps(DynamicRect otherRect)
    {
        return mRect.Overlaps(otherRect.mRect);
    }

    /// <summary>
    /// 是否相交
    /// </summary>
    /// <param name="otherRect"></param>
    /// <returns></returns>
    public bool Overlaps(Rect otherRect)
    {
        return mRect.Overlaps(otherRect);
    }
    public override string ToString()
    {
        return string.Format("index:{0},x:{1},y:{2},w:{3},h:{4}", Index, mRect.x, mRect.y, mRect.width, mRect.height);
    }


}