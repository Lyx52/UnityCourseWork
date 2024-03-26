using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.XR;
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
    private const long triggerEndpoint = 30;
    private const long triggerStartpoint = 15;

    public long circleEndedTimeMs = 0;
    public long circleFiredTimeMs = 0;
    public Action<bool> OnCircleTriggered { get; set; }
    public ActionBasedController leftController;
    private bool colorChanged = false;
    private bool canTrigger = false;
    private bool isHovering = false;
    private Coroutine _updateCoroutine;
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
        _updateCoroutine = StartCoroutine(UpdateCircle());
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

            if (currentTick is >= triggerStartpoint and <= triggerEndpoint)
            {
                canTrigger = true;
                if (!colorChanged)
                {
                    ChangeColor(_outerShell, Color.green);
                    colorChanged = true;
                }
            }
            else
            {
                canTrigger = false;
                if (colorChanged)
                {
                    ChangeColor(_outerShell, Color.red);
                    colorChanged = false;
                }
            }
            
            _outerCircle.localScale = new Vector3(currentScale, 0.1f, currentScale);
            currentScale -= scaleSpeed;
            yield return new WaitForSeconds(waitTimeSec);
            currentTick++;
        }
        OnCircleTriggered?.Invoke(false);
    }

    private void Update()
    {
        if (!isHovering) return;
        if (((int)leftController.activateAction.action.ReadValue<float>()) == 1)
        {
            StopCoroutine(_updateCoroutine);
            OnCircleTriggered?.Invoke(canTrigger);
        }
    }

    private void ChangeColor(GameObject obj, Color color)
    {
        Material material = obj.GetComponent<Renderer>().material;
        material.SetColor(ColorId, color);
    }
    public void OnHoverEnter(HoverEnterEventArgs args)
    {
        isHovering = true;
    }
    public void OnHoverExit(HoverExitEventArgs args)
    {
        isHovering = false;
    }
}
