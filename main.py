import colorsys
import os
import time

import cv2
import numpy as np
from PIL import Image, ImageChops
from numpy import array

start_time = time.time()

# Creates the mask used to combine the texture's colors;
def CreateColoredMask(originalColor, newColor, colorRange):
    def MaxColor(v):
        return max(0, v - colorRange)

    def MinColor(v):
        return max(255, v + colorRange)

    def IsInRange(v, parameter):
        return (MaxColor(r1) <= parameter) & (parameter <= MinColor(r1))

    # Opening the image and converting it into a color array;
    color = currentTexture
    data = np.array(color)
    r1, g1, b1 = originalColor #Getting the color reference;

    red, green, blue = data[:,:, 0], data[:,:, 1], data[:,:, 2]

    mask = IsInRange(r1, red) & IsInRange(g1, green) & IsInRange(b1, blue)
    data[:, :, :3][mask] = [newColor]
    color = Image.fromarray(data)

    # Multiplying colors;
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
    colorize = 1
    gamma = 1.15

    # Blank image to inject the colors;
    result = Image.new("RGBA", (width, height), (255, 255, 255, 0))

    for x in range(width):
        for y in range(height):
            # Image's RGB values
            r1, g1, b1, a1 = image1.getpixel((x, y))
            r2, g2, b2, a2 = image2.getpixel((x, y))

            # Blended RGBA with luminance;
            luminanceMath = (0.2126 * r1 + 0.7152 * g1 + 0.0722 * b1) * gamma;
            if(luminanceMath > 210): luminance = luminanceMath
            else : luminance = 0

            # RGBA HSV hue; Color Blend Mode;
            hsvBlend = colorsys.rgb_to_hsv(r2 / 255 , g2 / 255, b2 / 255);
            r, g, b = colorsys.hsv_to_rgb(hsvBlend[0] * colorize, hsvBlend[1] * colorize, luminance / 255)

            #Manage alpha;
            a = int((1 - alpha) * a1 + alpha * a2)

            color = (int(r * 255), int(g * 255), int(b * 255), a)
            result.putpixel((x, y), color)
    return result;


imageFiles = [f for f in os.listdir() if f.endswith(".png") and f != "main.py"]

# Constants;
currentTexture = None

# Image parameters;
originalColor = (191, 191, 191)
newColor = (227, 187, 126)
colorRange = 100;

for png in imageFiles:
    try:
        currentTexture = Image.open(png) # What's currently being processed;
        CreateColoredMask(originalColor, newColor, colorRange)
    except(FileNotFoundError, IOError):
        print(f"Error modifying image {currentTexture}")

print("--- %s seconds ---" % (time.time() - start_time))