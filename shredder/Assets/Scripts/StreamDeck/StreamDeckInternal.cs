using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using HidSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Unity.Mathematics;
using UnityEngine;
using Color = UnityEngine.Color;

/// <summary>
/// Static functions used for StreamDecks. Mainly for image converting and encoding.
/// </summary>
public static class StreamDeckInternal
{
  // --- CONST --- //
  // Internal IDs used to locate the correct HID Devices
  public const int ElgatoSystemsGmbH = 0x0fd9;
  public const int StreamDeckXL      = 0x006c;

  // --- HID DEVICES --- //
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<HidDevice> GetStreamDeckHidDevices() => 
    DeviceList.Local.GetHidDevices(ElgatoSystemsGmbH, StreamDeckXL);

  // --- JPEG CONVERTING --- //
  private static readonly JpegEncoder JpegEncoder = new JpegEncoder() { Quality = 100 };
  
  // NOTE(WSWhitehouse): DO NOT USE AT RUN TIME! Allocates and disposes of a memory stream of the correct capacity.
  // During run time use ConvertButtonImageToJpeg() and allocate the memory stream yourself where it wont be created
  // or destroyed repeatedly. 
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte[] ConvertButtonImageToJpegAlloc(Image<Bgr24> img)
  {
    using MemoryStream memoryStream = new MemoryStream(StreamDeck.ButtonAreaSize * 3);
    return ConvertButtonImageToJpeg(img, memoryStream);
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte[] ConvertButtonImageToJpeg(Image<Bgr24> img, MemoryStream memoryStream)
  {
    img.SaveAsJpeg(memoryStream, JpegEncoder);
    return memoryStream.ToArray();
  }

  // --- IMAGE CONVERTING --- //

  /// <summary>
  /// Converts a Texture2D into an image that fits on a Stream Deck Button
  /// </summary>
  public static Image<Bgr24> Texture2DToButtonImage(Texture2D texture, Color colourMultiplier)
  {
    // Create image from texture and resize it to fit on a Stream Deck button
    Image<Bgr24> image = Texture2DToImage(texture, colourMultiplier);
    ResizeImage(ref image, StreamDeck.ButtonSize, StreamDeck.ButtonSize);
    return image;
  }
  
  /// <summary>
  /// Converts a Texture2D into a image that fits on the entire Stream Deck.
  /// However, the image still needs to be split up into individual button images,
  /// use <see cref="ImageToButtonImages"/> to convert this image.
  /// </summary>
  public static Image<Bgr24> Texture2DToDeckImage(Texture2D texture, Color colourMultiplier)
  {
    // Create image from texture and resize it to fit on a Stream Deck
    Image<Bgr24> image = Texture2DToImage(texture, colourMultiplier);
    ResizeImage(ref image, StreamDeck.DeckWidth, StreamDeck.DeckHeight);
    return image;
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Image<Bgr24> Texture2DToImage(Texture2D texture, Color colourMultiplier)
  {
    if (!TextureUtil.IsTextureFormatSupported(texture)) return null;
    TextureUtil.TextureInfo textureInfo = TextureUtil.GetTexture2DInfo(texture);
    
    Image<Bgr24> image = new Image<Bgr24>(texture.width, texture.height);
    byte[] textureData = texture.GetRawTextureData();
      
    for (int y = 0; y < texture.height; y++)
    {
      for (int x = 0; x < texture.width; x++)
      {
        int pixelIndex = (y * texture.width + x) * textureInfo.stride;
        image[x, y] = new Bgr24((byte)(textureData[pixelIndex + textureInfo.rOffset] * colourMultiplier.r),
                                (byte)(textureData[pixelIndex + textureInfo.gOffset] * colourMultiplier.g),
                                (byte)(textureData[pixelIndex + textureInfo.bOffset] * colourMultiplier.b));
      }
    }
    
    return image;
  }
  
  /// <summary>
  /// Converts a colour into an image that fits on a Stream Deck Button
  /// </summary>
  public static Image<Bgr24> ColourToButtonImage(Color colour, Color colourMultiplier)
  {
    Color32 color32    = (Color32)(colour * colourMultiplier);
    Image<Bgr24> image = new Image<Bgr24>(StreamDeck.ButtonSize, StreamDeck.ButtonSize);

    for (int y = 0; y < StreamDeck.ButtonSize; y++)
    {
      for (int x = 0; x < StreamDeck.ButtonSize; x++)
      {
        image[x, y] = new Bgr24(color32.r, color32.g, color32.b);
      }
    }
    
    return image;
  }

  /// <summary>
  /// Takes in an image and splits it into button images using the button count. Please ensure the image is
  /// the correct size corresponding to the button count. See <see cref="StreamDeck.FullButtonSize"/> for
  /// button sizes.
  /// </summary>
  public static List<Image<Bgr24>> ImageToButtonImages(Image<Bgr24> image, int2 buttonCount)
  {
    int totalButtons = buttonCount.x + buttonCount.y;
    int imageWidth   = StreamDeck.FullButtonSize * buttonCount.x - StreamDeck.ButtonGapSize;
    int imageHeight  = StreamDeck.FullButtonSize * buttonCount.y - StreamDeck.ButtonGapSize;

    // Check Image is of the expected width or height - error if not
    if (image.Width != imageWidth || image.Height != imageHeight)
    {
      Log.Error($"Image is not of the expected size! Please resize image first before calling {nameof(ImageToButtonImages)}");
      Log.Error($"image size: ({image.Width}, {image.Height})   expected size: ({imageWidth}, {imageHeight})");
      return null;
    }

    // Create list of images for all the buttons
    List<Image<Bgr24>> buttonImages = new List<Image<Bgr24>>(totalButtons);
    
    // Loop through all buttons
    for (int y = 0; y < buttonCount.y; y++)
    {
      for (int x = 0; x < buttonCount.x; x++)
      {
        // Create new button image
        Image<Bgr24> buttonImage = new Image<Bgr24>(StreamDeck.ButtonSize, StreamDeck.ButtonSize);

        // Calculate x and y index in image
        int imageIndexX = (buttonCount.x - 1 - x) * StreamDeck.FullButtonSize;
        int imageIndexY = (buttonCount.y - 1 - y) * StreamDeck.FullButtonSize;

        // Copy pixels from image to button image
        for (int buttonY = 0; buttonY < StreamDeck.ButtonSize; buttonY++)
        {
          for (int buttonX = 0; buttonX < StreamDeck.ButtonSize; buttonX++)
          {
            // NOTE(WSWhitehouse): The deck x index is flipped
            int xIndex = imageWidth - 1 - (imageIndexX + buttonX);
            int yIndex = imageIndexY + buttonY;

            buttonImage[buttonX, buttonY] = image[xIndex, yIndex];
          }
        }

        buttonImages.Add(buttonImage);
      }
    }

    return buttonImages;
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void ResizeImage(ref Image<Bgr24> image, int width, int height) => image.Mutate(x => x.Resize(width, height));
}