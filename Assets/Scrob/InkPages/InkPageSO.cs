using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


[CreateAssetMenu(fileName = "New InkPage SO", menuName = "InkPage")]
public class InkPageSO : ScriptableObject
{
    public Transform InkPagePrefab;
    public Transform InkTextObjectPrefab;
    public TMP_FontAsset font;

    [System.NonSerialized]
    public int nameToHash;

    private void OnEnable()
    {
        Debug.Log(name + " scrob just ran OnEnable.");
        nameToHash = name.ToLower().GetHashCode();
        if (!inkPageReference.ContainsKey(nameToHash)) inkPageReference.Add(nameToHash, this);
    }

    private void OnDisable()
    {
        if (inkPageReference.ContainsKey(nameToHash)) inkPageReference.Remove(nameToHash);
    }

    #region Static elements

    public static Dictionary<int, InkPageSO> inkPageReference = new Dictionary<int, InkPageSO>();
    public static Dictionary<int, PageDirector> pageDirectorDictionary = new Dictionary<int, PageDirector>();

    public static PageDirector InstantiatePage(string inkPageName,Vector3 instantiationPosition, Quaternion instantiateRotation, InputSO inputSO) => InstantiatePage(inkPageName.ToLower().GetHashCode(), instantiationPosition, instantiateRotation, inputSO);

    public static PageDirector InstantiatePage(int inkPageHash, Vector3 instantiationPosition, Quaternion instantiateRotation, InputSO inputSO)
    {
        if (!inkPageReference.ContainsKey(inkPageHash))
        {
            // We asked for a page that doesn't exist
            return null;
        }

        if (VerifyAndHandlePageDirector(inkPageHash, out PageDirector existingPageDirector))
        {
            return existingPageDirector;
        }

        InkPageSO inkPage = inkPageReference[inkPageHash];

        PageDirector pageDirector = Instantiate(inkPage.InkPagePrefab, instantiationPosition, instantiateRotation).GetComponent<PageDirector>();

        pageDirectorDictionary.Add(inkPage.nameToHash, pageDirector);

        pageDirector.SetupScene(inkPage, inputSO);

        return pageDirector;

    }

    private static bool VerifyAndHandlePageDirector(int pageHash, out PageDirector existingPageDirector)
    {
        existingPageDirector = null;
        if (pageDirectorDictionary.ContainsKey(pageHash))
        {
            existingPageDirector = pageDirectorDictionary[pageHash];

            if (existingPageDirector) return true;
            else pageDirectorDictionary.Remove(pageHash);
        }

        return false;

    }

    public static bool CheckPageDirectorExists(int pageHash, out PageDirector existingPageDirector) => VerifyAndHandlePageDirector(pageHash, out existingPageDirector);

    #endregion
}
