using System;
using System.Collections;
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
    private const long circleTicks = 64;
    private const long triggerEndpoint = 62;
    private const long triggerStartpoint = 30;

    public long circleEndedTimeMs = 0;
    public long circleFiredTimeMs = 0;
    public Action<bool> OnCircleTriggered { get; set; }
    public ActionBasedController rightController;
    public ActionBasedController leftController;
    private bool colorChanged = false;
    private bool canTrigger = false;
    private bool isHoveringRight = false;
    private bool isHoveringLeft = false;
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
        if (isHoveringLeft && ((int)leftController.activateActionValue.action.ReadValue<float>()) == 1) {
            OnCircleTriggered?.Invoke(canTrigger);
            return;
        }
        if (isHoveringRight && ((int)rightController.activateActionValue.action.ReadValue<float>()) == 1) {

            OnCircleTriggered?.Invoke(canTrigger);
            return;
        }
    }
    public void StopUpdate() => StopCoroutine(_updateCoroutine);
    private void ChangeColor(GameObject obj, Color color)
    {
        Material material = obj.GetComponent<Renderer>().material;
        material.SetColor(ColorId, color);
    }
    public void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (args.interactorObject.ToString().Contains("Right Controller")) isHoveringRight = true;
        if (args.interactorObject.ToString().Contains("Left Controller")) isHoveringLeft = true;
    }
    public void OnHoverExit(HoverExitEventArgs args)
    {
        if (args.interactorObject.ToString().Contains("Right Controller")) isHoveringRight = false;
        if (args.interactorObject.ToString().Contains("Left Controller")) isHoveringLeft = false;
    }
}
