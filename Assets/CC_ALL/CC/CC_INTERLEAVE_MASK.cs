using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* (C) 09/28/2024 Jason Leigh - Laboratory for Advanced Visualization & Applications

This script performs the interleaving of the mask. Basically it creates alternating rows of opaque and transparent lines
to form the mask.
The flip parameter is used to flip whether an opaque or transparent line
comes first.

If your left eye image uses an unflipped mask then your right eye image needs
to use a flipped mask.
*/

public class CC_INTERLEAVE_MASK : MonoBehaviour
{
    public bool flip=true;
    private Texture2D screenTexture;
    // Start is called before the first frame update
    private int currentWidth, currentHeight;
    void Start()
    {
 // Get the screen width and height
        int screenWidth = Screen.width;
        int screenHeight = Screen.height;
        int currentWidth = screenWidth;
        int currentHeight = screenHeight;
        screenTexture = CreateStripedTexture(screenWidth,screenHeight, flip);
  
        // Assign the newly created texture to the RawImage
        GetComponent<RawImage>().texture = screenTexture;       
    }

    public Texture2D CreateStripedTexture(int width, int height, bool flip){
        int lineThickness = 1;
                // Create a new Texture2D
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // Loop through each pixel in the texture
        for (int y = 0; y < height; y++)
        {
            // Determine whether this row should be opaque or transparent
            bool isOpaqueRow = (y / lineThickness) % 2 == 0;

            for (int x = 0; x < width; x++)
            {
                // Set the color for this pixel (opaque white or fully transparent)
                if (isOpaqueRow)
                {
                    if (flip) texture.SetPixel(x, y, new Color(1,1,1,1));  // Opaque white line
                    else 
                    texture.SetPixel(x, y, new Color(0, 0, 0, 0));  // Transparent line
                }
                else
                {
                     if (!flip) texture.SetPixel(x, y, new Color(1,1,1,1));  // Opaque white line
                    else 
                    texture.SetPixel(x, y, new Color(0, 0, 0, 0));  // Transparent line                   
                }
            }
        }

        // Apply the changes to the texture
        texture.Apply();

        return texture;
    }

    // Update is called once per frame
    void Update()
    {
        int screenWidth = Screen.width;
        int screenHeight = Screen.height;

        // If the screen size changed you need to re-stripe the mask
        if (currentWidth == Screen.width && currentHeight == Screen.height) return;
        currentWidth = screenWidth;
        currentHeight= screenHeight;

        screenTexture = CreateStripedTexture(screenWidth,screenHeight, flip);
        GetComponent<RawImage>().texture = screenTexture;      
        
    }

    void OnDestroy()
    {
        // Cleanup the texture to avoid memory leaks
        if (screenTexture != null)
        {
            Destroy(screenTexture);
        }
    }
}
