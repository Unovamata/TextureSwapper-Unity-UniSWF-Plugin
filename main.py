import os
import time

import cv2
import numpy as np
from PIL import Image, ImageChops
from numpy import array

start_time = time.time()

#Creates the mask used to combine the texture's colors;
def CreateColoredMask(originalColor, newColor, colorRange):
    def maxColor(v):
        return max(0, v - colorRange)

    def minColor(v):
        return max(255, v + colorRange)

    def isInRange(v, parameter):
        return (maxColor(r1) <= parameter) & (parameter <= minColor(r1))

    #Opening the image and converting it into a color array;
    color = currentTexture
    data = np.array(color)
    r1, g1, b1 = originalColor #Getting the color reference;

    red, green, blue = data[:,:, 0], data[:,:, 1], data[:,:, 2]

    mask = isInRange(r1, red) & isInRange(g1, green) & isInRange(b1, blue)
    data[:, :, :3][mask] = [newColor]
    color = Image.fromarray(data)

    #Multipliying colors;
    color = ColorBlend(currentTexture, color, 0.5)
    #base = color_blend(base, color, 1, base.width, base.height)

    # Save image;
    SaveImage(color, currentTexture)

def SaveImage(image, route):
    print(route.info)
    filename = route.filename;
    newFilePath = os.path.join("folder_name", f"{filename}")

    image.save(newFilePath)
    """return f"image {filename} modified"""

def ColorBlend(image1, image2, alpha):
    width, height = image2.size

    # Create a new blank image with the same size as the input images
    result = Image.new("RGBA", (width, height), (255, 255, 255, 0))

    # Iterate over each pixel in the images
    for x in range(width):
        for y in range(height):
            # Get the RGB values of the pixels in each image
            r1, g1, b1, a1 = image1.getpixel((x, y))
            r2, g2, b2, a2 = image2.getpixel((x, y))

            # Calculate the blended RGB values for each pixel using the Color blend mode
            r = int((1 - alpha) * r1 + alpha * r2)
            g = int((1 - alpha) * g1 + alpha * g2)
            b = int((1 - alpha) * b1 + alpha * b2)

            # Set color if it's not black;
            if a1 == 0: result.putpixel((x, y), (0, 0, 0, 0))
            else : result.putpixel((x, y), (r, g, b, a1))

    # Return the resulting blended image
    return result;


imageFiles = [f for f in os.listdir() if f.endswith(".png") and f != "main.py"]

#Constants;
currentTexture = None

#Image parameters;
originalColor = (191, 191, 191)
newColor = (227, 187, 126)
colorRange = 100;

for png in imageFiles:
    try:
        currentTexture = Image.open(png) #What's currently being processed;
        CreateColoredMask(originalColor, newColor, colorRange)
    except(FileNotFoundError, IOError):
        print(f"Error modifying image {currentTexture}")

print("--- %s seconds ---" % (time.time() - start_time))