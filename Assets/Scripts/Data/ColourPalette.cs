using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A ScriptableObject for holding a selection of colours and their weight map

[CreateAssetMenu(fileName = "Palette", menuName = "Colour Palette")]
public class ColourPalette : ScriptableObject 
{

    public ColourWeightMap weightMap;
    [HideInInspector] public PaletteColour[] colours;

    public int RandomColourIndex() 
    {
        if (weightMap) return weightMap.GetRandomIndex();
            else return Random.Range(0, colours.Length - 1);
    }

    public PaletteColour RandomColour() 
    {
        return colours[RandomColourIndex()];
    }
}