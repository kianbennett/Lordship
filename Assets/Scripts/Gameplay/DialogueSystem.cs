using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueSystem : Singleton<DialogueSystem> {

    [SerializeField] private StoryData _data;

    private BeatData _currentBeat;
    private WaitForSeconds _wait;

    protected override void Awake()
    {
        base.Awake();
        _currentBeat = null;
        _wait = new WaitForSeconds(0.5f);
    }

    private void Update()
    {
        // If the text isn't scrolling then show the first beat, otherwise check input
        // if(HUD.instance.dialogueMenu.NpcSpeechText.IsIdle)
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
    }

    private void UpdateInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(_currentBeat != null)
            {
                if (_currentBeat.ID == 1)
                {
                    Application.Quit();
                }
                else
                {
                    DisplayBeat(1);
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
                        ChoiceData choice = _currentBeat.Decision[count];
                        DisplayBeat(choice.NextID);
                        break;
                    }
                }

                ++alpha;
                ++keypad;
            }
        }
    }

    private void DisplayBeat(int id)
    {
        BeatData data = _data.GetBeatById(id);
        StartCoroutine(DoDisplay(data));
        _currentBeat = data;
    }

    public void DisplayInitialBeat() {
        HUD.instance.dialogueMenu.SetActive(true);

        BeatData data = new BeatData(0, "Random greeting", new List<ChoiceData>() {
            new ChoiceData("Flatter...", ChoiceType.Flatter),
            new ChoiceData("Threaten...", ChoiceType.Threaten),
            new ChoiceData("Bribe...", ChoiceType.Bribe),
            new ChoiceData("Rumours", ChoiceType.Rumours),
            new ChoiceData("Goodbye", ChoiceType.Goodbye),
        });
        DoDisplay(data);
    }

    // Starts coroutines in order to scroll beat text and choices on the screen
    private IEnumerator DoDisplay(BeatData data)
    {
        TextDisplay npcSpeech = HUD.instance.dialogueMenu.NpcSpeechText;
        npcSpeech.Clear();

        while (npcSpeech.IsBusy)
        {
            yield return null;
        }

        npcSpeech.Display(data.DisplayText);

        while(npcSpeech.IsBusy)
        {
            yield return null;
        }

        HUD.instance.dialogueMenu.ShowNpcInfo(PlayerController.instance.npcSpeaking);
        // yield return _wait;
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
}
