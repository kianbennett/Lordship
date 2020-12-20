using UnityEngine;

/* Class name for [Property] is PropertyAttribute */

public class ReadOnlyAttribute : PropertyAttribute { }
public class PaletteColourAttribute : PropertyAttribute {
    public PaletteColour[] Palette;
    public PaletteColourAttribute(PaletteColour[] palette) {
        Palette = palette;
    }
}
public class UniqueIdentifierAttribute : PropertyAttribute { }
public class SceneAttribute : PropertyAttribute { }