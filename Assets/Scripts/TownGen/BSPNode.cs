using UnityEngine;

// A BSP cuts a node at a random point, then cuts the two child nodes, then cuts those etc
// Keep cutting until a minimum node size is exceeded

public class BSPNode 
{
    public int x, y, width, height;
    public BSPNode leftChild, rightChild;
    public Vector2 centre;

    private int minNodeSize;

    public BSPNode(int x, int y, int width, int height, int minNodeSize) 
    {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.minNodeSize = minNodeSize;
        centre = new Vector2(x + width / 2f, y + height / 2f);
    }

    // Returns false if too small to split anymore
    public bool Split() 
    {
        // Already split
        if(leftChild != null || rightChild != null) return false;

        // Get direction of split, if width is at least 25% bigger than height then split height then split vertically (and vice verse)
        bool splitH = Random.value > 0.5f;
        if(width > height && width / height >= 1.25f) 
        {
            splitH = false;
        } 
        else if(height > width && height / width >= 1.25f) 
        {
            splitH = true;
        }

        // Determine maximum node size
        int max = (splitH ? height : width) - minNodeSize;
        if (max <= minNodeSize) return false;

        // Distance at which to make the split
        int split = Random.Range(minNodeSize, max);

        if(splitH) 
        {
            leftChild = new BSPNode(x, y, width, split, minNodeSize);
            rightChild = new BSPNode(x, y + split, width, height - split, minNodeSize);
        } else 
        {
            leftChild = new BSPNode(x, y, split, height, minNodeSize);
            rightChild = new BSPNode(x + split, y, width - split, height, minNodeSize);
        }

        // Split successfull
        return true;
    }
}