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
    

    public DataState choiceObjectsAndData { get; protected set; }
    public DataState narrativeObjectsAndData { get; protected set; }

    protected InputSO input;
    protected InkPageSO sceneSO;
    protected Transform inkTextObjectPrefab;

    public int nameToHash = -1;
    private int pendingPackageId = -1;

    private void Awake()
    {
        choiceObjectsAndData = DataState.Unintialized;
        narrativeObjectsAndData = DataState.Unintialized;
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
}
