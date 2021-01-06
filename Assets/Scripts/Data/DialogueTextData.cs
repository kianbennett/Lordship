using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// Class to list all random lines of dialogue, with functions to access them with parameters like NPC disposition, profession etc

[System.Serializable]
public class DialogueTextData : ScriptableObject
{
    // Use TextList instead of List<string> or string[] because 
    //  a) can have a function that gives a random string and b) can be serialized properly with SerializableDictionary
    [SerializeField] private TextList _greetingsPositive;
    [SerializeField] private TextList _greetingsNeutral;
    [SerializeField] private TextList _greetingsNegative;

    [SerializeField] private TextList _listeningPositive;
    [SerializeField] private TextList _listeningNeutral;
    [SerializeField] private TextList _listeningNegative;

    // Flattery
    [SerializeField] private SerializableDictionary<CharacterOccupation, TextList> _flatteryOccupation;
    [SerializeField] private SerializableDictionary<CharacterAge, TextList> _flatteryAge;
    [SerializeField] private SerializableDictionary<CharacterWealth, TextList> _flatteryWealth;
    [SerializeField] private TextList _flatteryResponsesSuccess;
    [SerializeField] private TextList _flatteryResponsesFail;

    // Threaten
    [SerializeField] private SerializableDictionary<CharacterOccupation, TextList> _threatenOccupation;
    [SerializeField] private SerializableDictionary<CharacterAge, TextList> _threatenAge;
    [SerializeField] private SerializableDictionary<CharacterWealth, TextList> _threatenWealth;
    [SerializeField] private TextList _threatenResponsesSuccess;
    [SerializeField] private TextList _threatenResponsesFail;
    
    // Bribe
    [SerializeField] private TextList _bribeResponsesSuccess;
    [SerializeField] private TextList _bribeResponsesFail;

    public string GetRandomGreeting(DispositionType disposition) {
        switch(disposition) {
            case DispositionType.Positive:
                return _greetingsPositive.RandomText;
            case DispositionType.Neutral:
                return _greetingsNeutral.RandomText;
            case DispositionType.Negative:
                return _greetingsNegative.RandomText;
        }
        return "";
    }

    public string GetRandomListening(DispositionType disposition) {
        switch(disposition) {
            case DispositionType.Positive:
                return _listeningPositive.RandomText;
            case DispositionType.Neutral:
                return _listeningNeutral.RandomText;
            case DispositionType.Negative:
                return _listeningNegative.RandomText;
        }
        return "";
    }

    public string GetRandomFlattery(CharacterOccupation occupation) {
        if(_flatteryOccupation.ContainsKey(occupation)) {
            return _flatteryOccupation.GetValue(occupation).RandomText;
        }
        return "";
    }

    public string GetRandomFlattery(CharacterAge age) {
        if(_flatteryAge.ContainsKey(age)) {
            return _flatteryAge.GetValue(age).RandomText;
        }
        return "";
    }

    public string GetRandomFlattery(CharacterWealth wealth) {
        if(_flatteryWealth.ContainsKey(wealth)) {
            return _flatteryWealth.GetValue(wealth).RandomText;
        }
        return "";
    }

    public string GetRandomFlattery(NPC npc, bool correct) {
        float occupationCount = System.Enum.GetNames(typeof(CharacterOccupation)).Length;
        float ageCount = 2; // only old and young, not middle-aged
        float wealthCount = 2; // only rich and poor, not average
        float totalCount = occupationCount + ageCount + wealthCount;

        float rVal = Random.value;
        
        // Picks wealth (there are no lines for average age and wealth so default to occupation for those)
        if(rVal < wealthCount / totalCount && npc.wealth != CharacterWealth.GettingBy) {
            // Pick the NPCs if correct, otherwise pick the other age (average wealth doesn't reach here)
            if(correct) {
                return GetRandomFlattery(npc.wealth);
            } else {
                return GetRandomFlattery(npc.wealth == CharacterWealth.Poor ? CharacterWealth.Rich : CharacterWealth.Poor);
            }
        }
        // Picks age
        else if(rVal < (wealthCount + ageCount) / totalCount && npc.age != CharacterAge.MiddleAged) {
            // Pick the NPCs if correct, otherwise pick the other age (middle aged doesn't reach here)
            if(correct) {
                return GetRandomFlattery(npc.age);
            } else {
                return GetRandomFlattery(npc.age == CharacterAge.Youthful ? CharacterAge.Elderly : CharacterAge.Youthful);
            }
        }
        // Picks occupation
        else {
            CharacterOccupation occupation = npc.occupation;
            if(!correct) {
                // Keep picking an occupation that isn't the same as the NPCs
                do {
                    occupation = (CharacterOccupation) Random.Range(0, occupationCount);
                } while(occupation == npc.occupation);
            }
            return GetRandomFlattery(occupation);
        }
    }

    public string GetRandomFlatteryResponse(bool success) {
        if(success) return _flatteryResponsesSuccess.RandomText;
            else return _flatteryResponsesFail.RandomText;
    }

    public string GetRandomThreaten(CharacterOccupation occupation) {
        if(_threatenOccupation.ContainsKey(occupation)) {
            return _threatenOccupation.GetValue(occupation).RandomText;
        }
        return "";
    }

    public string GetRandomThreaten(CharacterAge age) {
        if(_threatenAge.ContainsKey(age)) {
            return _threatenAge.GetValue(age).RandomText;
        }
        return "";
    }

    public string GetRandomThreaten(CharacterWealth wealth) {
        if(_threatenWealth.ContainsKey(wealth)) {
            return _threatenWealth.GetValue(wealth).RandomText;
        }
        return "";
    }

    public string GetRandomThreatenResponse(bool success) {
        if(success) return _threatenResponsesSuccess.RandomText;
            else return _threatenResponsesFail.RandomText;
    }

    public string GetRandomThreaten(NPC npc, bool correct) {
        float occupationCount = System.Enum.GetNames(typeof(CharacterOccupation)).Length;
        float ageCount = 2; // only old and young, not middle-aged
        float wealthCount = 2; // only rich and poor, not average
        float totalCount = occupationCount + ageCount + wealthCount;

        float rVal = Random.value;
        
        // Picks wealth (there are no lines for average age and wealth so default to occupation for those)
        if(rVal < wealthCount / totalCount && npc.wealth != CharacterWealth.GettingBy) {
            // Pick the NPCs if correct, otherwise pick the other age (average wealth doesn't reach here)
            if(correct) {
                return GetRandomThreaten(npc.wealth);
            } else {
                return GetRandomThreaten(npc.wealth == CharacterWealth.Poor ? CharacterWealth.Rich : CharacterWealth.Poor);
            }
        }
        // Picks age
        else if(rVal < (wealthCount + ageCount) / totalCount && npc.age != CharacterAge.MiddleAged) {
            // Pick the NPCs if correct, otherwise pick the other age (middle aged doesn't reach here)
            if(correct) {
                return GetRandomThreaten(npc.age);
            } else {
                return GetRandomThreaten(npc.age == CharacterAge.Youthful ? CharacterAge.Elderly : CharacterAge.Youthful);
            }
        }
        // Picks occupation
        else {
            CharacterOccupation occupation = npc.occupation;
            if(!correct) {
                // Keep picking an occupation that isn't the same as the NPCs
                do {
                    occupation = (CharacterOccupation) Random.Range(0, occupationCount);
                } while(occupation == npc.occupation);
            }
            return GetRandomThreaten(occupation);
        }
    }

    public string GetRandomBribeResponse(bool success) {
        if(success) return _bribeResponsesSuccess.RandomText;
            else return _bribeResponsesFail.RandomText;
    }

#if UNITY_EDITOR
    public const string PathToAsset = "Assets/Data/TextData.asset";

    public static DialogueTextData LoadData()
    {
        DialogueTextData data = AssetDatabase.LoadAssetAtPath<DialogueTextData>(PathToAsset);
        if (data == null)
        {
            data = CreateInstance<DialogueTextData>();
            AssetDatabase.CreateAsset(data, PathToAsset);
        }

        return data;
    }
#endif
}

