using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class MaterialSet 
{
    public bool hasColour1, hasColour2, hasSkin;
    public Material[] additionalMaterials;
    public ColourPalette colourPalette1, colourPalette2;
}

[Serializable]
public class MeshMaterialSet 
{
    public string name;
    public Mesh mesh;
    public MaterialSet materials;
}

// Use for pairs of identical meshes (i.e. hands)
[Serializable]
public class MeshPairMaterialSet : MeshMaterialSet 
{
    public Mesh left, right;
}

[Serializable]
public class PaletteColour 
{
    public string name;
    public Color colour;
}

public class AssetManager : Singleton<AssetManager> 
{
    [Header("Materials")]
    public Material vertexColorMaterial;

    [Header("Meshes")]
    public MeshMaterialSet[] hairMeshes;
    public MeshMaterialSet[] bodyMeshes;
    public MeshPairMaterialSet[] handMeshes;
    public MeshMaterialSet[] hatMeshes;

    [Header("Weight Maps")]
    public MeshWeightMap hairWeightMap;
    public MeshWeightMap bodyWeightMap;
    public MeshWeightMap handsWeightMap;
    public MeshWeightMap hatWeightMap;

    [Header("Colour Palettes")]
    public ColourPalette skinColours;

    // Random names from https://blog.reedsy.com/character-name-generator/language/english/
    [Header("Character")]
    public string[] firstNames;
    public string[] lastNames;

    // When creating a new material for a specific colour, store reference to it so it can be reused
    private Dictionary<Color, Material> colouredMaterialDictionary = new Dictionary<Color, Material>();
    private List<Tuple<string, string>> usedNames = new List<Tuple<string, string>>();

    // Gets a VertexColour material with a certain colour, or create a new one if it doesn't exist
    public static Material GetColouredMaterial(Color colour) 
    {
        if (instance.colouredMaterialDictionary.ContainsKey(colour)) 
        {
            return instance.colouredMaterialDictionary[colour];
        } 
        else 
        {
            Material material = new Material(instance.vertexColorMaterial);
            material.SetColor("_Color", colour);
            material.EnableKeyword ("_EMISSION"); // Needs this to be able to set emission colour
            instance.colouredMaterialDictionary.Add(colour, material);
            return material;
        }
    }

    public MeshMaterialSet[] GetMeshesFromBodyPart(BodyPart bodyPart) 
    {
        switch(bodyPart) 
        {
            case BodyPart.Hair: return hairMeshes;
            case BodyPart.Body: return bodyMeshes;
            case BodyPart.Hands: return handMeshes;
            case BodyPart.Hat: return hatMeshes;
            default: return null;
        }
    }

    public int GetColouredMaterialCount() 
    {
        return colouredMaterialDictionary.Count;
    }

    public void ClearMaterialDictionary() 
    {
        colouredMaterialDictionary.Clear();
    }

    // Keep checking random generated names against the list of already used names
    public Tuple<string, string> GetUniqueNpcName()
    {
        Tuple<string, string> name;
        do 
        {
            string firstName = firstNames[UnityEngine.Random.Range(0, firstNames.Length)];
            string lastName = lastNames[UnityEngine.Random.Range(0, lastNames.Length)];
            name = new Tuple<string, string>(firstName, lastName);
        } 
        while(usedNames.Contains(name));
        usedNames.Add(name);
        return name;
    }
}
