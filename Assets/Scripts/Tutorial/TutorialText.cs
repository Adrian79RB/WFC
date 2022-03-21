using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialText : MonoBehaviour
{
    public int quoteIndex;
    public string[] quotes;
    public Text text;

    float showTime;
    float showTimer;
    bool isShowingText;

    private void Start()
    {
        showTime = 0f;
        showTimer = 5f;
        isShowingText = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isShowingText)
        {
            showTime += Time.deltaTime;
            if(showTime >= showTimer)
            {
                text.text = "";
                isShowingText = false;
                showTime = 0f;
            }
        }
        else if( quoteIndex < quotes.Length && (quoteIndex <= 4 || quoteIndex > 6) )
        {
            showNextQuote();
        }
    }

    public void showNextQuote()
    {
        text.text = quotes[quoteIndex];
        quoteIndex++;
        isShowingText = true;
    }
}
