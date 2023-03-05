import os
import time

import cv2
import numpy as np
from PIL import Image, ImageChops
from numpy import array

start_time = time.time()

def ModifyImageColor(imageFile, originalColor, newColor, range):
    def maxColor(v):
        return max(0, v - range)

    def minColor(v):
        return max(255, v + range)

    def isInRange(v, parameter):
        return (maxColor(r1) <= parameter) & (parameter <= minColor(r1))

    #Opening the image and converting it into a color array;
    color = Image.open(imageFile)
    data = np.array(color)
    r1, g1, b1 = originalColor #Getting the color reference;

    red, green, blue = data[:,:, 0], data[:,:, 1], data[:,:, 2]

    mask = isInRange(r1, red) & isInRange(g1, green) & isInRange(b1, blue)
    data[:, :, :3][mask] = [newColor]
    color = Image.fromarray(data)

    #Multipliying colors;
    #base = color_blend(base, color, 1, base.width, base.height)

    # Save image;
    SaveImage(color, imageFile)

    return color

def SaveImage(image, route):
    # Save image;
    filename = route;
    newFilePath = os.path.join("folder_name", f"{filename}")
    image.save(newFilePath)

def ShowImage(image):
    image.show()

def color_blend(image1, image2, alpha, width, height):
    print("Width:", width, "Height:", height)

    # Create a new blank image with the same size as the input images
    result = Image.new("RGB", (width, height), (255, 255, 255))

    # Iterate over each pixel in the images
    for x in range(width):
        for y in range(height):
            # Get the RGB values of the pixels in each image
            r1, g1, b1 = image1.getpixel((x, y))
            r2, g2, b2 = image2.getpixel((x, y))

            # Calculate the blended RGB values for each pixel using the Color blend mode
            r = int((1 - alpha) * r1 + alpha * r2)
            g = int((1 - alpha) * g1 + alpha * g2)
            b = int((1 - alpha) * b1 + alpha * b2)

            # Set the corresponding pixel in the result image to the blended RGB values
            result.putpixel((x, y), (r, g, b))

    # Return the resulting blended image
    return result;


imageFiles = [f for f in os.listdir() if f.endswith(".png") and f != "main.py"]

#Image parameters;
originalColor = (191, 191, 191)
newColor = (227, 187, 126)
range = 100;

for png in imageFiles:
    try:


        ModifyImageColor(png, originalColor, newColor, range)
    except(FileNotFoundError, IOError):
        print(f"Error modifying image {png}")

print("--- %s seconds ---" % (time.time() - start_time))