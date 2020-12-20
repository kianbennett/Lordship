using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Playables;
using UnityEngine.Animations;

public enum BodyPart {
    Hair, Body, Hands, Hat
}

[ExecuteInEditMode]
public class CharacterAppearance : MonoBehaviour {

    public Animator modelAnim;
    [SerializeField] private Character character;
    [SerializeField] private Transform armature;
    [SerializeField] private Transform headBone;
    [SerializeField] private SkinnedMeshRenderer headMesh, hairMesh, bodyMesh, handLMesh, handRMesh;
    [SerializeField] private MeshFilter hatMeshFilter;
    [SerializeField] private MeshRenderer hatMeshRenderer;
    [SerializeField] private bool randomiseOnAwake;

    [HideInInspector] public int hair, hairColour1, hairColour2;
    [HideInInspector] public int body, bodyColour1, bodyColour2;
    [HideInInspector] public int hands, handsColour1, handsColour2;
    [HideInInspector] public int hat, hatColour1, hatColour2;
    [HideInInspector] public int skinColour;

    void Awake() {
        if (randomiseOnAwake) Randomise();
            else ApplyAll();
    }

    void Update() {
        // Lerp towards the desired animation speed
        // if (modelAnim) {
        //     float speed = Mathf.Lerp(modelAnim.GetFloat("Speed"), character.movement.GetAnimSpeed(), Time.deltaTime * 10);
        //     modelAnim.SetFloat("Speed", speed);
        // }

        // Debug.DrawRay(hairMesh.sharedMesh.bounds.center + Vector3.up * hairMesh.bounds.extents.y, Vector3.up * 0.1f);
        // Debug.DrawLine(transform.position + Vector3.up * hairMesh.sharedMesh.bounds.min.y, transform.position + Vector3.up * hairMesh.sharedMesh.bounds.max.y);
    }

    private void applyHair() {
        MeshMaterialSet meshSet = AssetManager.instance.hairMeshes[hair];
        updateMeshRenderer(hairMesh, meshSet.mesh);
        updateMeshMaterials(hairMesh, meshSet, hairColour1, hairColour2);
        if(hairMesh.sharedMesh) hairMesh.sharedMesh.RecalculateBounds();
        updateHatPos();
    }

    private void applyBody() {
        MeshMaterialSet meshSet = AssetManager.instance.bodyMeshes[body];
        updateMeshRenderer(bodyMesh, meshSet.mesh);
        updateMeshMaterials(bodyMesh, meshSet, bodyColour1, bodyColour2);
    }

    private void applyHands() {
        MeshPairMaterialSet meshSet = AssetManager.instance.handMeshes[hands];
        updateMeshRenderer(handLMesh, meshSet.left);
        updateMeshRenderer(handRMesh, meshSet.right);
        updateMeshMaterials(handLMesh, meshSet, handsColour1, handsColour2);
        updateMeshMaterials(handRMesh, meshSet, handsColour1, handsColour2);
    }

    private void applyHat() {
        MeshMaterialSet meshSet = AssetManager.instance.hatMeshes[hat];
        updateMeshFilter(hatMeshFilter, meshSet.mesh);
        updateMeshMaterials(hatMeshRenderer, meshSet, hatColour1, hatColour2);
        updateHatPos();
    }

    private void updateHatPos() {
        float headHeight = headMesh.sharedMesh.bounds.max.y;
        float hairHeight = hairMesh.sharedMesh != null ? hairMesh.sharedMesh.bounds.max.y : 0;
        hatMeshFilter.transform.position = transform.position + Vector3.up * Mathf.Max(hairHeight, headHeight) * modelAnim.transform.localScale.y;
    }

    private void applySkin() {
        setMeshMaterialColour(headMesh, 0, getSkinColor());
        applyBody();
        applyHands();
    }

    public void ApplyAll() {
        applyHair();
        applySkin(); // applyBody and applyHands are both called from applySkin
        applyHat();
    }

    public void Randomise() {
        hair = AssetManager.instance.hairWeightMap.GetRandomIndex();
        body = AssetManager.instance.bodyWeightMap.GetRandomIndex();
        hands = AssetManager.instance.handsWeightMap.GetRandomIndex();
        hat = AssetManager.instance.hatWeightMap.GetRandomIndex();

        ColourPalette palette;
        if (palette = AssetManager.instance.hairMeshes[hair].materials.colourPalette1) hairColour1 = palette.RandomColourIndex();
        if (palette = AssetManager.instance.hairMeshes[hair].materials.colourPalette2) hairColour2 = palette.RandomColourIndex();
        if (palette = AssetManager.instance.bodyMeshes[body].materials.colourPalette1) bodyColour1 = palette.RandomColourIndex();
        if (palette = AssetManager.instance.bodyMeshes[body].materials.colourPalette2) bodyColour2 = palette.RandomColourIndex();
        if (palette = AssetManager.instance.handMeshes[hands].materials.colourPalette1) handsColour1 = palette.RandomColourIndex();
        if (palette = AssetManager.instance.handMeshes[hands].materials.colourPalette2) handsColour2 = palette.RandomColourIndex();
        if (palette = AssetManager.instance.hatMeshes[hat].materials.colourPalette1) hatColour1 = palette.RandomColourIndex();
        if (palette = AssetManager.instance.hatMeshes[hat].materials.colourPalette2) hatColour2 = palette.RandomColourIndex();
        skinColour = AssetManager.instance.skinColours.weightMap.GetRandomIndex();

        ApplyAll();
    }

    private Color getSkinColor() {
        if (skinColour < AssetManager.instance.skinColours.colours.Length) return AssetManager.instance.skinColours.colours[skinColour].colour;
        return Color.white;
    }

    private void updateMeshMaterials(Renderer renderer, MeshMaterialSet materialSet, int colour1, int colour2) {
        List<Material> materials = new List<Material>();

        if (materialSet.materials.hasColour1) materials.Add(AssetManager.GetColouredMaterial(materialSet.materials.colourPalette1.colours[colour1].colour));
        if (materialSet.materials.hasColour2) materials.Add(AssetManager.GetColouredMaterial(materialSet.materials.colourPalette2.colours[colour2].colour));
        if (materialSet.materials.hasSkin) materials.Add(AssetManager.GetColouredMaterial(getSkinColor()));
        foreach (Material material in materialSet.materials.additionalMaterials) {
            materials.Add(material);
        }

        renderer.sharedMaterials = materials.ToArray();
    }

    private void setMeshMaterialColour(Renderer renderer, int materialIndex, Color color) {
        // Get a material from color dictionary or create one if doesn't exist
        Material newMaterial = AssetManager.GetColouredMaterial(color);

        // Copy of the mesh's material array
        Material[] materials = renderer.sharedMaterials;

        // If the array is too small, resize it
        if (materialIndex >= renderer.sharedMaterials.Length) {
            System.Array.Resize(ref materials, materialIndex + 1);
        }

        // Assign the new materials
        materials[materialIndex] = newMaterial;
        renderer.sharedMaterials = materials;

        return;
    }

    private void updateMeshRenderer(SkinnedMeshRenderer renderer, Mesh newMesh) {
        renderer.sharedMesh = newMesh;

        if (renderer.sharedMesh != null) {
            Bounds bounds = renderer.localBounds;
            bounds.center = renderer.sharedMesh.bounds.center;
            bounds.extents = renderer.sharedMesh.bounds.extents;
            renderer.localBounds = bounds;
        }
    }

    // For the hat mesh which uses a MeshFilter + MeshRenderer instead of SkinnedMeshRenderer
    private void updateMeshFilter(MeshFilter filter, Mesh newMesh) {
        filter.sharedMesh = newMesh;
    }

    public void SkipAnimation(string stateName) {
        if (modelAnim.GetCurrentAnimatorStateInfo(0).IsName(stateName)) {
            modelAnim.Play(0, 0, 1);
        }
    }

    public bool IsPlayingAnimation(string name) {
        return modelAnim.GetCurrentAnimatorStateInfo(0).IsName(name);
    }

    public float CurrentAnimationTime() {
        return modelAnim.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }
}
