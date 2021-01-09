using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DialogueSystem : Singleton<DialogueSystem> {

    [SerializeField] private StoryData _data;
    [SerializeField] private RumourData _rumourData;
    [SerializeField] private DialogueTextData _textData;

    private BeatData _currentBeat;
    private WaitForSeconds _wait, _waitInitial;

    // Store reference to DoDisplay coroutine so we can stop it when exiting dialogue
    private Coroutine displayBeatCoroutine;

    private List<Rumour> availableRumours;
    private Dictionary<NPC, Rumour> unlockedRumours; // Key is the NPC that gives you the rumour

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
        // if (Input.GetKeyDown(KeyCode.Escape))
        // {
        //     if(_currentBeat != null)
        //     {
        //         if (_currentBeat.ID == 1)
        //         {
        //             // Application.Quit();
        //             PlayerController.instance.ExitDialogue();
        //         }
        //         else
        //         {
        //             DisplayBeat(1, false);
        //         }
        //     }
        // }
        // else
        // {
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
        // }
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
        NPC npcSpeaking = PlayerController.instance.npcSpeaking;
        if(nextBeat.Type != DialogueType.Goodbye) 
        {
            switch(nextBeat.DisplayTextType) 
            {
                case SpeechType.FlatterResponse:
                    npcSpeaking.RespondToFlattery(choice.IsCorrectChoice);
                    break;
                case SpeechType.ThreatenResponse:
                    npcSpeaking.RespondToThreaten(choice.IsCorrectChoice);
                    break;
                case SpeechType.BribeResponse:
                    choice.IsCorrectChoice = npcSpeaking.ReceiveBribe(i);
                    LevelManager.instance.SpendBribe(i);
                    break;
                case SpeechType.RumourStart:
                    choice.IsCorrectChoice = npcSpeaking.CanGiveRumour() && HasRumourAvailable(npcSpeaking);
                    if(choice.IsCorrectChoice && !unlockedRumours.ContainsKey(npcSpeaking))
                    {
                        UnlockRandomRumour(npcSpeaking);
                    }
                    break;
                case SpeechType.RumourEnd:
                    npcSpeaking.CompleteRumour();
                    break;
            }
            DisplayBeat(choice.NextID, false, choice.IsCorrectChoice);
        } 
        else 
        {
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
                data.DisplayTextType == SpeechType.BribeResponse) 
        {
            HUD.instance.dialogueMenu.ShowNpcSpeech(data.GetDisplayText(_textData, success), showAnimations);
        } 
        else if(data.DisplayTextType == SpeechType.RumourStart) 
        {
            string text;
            if(success) {
                text = unlockedRumours[npcSpeaking].StartText;
            } else if(HasRumourAvailable(npcSpeaking)) {
                // If there are rumours available but the NPC disposition is too low then give a rejection line
                text = _textData.GetRandomRumourFail();
            } else {
                // If there aren't any rumours available give a neutral apology
                text = _textData.GetRandomRumourUnknown();
            }
            HUD.instance.dialogueMenu.ShowNpcSpeech(text, showAnimations);
        }
        else if(data.DisplayTextType == SpeechType.RumourEnd) 
        {
            Rumour rumour = GetRumourForTargetNpc(npcSpeaking);
            HUD.instance.dialogueMenu.ShowNpcSpeech(rumour.EndText, showAnimations);
            CompleteRumour(rumour);
        }
        else 
        {
            HUD.instance.dialogueMenu.ShowNpcSpeech(data.GetDisplayText(_textData, npcSpeaking.DispositionType), showAnimations);
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

        // Set choices
        List<ChoiceData> choices = new List<ChoiceData>();;

        int correctChoice = Random.Range(0, data.Decision.Count - 1); // -1 to account for the nevermind choice
        List<string> usedTextLines = new List<string>(); // Keep track of use choice text to avoid getting the same line twice
        for(int i = 0; i < data.Decision.Count; i++) 
        {
            data.Decision[i].IsCorrectChoice = i == correctChoice;

            switch(data.Decision[i].TextType) 
            {
                case ChoiceTextType.RandomFlatter:
                    string text;
                    do 
                    {
                        text = _textData.GetRandomFlattery(npcSpeaking, i == correctChoice);
                    } 
                    while(usedTextLines.Contains(text));
                    
                    usedTextLines.Add(text);
                    data.Decision[i].DisplayText = text;
                    if(i == correctChoice) data.Decision[i].DisplayText += " *";
                    break;
                case ChoiceTextType.RandomThreaten:
                    do 
                    {
                        text = _textData.GetRandomThreaten(npcSpeaking, i == correctChoice);
                    } 
                    while(usedTextLines.Contains(text));
                    
                    usedTextLines.Add(text);
                    data.Decision[i].DisplayText = text;
                    if(i == correctChoice) data.Decision[i].DisplayText += " *";
                    break;
                case ChoiceTextType.BribeAmount:
                    data.Decision[i].DisplayText = LevelManager.instance.BribeGoldAmount(i) + " gold";
                    break;
                case ChoiceTextType.RumourMid:
                    Rumour rumour = GetRumourForTargetNpc(npcSpeaking);
                    if(rumour != null) 
                    {
                        data.Decision[i].DisplayText = rumour.MiddleText;
                        choices.Add(data.Decision[i]);
                    }
                    break;
            }

            // Add all choices other than rumour as that is handled in the switch
            if(data.Decision[i].TextType != ChoiceTextType.RumourMid) 
            {
                choices.Add(data.Decision[i]);
            }
        }

        HUD.instance.dialogueMenu.ShowChoicesPanel(choices.ToArray());

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

    // Called from LevelManager to make sure the NPCs have been spawned beforehand
    public void InitialiseRumours() {
        availableRumours = new List<Rumour>();
        unlockedRumours = new Dictionary<NPC, Rumour>();
        List<NPC> availableNpcs = new List<NPC>(TownGenerator.instance.npcSpawner.NpcList);
        for(int i = 0; i < _rumourData.RumourCount; i++) 
        {
            NPC npc = availableNpcs[Random.Range(0, availableNpcs.Count)];
            Rumour rumour = new Rumour(npc, _rumourData.GetRumourStart(i), _rumourData.GetRumourMid(i), _rumourData.GetRumourEnd(i));
            availableRumours.Add(rumour);
            availableNpcs.Remove(npc);
        }
    }

    public bool HasRumourAvailable(NPC npcGiving) {
        // Get sub list of available rumours where the target npc is not the same as the npc giving the rumour
        List<Rumour> possible = availableRumours.Where(o => o.targetNpc != npcGiving).ToList();
        return possible.Count > 0;
    }

    public Rumour UnlockRandomRumour(NPC npcGiving) {
        List<Rumour> possible = availableRumours.Where(o => o.targetNpc != npcGiving).ToList();
        if(possible.Count > 0) {
            Rumour rumour = possible[Random.Range(0, possible.Count)];
            UnlockRumour(npcGiving, rumour);
            return rumour;
        }
        return null;
    }

    // Returns the unlocked rumour with a specified target npc, or null if no rumour with that NPC exists
    public Rumour GetRumourForTargetNpc(NPC targetNpc) {
        List<Rumour> rumours = unlockedRumours.Values.Where(o => o.targetNpc == targetNpc).ToList();
        return rumours.Count > 0 ? rumours[0] : null;
    }

    public void UnlockRumour(NPC npcGiving, Rumour rumour) {
        availableRumours.Remove(rumour);
        unlockedRumours.Add(npcGiving, rumour);
    }

    public void CompleteRumour(Rumour rumour) {
        // Remove the rumour from unlockedRumours (Rumour is value so need to search all Keys to find it)
        foreach(NPC npc in unlockedRumours.Keys) {
            if(unlockedRumours[npc] == rumour) {
                unlockedRumours.Remove(npc);
                break;
            }
        }
    }
}
