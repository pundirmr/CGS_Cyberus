using System;
using System.Collections;
using UnityEngine;

public class LaserInput : MonoBehaviour
{
  [Header("Scene References")]
  [SerializeField] private PlayerID playerID;
  
  [Header("Stream Deck")]
  [SerializeField] public ButtonIndices[] colourButtons = Array.Empty<ButtonIndices>();
  
  public delegate void ColourButtonEvent(int colourIndex);
  public ColourButtonEvent OnColourButtonPerformed;
  public ColourButtonEvent OnColourButtonCancelled;
  
  private ConsumableAction.Delegate[] _performedLambdas;
  private ConsumableAction.Delegate[] _cancelledLambdas;
  
  private StreamDeck streamDeck => StreamDeckManager.StreamDecks[playerID.ID];

  private void Awake()
  {
    Debug.Assert(playerID != null, "Player ID is null! Please assign one.");
  }

  private IEnumerator Start()
  {
    yield return new WaitForSeconds(1f);   //added

    //commented//yield return PlayerManager.WaitForValidPlayer(playerID.ID);
    //commmented//yield return StreamDeckManager.WaitForValidStreamDeck(playerID.ID);

    //commmented//Debug.Assert(colourButtons.Length == playerID.PlayerData.ColourScheme.Colours.Length, "The amount of colour buttons doesnt match the number of colours in the players colour scheme.");

    //commmented//streamDeck.ClearDeck();
    //commmented//streamDeck.OnConnect += SetButtonColours;

    _performedLambdas = new ConsumableAction.Delegate[colourButtons.Length];
    _cancelledLambdas = new ConsumableAction.Delegate[colourButtons.Length];

        Debug.Log("laseInput. colorbuttons length :" + colourButtons.Length);

    for (int i = 0; i < colourButtons.Length; ++i)
    {
      // Capturing index so its unique in each action
      int index = i;
      
      // Setup performed events
      _performedLambdas[i] = delegate() { OnColourButtonPerformed?.Invoke(index); };
        //commmented//colourButtons[i].SubscribeToButtonPerformed(playerID.ID, _performedLambdas[i]);   

        // Setup cancelled events
        _cancelledLambdas[i] = delegate() { OnColourButtonCancelled?.Invoke(index); };
        //commmented//colourButtons[i].SubscribeToButtonCancelled(playerID.ID, _cancelledLambdas[i]);
    }

        //commmented//SetButtonColours();
    }

  private void SetButtonColours()
  {
    for (int i = 0; i < colourButtons.Length; ++i)
    {
      colourButtons[i].SetButtonColour(playerID.ID, playerID.PlayerData.ColourScheme.StreamDeckColours[i]);
    }
  }

  private void OnDestroy()
  {
    if (!playerID.IsValid) return;
    
    if (streamDeck != null) streamDeck.OnConnect -= SetButtonColours;
    
    if (_performedLambdas == null || _cancelledLambdas == null) return;

    for (int i = 0; i < colourButtons.Length; i += 1)
    {
      colourButtons[i].UnsubscribeFromButtonPerformed(playerID.ID, _performedLambdas[i]);
      colourButtons[i].UnsubscribeFromButtonPerformed(playerID.ID, _cancelledLambdas[i]);
    }
  }
}
