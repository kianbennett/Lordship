using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Weight map for colour palette values

[CreateAssetMenu(fileName = "ColourWeights", menuName = "Weight Map/Colour")]
public class ColourWeightMap : WeightMap<PaletteColour> 
{
    public ColourPalette ColourPalette;
    [HideInInspector] public ColourFloatDictionary weightMap = new ColourFloatDictionary();

    public override SerializableDictionary<PaletteColour, float> Map 
    {
        get { return weightMap; }
    }
}