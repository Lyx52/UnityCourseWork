using System;
using System.Collections;
using System.Diagnostics;
using DefaultNamespace.Models;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Debug = UnityEngine.Debug;

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
    private const long triggerHalfPointStart = 30;
    private const long triggerFullPointStart = 40;
    private long currentTick = 0;
    public long circleEndTimeMs = 0;
    public long circleStartTimeMs = 0;
    public bool IsPaused { get; set; } = false;
    private bool IsVisible { get; set; } = true;
    public Action<HitPointResult> OnCircleTriggered { get; set; }
    private HitPointResult _currentResult;
    private Coroutine _updateCoroutine;
    public bool IsHittable => _currentResult != HitPointResult.NoPoints;
    public void Initialize(long startTime, long endTime)
    {
        _currentResult = HitPointResult.NoPoints;
        _innerCircle = transform.Find("Inner");
        _outerCircle = transform.Find("Outer");
        _outerShell = _outerCircle.Find("OuterObj").gameObject;
        _innerShell = _innerCircle.Find("InnerObj").gameObject;
        ChangeColor(_innerShell, Color.blue);
        ChangeColor(_outerShell, Color.red);
        circleStartTimeMs = startTime;
        circleEndTimeMs = endTime;

        gameObject.SetActive(true);
        _updateCoroutine = StartCoroutine(UpdateCircle());
    }

    private float GetSpeedFactor()
    {
        float progress = (float)currentTick / circleTicks;
        return (float)(1f + 1 / (1 + Math.Exp(-10 * (progress - 1))));
    }
    private IEnumerator UpdateCircle()
    {
        currentTick = 0;
        var stopwatch = Stopwatch.StartNew();
        float waitTimeSec = ((circleEndTimeMs - circleStartTimeMs - 100) / (float)circleTicks) / 1000.0f;
        float scaleSpeed = (defaultScale - minScale) / circleTicks;
        float currentScale = defaultScale;
        
        while (currentTick <= circleTicks)
        {
            if (IsPaused)
            {
                _innerShell.SetActive(false);
                _outerShell.SetActive(false);
                yield return new WaitUntil(() => !IsPaused);
            }
            else if (!_innerShell.activeSelf || !_outerShell.activeSelf)
            {
                _innerShell.SetActive(true);
                _outerShell.SetActive(true);    
            }
            
            
            if (currentTick is >= triggerHalfPointStart and < triggerFullPointStart)
            {
                
                if (_currentResult != HitPointResult.HalfPoints)
                {
                    _currentResult = HitPointResult.HalfPoints;
                    ChangeColor(_outerShell, Color.yellow);
                }
            } else if (currentTick >= triggerFullPointStart)
            {
                ;
                if (_currentResult != HitPointResult.MaxPoints)
                {
                    _currentResult = HitPointResult.MaxPoints;
                    ChangeColor(_outerShell, Color.green);
                }
            }
            else
            {
                if (_currentResult != HitPointResult.NoPoints)
                {
                    _currentResult = HitPointResult.NoPoints;
                    ChangeColor(_outerShell, Color.red);
                }
            }
            
            _outerCircle.localScale = new Vector3(currentScale, 0.1f, currentScale);
            currentScale -= scaleSpeed;
            yield return new WaitForSeconds(waitTimeSec);
            currentTick++;
        }
        stopwatch.Stop();
        Debug.Log($"Excpected ({circleEndTimeMs}-{circleStartTimeMs}): {circleEndTimeMs-circleStartTimeMs}, Actual: {stopwatch.ElapsedMilliseconds}");
        OnCircleTriggered?.Invoke(HitPointResult.NoPoints);
    }
    public void OnControllerHit(ControllerHand controllerHand)
    {
        OnCircleTriggered?.Invoke(_currentResult);
    }
    public void UpdatePosition(float zSpeed, Vector3 playerPosition)
    {
        var position = transform.position;
        Vector3 direction = (playerPosition - position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
        position += direction * (zSpeed * GetSpeedFactor());
        transform.position = position;
    }
    public void StopUpdate() => StopCoroutine(_updateCoroutine);
    private void ChangeColor(GameObject obj, Color color)
    {
        Material material = obj.GetComponent<Renderer>().material;
        material.SetColor(ColorId, color);
    }
}
