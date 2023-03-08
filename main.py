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

def ColorMaskOnce(original, current, mask, color):
    # Converting the mask to a viable format;
    maskImage = ImageChops.multiply(original, CreateMask(mask))

    rg, gg, bg = color  # Target RGB;

    # Coloring the image;
    colorOverlay = Image.new("RGBA", maskImage.size, (int(rg), int(gg), int(bg)))
    recoloredMask = ImageChops.multiply(maskImage, colorOverlay)

    # Editing the final image;
    recoloredMask = Brightness(recoloredMask, 1.2)

    # Pasting it into the current image;
    current = Brightness(current, 1.2)
    current.filter(ImageFilter.BoxBlur(1))
    current.paste(recoloredMask, (0, 0), recoloredMask)
    current = current.filter(ImageFilter.SMOOTH)
    return current

# ColorMaskMultiple(currentTexture, (image, (0, 0, 0)), (image, (0, 0, 0)), ...);
def ColorMaskMultiple(original, current, *args):
    maskedResult = Image.new("RGBA", original.size, (0, 0, 0, 0))
    width, height = original.size

    for clippingMask, color in args:
        # Multiply blend mode with the original sprite for color mapping;
        clippingMask = ImageChops.multiply(original, CreateMask(clippingMask))
        rg, gg, bg = color  # Target RGB;

        # Coloring the image;
        colorOverlay = Image.new("RGBA", (width, height), (int(rg), int(gg), int(bg)))
        recoloredMask = ImageChops.multiply(clippingMask, colorOverlay)
        maskedResult.paste(recoloredMask, (0, 0), recoloredMask)

    #Pasting the image into the currentTexture
    current.paste(maskedResult, (0, 0), maskedResult)
    current = Brightness(current, 1.2)
    current = current.filter(ImageFilter.SMOOTH)
    return current


def Saturate(image, sat):
    enhancer = ImageEnhance.Color(image)
    image = enhancer.enhance(sat)
    return image

def Brightness(image, gamma):
    enhancer = ImageEnhance.Brightness(image)
    image = enhancer.enhance(gamma)
    return image

def ColorBlend(image, newColor):
    # Get the luminance threshold out of an input color;
    def GetLuminance(color):
        return floor((0.2126 * color[0] + 0.7152 * color[1] + 0.0722 * color[2]))

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

    for x in range(width):
        for y in range(height):
            # Image's RGB values
            r, g, b, a = image.getpixel((x, y))

            match blendMode:
                case "Multiply":
                    # Create a solid color image with the input color
                    rg, gg, bg = newColor

                    colorMask = Image.new("RGBA", image.size, (int(rg), (gg), (bg)))

                    # Multiply the input image with the color image
                    result = ImageChops.multiply(image, colorMask)
                    result = Brightness(result, gamma * 1.2)
                    result = Saturate(result, saturation)

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
newColor = (191, 59, 135)

for png in imageFiles:
    try:
        originalTexture = Image.open(png) # What's currently being processed;
        currentTexture = originalTexture.copy()
        name = originalTexture.filename
        currentTexture = ColorBlend(currentTexture, newColor)

        #Adding the masks;
        currentTexture = ColorMaskOnce(originalTexture, currentTexture, Image.open("Masks/mask.png"), (255, 255, 255))

        currentTexture = ColorMaskMultiple(originalTexture, currentTexture,
                         (Image.open("Masks/mask.png"), (230, 196, 211)),
                         (Image.open("Masks/mask01.png"), (255, 255, 255)),
                         (Image.open("Masks/mask02.png"), (255, 0, 0)),
                         (Image.open("Masks/mask03.png"), (255, 242, 112)))
        SaveImage(currentTexture, name)
    except(FileNotFoundError, IOError):
        print(f"Error modifying image {currentTexture}")

print("--- %s seconds ---" % (time.time() - start_time))