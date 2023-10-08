using System;
using UnityEngine;

public class BackgroundSection : MonoBehaviour
{
  [SerializeField] private BackgroundObjectRandomiser[] objectRandomisers = Array.Empty<BackgroundObjectRandomiser>();
  
  public void ChooseSectionAssets()
  {
    foreach (BackgroundObjectRandomiser randomiser in objectRandomisers)
    {
      randomiser.RandomiseObject();
    }
  }
}