using System;

[Flags]
public enum LaserHitTiming
{
  INVALID      = 1 << 0, // 1
  MISS         = 1 << 1, // 2
  WRONG_COLOUR = 1 << 2, // 4
  EARLY        = 1 << 3, // 8
  LATE         = 1 << 4, // 16
  OKAY         = 1 << 5, // 32
  GOOD         = 1 << 6, // 64
  PERFECT      = 1 << 7, // 128
}
