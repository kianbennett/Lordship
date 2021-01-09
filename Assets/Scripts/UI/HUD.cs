using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUD : Singleton<HUD> {

    public Tooltip tooltip;
    public ScreenFader screenFader;
    public GameObject darkOverlay;
    public Clock clock;

    public DialogueMenu dialogueMenu;
    public OptionsMenu optionsMenu;
    public PauseMenu pauseMenu;
    public ResultsMenu resultsMenu;
    public EndingMenu endingMenu;
    public TutorialMenu tutorialMenu;
    public TextMeshProUGUI textDate;
    public CanvasGroup textDateCanvasGroup;
}
