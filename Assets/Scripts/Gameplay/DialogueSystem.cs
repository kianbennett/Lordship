using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueSystem : Singleton<DialogueSystem> {

    [SerializeField] private StoryData _data;

    private BeatData _currentBeat;
    private WaitForSeconds _wait, _waitInitial;

    // Store reference to DoDisplay coroutine so we can stop it when exiting dialogue
    private Coroutine displayBeatCoroutine;

    private bool IsTextScrolling {
        get {
            return HUD.instance.dialogueMenu.NpcSpeechText.IsBusy;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        _currentBeat = null;
        _wait = new WaitForSeconds(0.5f);
        _waitInitial = new WaitForSeconds(0.3f);
    }

    private void Update()
    {
        // If the text isn't scrolling then show the first beat, otherwise check input
        // if(!IsTextScrolling)
        // {
        //     if (_currentBeat == null)
        //     {
        //         DisplayBeat(1);
        //     }
        //     else
        //     {
        //         UpdateInput();
        //     }
        // }

        if(!IsTextScrolling && _currentBeat != null) 
        {
            UpdateInput();
        }
    }

    private void UpdateInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(_currentBeat != null)
            {
                if (_currentBeat.ID == 1)
                {
                    // Application.Quit();
                    PlayerController.instance.ExitDialogue();
                }
                else
                {
                    DisplayBeat(1, false);
                }
            }
        }
        else
        {
            KeyCode alpha = KeyCode.Alpha1;
            KeyCode keypad = KeyCode.Keypad1;
            
            // Loops through each beat and check if the corresponding key is pressed
            for (int count = 0; count < _currentBeat.Decision.Count; ++count)
            {
                if (alpha <= KeyCode.Alpha9 && keypad <= KeyCode.Keypad9)
                {
                    if (Input.GetKeyDown(alpha) || Input.GetKeyDown(keypad))
                    {
                        // DisplayBeat(choice.NextID);
                        handleChoice(count);
                        break;
                    }
                }

                ++alpha;
                ++keypad;
            }
        }
    }

    public void PickChoice(int i) {
        if(!IsTextScrolling && _currentBeat != null) {
            handleChoice(i);
        }
    }

    private void handleChoice(int i) 
    {
        ChoiceData choice = _currentBeat.Decision[i];
        BeatData nextBeat = _data.GetBeatById(choice.NextID);
        if(nextBeat.Type != DialogueType.Goodbye) {
            if(nextBeat.DisplayTextType == SpeechType.FlatterResponse) {
                PlayerController.instance.npcSpeaking.RespondToFlattery(choice.IsCorrectChoice);
            } else if (nextBeat.DisplayTextType == SpeechType.ThreatenResponse) {
                PlayerController.instance.npcSpeaking.RespondToThreaten(choice.IsCorrectChoice);
            } else if (nextBeat.DisplayTextType == SpeechType.BribeResponse) {
                choice.IsCorrectChoice = PlayerController.instance.npcSpeaking.ReceiveBribe(i);
            }
            DisplayBeat(choice.NextID, false, choice.IsCorrectChoice);
        } else {
            PlayerController.instance.ExitDialogue();
        }
    }

    public void DisplayBeat(int id, bool showAnimations, bool success = false)
    {
        BeatData data = _data.GetBeatById(id);
        // If the coroutine is already going then stop it
        if(displayBeatCoroutine != null) StopCoroutine(displayBeatCoroutine);
        displayBeatCoroutine = StartCoroutine(DoDisplay(data, showAnimations, success));
        _currentBeat = data;
    }

    // Starts coroutines in order to scroll beat text and choices on the screen
    // success parameter is used for certain choice responses
    private IEnumerator DoDisplay(BeatData data, bool showAnimations, bool success = false)
    {
        if(showAnimations) {
            HUD.instance.dialogueMenu.SetActive(true);
            yield return _waitInitial;
        } else {
            HUD.instance.dialogueMenu.HideChoicesPanel();
        }

        NPC npcSpeaking = PlayerController.instance.npcSpeaking;
        TextDisplay npcSpeech = HUD.instance.dialogueMenu.NpcSpeechText;
        npcSpeech.Clear();

        while (npcSpeech.IsBusy)
        {
            yield return null;
        }

        if(data.DisplayTextType == SpeechType.FlatterResponse || data.DisplayTextType == SpeechType.ThreatenResponse ||
                data.DisplayTextType == SpeechType.BribeResponse) {
            HUD.instance.dialogueMenu.ShowNpcSpeech(data.GetDisplayText(success), showAnimations);
        } else {
            HUD.instance.dialogueMenu.ShowNpcSpeech(data.GetDisplayText(npcSpeaking.DispositionType), showAnimations);
        }
        // npcSpeech.Display(data.GetDisplayText(npcSpeaking.disposition));

        // On the greeting beat wait until the speech has finished before showing the other panels
        while(npcSpeech.IsBusy)
        {
            yield return null;
        }

        if(showAnimations) HUD.instance.dialogueMenu.ShowNpcInfo(npcSpeaking);
        // yield return _wait;

        // Copy choices from other beat if needed
        if(data.CopyChoicesFromBeat) {
            int id = data.BeatIdToCopyFrom;
            data.Decision = _data.GetBeatById(id).Decision;
        }

        // Set choice text
        int correctChoice = Random.Range(0, data.Decision.Count - 1); // -1 to account for the nevermind choice
        DialogueTextData textData = DialogueTextData.LoadData();
        List<string> usedTextLines = new List<string>(); // Keep track of use choice text to avoid getting the same line twice
        for(int i = 0; i < data.Decision.Count; i++) {
            data.Decision[i].IsCorrectChoice = i == correctChoice;
            switch(data.Decision[i].TextType) {
                case ChoiceTextType.RandomFlatter:
                    string text;
                    do {
                        text = textData.GetRandomFlattery(npcSpeaking, i == correctChoice);
                    } while(usedTextLines.Contains(text));
                    usedTextLines.Add(text);
                    data.Decision[i].DisplayText = text;
                    if(i == correctChoice) data.Decision[i].DisplayText += " *";
                    break;
                case ChoiceTextType.RandomThreaten:
                    do {
                        text = textData.GetRandomThreaten(npcSpeaking, i == correctChoice);
                    } while(usedTextLines.Contains(text));
                    usedTextLines.Add(text);
                    data.Decision[i].DisplayText = text;
                    if(i == correctChoice) data.Decision[i].DisplayText += " *";
                    break;
            }
        }

        HUD.instance.dialogueMenu.ShowChoicesPanel(data.Decision.ToArray());

        // for (int count = 0; count < data.Decision.Count; ++count)
        // {
        //     ChoiceData choice = data.Decision[count];
        //     npcSpeech.Display(string.Format("{0}: {1}", (count + 1), choice.DisplayText));

        //     while (npcSpeech.IsBusy)
        //     {
        //         yield return null;
        //     }
        // }

        // if(data.Decision.Count > 0)
        // {
        //     npcSpeech.ShowWaitingForInput();
        // }
    }

    public void ExitDialogue() 
    {
        if(displayBeatCoroutine != null) StopCoroutine(displayBeatCoroutine);
        HUD.instance.dialogueMenu.HideAll();
    }
}
