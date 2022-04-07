using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameDialogue : MonoBehaviour, ILevelCompleteInterface
{
    [SerializeField] TMP_Text dialogueText;

    public void OnLevelCompleted(int level)
    {
        dialogueText.text = "Level " + level.ToString() + " completed. Door opened, proceed to the next area.";
    }
    public void OnCloseDoors()
    {
        dialogueText.text = "Doors closed, get ready for more zombies!";
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
