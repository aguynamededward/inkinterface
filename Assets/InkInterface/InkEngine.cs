using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;

public class InkEngine
{
    public enum State
    {
        None,
        Uninitialized,
        Display_Next_Line,
        Tags_Only_Line,
        Waiting_For_External_Input,
        Choice_Point,
        End
    }

    

    private State state = State.None;

    private TextAsset inkJSONAsset;
    private Story story;

    private InkChoicePoint currentChoicePoint;
    private bool isCurrentChoiceInitialized;

    private bool isStoryInitialized = false;
    /// <summary>
    /// Did we already generate the choices information for this point?
    /// </summary>


    public void LoadNewStory(TextAsset _inkStory)
    {
        inkJSONAsset = _inkStory;
        isStoryInitialized = false;
        isCurrentChoiceInitialized = false;
        state = State.Uninitialized;
    }

    public void InitializeStory()
    {
        story = new Story(inkJSONAsset.text);
        isStoryInitialized = true;
        isCurrentChoiceInitialized = false;
        state = IdentifyCurrentState();
    }

    public State GetCurrentState()
    {
        return state;
    }

    public InkParagraph GetNextLine()
    {
        if (isStoryInitialized == false || story.canContinue == false) return null;

        InkParagraph sentence = new InkParagraph(story.Continue());
        if(story.currentTags.Count > 0)
        {
            int totalTags = story.currentTags.Count;
            for (var q = 0; q < totalTags; q++)
            {
                sentence.AddTag(story.currentTags[q]);
            }
        }

        state = IdentifyCurrentState();
        isCurrentChoiceInitialized = false;

        return sentence;
    }

    public InkChoicePoint GetChoices()
    {
        if (isStoryInitialized == false || story.currentChoices.Count < 1) return null;

        if (isCurrentChoiceInitialized == false)
        {
            int totalChoices = story.currentChoices.Count;

            currentChoicePoint = new InkChoicePoint();
            InkParagraph choice;
            int totalTags;
            for (var q = 0; q < totalChoices; q++)
            {
                choice = new InkParagraph(story.currentChoices[q].text, q);

                if(story.currentChoices[q].tags != null)
                {
                    totalTags = story.currentChoices[q].tags.Count;

                    for (var j = 0; j < totalTags; j++)
                    {
                        choice.AddTag(story.currentChoices[q].tags[j]);
                    }
                }

                currentChoicePoint.AddChoice(choice);
            }

            isCurrentChoiceInitialized = true;
        }
        return currentChoicePoint;
    }

    public void SelectChoice(int choiceIndex)
    {
        if (isStoryInitialized == false || story.currentChoices.Count < 1) { Debug.Log("Story not initialized."); return; };

        if (story.currentChoices.Count < 1) { Debug.Log("No choices available right now."); return; };

        if (choiceIndex >= story.currentChoices.Count || choiceIndex < 0) { Debug.Log("Choice index " + choiceIndex + " is out of range of current choice total (" + story.currentChoices.Count + ")"); return; };

        story.ChooseChoiceIndex(choiceIndex);

        state = IdentifyCurrentState();
        
        isCurrentChoiceInitialized = false;
    }


    private State IdentifyCurrentState()
    {
        if (isStoryInitialized == false) return State.None;

        if (story.currentText.Length < 1 && story.currentTags.Count > 1) return State.Tags_Only_Line;

        if (story.canContinue) return State.Display_Next_Line;

        if (story.currentChoices.Count > 0) return State.Choice_Point;

        return State.End;
    }
}

public class InkParagraph
{

    public InkParagraph(string _text)
    {
        text = _text;
        tags = new List<string>();
        choiceIndex = -1;
    }
    public InkParagraph(string _text, int _choiceIndex)
    {
        text = _text;
        tags = new List<string>();
        choiceIndex = _choiceIndex;
    }

    public void AddTag(string _tag)
    {
        tags.Add(_tag);
    }
    public string text;
    public List<string> tags;

    public int GetChoiceIndex()
    {
        return choiceIndex;
    }

    int choiceIndex;
    public bool IsChoice()
    {
        return choiceIndex > -1;
    }
}

    public class InkChoicePoint
{
    List<InkParagraph> choices;
    int selectedIndex = -1;

    public InkChoicePoint()
    {
        choices = new List<InkParagraph>();
        selectedIndex = -1;
    }

    public void AddChoice(InkParagraph _choice)
    {
        choices.Add(_choice);
    }

    public List<InkParagraph> GetListOfChoices()
    {
        return choices;
    }
}

