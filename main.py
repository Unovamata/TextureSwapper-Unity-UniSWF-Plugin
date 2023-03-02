import os
import time
import numpy as np
from PIL import Image

start_time = time.time()

def modifyImageColor(imageFile, originalColor, newColor, range):
    def maxColor(v):
        return max(0, v - range)

    def minColor(v):
        return max(255, v + range)

    def isInRange(v, parameter):
        return (maxColor(r1) <= parameter) & (parameter <= minColor(r1))

    #Opening the image and converting it into a color array;
    image = Image.open(imageFile)
    data = np.array(image)
    r1, g1, b1 = originalColor #Getting the color reference;

    red, green, blue = data[:,:, 0], data[:,:, 1], data[:,:, 2]



    mask = isInRange(r1, red) & isInRange(g1, green) & isInRange(b1, blue)
    data[:, :, :3][mask] = [newColor]
    image = Image.fromarray(data)

    # Save image;
    filename = "image"
    newFilePath = os.path.join("folder_name", f"{filename}.png")
    image.save(newFilePath)

    return f"image {filename} modified"


imageFiles = [f for f in os.listdir() if f.endswith(".png") and f != "main.py"]

for png in imageFiles:
    try:
        originalColor = (191, 191, 191)
        newColor = (122, 210, 229)
        range = 100;

        modifyImageColor(png, originalColor, newColor, range)
    except(FileNotFoundError, IOError):
        print(f"Error modifying image {png}")

print("--- %s seconds ---" % (time.time() - start_time))