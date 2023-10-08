using System.Collections;
using UnityEngine;

public class MainMenuStreamDeckSetUp : MonoBehaviour
{
  [SerializeField] private DeckTexture deckTexture;
  
  private delegate IEnumerator SetStreamDeckTextureDel(int index);
  private SetStreamDeckTextureDel SetStreamDeckTexture;

  private void Awake()
  {
    SetStreamDeckTexture = __SetStreamDeckTexture;
    Debug.Assert(deckTexture != null, "Deck Texture is null! Please assign one.");
  }

  private void Start()
  {
    for (int i = 0; i < StreamDeckManager.MaxStreamDeckCount; i++)
    {
      StartCoroutine(SetStreamDeckTexture(i));
    }
  }

  private IEnumerator __SetStreamDeckTexture(int index)
  {
    yield return StreamDeckManager.WaitForValidStreamDeck(index);
    StreamDeckManager.StreamDecks[index].SetDeckImage(deckTexture);
  }
}
