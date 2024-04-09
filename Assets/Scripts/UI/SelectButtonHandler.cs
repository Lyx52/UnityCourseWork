using System;
using UnityEngine;
using UnityEngine.UI;

public class SelectButtonHandler : MonoBehaviour
{
    public Text titleComponent;
    public Image backgroundComponent;
    public Image borderComponent;
    public Toggle toggleComponent;
    public Action<string, bool> onSelected;
    private string buttonKey = string.Empty;
    public void Init(string title, string key, Texture2D background)
    {
        titleComponent.text = title;
        buttonKey = key;
        SetBackgroundImage(background);
    }

    public void OnButtonClick(bool active) => onSelected?.Invoke(buttonKey, active);

    public void SetSelected(bool active)
    {
        toggleComponent.SetIsOnWithoutNotify(false);
        SetBorderActive(active);
    }

    public void SetBackgroundImage(Texture2D texture)
    {
        backgroundComponent.sprite = Sprite.Create(texture, backgroundComponent.sprite.rect, backgroundComponent.sprite.pivot);
    }
    public void SetBorderActive(bool active) => borderComponent.gameObject.SetActive(active);
}
