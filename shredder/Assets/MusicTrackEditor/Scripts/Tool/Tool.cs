using System;

public enum ToolMode : int
{
  MOVE = 0,
  SELECT,
  EDIT,
}

public static class Tool
{
  public static Action OnModeChanged;
  private static ToolMode _mode = ToolMode.MOVE;
  public static ToolMode Mode
  {
    get => _mode;
    set
    {
      _mode = value;
      OnModeChanged?.Invoke();
    }
  }
}
