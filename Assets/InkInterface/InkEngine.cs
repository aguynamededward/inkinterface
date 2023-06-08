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

    private bool isStoryInitialized = false;

    public void LoadNewStory(TextAsset _inkStory)
    {
        inkJSONAsset = _inkStory;
        isStoryInitialized = false;
        state = State.Uninitialized;
    }

    public void InitializeStory()
    {
        story = new Story(inkJSONAsset.text);
        isStoryInitialized = true;
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

        return sentence;
    }

    public InkChoicePoint GetChoices()
    {
        if (isStoryInitialized == false || story.currentChoices.Count < 1) return null;

        int totalChoices = story.currentChoices.Count;

        InkChoicePoint choicePoint = new InkChoicePoint();
        for(var q = 0; q < totalChoices; q++)
        {
            choicePoint.AddChoice(story.currentChoices[q].text);
        }

        return choicePoint;
    }

    public void SelectChoice(int choiceIndex)
    {
        if (isStoryInitialized == false || story.currentChoices.Count < 1) { Debug.Log("Story not initialized."); return; };

        if (story.currentChoices.Count < 1) { Debug.Log("No choices available right now."); return; };

        if (choiceIndex >= story.currentChoices.Count) { Debug.Log("Choice index " + choiceIndex + " is out of range of current choice total (" + story.currentChoices.Count + ")"); return; };

        story.ChooseChoiceIndex(choiceIndex);

        state = IdentifyCurrentState();
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
    }

    public void AddTag(string _tag)
    {
        tags.Add(_tag);
    }
    public string text;
    public List<string> tags;
}

public class InkChoicePoint
{
    List<string> choices;
    int selectedIndex = -1;

    public InkChoicePoint()
    {
        choices = new List<string>();
        selectedIndex = -1;
    }

    public void AddChoice(string _choice)
    {
        choices.Add(_choice);
    }

    public List<string> GetChoices()
    {
        return choices;
    }


}

