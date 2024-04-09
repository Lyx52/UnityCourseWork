using TMPro;
using UnityEngine;

public class LoadingAnimator : MonoBehaviour
{
    private float _totalTime = 0;
    private int _index = 0;
    public TextMeshProUGUI textComponent;
    private readonly string[] _frames = new[]
    {
        "Loading.",
        "Loading..",
        "Loading..."
    };
    void FixedUpdate()
    {
        if (!gameObject.activeSelf) return;
        if (_totalTime >= 0.75f)
        {
            textComponent.text = _frames[_index];
            _index++;
            _totalTime = 0;
            if (_index >= _frames.Length) _index = 0;
        }

        _totalTime += Time.fixedDeltaTime;
    }

    public void SetActive(bool active) => gameObject.SetActive(active);
}
