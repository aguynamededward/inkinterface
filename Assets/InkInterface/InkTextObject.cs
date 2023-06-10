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

    public bool IsVisible()
    {
        return textVisible;
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

    public bool CheckForClick(Vector3 inputWorldPosition)
    {
        if (inkParagraph.IsChoice() == false) return false;
        if (textVisible == false) return false;

        //Vector2 localInputPosition = rectTransform.InverseTransformPoint(inputWorldPosition);
        
        // trying with textmeshpro recttransform
        localInputPosition = textmeshPro.rectTransform.InverseTransformPoint(inputWorldPosition);
        mouseWorldPosition = inputWorldPosition;
        //bool amInsideme = rectTransform.rect.Contains(localInputPosition, true);
        //bool amInsideme = textmeshPro.bounds.Contains(localInputPosition);
        Bounds tmpBounds = textmeshPro.textBounds;
        tmpBounds.max += textmeshPro.rectTransform.position;
        tmpBounds.min += textmeshPro.rectTransform.position;
        bool amInsideme = inputWorldPosition.x < tmpBounds.max.x &&
                            inputWorldPosition.x > tmpBounds.min.x &&
                            inputWorldPosition.y < tmpBounds.max.y &&
                            inputWorldPosition.y > tmpBounds.min.y;

        tempBounds = tmpBounds;
        Debug.Log("Original mouse position " + inputWorldPosition + "\nConverted to Local: " + localInputPosition + "\ntmpBounds: " + tmpBounds.max + " / " + tmpBounds.min);

        return amInsideme;
    }
    Vector2 localInputPosition;
    Vector3 mouseWorldPosition;
    Bounds tempBounds;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(localInputPosition, 0.5f);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(mouseWorldPosition, 0.5f);

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(tempBounds.max, tempBounds.min);
    }
    public bool CheckForClickOnCharacter(Vector2 inputScreenPosition,Camera camera)
    {
        if (inkParagraph.IsChoice() == false) return false;
        if (textVisible == false) return false;

        if (!TMP_TextUtilities.IsIntersectingRectTransform(textmeshPro.rectTransform, inputScreenPosition, camera)) return false;

        int charClicked = TMP_TextUtilities.FindIntersectingCharacter(textmeshPro, inputScreenPosition, camera, true);
        bool amInsideme = charClicked != -1;

        //if(amInsideme) Debug.Log("Char clicked returned" + charClicked + " aka, the letter " + textmeshPro.textInfo.characterInfo[charClicked].character);
        return amInsideme;
    }
    public bool CheckForClickOnLine(Vector2 inputScreenPosition, Camera camera)
    {
        if (inkParagraph.IsChoice() == false) return false;
        if (textVisible == false) return false;

        if (!TMP_TextUtilities.IsIntersectingRectTransform(textmeshPro.rectTransform, inputScreenPosition, camera)) return false;

        int lineClicked = TMP_TextUtilities.FindIntersectingLine(textmeshPro, inputScreenPosition, camera);
        bool amInsideme = lineClicked != -1;

        
        //if (amInsideme)
        //{
        //    TMP_LineInfo tmpLineInfo = textmeshPro.textInfo.lineInfo[lineClicked];
        //    string textShown = textmeshPro.text.Substring(tmpLineInfo.firstCharacterIndex, tmpLineInfo.lastCharacterIndex - tmpLineInfo.firstCharacterIndex + 1);
        //    Debug.Log("Line clicked returned " + lineClicked + " aka, the line that says " + textShown + "||");
        //}
        return amInsideme;
    }

    public bool CheckForClickOnRect(Vector2 inputScreenPosition, Camera camera)
    {
        if (inkParagraph.IsChoice() == false) return false;
        if (textVisible == false) return false;

        bool amInsideme = TMP_TextUtilities.IsIntersectingRectTransform(textmeshPro.rectTransform, inputScreenPosition, camera);


        //if (amInsideme)
        //{
        //    TMP_LineInfo tmpLineInfo = textmeshPro.textInfo.lineInfo[lineClicked];
        //    string textShown = textmeshPro.text.Substring(tmpLineInfo.firstCharacterIndex, tmpLineInfo.lastCharacterIndex - tmpLineInfo.firstCharacterIndex + 1);
        //    Debug.Log("Line clicked returned " + lineClicked + " aka, the line that says " + textShown + "||");
        //}
        return amInsideme;
    }

    public bool IsChoice()
    {
        if (inkParagraph == null) return false;
        else return inkParagraph.GetChoiceIndex() != -1;
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
        //Debug.DrawLine(textmeshPro.bounds.min + rectTransform.position, textmeshPro.bounds.max + rectTransform.position, Color.gray);
        //Debug.DrawLine(textmeshPro.textBounds.min + rectTransform.position, textmeshPro.textBounds.max + rectTransform.position, Color.red);

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
