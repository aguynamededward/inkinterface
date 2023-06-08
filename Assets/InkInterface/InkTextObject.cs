using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class InkTextObject : MonoBehaviour,IObjectPoolElement
{
    [SerializeField] protected TextMeshPro textmeshPro;
    protected RectTransform rectTransform;
    InkParagraph inkParagraph;

    bool textVisible = false;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        //textmeshPro.autoSizeTextContainer = true;
    }
    
    private void OnDestroy()
    {
        ObjectPool<InkTextObject>.RemoveFromPool(this);
    }

    public void Init(InkParagraph _inkPar)
    {
        HideText();
        textmeshPro.text = _inkPar.text;
        inkParagraph = _inkPar;
        gameObject.SetActive(true);


        textObjectsToBeUpdated.Add(this);


    }

    public void ShowText()
    {
        Color c = textmeshPro.color;

        if (inkParagraph.IsChoice())
        {
            c = Color.red;
        }
        else c = Color.white;

        c.a = 1f;

        textmeshPro.color = c;
        textVisible = true;
    }

    public void HideText()
    {
        Color c = textmeshPro.color;
        c.a = 0f;
        textmeshPro.color = c;
        textVisible = false;
    }

    public bool CheckForClick(Vector3 inputScreenPosition)
    {
        if (inkParagraph.IsChoice() == false) return false;
        if (textVisible == false) return false;

        Vector2 localInputPosition = rectTransform.InverseTransformPoint(inputScreenPosition);
        //bool amInsideme = rectTransform.rect.Contains(localInputPosition, true);
        //bool amInsideme = textmeshPro.bounds.Contains(localInputPosition);
        Bounds tmpBounds = textmeshPro.textBounds;
        bool amInsideme = localInputPosition.x < tmpBounds.max.x &&
                            localInputPosition.x > tmpBounds.min.x &&
                            localInputPosition.y < tmpBounds.max.y &&
                            localInputPosition.y > tmpBounds.min.y;
        return amInsideme;
    }

    public int GetChoiceIndex()
    {
        return inkParagraph.GetChoiceIndex();
    }

    public float GetBottomOfText()
    {
        return textmeshPro.bounds.size.y;
    }

    public void SetLocalPosition(Vector3 _localPosition)
    {
        transform.localPosition = _localPosition;
    }

    private static bool adjustingTextBounds;
    private static List<InkTextObject> textObjectsToBeUpdated = new List<InkTextObject>();
    static IEnumerator AdjustTextBounds()
    {
        yield return 0;

        if(textObjectsToBeUpdated.Count < 1)
        {
            adjustingTextBounds = false;
            yield break;
        }
        int objectsTotal = textObjectsToBeUpdated.Count;

        InkTextObject inkTestObj;
        
        for(var q = objectsTotal - 1; q >= 0; q--)
        {
            inkTestObj = textObjectsToBeUpdated[q];
            Bounds bounds = inkTestObj.textmeshPro.textBounds;

            float mag = inkTestObj.rectTransform.sizeDelta.magnitude;
            inkTestObj.rectTransform.sizeDelta = new Vector2(bounds.size.normalized.x * mag, bounds.size.normalized.y * mag);

            textObjectsToBeUpdated.RemoveAt(q);
        }

        adjustingTextBounds = false;
    }

    private void Update()
    {
        Debug.DrawLine(textmeshPro.bounds.min + rectTransform.position, textmeshPro.bounds.max + rectTransform.position, Color.gray);
        Debug.DrawLine(textmeshPro.textBounds.min + rectTransform.position, textmeshPro.textBounds.max + rectTransform.position, Color.red);

    }

    #region Pool Methods
    public void PoolShutdown()
    {
        HideText();
        gameObject.SetActive(false); // Later, we'll do fade out of text
    }

    public void InitInPool()
    {
        ObjectPool<InkTextObject>.AddToPool(this, PoolShutdown);
    }

    #endregion
}
