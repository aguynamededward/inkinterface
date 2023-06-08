using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class InkTextObject : MonoBehaviour,IObjectPoolElement
{
    [SerializeField] TextMeshPro textmeshPro;
    private RectTransform rectTransform;
    InkParagraph inkParagraph;

    bool textVisible = false;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
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
        bool amInsideme = rectTransform.rect.Contains(localInputPosition, true);
        return amInsideme;
    }

    public int GetChoiceIndex()
    {
        return inkParagraph.GetChoiceIndex();
    }

    public float GetBottomOfText()
    {
        return rectTransform.rect.yMax;
    }

    public void SetLocalPosition(Vector3 _localPosition)
    {
        transform.localPosition = _localPosition;
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
