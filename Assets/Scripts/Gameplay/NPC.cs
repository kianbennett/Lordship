using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterAge {
    Youthful, MiddleAged, Elderly
}

public enum CharacterWealth {
    Rich, GettingBy, Poor
}

public enum CharacterOccupation {
    Monk, Knight, Craftsman, Homemaker, Farmer, Politician, Unemployed
}

public class NPC : Character {

    [Header("Attributes")]
    [ReadOnly] public string charName;
    [ReadOnly] public CharacterAge age;
    [ReadOnly] public CharacterOccupation occupation;
    [ReadOnly] public CharacterWealth wealth;
    [ReadOnly] public int disposition; // 0 to 100

    // Assign random attributes
    public void Randomise() {
        appearance.Randomise();
        movement.SetLookDir(new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)));

        string firstName = AssetManager.instance.firstNames[Random.Range(0, AssetManager.instance.firstNames.Length)];
        string lastName = AssetManager.instance.lastNames[Random.Range(0, AssetManager.instance.lastNames.Length)];
        charName = firstName + " " + lastName;
        age = (CharacterAge) Random.Range(0, System.Enum.GetNames(typeof(CharacterAge)).Length);
        occupation = (CharacterOccupation) Random.Range(0, System.Enum.GetNames(typeof(CharacterOccupation)).Length);
        wealth = (CharacterWealth) Random.Range(0, System.Enum.GetNames(typeof(CharacterWealth)).Length);

        disposition = Random.Range(40, 60);
    }

    public override void SetHovered(bool hovered) {
        base.SetHovered(hovered);
        if(hovered) {
            HUD.instance.tooltip.Show(charName);
        } else {
            HUD.instance.tooltip.Hide();
        }
    }

    public override void OnRightClick() {
        PlayerController.instance.TalkToNpc(this);
    }

    // Most enums can be represented by a string from a direct cast, but some need changes (spaces, hyphons etc)
    public string GetAgeString() {
        if(age == CharacterAge.MiddleAged) {
            return "Middle-aged";
        } else {
            return age.ToString();
        }
    }

    public string GetWealthString() {
        if(wealth == CharacterWealth.GettingBy) {
            return "Getting by";
        } else {
            return wealth.ToString();
        }
    }

    public string GetOccupationString() {
        return occupation.ToString();
    }
}
