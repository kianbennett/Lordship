using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUD : Singleton<HUD> {

    public DialogueMenu dialogueMenu;
    public Tooltip tooltip;
    public ScreenFader screenFader;
    public OptionsMenu optionsMenu;
    public PauseMenu pauseMenu;
    public GameObject darkOverlay;
}
