using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using HidSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Unity.Mathematics;

// Defining Input types
using DeckInput   = ConsumableAction<int>;
using ButtonInput = ConsumableAction;

public class StreamDeck : IDisposable
{
  // --- CONST --- //
  // Buttons
  public const int ButtonXCount           = 8;
  public const int ButtonYCount           = 4;
  public const int ButtonTotalCount       = ButtonXCount * ButtonYCount;
  public static readonly int2 ButtonCount = new int2(ButtonXCount, ButtonYCount);
  
  public const int ButtonGapSize  = 38;
  public const int ButtonSize     = 96;
  public const int ButtonAreaSize = ButtonSize * ButtonSize;
  public const int FullButtonSize = ButtonSize + ButtonGapSize;
  
  // Deck
  public const int DeckWidth           = FullButtonSize * ButtonXCount - ButtonGapSize;
  public const int DeckHeight          = FullButtonSize * ButtonYCount - ButtonGapSize;
  public const int DeckAreaSize        = DeckWidth * DeckHeight;
  public static readonly int2 DeckSize = new int2(DeckWidth, DeckHeight);

  // --- INTERNAL STREAM DECK DATA --- //
  private string DevicePath { get; }
  
  private HidStream Stream { get; set; }
  private bool IsConnected => Stream != null;

  // NOTE(WSWhitehouse): These are hardcoded versions of the lengths received by the device
  // They should always be identical if running on Windows and using StreamDeckXLs
  public const int ReadBufferLength    = 1024;
  public const int WriteBufferLength   = 1024;
  public const int FeatureBufferLength = 32;
  
  private byte[] ReadBuffer    { get; } = new byte[ReadBufferLength];
  private byte[] FeatureBuffer { get; } = new byte[FeatureBufferLength];

  // This image is generated in the static constructor. Can be used to set the 
  // buttons to black on clear without the need of a ScriptableObject.
  private static readonly List<StreamDeckPacket> BlackButtonImage;
  
  // --- CONNECTION EVENTS --- //
  public Action OnDisconnect;
  public Action OnConnect;

  // --- CONNECTION STATE --- //
  private bool alreadySignaledDisconnect = false;

  // --- INPUT --- //
  // NOTE(WSWhitehouse): Initialising button changes to the maximum amount of button changes reasonable 
  // possible in a frame. This is to reduce reallocating memory for the lists.
  private const int ButtonUpdatesPerFrameCapacity = ButtonTotalCount * 2;
  
  // NOTE(WSWhitehouse): _buttonChanges list is used on another thread and must be copied to the 
  // _buttonChangesCopy list before being used safely. int = button index, bool = is button down
  private readonly List<(int, bool)> _buttonChanges     = new List<(int, bool)>(ButtonUpdatesPerFrameCapacity); 
  private readonly List<(int, bool)> _buttonChangesCopy = new List<(int, bool)>(ButtonUpdatesPerFrameCapacity);
  
  // NOTE(WSWhitehouse): This array holds the current button states and the previous frames
  // state, use a 1D button index to get the current state of the button. If true the button
  // is currently being held down. Use the GetButtonState and GetPrevButtonState functions.
  private readonly bool[] _currentInputState = new bool[ButtonTotalCount];
  private readonly bool[] _prevInputState    = new bool[ButtonTotalCount];

  // NOTE(WSWhitehouse): Used to process button changes on another thread
  private readonly byte[] _buttonStates = new byte[ButtonTotalCount];
  
  // Deck Input
  private const int InitialInputActionCapacity = 100; 
  public DeckInput OnDeckInputPerformed = new DeckInput(InitialInputActionCapacity);
  public DeckInput OnDeckInputCancelled = new DeckInput(InitialInputActionCapacity);

  // Button Input
  public readonly Dictionary<int, ButtonInput> OnButtonPerformed = new Dictionary<int, ButtonInput>();
  public readonly Dictionary<int, ButtonInput> OnButtonCancelled = new Dictionary<int, ButtonInput>();
  
  // NOTE(WSWhitehouse): This int2 holds which input button event is being invoked, so it can be
  // found and consumed when calling the ConsumeInput function. The X value holds the event type,
  // the Y value holds the button index. The constant values below represent what values the X
  // value can hold for an Invalid, Performed or Cancelled event. We also set the Y to invalid when 
  // not in use to ensure if something breaks it will cause an error.
  private const int BUTTON_EVENT_INVALID   = -1;
  private const int BUTTON_EVENT_PERFORMED = 0;
  private const int BUTTON_EVENT_CANCELLED = 1;
  private int2 _currentButtonInput = new int2(BUTTON_EVENT_INVALID, BUTTON_EVENT_INVALID);

  // NOTE(WSWhitehouse): This boolean is true any frame that a button is pressed on the Stream Deck
  public bool WasButtonPressedThisFrame { get; private set; } = false;
  
  // --- BRIGHTNESS --- //
  // NOTE(WSWhitehouse): Brightness is a percentage (between 0 - 100). More info taken from StreamDeckSharp docs:
  // The brightness on the device is controlled with PWM (https://en.wikipedia.org/wiki/Pulse-width_modulation).
  // This results in a non-linear correlation between set percentage and perceived brightness.
  // In a nutshell: changing from 10 - 30 results in a bigger change than 80 - 100 (barely visible change)
  // This effect should be compensated outside this library
  
  private readonly byte[] _brightnessBuffer = new byte[]
  {
    0x03, 0x08, 0x64, 0x23, 0xB8, 0x01, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0xA5, 0x49, 0xCD, 0x02, 0xFE, 0x7F, 0x00, 0x00,
  };
  
  private int _brightness = 100;
  public int Brightness
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => _brightness;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set
    {
      _brightness = maths.Clamp(value, 0, 100);
      _brightnessBuffer[2] = (byte)_brightness;
      SetFeature(_brightnessBuffer);
    }
  }

  static StreamDeck()
  {
    // Create a Black Button Image that can be used in Clear functions without allocating new memory 
    Image<Bgr24> image     = new Image<Bgr24>(ButtonSize, ButtonSize);
    byte[] jpegTextureData = StreamDeckInternal.ConvertButtonImageToJpegAlloc(image);
    BlackButtonImage       = StreamDeckPacket.GenerateData(jpegTextureData);
  }

  public StreamDeck(HidDevice device)
  {
    DevicePath = device.DevicePath;
    StartConnection(device);
    
    for (int buttonIndex = 0; buttonIndex < ButtonTotalCount; buttonIndex++)
    {
      OnButtonPerformed.Add(buttonIndex, new ButtonInput(InitialInputActionCapacity));
      OnButtonCancelled.Add(buttonIndex, new ButtonInput(InitialInputActionCapacity));
    }
  }

  public void Dispose() 
  {
    if (IsConnected) 
    {
      ClearDeck();
      DisposeConnection();
    }
    
    GC.SuppressFinalize(this);
  }

  public void RefreshConnection() 
  {
    HidDevice device = StreamDeckInternal.GetStreamDeckHidDevices().FirstOrDefault(dev => dev.DevicePath == DevicePath);
    bool deviceFound = device != null;
    
    if (deviceFound && IsConnected) return;
    
    if (!deviceFound) 
    {
      DisposeConnection();

      // NOTE(Zack): this guard is so that we only signal that this stream deck has been disconnected once
      if (alreadySignaledDisconnect) return;
      alreadySignaledDisconnect = true;
      OnDisconnect?.Invoke();
    }
    else
    {
      alreadySignaledDisconnect = false;
      StartConnection(device);
      OnConnect?.Invoke();
    }
  }

  private void StartConnection(HidDevice device)
  {
    // Open Device
    bool success = device.TryOpen(out HidStream stream);

    if (!success)
    {
      Log.Error($"Unable to open Stream Deck Device (Device Path: {DevicePath})");
      return;
    }
    
    Log.Print($"Started Connection with Stream Deck Device (Device Path: {DevicePath}");
     
    // DEVICE STREAM
    Stream             = stream;
    Stream.ReadTimeout = Timeout.Infinite;

    // INPUT
    ReadButtonStates(stream);
    
    // BRIGHTNESS
    Brightness = 100;
  }

  private void DisposeConnection()
  {
    if (Stream == null) return;
    
    Stream.Dispose();
    Stream = null;
  }

  private void ReadButtonStates(HidStream stream) 
  {
    void ProcessButtonStateChange(IAsyncResult asyncResult) 
    {
      // Device has disconnected
      if (!IsConnected) return;

      try
      {
        HidStream stream = (HidStream)asyncResult.AsyncState;
        stream.EndRead(asyncResult);
      }
      catch (Exception e)
      when  (e is TimeoutException or IOException or ObjectDisposedException)
      {
        return; 
      }

      const int ButtonOffset = 4;
      lock (_buttonChanges)
      {
        for (int i = 0; i < ButtonTotalCount; i++)
        {
          int statePos = i + ButtonOffset;

          // This button has not changed
          if (_buttonStates[i] == ReadBuffer[statePos]) continue;
         
          _buttonChanges.Add((i, ReadBuffer[statePos] != 0));
          _buttonStates[i] = ReadBuffer[statePos];
        }
      }

      // Check for key changes again
      ReadButtonStates(stream);
    }
    
    stream.BeginRead(ReadBuffer, 0, ReadBufferLength, ProcessButtonStateChange, stream);
  }
  
  public void SetFeature(byte[] featureData)
  {
    int minLength = maths.Min(FeatureBufferLength, featureData.Length);
    Array.Copy(featureData, 0, FeatureBuffer, 0, minLength);
    Stream.SetFeature(featureData);
  }

  public void WriteToStream(byte[] imageData)
  {
    if (!IsConnected) return;
    Stream.Write(imageData);
  }

  public void UpdateInput()
  {
    WasButtonPressedThisFrame = false; // Reset on new frame
    
    // Copy current input state to prev
    Array.Copy(_currentInputState, _prevInputState, ButtonTotalCount);

    // Get new button changes
    _buttonChangesCopy.Clear();
    lock (_buttonChanges)
    {
      // Copy _buttonChanges to _buttonChangesCopy
      foreach ((int, bool) change in _buttonChanges)
      {
        _buttonChangesCopy.Add(change);
      }

      _buttonChanges.Clear();
    }
    
    // Go through all button changes
    foreach ((int buttonIndex, bool isButtonDown) in _buttonChangesCopy)
    {
      // Update current input state
      _currentInputState[buttonIndex] = isButtonDown;
      
      // Invoke events
      if (isButtonDown)
      {
        // NOTE(WSWhitehouse): Set this property to true if a button has been pressed
        // this frame. It doesn't matter how many buttons are pressed, as long as 
        // there is one button down this will be true this frame.
        WasButtonPressedThisFrame = true;
    
        // Invoke Performed Input Events 
        OnDeckInputPerformed.Invoke(buttonIndex);
        _currentButtonInput = new int2(BUTTON_EVENT_PERFORMED, buttonIndex);
        OnButtonPerformed[buttonIndex].Invoke();
      }
      else
      {
        // Invoke Cancelled Input Events 
        OnDeckInputCancelled.Invoke(buttonIndex);
        _currentButtonInput = new int2(BUTTON_EVENT_CANCELLED, buttonIndex);
        OnButtonCancelled[buttonIndex].Invoke();
      }
      
      _currentButtonInput = new int2(BUTTON_EVENT_INVALID, BUTTON_EVENT_INVALID);
    }
  }

  public void ConsumeInput()
  {
    OnDeckInputPerformed.Consume();
    OnDeckInputCancelled.Consume();

    switch (_currentButtonInput.x)
    {
      case BUTTON_EVENT_PERFORMED: OnButtonPerformed[_currentButtonInput.y].Consume(); break;
      case BUTTON_EVENT_CANCELLED: OnButtonCancelled[_currentButtonInput.y].Consume(); break;
    }
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool GetButtonState(int buttonIndex)
  {
    return _currentInputState[buttonIndex];
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool GetButtonState(ButtonIndices indices)
  {
    for (int i = 0; i < indices.Count; i++)
    {
      if (!_currentInputState[indices[i]]) continue;
      
      return true;
    }
    
    return false;
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool GetPrevButtonState(int buttonIndex)
  {
    return _prevInputState[buttonIndex];
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool GetPrevButtonState(ButtonIndices indices)
  {
    for (int i = 0; i < indices.Count; i++)
    {
      if (!_prevInputState[indices[i]]) continue;
      
      return true;
    }
    
    return false;
  }


  public void SetButtonColour(int buttonIndex, StreamDeckColour colour)
  {
    #if UNITY_EDITOR
    if (colour.buttonPackets.Count == 0) Log.Warning($"StreamDeckColour ({colour.name}) has 0 button packets! Please ensure you have generated packet data");
    #endif
    
    foreach (StreamDeckPacket packet in colour.buttonPackets)
    {
      packet.SetButtonIndex((byte)buttonIndex);
      WriteToStream(packet.data);
    }
  }

  public void SetDeckColour(StreamDeckColour colour)
  {
    #if UNITY_EDITOR
    if (colour.deckPackets.Count == 0) Log.Warning($"StreamDeckColour ({colour.name}) has 0 deck packets! Please ensure you have generated packet data");
    #endif
    
    foreach (StreamDeckPacket packet in colour.deckPackets)
    {
      WriteToStream(packet.data);
    }
  }

  public void SetButtonImage(int buttonIndex, ButtonTexture texture)
  {
    #if UNITY_EDITOR
    if (texture.packets.Count == 0) Log.Warning($"ButtonTexture ({texture.name}) has 0 packets! Please ensure you have generated packet data");
    #endif
    
    foreach (StreamDeckPacket packet in texture.packets)
    {
      packet.SetButtonIndex((byte)buttonIndex);
      WriteToStream(packet.data);
    }
  }
  
  public void SetDeckImage(DeckTexture texture)
  {
    #if UNITY_EDITOR
    if (texture.packets.Count == 0) Log.Warning($"DeckTexture ({texture.name}) has 0 packets! Please ensure you have generated packet data");
    #endif
    
    foreach (StreamDeckPacket packet in texture.packets)
    {
      WriteToStream(packet.data);
    }
  }

  public void SetMultiButtonImage(int startButtonIndex, MultiButtonTexture texture)
  {
    #if UNITY_EDITOR
    if (texture.packets.Count == 0) Log.Warning($"MultiButtonTexture ({texture.name}) has 0 packets! Please ensure you have generated packet data");
    #endif
    
    int2 startButtonIndex2D = StreamDeckManager.Index1DTo2D(startButtonIndex);
    
    for (int y = 0; y < texture.buttonCount.y; y++)
    {
      for (int x = 0; x < texture.buttonCount.x; x++)
      {
        int texIndex = ArrayUtil.Index2DTo1D(x, y, texture.buttonCount.x);
        List<StreamDeckPacket> packets = texture.packets[texIndex].packets;
        
        #if UNITY_EDITOR
        if (packets.Count == 0) Log.Warning($"MultiButtonTexture ({texture.name}) has 0 packets! Please ensure you have generated packet data");
        #endif
        
        byte buttonIndex = (byte)StreamDeckManager.Index2DTo1D(x + startButtonIndex2D.x, y + startButtonIndex2D.y);
        foreach (StreamDeckPacket packet in packets)
        {
          packet.SetButtonIndex(buttonIndex);
          WriteToStream(packet.data);
        }
      }
    }
  }

  public void ClearButton(int buttonIndex)
  {
    foreach (StreamDeckPacket packet in BlackButtonImage)
    {
      packet.SetButtonIndex((byte)buttonIndex);
      WriteToStream(packet.data);
    }
  }

  public void ClearDeck()
  {
    for (int buttonIndex = 0; buttonIndex < ButtonTotalCount; buttonIndex++)
    {
      foreach (StreamDeckPacket packet in BlackButtonImage)
      {
        packet.SetButtonIndex((byte)buttonIndex);
        WriteToStream(packet.data);
      }
    }
  }
}
