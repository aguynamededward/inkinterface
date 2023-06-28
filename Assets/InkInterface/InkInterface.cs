using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DataState
{
    Unintialized,   // Unloaded state, or after we've completed a dialogue run and are waiting for input
    Loading,        // Currently loading dialogue objects in the background
    Loading_Complete,        // All dialogue objects loaded, waiting for end of dialogue run
    Waiting_For_Player_To_Finish // We've sent the package to the progressor, we're just waiting for it to be done.
}
public class InkInterface : MonoBehaviour
{
    // Internally, we continue pushing through dialogue until we hit a choice point (later we'll add support for tags that request specific inputs - we'll stop before we grab more dialogue)
    enum State
    {
        Unintialized,
        GetDataFromInkEngine,
        Loading_Narrative,              // --- Narrative Progression
        Waiting_For_NarrativeComplete,  // --- returns blank InkDelegate.Callback()
        Loading_Choices,                // ---- Choose Option
        Waiting_For_ChoicesComplete,    // Returns an InkDelegate.Callback(int)
        Loading_Input,                  // --- Ink Custom Input
        Waiting_For_InputComplete       // --- returns a blank InkDelegate.Callback
    }



    [SerializeField] State state = State.Unintialized;
    [SerializeField] TextAsset inkJson;

    [SerializeField] InkPlayerInput_NarrativeProgression dialogueProgressor;
    [SerializeField] InkPlayerInput_ChoiceProgression choiceProgressor;
    [SerializeField] TextDirector textDirector;

    
    [Header("Page Setup Variables")]
    [SerializeField] InputSO inputScrob;
    [SerializeField] public InkPageSO defaultPageDirectorSO;
    [SerializeField] public WorldDepth pageBackgroundDepth = WorldDepth.Background;
    [SerializeField] public WorldDepth pageTextDepth = WorldDepth.Text;

    [Header("Important Ink Tags")]
    [SerializeField] InkTagSO CustomInterfaceTag;
    [SerializeField] InkTagSO NewSceneTag;

    InkEngine inkEngine;

    public PageDirector currentPageDirector;
    public PageDirector prevPageDirector;
    public PageDirector defaultPageDirector;

    #region Loading Variables

    private int pendingPackageId = -1;
    private Queue<ContentPackage> contentPackageQueue = new Queue<ContentPackage>();

    #endregion

    #region Page Instantiation Variables

    private Vector3 pageInstatiationPosition;
    private Quaternion pageInstantiationRotation;

    #endregion


    // Start is called before the first frame update
    void Start()
    {
        inkEngine = new InkEngine();
        inkEngine.LoadNewStory(inkJson);
        inkEngine.InitializeStory();
        state = State.GetDataFromInkEngine;

        SetupPageInstantiation();
        InstantiateDefaultPageDirector();

    }

    private void SetupPageInstantiation()
    {
        Vector2 displayCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        pageInstatiationPosition = WorldInterface.ScreenSpaceToWorldSpaceAtDepth(displayCenter, pageBackgroundDepth);
        pageInstantiationRotation = Quaternion.identity;
    }

    private void InstantiateDefaultPageDirector()
    {
        defaultPageDirector = InkPageSO.InstantiatePage(defaultPageDirectorSO.name, pageInstatiationPosition, pageInstantiationRotation, inputScrob);

    }

    private void OnEnable()
    {
        ContentPackage.OnPackageComplete += ContentPackage_OnPackageComplete;
        ContentPackage.OnPackageDecisionComplete += ContentPackage_OnPackageDecisionComplete;
    }
    private void OnDisable()
    {
        ContentPackage.OnPackageComplete -= ContentPackage_OnPackageComplete;
        ContentPackage.OnPackageDecisionComplete -= ContentPackage_OnPackageDecisionComplete;
    }


    #region Content Package callbacks 
    private void ContentPackage_OnPackageComplete(int i)
    {
        if (i == pendingPackageId) pendingPackageId = -1;
        Debug.Log("Package #" + i + " reports finished.");
    }


    private void ContentPackage_OnPackageDecisionComplete(int _id, int _choiceIndex)
    {
        if (_id != pendingPackageId) return;

        if(inkEngine.GetCurrentState() != InkEngine.State.Choice_Point)
            {
                Debug.Log("Attempted to declare a choice but we're not at a choice point!  Something has gone wrong.");
                return;
            }

        if(Mathf.Clamp(_choiceIndex,0,inkEngine.GetChoiceTotal()) != _choiceIndex)
        {
            Debug.Log("WRONG CHOICE INDEX: Sent " + _choiceIndex + " but we only have " + inkEngine.GetChoiceTotal() + " choices!!");
            return;
        }

        inkEngine.SelectChoice(_choiceIndex);
        pendingPackageId = -1;

            // This needs to correspond to the 
            //ContentPackage currentWaitingPackage = contentPackageQueue.Peek();
            //if(currentWaitingPackage.id == _id && currentWaitingPackage.packageType == PackageType.Choice)
            //{
            //    if (Mathf.Clamp(_choiceIndex, 0, currentWaitingPackage.inkParagraphList.Count - 1) == _choiceIndex) // Did we pick an item inside the choice list?
            //    {
            //        inkEngine.SelectChoice(_choiceIndex);
            //        pendingPackageId = -1;
            //        contentPackageQueue.Dequeue(); // Pop the choice off the list, we're good
            //    }
            //    else
            //    {
            //        Debug.Log("InkInterface: Chose " + _choiceIndex + " which is outside the bounds of 0 and " + (currentWaitingPackage.inkParagraphList.Count - 1) + ".");
            //    }
            //}
        
    }

    #endregion


    private void Update()
    {
        HandlePackageLoading();

        HandlePackageDelivery();
    }

    private void HandlePackageLoading()
    {
        // Loading content packages
        if (pendingPackageId == -1) LoadContentPackageIntoQueue();
    }

    private void HandlePackageDelivery()
    {
        if (contentPackageQueue.Count <= 0) return;

        ContentPackage currentPkg = contentPackageQueue.Peek();

        // Are we waiting on a package to finish?
        if (pendingPackageId == -1 || 
            (pendingPackageId != -1 && currentPkg.id == pendingPackageId)) // If we're waiting, do we still have the awaited package in the queue?
        {
            if (TrySendNextPackage(currentPkg))
            {
                contentPackageQueue.Dequeue();
            }
        }
    }

    private bool TrySendNextPackage(ContentPackage currentPkg)
    {
        if(currentPkg.pageHash == -1)
        {
            Debug.Log("InkInterface: currentPkg #" + currentPkg.id + " pageHash is -1. Returning TRUE, because there's an error somewhere, but we need to get rid of this package.");
            return true;
        }

        if(InkPageSO.CheckPageDirectorExists(currentPkg.pageHash,out PageDirector recipient))
        {
            return recipient.TrySendNextPackage(currentPkg);
        }

        return false;
    }

    private void LoadContentPackageIntoQueue()
    {
        int pageDirectorHash = -1;
        if (currentPageDirector) pageDirectorHash = currentPageDirector.nameToHash;
        else pageDirectorHash = defaultPageDirector.nameToHash;

        ContentPackage pkg = new ContentPackage(pageDirectorHash);
        
        var inkEngineState = inkEngine.GetCurrentState();

        switch (inkEngineState)
        {
            case InkEngine.State.Display_Next_Line:
                {
                    HandleLoadNarrativeLines(ref pkg);
                    break;
                }
            case InkEngine.State.Choice_Point:
                {
                    //pkg.SetPackageType(PackageType.Choice); 
                    inkEngine.GetChoices(ref pkg);
                    pkg.SetPackageType(PackageType.Choice);
                    contentPackageQueue.Enqueue(pkg);
                    pendingPackageId = pkg.id;
                    break;
                }
            case InkEngine.State.End: return;
        }

    }

    private void HandleLoadNarrativeLines(ref ContentPackage pkg)
    {
        pkg.SetPackageType(PackageType.Narrative);

        InkParagraph inkPar;
        List<InkParagraph> inkPars = new List<InkParagraph>();
        bool hasNewScene;
        while (inkEngine.GetCurrentState() == InkEngine.State.Display_Next_Line || inkEngine.GetCurrentState() == InkEngine.State.Tags_Only_Line)
        {
            inkPar = inkEngine.GetNextLine();
            hasNewScene = inkPar.tags.Contains(NewSceneTag);

            if (hasNewScene) 
            {
                pkg.SetParagraphList(inkPars);
                contentPackageQueue.Enqueue(pkg);

                ContentPackage stopPkg = new ContentPackage(currentPageDirector.nameToHash, PackageType.Pause);
                contentPackageQueue.Enqueue(stopPkg);
                pendingPackageId = stopPkg.id;


                prevPageDirector = currentPageDirector;
                currentPageDirector = null; //  TODO - get the new scene

                inkPars = new List<InkParagraph>();
                inkPars.Add(inkPar);
                pkg = new ContentPackage(currentPageDirector.nameToHash, PackageType.Narrative);

                break;
            }
            // TODO: Custom interface - Save the custom interaction to our queue, change to the new scene director, and break
            else
            {
                inkPars.Add(inkPar);
            }
        }

        pkg.SetParagraphList(inkPars);
        contentPackageQueue.Enqueue(pkg);
    }

    List<InkTextObject> inkTextObjects = new List<InkTextObject>();
    List<InkTextObject> inkChoiceObjects = new List<InkTextObject>();
    Queue<InkParagraph> customInteractionQueue = new Queue<InkParagraph>();

    

   
    InkTextObject selectedChoice;

   // public void NextStep()
   // {
   //     Debug.Log("InkInterface: NextStep");
   //     switch (state)
   //     {
   //         case State.GetDataFromInkEngine:
   //         {
   //                 GetDataFromInkEngine();
   //                 break;
   //         }
   //         default:
   //         {
   //                 break;
   //         }
   //
   //     }
   //     
   // }
   //
   // public void GetDataFromInkEngine()
   // {
   //     switch (inkEngine.GetCurrentState())
   //     {
   //         case InkEngine.State.Display_Next_Line:
   //             {
   //                 if (!currentPageDirector || currentPageDirector.nextPackageDataState == DataState.Unintialized)
   //                 {
   //                     state = State.Loading_Narrative;
   //                     LoadAllAvailableNarrativeLines();
   //                 }
   //                 else state = State.Waiting_For_NarrativeComplete;
   //                 break;
   //             }
   //         case InkEngine.State.Choice_Point:
   //             {
   //                 StartChoicePoint();
   //                 break;
   //             }
   //         case InkEngine.State.End:
   //             {
   //                 Debug.Log("END OF GAME");
   //                 break;
   //             }
   //     }
   // }
   //
   // private void LoadAllAvailableChoices()
    //{
    //    if (inkEngine.GetCurrentState() != InkEngine.State.Choice_Point) return;
    //    if (textDirector.choiceObjectsAndData != DataState.Unintialized) return;
    //
    //    //textDirector.ClearChoicePoint();
    //
    //    //List<InkParagraph> choices = inkEngine.GetChoices()?.GetListOfChoices();
    //    //if (choices == null) { Debug.LogWarning("Error: No choices available."); return; }
    //    //
    //    //textDirector.LoadChoices(choices, ChoiceLoadCompleteCallback);
    //
    //}
   //
   // public void StartChoicePoint()
   // {
   //     if (textDirector.choiceObjectsAndData == DataState.Unintialized)
   //     {
   //         state = State.Loading_Choices;
   //         LoadAllAvailableChoices();
   //         Debug.Log("Choices were not pre-loaded, loading now.");
   //         return;
   //     }
   //     else if(textDirector.choiceObjectsAndData == DataState.Loading)
   //     {
   //         state = State.Loading_Choices;
   //         return;
   //     }
   //
   //     state = State.Waiting_For_ChoicesComplete;
   //     textDirector.ShowChoices();
   //     choiceProgressor.SetupChoiceMoment(textDirector.GetChoiceObjects(), FinishChoicePoint);
   // }
   //
   // public void FinishChoicePoint(int i)
   // {
   //     inkEngine.SelectChoice(i);
   //     textDirector.ClearChoicePoint();
   //
   //     state = State.GetDataFromInkEngine;
   //     NextStep();
   // }
   //
   // public void ClearAllLines()
    //{
    //    ObjectPool<InkTextObject>.ReturnAllListItemsToPool(inkTextObjects);
    //    inkTextObjects.Clear();
    //}
   //
   //
   // private void LoadAllAvailableNarrativeLines()
   // {
   //    if(inkEngine.GetCurrentState() != InkEngine.State.Display_Next_Line)
   //     {
   //         Debug.Log("NO dialogue to display yet.");
   //         NextStep();
   //         return;
   //     }
   //
   //    List<InkParagraph> inkPars = new List<InkParagraph>();
   //     InkParagraph inkPar;
   //    while(inkEngine.GetCurrentState() == InkEngine.State.Display_Next_Line)
   //     {
   //         inkPar = inkEngine.GetNextLine();
   //         inkPars.Add(inkPar);
   //
   //         if(inkPar.tags.Contains(CustomInterfaceTag)) // Save the custom interaction to our queue and break
   //         {
   //             customInteractionQueue.Enqueue(inkPar);
   //             break;
   //         }
   //     }
   //
   //     textDirector.LoadNarrative(inkPars, NarrativeLoadCompleteCallback);
   // }

    //private void NarrativeLoadCompleteCallback(int startingPosition)
    //{
    //    dialogueProgressor.Init(textDirector.GetNarrativeObjects(), startingPosition,textDirector.ShowNarrativeAtIndex,FinishNarrative);
    //
    //    state = State.Waiting_For_NarrativeComplete;
    //
    //    // Add check if we have a custom interaction object we need to load
    //
    //    if(inkEngine.GetCurrentState() == InkEngine.State.Choice_Point)
    //    {
    //        LoadAllAvailableChoices();
    //    }
    //}
    //
    //private void ChoiceLoadCompleteCallback(int i)
    //{
    //    if (state == State.Loading_Choices) StartChoicePoint();
    //}
    //
    //private void FinishNarrative()
    //{
    //    Debug.Log("InkInterface: FinishDialogue - finished dialogue, running callback");
    //    textDirector.ResetNarrativeDataState();
    //
    //    state = State.GetDataFromInkEngine;
    //    NextStep();
    //}
    //

}
