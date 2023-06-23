using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 
    How does this work?
    Basically, this takes the theory of InkChoicePoint (a bundled group of InkParagraphs) and makes it applicable for any type of content, including empty lines and "stop" commands, etc.

Package Loading half

    Update loop
        Is the queue empty?  And we're not waiting? (pendingPackageId = -1?)
            Start loading more content.

    The InkInterface goes to work loading content into the content package queue.

    One time check: 
        It pulls the very first narrative line. 
            -> Check tags: does it have a scene established? 
                If so, set that to current scene.
                If not, set current scene to default.

        Keep loading content into this package until we hit one of these:
            -> A choice
                Set the current package to "Narrative" and add to queue.  Start a new package (Choice) with the same scene. Load up the choices into the new package and add it to the queue.
                Set our pendingPackageId to the ID of the choices
            -> A scene change
                Finish the current package and add to queue.  Add ANOTHER package that's "End" type. Call the scene loader to pre-load the new scene, and set the current page to that. (Save prev scene as PrevScene)
                Go back to the top of the ordes and keep loading.
            -> An interface request
                Finish current package, add to queue.  Add another package that's "pause" (page will de-emphasize / shut down any clickables, but won't leave). Set our pendingPackageId to that.
                Pre-load the interface. Package up any data it needs and add it to the queue (Do interfaces only have one line?)
                

        
Package Delivering half
    
    Update loop
        Is there anything in the queue?  No?
            Get out.
        If there's stuff in the queue, does pendingPackageId != the next item in the list?
                If so, grab the next one, and send the next block to the next page.
                TrySendNextPackage(package) - returns true if the PageDirector is ready to receive it.
                


Package Callbacks
    The package always calls back with its own ID #.
    if that's not the number we're looking for, we ignore it.
    If it DOES match pendingPackageId, we set pendingPackageId to -1

On the Page Director's side:
    We have 
    state - state is determined based on several factors:
        is currentPackage null? - it's Empty
        is currentPackage something
                but the length of the inkPar > currentInkTextObjects?  
                        We're Waiting_For_Load
                the length is same as Current Ink Text Objects?
                        we're in whatever state the package is
        (we can do the same thing with nextState and the nextInkTextObjectList, actually - could make it one method called ResolveState(currentPackage,currentInkTextObjects) )
        

    currentInkObjectsList
    currentPackage
    
    nextInkObjectsList - the objects we're instantiating in advance 
    nextPackage - the package we're going to be processing next


  It starts with InkInterface calling TrySendNextPackage(package)
    What are we currently doing?
        if our state is Empty, (currentPackage is null), we accept the package. Then we process the package (below), and send it immediately to the relevant progressor.
        If our currentPackage is something, but our nextPackage is empty, we set our NextPackage to that and start it processing, but we STILL send "false" to the InkInterface.
        If the packageId == our current package, we send back true and do nothing else (we've already processed it).
        If both current and nextPackage are full, we send back "false".


    Update loop
        what's our current state?
            empty?
                what's our next state?
                    empty?  Get out.
                    something?  Move the nextPackage and nextInkObjectsList to currentInkObjectsList and currentPackage

        is currentList empty and we have a package? what's the category?
            Pause?  Leave the package as it is, we're done for the moment.
            Stop?
                Start shutting down all the objects - when that's done, we'll run the finishcallback on the stop package and that will get the system going again.

            Narrative?
                Initialize the next text object, add it to the currentList and the TextObjectList (get this index)
                If the narrative progressor isn't active, activate it, and pass it the index fromt he TextObjectlist, and the total # of new items coming, and the packageID
                    when narrative progressor hits the last item #, it'll call back with the package ID #
                            if our currentPackage still equals that, we clear the current package and the currentList
                
            Choice?
                Start the coroutine to initialize all the text objects.  At the end, send the list to the choice progressor, along with the package ID
                    when choice progressor is done, it'll pass back the choice index, and we'll somehow report it back to the Ink interface (maybe pass the packageID and the choice index?)
                    then we'll clear the current package / list
    
        Is currentList.Count == total items in package?  We're done for now
        

        Is nextList empty and we have a package?
            Start processing it, but instead of passing the data to the progressor, just stop.  

        is nextList partially full but not as much as the package? Get out.

        
            
            
        
        

 */


public class ContentPackage : IEqualityComparer<ContentPackage>
{
    /// <summary>
    /// contentCount is reset on load, and the numbers may not be in order. It's purely a system for knowing whether a package is the same or not.
    /// </summary>
    private static int contentCount = 0;
    public static InkDelegate.CallbackInt OnPackageComplete;
    public static InkDelegate.CallbackIntInt OnPackageDecisionComplete;

    public int id { get; private set; }
    public int pageHash { get; private set; }
    public List<InkParagraph> inkParagraphList { get; private set; }
    public PackageType packageType { get; private set; }

    public ContentPackage(int _pageHash)
    {
        id = contentCount++;
        pageHash = _pageHash;
        packageType = PackageType.Null;
    }


    public ContentPackage(PackageType _packageType)
    {
        id = contentCount++;
        pageHash = -1;
        packageType = _packageType;
    }

    public ContentPackage(int _pageHash, PackageType _packageType)
    {
        id = contentCount++;
        pageHash = _pageHash;
        packageType = _packageType;
    }
    
    public void SetPageHash(int _pageHash)
    {
        pageHash = _pageHash;
    }
    public void SetParagraphList(List<InkParagraph> _parList)
    {
        inkParagraphList = _parList;
    }

    public void SetPackageType(PackageType _packageType)
    {
        packageType = _packageType;
    }

    // Page Director calls these after receiving the callbacks from its progressors
    public void FinishCallback()
    {
        OnPackageComplete?.Invoke(id);
    }

    public void FinishChoiceCallback(int i)
    {
        OnPackageDecisionComplete?.Invoke(id, i);
    }

    public bool Equals(ContentPackage x, ContentPackage y)
    {
        return x.id == y.id;
    }

    public int GetHashCode(ContentPackage obj)
    {
        return obj.id;
    }
}


public enum PackageType { 
    Null,
    Shutdown,           // Cleanup whatever content you created and get out
    Pause,          // You're done for now, but don't close up shop, we're coming back to you
    Narrative,      // This is just a block of narrative text. The next one may or may not be for you
    Choice,         // This a block of choices. There will be nothing else loaded till this is done.
    CustomInput     // This is a custom input package
}

