import colorsys
import os
import time
from math import floor
from PIL import Image, ImageEnhance, ImageChops

start_time = time.time()

# Creates the mask used to combine the texture's colors;
"""def CreateColoredMask(originalColor, newColor, colorRange):
    def MaxColor(v):
        return max(0, v - colorRange)

    def MinColor(v):
        return max(255, v + colorRange)

    def IsInRange(v, parameter):
        return (MaxColor(r1) <= parameter) & (parameter <= MinColor(r1))

    # Opening the image and converting it into a color array;
    color = currentTexture
    data = np.array(color)
    r1, g1, b1 = originalColor # Getting the color reference;

    red, green, blue = data[:,:, 0], data[:,:, 1], data[:,:, 2]

    mask = IsInRange(r1, red) & IsInRange(g1, green) & IsInRange(b1, blue)
    data[:, :, :3][mask] = [newColor]
    color = Image.fromarray(data)

    # Multiplying colors;
    color = ColorBlend(currentTexture, color)

    # Save image;
    SaveImage(color, currentTexture)"""

def SaveImage(image, route):
    filename = route.filename;
    newFilepath = os.path.join("folder_name", f"{filename}")
    image.save(newFilepath)

def ColorBlend(image, newColor):
    # Get the luminance threshold out of an input color;
    def GetLuminance(color):
        return floor((0.2126 * color[0] + 0.7152 * color[1] + 0.0722 * color[2]))

    # Setting up the multiply blend mode's color for the image;
    def Multiply(r, g, b, color):
        # A simple way to handle the pixel multiplication operations;
        def MultiplyOperation(pixelColor, color):
            return (pixelColor / 255) * (color / 255)

        r = MultiplyOperation(r, color[0])
        g = MultiplyOperation(g, color[1])
        b = MultiplyOperation(b, color[2])
        return int(r), int(g), int(b)

    # Processes the final color of the pixel to insert;
    def ProcessColor(r, g, b, a):
        return (int(r * 255), int(g * 255), int(b * 255), a)

    # The luminance value helps us determine which blend mode is the best for a specific color;
    luminance = GetLuminance(newColor)

    if luminance < 100: blendMode = "Multiply"
    else: blendMode = "Color"

    # Image parameters;
    gamma = 1.2
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
                    r, g, b = Multiply(r, g, b, newColor)

                    # Create a solid color image with the input color
                    colorMask = Image.new("RGBA", image.size, (r, g, b, a))

                    # Multiply the input image with the color image
                    result = ImageChops.multiply(image, colorMask)
                    return result

                case "Color":
                    # Blended RGBA with luminance;
                    luminance = GetLuminance(image.getpixel((x, y))) * gamma

                    # RGBA HSV hue; Color Blend Mode;
                    hsvBlend = colorsys.rgb_to_hsv(newColor[0] / 255, newColor[1] / 255, newColor[2] / 255);
                    r, g, b = colorsys.hsv_to_rgb(hsvBlend[0], hsvBlend[1], luminance / 255)

            color = ProcessColor(r, g, b, a)
            result.putpixel((x, y), color)

    converter = ImageEnhance.Color(result)
    converter.enhance(saturation)
    return result


imageFiles = [f for f in os.listdir() if f.endswith(".png") and f != "main.py"]

# Constants;
currentTexture = None

# Image parameters;
originalColor = (191, 191, 191)
newColor = (255, 197, 0)
colorRange = 100;

for png in imageFiles:
    try:
        currentTexture = Image.open(png) # What's currently being processed;
        ColorBlend(currentTexture, newColor)
    except(FileNotFoundError, IOError):
        print(f"Error modifying image {currentTexture}")

print("--- %s seconds ---" % (time.time() - start_time))