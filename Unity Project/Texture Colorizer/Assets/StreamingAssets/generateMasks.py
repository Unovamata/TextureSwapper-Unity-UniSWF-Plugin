import os.path
import sys
from PIL import Image

#print(sys.path)

path = r'C:\Users\Administrator\Documents\GitHub\LPSO-Revived-UniSWF-Texture-Colorizer\Unity Project\Texture Colorizer\Assets\Resources\Skeletons\Kitty\Kitty1\Ear Left.png'

def CreateMask(image, RGB, threshold, index):
    mask = Image.new("RGB", image.size, (0, 0, 0))

    for x in range(image.width):
        for y in range(image.height):
            pixel = image.getpixel((x, y))

            if all(p <= t and t - p <= threshold for p, t in zip(pixel[:3], RGB)) and pixel[3] > 0:
                a = int((pixel[3] / 255) * 255)
                mask.putpixel((x, y), (a, a, a))

    imageFilename = os.path.basename(path)
    maskSaveRoute = image.filename.replace(imageFilename, "")
    filenameWithoutType = imageFilename.replace(".png", "");

    mask.save(f"{maskSaveRoute}{filenameWithoutType} Mask [{index}]{'.png'}")
    return mask

"""image = Image.open(path)
mask = CreateMask(image, (207, 207, 207), 42, 0)
mask.show()"""
