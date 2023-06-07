using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InkTextObject : MonoBehaviour,IObjectPoolElement
{
    [SerializeField] TextMeshPro text;

    void Start()
    {
        text.text = ""; // Clear text by default
        ObjectPool<InkTextObject>.AddToPool(this,PoolShutdown);
    }

    private void OnDestroy()
    {
        ObjectPool<InkTextObject>.RemoveFromPool(this);
    }

    public void Init(string _textString)
    {
        text.text = _textString;
        gameObject.SetActive(true);
    }

    public void PoolShutdown()
    {
        gameObject.SetActive(false); // Later, we'll do fade out of text
    }

}
