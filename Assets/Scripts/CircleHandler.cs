using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CircleHandler : MonoBehaviour
{
    private GameObject _outerShell;
    private GameObject _innerShell;
    private Transform _outerCircle;
    private Transform _innerCircle;
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private const float minScale = 0.08f;
    private const float defaultScale = 0.15f;
    private const long circleTicks = 32;

    public long circleEndedTimeMs = 0;
    public long circleFiredTimeMs = 0;
    public Action OnCircleTriggered { get; set; }

    public void Initialize(long firedAt, long endedAt)
    {
        _innerCircle = transform.Find("Inner");
        _outerCircle = transform.Find("Outer");
        _outerShell = _outerCircle.Find("OuterObj").gameObject;
        _innerShell = _innerCircle.Find("InnerObj").gameObject;
        ChangeColor(_innerShell, Color.blue);
        ChangeColor(_outerShell, Color.red);
        circleFiredTimeMs = firedAt;
        circleEndedTimeMs = endedAt;

        gameObject.SetActive(true);
        StartCoroutine(UpdateCircle());
    }

    private IEnumerator UpdateCircle()
    {
        long currentTick = 0;
        float waitTimeSec = ((circleEndedTimeMs - circleFiredTimeMs) / (float)circleTicks) / 1000.0f;
        float scaleSpeed = (defaultScale - minScale) / circleTicks;
        float currentScale = defaultScale;
        while (currentTick <= circleTicks)
        {
            if (!gameObject.activeSelf) yield return null;
            _outerCircle.localScale = new Vector3(currentScale, 0.1f, currentScale);
            currentScale -= scaleSpeed;
            yield return new WaitForSecondsRealtime(waitTimeSec);
            currentTick++;
        }

        OnCircleTriggered?.Invoke();
    }

    private void ChangeColor(GameObject obj, Color color)
    {
        Material material = obj.GetComponent<Renderer>().material;
        material.SetColor(ColorId, color);
    }

    public void OnInteracted(HoverEnterEventArgs args)
    {
        var pos = gameObject.transform.position;
        Debug.Log($"TRIGGERED ON ({pos.x}, {pos.y}, {pos.z})");
        OnCircleTriggered?.Invoke();
    }

    public void OnActivated(ActivateEventArgs args)
    {
        // var pos = gameObject.transform.position;
        // Debug.Log($"ACTIVATED ON ({pos.x}, {pos.y}, {pos.z})");
        // OnCircleTriggered?.Invoke();    
    }

}
