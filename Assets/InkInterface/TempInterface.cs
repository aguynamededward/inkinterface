using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempInterface : MonoBehaviour
{
    // Internally, we continue pushing through dialogue until we hit a choice point (later we'll add support for tags that request specific inputs - we'll stop before we grab more dialogue)
    enum State
    {
        Unintialized,
        GetDataFromInkEngine,
        Loading_Dialogue,
        Waiting_For_DialogueEnd,
        Loading_Choices,
        Waiting_For_ChoicesEnd
    }

    // 
    enum DataState
    {
        Unintialized,   // Unloaded state, or after we've completed a dialogue run and are waiting for input
        Loading,        // Currently loading dialogue objects in the background
        Complete        // All dialogue objects loaded, waiting for end of dialogue run
    }

    [SerializeField] State state = State.Unintialized;
    [SerializeField] TextAsset inkJson;
    [SerializeField] Transform textSceneParent;
    [SerializeField] Transform inkTextObjectPrefab;
    [SerializeField] InkInput_DialogueProgression dialogueProgressor;
    [SerializeField] InkPlayerInput_ChooseOption choiceProgressor;

    InkEngine inkEngine;

    DataState choiceObjectsAndData = DataState.Unintialized;
    DataState dialogueObjectsAndData = DataState.Unintialized;

    // Start is called before the first frame update
    void Start()
    {
        choiceObjectsAndData = DataState.Unintialized;
        dialogueObjectsAndData = DataState.Unintialized;
        
        inkEngine = new InkEngine();
        inkEngine.LoadNewStory(inkJson);
        inkEngine.InitializeStory();
        state = State.GetDataFromInkEngine;
        NextStep();
    }

    List<InkTextObject> inkTextObjects = new List<InkTextObject>();
    List<InkTextObject> inkChoiceObjects = new List<InkTextObject>();

    InkTextObject selectedChoice;

    public void NextStep()
    {
        Debug.Log("InkInterface: NextStep");
        switch (state)
        {
            case State.GetDataFromInkEngine:
            {
                    GetDataFromInkEngine();
                    break;
            }
            default:
            {
                    break;
            }

        }
        
    }

    public void GetDataFromInkEngine()
    {
        switch (inkEngine.GetCurrentState())
        {
            case InkEngine.State.Display_Next_Line:
                {
                    if (dialogueObjectsAndData == DataState.Unintialized)
                    {
                        state = State.Loading_Dialogue;
                        GetAllAvailableLines();
                    }
                    else state = State.Waiting_For_DialogueEnd;
                    break;
                }
            case InkEngine.State.Choice_Point:
                {
                    StartChoicePoint();
                    break;
                }
            case InkEngine.State.End:
                {
                    Debug.Log("END OF GAME");
                    break;
                }
        }
    }

    private void GetAllAvailableChoices()
    {
        if (inkEngine.GetCurrentState() != InkEngine.State.Choice_Point) return;
        if (choiceObjectsAndData != DataState.Unintialized) return;

        ObjectPool<InkTextObject>.ReturnAllListItemsToPool(inkChoiceObjects);
        inkChoiceObjects.Clear();

        List<InkParagraph> choices = inkEngine.GetChoices()?.GetListOfChoices();

        if (choices == null) { Debug.LogWarning("Error: No choices available."); return; }

        
        StartCoroutine(LoadAllAvailableChoices(choices));
    }

    private IEnumerator LoadAllAvailableChoices(List<InkParagraph> choices)
    {
        int totalChoices = choices.Count;

        choiceObjectsAndData = DataState.Loading;

        InkTextObject inkTextObj;
        InkParagraph _nextPar;
        InkTextObject _prevTextObj = null;
        for (var q = 0; q < totalChoices; q++)
        {
            if(q != 0) yield return 0;

            inkTextObj = ObjectPool<InkTextObject>.GetPoolObject(textSceneParent, inkTextObjectPrefab);
            //inkTextObjects.Add(inkTextObj); We keep text separate from choices
            inkChoiceObjects.Add(inkTextObj);

            _nextPar = choices[q];
            inkTextObj.Init(_nextPar);

            if (_prevTextObj != null)
            {
                inkTextObj.SetLocalPosition(new Vector3(inkTextObj.transform.localPosition.x, -_prevTextObj.GetBottomOfText(), inkTextObj.transform.localPosition.z));
            }

            //inkTextObj.ShowText();

            _prevTextObj = inkTextObj;
        }

        choiceObjectsAndData = DataState.Complete;
        if (state == State.Loading_Choices) StartChoicePoint();
    }

    public void StartChoicePoint()
    {
        if (choiceObjectsAndData != DataState.Complete)
        {
            state = State.Loading_Choices;
            GetAllAvailableChoices();
            Debug.Log("Choices were not pre-loaded, loading now.");
            return;
        }

        state = State.Waiting_For_ChoicesEnd;
        choiceProgressor.Init(inkChoiceObjects, FinishChoicePoint);
    }

    public void FinishChoicePoint(int i)
    {
        inkEngine.SelectChoice(i);
        ClearChoicePoint();

        state = State.GetDataFromInkEngine;
        NextStep();
    }

    public void ClearAllLines()
    {
        ObjectPool<InkTextObject>.ReturnAllListItemsToPool(inkTextObjects);
        inkTextObjects.Clear();
    }

    public void ClearChoicePoint()
    {
        ObjectPool<InkTextObject>.ReturnAllListItemsToPool(inkChoiceObjects);
        inkChoiceObjects.Clear();

        choiceObjectsAndData = DataState.Unintialized;
        
    }

    private void GetAllAvailableLines()
    {
        dialogueObjectsAndData = DataState.Loading;
        ClearChoicePoint();
        ClearAllLines();

        InkTextObject inkTextObj;
        InkParagraph _nextPar;
        
        state = State.Loading_Dialogue;

        while (inkEngine.GetCurrentState() == InkEngine.State.Display_Next_Line)
        {
            inkTextObj = ObjectPool<InkTextObject>.GetPoolObject(textSceneParent, inkTextObjectPrefab);
            inkTextObjects.Add(inkTextObj);

            _nextPar = inkEngine.GetNextLine();
            inkTextObj.Init(_nextPar);
        }

        dialogueObjectsAndData = DataState.Complete;
        dialogueProgressor.Init(inkTextObjects, FinishDialogue, false);
        
        state = State.Waiting_For_DialogueEnd;
        Invoke(nameof(GetAllAvailableChoices), 0.5f); // Get a head start loading in the choices
    }

    private void FinishDialogue()
    {
        Debug.Log("InkInterface: FinishDialogue - finished dialogue, running callback");
        dialogueObjectsAndData = DataState.Unintialized;
        
        state = State.GetDataFromInkEngine;
        NextStep();
    }


}
