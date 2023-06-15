using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TextDirector : MonoBehaviour
{
    public static TextDirector I;

    [SerializeField] Transform textSceneParent;
    [SerializeField] Transform inkTextObjectPrefab;
    private int currentIndex = 0;
    private Vector3 originalParentPosition;
    List<InkTextObject> inkTextObjects = new List<InkTextObject>();
    List<InkTextObject> inkChoiceObjects = new List<InkTextObject>();

    public DataState choiceObjectsAndData { get; protected set; }
    public DataState narrativeObjectsAndData { get; protected set; }

    private void Awake()
    {
        if(I != null)
        {
            Debug.Log("TWO TextLayoutDirectors - deleting this one.", gameObject);
            Destroy(gameObject);
            return;
        }

        I = this;

        choiceObjectsAndData = DataState.Unintialized;
        narrativeObjectsAndData = DataState.Unintialized;
    }

    private void Start()
    {
        originalParentPosition = textSceneParent.position;
    }

    public virtual void ClearChoicePoint()
    {
        ObjectPool<InkTextObject>.ReturnAllListItemsToPool(inkChoiceObjects);
        inkChoiceObjects.Clear();

        choiceObjectsAndData = DataState.Unintialized;
    }
    public virtual void LoadNarrative(List<InkParagraph> listOfPars,InkDelegate.CallbackInt callBack)
    {
        if (narrativeObjectsAndData == DataState.Loading)
        {
            Debug.Log("Trying to Load more lines when we're still loading!",gameObject);
            return;
        }

        StartCoroutine(LoadInkParagraphs(listOfPars, SetNarrativeDataStateValue, inkTextObjects, callBack));
    }

    public virtual void LoadChoices(List<InkParagraph> listofChoices,InkDelegate.CallbackInt callBack)
    {
        if(choiceObjectsAndData == DataState.Loading)
        {
            Debug.Log("Trying to Load choices when we're still loading!", gameObject);
            return;
        }

        ClearObjectList(inkChoiceObjects);
        StartCoroutine(LoadInkParagraphs(listofChoices,SetChoiceDataStateValue, inkChoiceObjects, callBack));
    }

    private void ClearObjectList(List<InkTextObject> inkObjectsList, bool immediately = true)
    {
        if (immediately)
        {
            foreach(InkTextObject ito in inkObjectsList)
            {
                ito.HideText();
                ObjectPool<InkTextObject>.ReturnToObjectPool(ito);
            }

            return;
        }
    }

    private IEnumerator LoadInkParagraphs(List<InkParagraph> inkPars, InkDelegate.CallbackInt setDataStateValue,List<InkTextObject> textObjectList, InkDelegate.CallbackInt callback)
    {
        setDataStateValue((int)DataState.Loading);

        InkTextObject inkTextObj;
        InkTextObject _prevTextObj = null;

        if(textObjectList.Count > 0)
        {
            _prevTextObj = textObjectList[textObjectList.Count - 1];
        }

        int startingPosition = textObjectList.Count;

        for(var q = 0;q < inkPars.Count; q++)
        {
            yield return 0; // Before anything else - wait a frame. In case the last text object loaded needs time to update

            inkTextObj = ObjectPool<InkTextObject>.GetPoolObject(textSceneParent, inkTextObjectPrefab);
            textObjectList.Add(inkTextObj);

            inkTextObj.Init(inkPars[q]);

            if(_prevTextObj == null && inkTextObj.IsChoice())
            {
                // Because we're a choice, our first position should be under the last bit of text
                if(inkTextObjects.Count > 0)
                {
                    _prevTextObj = inkTextObjects[^1];
                }
            }

            if (_prevTextObj != null)
            {
                PositionTextObject(inkTextObj, _prevTextObj);
            }

            _prevTextObj = inkTextObj;
            
        }

        setDataStateValue((int)DataState.Complete);

        callback?.Invoke(startingPosition);
    }

    private void SetNarrativeDataStateValue(int i)
    {
        DataState _i = (DataState)i;
        narrativeObjectsAndData = _i;
    }

    private void SetChoiceDataStateValue(int i)
    {
        DataState _i = (DataState)i;
        choiceObjectsAndData = _i;
    }


    public void ShowNarrativeAtIndex(int i)
    {
        if (i >= inkTextObjects.Count)
        {
            Debug.Log("Attempted to show narrative at index " + i + " - outside of range!",gameObject);
            return;
        }

        inkTextObjects[i].ShowText();
        currentIndex = i;
        FocusOnTextObject(inkTextObjects[i]);
    }

    public void ShowChoices()
    {
        if(inkChoiceObjects.Count > 0)
        {
            FocusOnTextObject(inkChoiceObjects[0]);
        }
    }
    public void FocusOnTextObject(InkTextObject ito)
    {

        textSceneParent.transform.DOMove(originalParentPosition - ito.transform.localPosition, 1f);
        
    }

    public List<InkTextObject> GetNarrativeObjects()
    {
        if (narrativeObjectsAndData == DataState.Complete) return inkTextObjects;
        else return null;
    }

    public void ResetNarrativeDataState()
    {
        narrativeObjectsAndData = DataState.Unintialized;
    }

    public List<InkTextObject> GetChoiceObjects()
    {
        if (choiceObjectsAndData == DataState.Complete) return inkChoiceObjects;
        else return null;
    }
    private void PositionTextObject(InkTextObject inkTextObj, InkTextObject _prevTextObj)
    {
        inkTextObj.SetLocalPosition(new Vector3(inkTextObj.transform.localPosition.x,_prevTextObj.transform.localPosition.y -_prevTextObj.GetBottomOfText(), inkTextObj.transform.localPosition.z));
    }

    //protected IEnumerator HideTextInStages(float totalDuration, bool runCallbackAtEnd)
    //{
    //    if (inkTextObjects.Count < 1 || !activated)
    //    {
    //
    //        yield break;
    //    }
    //
    //
    //    int totalVisibleInkObjects = 0;
    //    for (var q = 0; q < inkTextObjects.Count; q++)
    //    {
    //        if (inkTextObjects[q].IsVisible()) totalVisibleInkObjects++;
    //    }
    //
    //    if (totalVisibleInkObjects <= 1)
    //    {
    //        Debug.Log("DialogueProgression: HideTextInStages - only one object, stopping now");
    //        // Don't hold us up for one object
    //
    //        foreach (InkTextObject ito in inkTextObjects)
    //        {
    //            ito.HideText();
    //        }
    //
    //        if (runCallbackAtEnd) CompleteDialogueDisplay();
    //        yield break;
    //    }
    //
    //
    //    float waitDuration = totalDuration / totalVisibleInkObjects;
    //
    //    for (var q = 0; q < inkTextObjects.Count; q++)
    //    {
    //        if (!inkTextObjects[q].IsVisible()) continue;
    //        inkTextObjects[q].HideText();
    //        Debug.Log("DialogueProgression: HideTextInStages - hiding textObject #" + q);
    //        yield return new WaitForSeconds(waitDuration);
    //    }
    //
    //    if (runCallbackAtEnd) CompleteDialogueDisplay();
    //
    //}


}
