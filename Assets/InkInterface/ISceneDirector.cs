using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneDirector : MonoBehaviour
{
    [SerializeField] Transform textSceneParent;
    [SerializeField] private Vector3 originalParentPosition;
    [SerializeField] InkPlayerInput_DialogueProgression narrativeProgressor;
    [SerializeField] InkPlayerInput_ChooseOption choiceProgressor;
    

    public DataState choiceObjectsAndData { get; protected set; }
    public DataState narrativeObjectsAndData { get; protected set; }

    protected InputSO input;
    protected InkSceneSO sceneSO;
    protected Transform inkTextObjectPrefab;

    private void Awake()
    {
        choiceObjectsAndData = DataState.Unintialized;
        narrativeObjectsAndData = DataState.Unintialized;
    }
    public virtual void PrepareScene(InkSceneSO _sceneSO, InputSO _input) {

        input = _input;
        sceneSO = _sceneSO;
    }

    public virtual void StartScene()
    {

    }
    public virtual void HideScene(InkDelegate.CallbackBool callbackBool)
    {

    }
}
