using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MeshWeights", menuName = "Weight Map/Mesh")]
public class MeshWeightMap : WeightMap<MeshMaterialSet> {

    public BodyPart bodyPart;
    [HideInInspector] public MeshFloatDictionary weightMap = new MeshFloatDictionary();

    public override SerializableDictionary<MeshMaterialSet, float> Map {
        get { return weightMap; }
    }
}