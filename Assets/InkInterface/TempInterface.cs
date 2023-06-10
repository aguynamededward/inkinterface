using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DataState
{
    Unintialized,   // Unloaded state, or after we've completed a dialogue run and are waiting for input
    Loading,        // Currently loading dialogue objects in the background
    Complete        // All dialogue objects loaded, waiting for end of dialogue run
}
public class TempInterface : MonoBehaviour
{
    // Internally, we continue pushing through dialogue until we hit a choice point (later we'll add support for tags that request specific inputs - we'll stop before we grab more dialogue)
    enum State
    {
        Unintialized,
        GetDataFromInkEngine,
        Loading_Narrative,
        Waiting_For_NarrativeComplete,
        Loading_Choices,
        Waiting_For_ChoicesComplete
    }

    // 


    [SerializeField] State state = State.Unintialized;
    [SerializeField] TextAsset inkJson;
    //[SerializeField] Transform textSceneParent;
    //[SerializeField] Transform inkTextObjectPrefab;
    [SerializeField] InkInput_DialogueProgression dialogueProgressor;
    [SerializeField] InkPlayerInput_ChooseOption choiceProgressor;
    [SerializeField] TextDirector textDirector;

    InkEngine inkEngine;

    // Start is called before the first frame update
    void Start()
    {
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
                    if (textDirector.narrativeObjectsAndData == DataState.Unintialized)
                    {
                        state = State.Loading_Narrative;
                        LoadAllAvailableNarrativeLines();
                    }
                    else state = State.Waiting_For_NarrativeComplete;
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

    private void LoadAllAvailableChoices()
    {
        if (inkEngine.GetCurrentState() != InkEngine.State.Choice_Point) return;
        if (textDirector.choiceObjectsAndData != DataState.Unintialized) return;

        textDirector.ClearChoicePoint();

        List<InkParagraph> choices = inkEngine.GetChoices()?.GetListOfChoices();
        if (choices == null) { Debug.LogWarning("Error: No choices available."); return; }

        textDirector.LoadChoices(choices, ChoiceLoadCompleteCallback);

    }

    public void StartChoicePoint()
    {
        if (textDirector.choiceObjectsAndData == DataState.Unintialized)
        {
            state = State.Loading_Choices;
            LoadAllAvailableChoices();
            Debug.Log("Choices were not pre-loaded, loading now.");
            return;
        }
        else if(textDirector.choiceObjectsAndData == DataState.Loading)
        {
            state = State.Loading_Choices;
            return;
        }

        state = State.Waiting_For_ChoicesComplete;
        textDirector.ShowChoices();
        choiceProgressor.Init(textDirector.GetChoiceObjects(), FinishChoicePoint);
    }

    public void FinishChoicePoint(int i)
    {
        inkEngine.SelectChoice(i);
        textDirector.ClearChoicePoint();

        state = State.GetDataFromInkEngine;
        NextStep();
    }

    public void ClearAllLines()
    {
        ObjectPool<InkTextObject>.ReturnAllListItemsToPool(inkTextObjects);
        inkTextObjects.Clear();
    }


    private void LoadAllAvailableNarrativeLines()
    {
       if(inkEngine.GetCurrentState() != InkEngine.State.Display_Next_Line)
        {
            Debug.Log("NO dialogue to display yet.");
            NextStep();
            return;
        }

       List<InkParagraph> inkPars = new List<InkParagraph>();
       while(inkEngine.GetCurrentState() == InkEngine.State.Display_Next_Line)
        {
            inkPars.Add(inkEngine.GetNextLine());
        }

        textDirector.LoadNarrative(inkPars, NarrativeLoadCompleteCallback);
    }

    private void NarrativeLoadCompleteCallback(int startingPosition)
    {
        dialogueProgressor.Init(textDirector.GetNarrativeObjects(), startingPosition,textDirector.ShowNarrativeAtIndex,FinishNarrative);

        state = State.Waiting_For_NarrativeComplete;

        if(inkEngine.GetCurrentState() == InkEngine.State.Choice_Point)
        {
            LoadAllAvailableChoices();
        }
    }

    private void ChoiceLoadCompleteCallback(int i)
    {
        if (state == State.Loading_Choices) StartChoicePoint();
    }

    private void FinishNarrative()
    {
        Debug.Log("InkInterface: FinishDialogue - finished dialogue, running callback");
        textDirector.ResetNarrativeDataState();

        state = State.GetDataFromInkEngine;
        NextStep();
    }


}
