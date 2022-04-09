using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameDialogue : MonoBehaviour, ILevelCompleteInterface
{
    [SerializeField] TMP_Text dialogueText;

    public void OnLevelCompleted(int level)
    {
        StartCoroutine(OnDialogue("Level " + level.ToString() + " completed. Door opened, proceed to the next area."));
    }
    public void OnCloseDoors()
    {
        StartCoroutine(OnDialogue("Doors closed, get ready for more zombies!"));
    }
    private IEnumerator OnDialogue(string dialogue)
    {
        dialogueText.text = dialogue;
        yield return new WaitForSeconds(5f);
        dialogueText.text = string.Empty;
    }

    // Start is called before the first frame update
    void Start()
    {
        dialogueText.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
