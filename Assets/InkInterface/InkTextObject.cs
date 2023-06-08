using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InkTextObject : MonoBehaviour,IObjectPoolElement
{
    [SerializeField] TextMeshPro textmeshPro;

    InkParagraph myInkPar;

    void Start()
    {
        HideText();
        InitInPool();
    }

    private void OnDestroy()
    {
        ObjectPool<InkTextObject>.RemoveFromPool(this);
    }

    public void Init(InkParagraph _inkPar)
    {
        HideText();
        textmeshPro.text = _inkPar.text;
        myInkPar = _inkPar;
        gameObject.SetActive(true);
    }

    public void ShowText()
    {
        Color c = textmeshPro.color;
        c.a = 1f;
        textmeshPro.color = c;
    }

    public void HideText()
    {
        Color c = textmeshPro.color;
        c.a = 0f;
        textmeshPro.color = c;
    }


    public void PoolShutdown()
    {
        HideText();
        gameObject.SetActive(false); // Later, we'll do fade out of text
    }

    public void InitInPool()
    {
        ObjectPool<InkTextObject>.AddToPool(this, PoolShutdown);
    }
}
