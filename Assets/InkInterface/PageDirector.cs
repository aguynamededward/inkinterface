using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PageDirector : MonoBehaviour
{
    [SerializeField] Transform textSceneParent;
    [SerializeField] private Vector3 originalParentPosition;
    [SerializeField] InkPlayerInput_NarrativeProgression narrativeProgressor;
    [SerializeField] InkPlayerInput_ChoiceProgression choiceProgressor;


    public DataState currentPackageDataState { get; protected set; }
    public DataState nextPackageDataState { get; protected set; }

    private ContentPackage currentPackage;
    private ContentPackage nextPackage;

    protected InputSO input;
    protected InkPageSO sceneSO;
    protected Transform inkTextObjectPrefab;



    protected List<InkTextObject> inkTextObjectList = new List<InkTextObject>();
    protected List<InkTextObject> inkChoiceObjectList = new List<InkTextObject>();
    protected int currentIndex = 0;

    public int nameToHash = -1;
    
    private void Awake()
    {
        currentPackageDataState = DataState.Unintialized;
        nextPackageDataState = DataState.Unintialized;
    }

   public virtual void Update()
    {
        HandleCurrentPackage();

        if (currentPackageDataState == DataState.Unintialized) HandleNextPackage();
        
    }


    protected virtual void HandleNextPackage()
    {
        if (currentPackageDataState != DataState.Unintialized) return;
        if (nextPackageDataState == DataState.Unintialized || nextPackageDataState == DataState.Loading) return;

        currentPackage = nextPackage;
        currentPackageDataState = nextPackageDataState;

        nextPackage = null;
        nextPackageDataState = DataState.Unintialized;
    }

    protected virtual void HandleCurrentPackage()
    {
        if (currentPackageDataState == DataState.Unintialized) return;

        if(currentPackageDataState == DataState.Loading_Complete) HandleCurrentPackageLoadingComplete();
        
    }

    private void HandleCurrentPackageLoadingComplete()
    {
        switch(currentPackage.packageType)
        {
            case PackageType.Narrative:
                narrativeProgressor.Init(inkTextObjectList, currentIndex, ShowNarrativeAtIndex, OnCurrentPackageComplete);
                currentPackageDataState = DataState.Waiting_For_Player_To_Finish;
                break;
            case PackageType.Choice:
                PositionChoiceObjects(inkChoiceObjectList,inkTextObjectList[currentIndex]);
                FocusOnTextObject(inkChoiceObjectList[0]);
                choiceProgressor.SetupChoiceMoment(inkChoiceObjectList, OnCurrentPackageCompleteChoice);
                currentPackageDataState = DataState.Waiting_For_Player_To_Finish;
                break;
        }
    }

    private void PositionChoiceObjects(List<InkTextObject> inkChoiceObjectList, InkTextObject prevTextObject)
    {
        foreach(InkTextObject ito in inkChoiceObjectList)
        {
            PositionTextObject(ito, prevTextObject);
            prevTextObject = ito;
        }
    }

    protected virtual void OnCurrentPackageCompleteChoice(int _choiceIndex)
    {
        currentPackageDataState = DataState.Unintialized;
        currentPackage.FinishChoiceCallback(_choiceIndex);
        currentPackage = null;
        ClearChoices();
    }

    protected virtual void OnCurrentPackageComplete()
    {
        currentPackageDataState = DataState.Unintialized;
        currentPackage.FinishCallback();
        currentPackage = null;
    }

    protected virtual void ClearChoices()
    {
        ObjectPool<InkTextObject>.ReturnAllListItemsToPool(inkChoiceObjectList);

    }

    private void ShowNarrativeAtIndex(int i)
    {
        if (i >= inkTextObjectList.Count)
        {
            Debug.Log("Attempted to show narrative at index " + i + " - outside of range!", gameObject);
            return;
        }

        inkTextObjectList[i].ShowText();
        currentIndex = i;
        FocusOnTextObject(inkTextObjectList[i]);
    }

    public void FocusOnTextObject(InkTextObject ito)
    {
        textSceneParent.transform.DOMove(new Vector3(originalParentPosition.x, originalParentPosition.y - ito.transform.localPosition.y,originalParentPosition.z),1f);
    }

    public virtual void SetupScene(InkPageSO _sceneSO, InputSO _input) {

        input = _input;
        sceneSO = _sceneSO;
        inkTextObjectPrefab = sceneSO.InkTextObjectPrefab;
        nameToHash = _sceneSO.nameToHash;

        // Make sure we have references to the progressors
        if (!narrativeProgressor) narrativeProgressor = GetComponentInChildren<InkPlayerInput_NarrativeProgression>();
        if (!choiceProgressor) choiceProgressor = GetComponentInChildren<InkPlayerInput_ChoiceProgression>();

        if (narrativeProgressor) narrativeProgressor.RegisterInput(_input);
        else Debug.Log("PAGE DIRECTOR: No narrative progressor assigned!!");

        if(choiceProgressor) choiceProgressor.RegisterInput(_input);
        else Debug.Log("PAGE DIRECTOR: No choice progressor assigned!!");

        float textParentZPosition = WorldInterface.ScreenSpaceToWorldSpaceAtDepth(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f), WorldDepth.Text).z;

        textSceneParent.position = new Vector3(textSceneParent.position.x, textSceneParent.position.y, textParentZPosition);
        originalParentPosition.z = textParentZPosition;
    }

    public virtual void AnimateInAndStartScene()
    {

    }
    public virtual void AnimateOutAndShutdownScene(InkDelegate.CallbackBool callbackBool)
    {

    }

    public virtual bool TrySendNextPackage(ContentPackage pkg)
    {
        if (pkg == currentPackage) return true; // We've already started on this package, but let the Interface know to remove it

        if(currentPackage == null)
        {
            currentPackage = pkg;
            HandleNewPackage(pkg);
            return true;
        }

        if(nextPackage == null)
        {
            nextPackage = pkg;
            HandleNewPackage(pkg);
            return false; // Return false, as we have not yet processed this package
        }

        return false; // We must be full in current and nextPackage, don't accept this yet
    }

    protected virtual void HandleNewPackage(ContentPackage pkg)
    {
        InkDelegate.CallbackInt setDataStateCallback = null;

        if (pkg == currentPackage) setDataStateCallback = SetCurrentDataStateValue;
        if (pkg == nextPackage) setDataStateCallback = SetNextDataStateValue;

        switch (pkg.packageType)
        {
            case PackageType.Narrative:
                {
                    StartCoroutine(LoadInkParagraphsIntoObjects(pkg.inkParagraphList, setDataStateCallback, inkTextObjectList));
                    break;
                }
            case PackageType.Choice:
                {
                    if(inkChoiceObjectList.Count > 0)
                    {
                        // clear the inkChoice object list - ideally by calling the choice progressor
                        inkChoiceObjectList.Clear();
                    }
                    StartCoroutine(LoadInkParagraphsIntoObjects(pkg.inkParagraphList, setDataStateCallback, inkChoiceObjectList));
                    break;
                }
            case PackageType.Pause:
                {


                    break;
                }
            case PackageType.Shutdown:
                {
                    break;
                }
        }
    } 

    protected virtual IEnumerator LoadInkParagraphsIntoObjects(List<InkParagraph> listOfPars,InkDelegate.CallbackInt setDataStateValue,List<InkTextObject> existingTextObjects)
    {
        setDataStateValue((int)DataState.Loading);

        InkTextObject inkTextObj;
        InkTextObject _prevTextObj = null;

        if (existingTextObjects.Count > 0)
        {
            _prevTextObj = existingTextObjects[existingTextObjects.Count - 1];
        }

        int startingPosition = existingTextObjects.Count;


        // TODO -- add a check for ink paragraphs with no text in them - basically skip them. (some inkpars will only have tags)
        for (var q = 0; q < listOfPars.Count; q++)
        {
            yield return 0; // Before anything else - wait a frame. In case the last text object loaded needs time to update

            inkTextObj = ObjectPool<InkTextObject>.GetPoolObject(textSceneParent, inkTextObjectPrefab);
            existingTextObjects.Add(inkTextObj);

            inkTextObj.Init(listOfPars[q]);

            if (_prevTextObj == null && inkTextObj.IsChoice())
            {
                // Because we're a choice, our first position should be under the last bit of text
                if (existingTextObjects.Count > 0)
                {
                    _prevTextObj = existingTextObjects[^1];
                }
            }

            if (_prevTextObj != null)
            {
                PositionTextObject(inkTextObj, _prevTextObj);
            }

            _prevTextObj = inkTextObj;

        }

        setDataStateValue((int)DataState.Loading_Complete);
    }

    protected virtual void PositionTextObject(InkTextObject inkTextObj, InkTextObject _prevTextObj)
    {
        inkTextObj.SetLocalPosition(new Vector3(inkTextObj.transform.localPosition.x, _prevTextObj.transform.localPosition.y - _prevTextObj.GetBottomOfText(), inkTextObj.transform.localPosition.z));
    }

    protected void SetCurrentDataStateValue(int i)
    {
        currentPackageDataState = (DataState)i;
    }

    protected void SetNextDataStateValue(int i)
    {
        nextPackageDataState = (DataState)i;
    }
}
