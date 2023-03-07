import colorsys
import os
import time
from math import floor
import numpy as np
from PIL import Image, ImageEnhance, ImageChops, ImageOps, ImageFilter

start_time = time.time()

# Creates the mask used to combine the texture's colors;
def CreateMask(image):
    grayscale = image.convert("L")

    # Set non-transparent pixels to white in the mask and adding transparency;
    mask = Image.new("RGBA", image.size, (255, 255, 255, 255))
    mask.putalpha(Image.eval(grayscale, lambda x: 0 if x == 0 else 255))
    return mask

def ColorMask(image, parent, color):
    mask = CreateMask(currentTexture)
    image = CreateMask(image)
    image = ImageChops.multiply(originalTexture, image)

    # Create a solid color image with the input color
    data = np.array(image)
    r, g, b, a = data.T
    whiteAreas = (r > 0) & (b > 0) & (g > 0)
    data[..., :- 1][whiteAreas.T] = color
    resultMask = Image.fromarray(data)

    newImage = parent.copy()
    newImage.paste(mask, (0, 0), resultMask)
    newImage = newImage.filter(ImageFilter.SMOOTH_MORE)
    return newImage

def ColorBlend(image, newColor):
    # Get the luminance threshold out of an input color;
    def GetLuminance(color):
        return floor((0.2126 * color[0] + 0.7152 * color[1] + 0.0722 * color[2]))

    # Setting up the multiply blend mode's color for the image;
    def Multiply(input, color, gamma):
        # A simple way to handle the pixel multiplication operations;
        def MultiplyOperation(pixelColor, color):
            return (pixelColor / 255) * (color / 255)

        r = MultiplyOperation(input[0], color[0]) * gamma
        g = MultiplyOperation(input[1], color[1]) * gamma
        b = MultiplyOperation(input[2], color[2]) * gamma
        return int(r), int(g), int(b)

    # Processes the final color of the pixel to insert;
    def ProcessColor(r, g, b, a):
        return (int(r * 255), int(g * 255), int(b * 255), a)

    # The luminance value helps us determine which blend mode is the best for a specific color;
    luminance = GetLuminance(newColor)

    if luminance < 100: blendMode = "Multiply"
    else: blendMode = "Color"

    # Image parameters;
    gamma = 1
    saturation = 1

    # Creating a blank image to inject the colors if needed;
    width, height = image.size
    result = Image.new("RGBA", (width, height), (255, 255, 255, 0))

    def Saturate(image, sat):
        converter = ImageEnhance.Color(image)
        converter.enhance(sat)
        return image

    for x in range(width):
        for y in range(height):
            # Image's RGB values
            r, g, b, a = image.getpixel((x, y))

            match blendMode:
                case "Multiply":
                    # Create a solid color image with the input color
                    rg = int(newColor[0] * gamma)
                    gg = int(newColor[1] * gamma)
                    bg = int(newColor[2] * gamma)

                    colorMask = Image.new("RGBA", image.size, (rg, gg, bg))

                    # Multiply the input image with the color image
                    result = ImageChops.multiply(image, colorMask)
                    Saturate(result, saturation)
                    return result

                case "Color":
                    # Blended RGBA with luminance;
                    luminance = GetLuminance(image.getpixel((x, y))) * gamma

                    # RGBA HSV hue; Color Blend Mode;
                    hsvBlend = colorsys.rgb_to_hsv(newColor[0] / 255, newColor[1] / 255, newColor[2] / 255);
                    r, g, b = colorsys.hsv_to_rgb(hsvBlend[0], hsvBlend[1], luminance / 255)

            color = ProcessColor(r, g, b, a)
            result.putpixel((x, y), color)
    return Saturate(result, saturation)

def SaveImage(image, route):
    filename = route;
    newFilepath = os.path.join("folder_name", f"{filename}")
    image.save(newFilepath)


imageFiles = [f for f in os.listdir() if f.endswith(".png") and f != "main.py"]

# Constants;
currentTexture = None
originalTexture = None

# Image parameters;
originalColor = (191, 191, 191)
newColor = (89, 96, 120)
colorRange = 100;

for png in imageFiles:
    try:
        originalTexture = Image.open(png) # What's currently being processed;
        currentTexture = originalTexture.copy()
        name = originalTexture.filename
        currentTexture = ColorBlend(currentTexture, newColor)

        #Adding the masks;
        loadedMask = ColorMask(Image.open("Masks/mask.png"), currentTexture, (255, 255, 255))
        currentTexture.paste(loadedMask)

        SaveImage(currentTexture, name)
    except(FileNotFoundError, IOError):
        print(f"Error modifying image {currentTexture}")

print("--- %s seconds ---" % (time.time() - start_time))