using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[Serializable]
public class StreamDeckPacket
{
  public StreamDeckPacket()            { this.data = null; }
  public StreamDeckPacket(byte[] data) { this.data = data; }
  
  public byte[] data;
    
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void SetButtonIndex(byte index) => data[2] = index;
  
  public static StreamDeckPacket Copy(StreamDeckPacket other)
  {
    StreamDeckPacket clone = new StreamDeckPacket();
    if (other.data == null) return clone;
      
    clone.data = new byte[other.data.Length];
    Array.Copy(other.data, clone.data, other.data.Length);
    return clone;
  }
  
  public static List<StreamDeckPacket> GenerateData(byte[] encodedData)
  {
    List<StreamDeckPacket> packets = new List<StreamDeckPacket>();

    const int HeaderSize = 8;
    const int MaxLength  = StreamDeck.WriteBufferLength - HeaderSize;

    int remainingBytes = encodedData.Length;
    int bytesSent = 0;

    for (int splitNum = 0; remainingBytes > 0; splitNum++)
    {
      bool isLast     = remainingBytes <= MaxLength;
      int bytesToSend = maths.Min(remainingBytes, MaxLength);
      
      byte[] writeBuffer = new byte[StreamDeck.WriteBufferLength];
      Array.Copy(encodedData, bytesSent, writeBuffer, HeaderSize, bytesToSend);
      
      writeBuffer[0] = 2;
      writeBuffer[1] = 7;
      writeBuffer[2] = (byte)0; // Button Index - is set in render
      writeBuffer[3] = (byte)(isLast ? 1 : 0);
      writeBuffer[4] = (byte)(bytesToSend & 255);
      writeBuffer[5] = (byte)(bytesToSend >> 8);
      writeBuffer[6] = (byte)splitNum;
      packets.Add(new StreamDeckPacket(writeBuffer));
      
      bytesSent      += bytesToSend;
      remainingBytes -= bytesToSend;
    }
    
    return packets;
  }
}

