using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public int nameToHash = -1;
    
    private void Awake()
    {
        currentPackageDataState = DataState.Unintialized;
        nextPackageDataState = DataState.Unintialized;
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
                    LoadInkParagraphsIntoObjects(pkg.inkParagraphList, setDataStateCallback, inkTextObjectList);
                    break;
                }
            case PackageType.Choice:
                {
                    if(inkChoiceObjectList.Count > 0)
                    {
                        // clear the inkChoice object list - ideally by calling the choice progressor
                        inkChoiceObjectList.Clear();
                    }
                    LoadInkParagraphsIntoObjects(pkg.inkParagraphList, setDataStateCallback, inkChoiceObjectList);
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
