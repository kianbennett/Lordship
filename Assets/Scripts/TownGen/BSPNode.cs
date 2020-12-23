using UnityEngine;

public class BSPNode {

    public int x, y, width, height;
    public BSPNode leftChild, rightChild;
    public Vector2 centre;

    private int minNodeSize;

    public BSPNode(int x, int y, int width, int height, int minNodeSize) {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.minNodeSize = minNodeSize;
        centre = new Vector2(x + width / 2f, y + height / 2f);
    }

    // Returns false if too small to split anymore
    public bool Split() {
        // Already split
        if(leftChild != null || rightChild != null) return false;

        // Get direction of split, if width is at least 25% bigger than height then split height then split vertically (and vice verse)
        bool splitH = Random.value > 0.5f;
        if(width > height && width / height >= 1.25f) {
            splitH = false;
        } else if(height > width && height / width >= 1.25f) {
            splitH = true;
        }

        // Determine maximum node size
        int max = (splitH ? height : width) - minNodeSize;
        if (max <= minNodeSize) return false;

        // Distance at which to make the split
        int split = Random.Range(minNodeSize, max);

        if(splitH) {
            leftChild = new BSPNode(x, y, width, split, minNodeSize);
            rightChild = new BSPNode(x, y + split, width, height - split, minNodeSize);
        } else {
            leftChild = new BSPNode(x, y, split, height, minNodeSize);
            rightChild = new BSPNode(x + split, y, width - split, height, minNodeSize);
        }

        // Split successfull
        return true;
    }

    public float GetPerimeter(float inset) {
        return width * 2 + height * 2 - inset * 8;
    }

    // Starting at bottom left, moving clockwise
    public Vector2 GetPointAlongPerimeter(float inset, float distance) {
        distance %= GetPerimeter(inset) - 4;

        Rect rect = new Rect(inset, inset, width - inset * 2 - 1, height - inset * 2 - 1);

        if(distance < rect.height) {
            return new Vector2(0, distance);
        } else if(distance < rect.height + rect.width) {
            return new Vector2(distance - rect.height, rect.height);
        } else if(distance < rect.height * 2 + rect.width) {
            return new Vector2(rect.width, rect.height - (distance - rect.width - rect.height));
        } else {
            return new Vector2(rect.width - (distance - rect.height * 2 - rect.width), 0);
        }
    }

    public Vector2 GetPointAtPercentageAlongPerimeter(float inset, float percentage) {
        return GetPointAlongPerimeter(inset, GetPerimeter(inset) * percentage);
    }

}