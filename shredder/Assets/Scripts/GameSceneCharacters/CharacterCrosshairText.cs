using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class CharacterCrosshairText : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private string searchingText;
    [SerializeField] private string targetFoundText;
    [SerializeField] private float elipsisAnimTime;
    [SerializeField] private TMP_FontAsset targetFoundFontAsset;
    
    // private delegate IEnumerator elipsisAnim();
    // private elipsisAnim DoElipsisAnim;
    
    private Coroutine elipsisAnimCo;
    
    private void Awake()
    {
        text.text = searchingText;
        elipsisAnimCo = StartCoroutine(elipsisAnim());
    }

    private void OnDestroy()
    {
        if (elipsisAnimCo != null)
        {
            StopCoroutine(elipsisAnimCo);
        }
    }

    public void LockOn()
    {
        StopCoroutine(elipsisAnimCo);
        elipsisAnimCo = null;
        text.text = targetFoundText;
        text.font = targetFoundFontAsset;
    }

    public void FadeText(float t)
    {
        text.color = new Color(text.color.r, text.color.g, text.color.b, t);
        statusText.color = new Color(text.color.r, text.color.g, text.color.b, t);
    }

    private IEnumerator elipsisAnim()
    {
        const string one = ".";
        const string two = "..";
        const string three = "...";
        int count = 0;
        float time = 0;

        while (true)
        {
            if (time >= elipsisAnimTime)
            {
                count++;
                if (count == 3)
                {
                    count = 0;
                }

                switch (count)
                {
                    case 0:
                        text.text = searchingText + one;
                        break;
                    case 1:
                        text.text = searchingText + two;
                        break;
                    case 2:
                        text.text = searchingText + three;
                        break;
                    default:
                        text.text = searchingText;
                        break;
                }
                
                time = 0;
            }
            
            time += Time.deltaTime;
            yield return CoroutineUtil.WaitForUpdate;
        }
    }

}
