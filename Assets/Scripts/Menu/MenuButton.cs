using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Text buttonText;
    public GameObject frame;
    public Color textColor;
    public MenuScript menu;
    public bool isActive;


    public void OnPointerEnter(PointerEventData eventData)
    {
        ActivateButton();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DeactivateButton();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClickButton();
    }

    public void ActivateButton()
    {
        if (!frame.activeSelf)
        {
            frame.SetActive(true);
            buttonText.color = textColor;
            isActive = true;
        }
    }

    public void OnClickButton()
    {
        if (transform.name.Contains("Start") || transform.name.Contains("Restart") )
            menu.StartButtonPressed();
        else if (transform.name.Contains("Tutorial"))
            menu.TutorialButtonPressed();
        else if(transform.name.Contains("Exit"))
            menu.ExitButtonPressed();
    }

    public void DeactivateButton()
    {
        if (frame.activeSelf)
        {
            frame.SetActive(false);
            buttonText.color = Color.black;
            isActive = false;
        }
    }
}
