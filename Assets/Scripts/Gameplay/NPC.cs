using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    public string charName;
    public CharacterAge age;
    public CharacterOccupation occupation;
    public CharacterWealth wealth;
    public int disposition; // 0 to 100

    [Header("Movement")]
    public bool wander;
    private float pauseTimer;

    protected override void Awake() {
        base.Awake();

        // Give random initial delay so loads of paths don't get calculated all at once
        pauseTimer = Random.Range(0f, 3f);
    }

    protected override void Update() {
        base.Update();

        if(wander && !movement.HasTarget() && !movement.IsSpeaking() && pauseTimer <= 0) {
            GridPoint[] roadGridPoints = LevelManager.instance.RoadGridPoints.Where(o => Vector3.Distance(TownGenerator.instance.GridPointToWorldPos(o), transform.position) < 30).ToArray();
            GridPoint target = roadGridPoints[Random.Range(0, roadGridPoints.Length)];
            movement.MoveToPoint(TownGenerator.instance.GridPointToWorldPos(target));
            bool pause = Random.value > 0.75f; // 75% chance to pause after reaching target
            pauseTimer = pause ? Random.Range(1f, 5f) : 0;
        }
        if(pauseTimer > 0) pauseTimer -= Time.deltaTime;
    }

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
