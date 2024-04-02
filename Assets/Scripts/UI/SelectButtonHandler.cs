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
    private string buttonKey;
    public void Init(string title, string backgroundImage, string key)
    {
        titleComponent.text = title;
        buttonKey = key;
    }

    public void OnButtonClick(bool active) => onSelected?.Invoke(buttonKey, active);

    public void SetSelected(bool active)
    {
        toggleComponent.SetIsOnWithoutNotify(false);
        SetBorderActive(active);
    }
    public void SetBorderActive(bool active) => borderComponent.gameObject.SetActive(active);
}
