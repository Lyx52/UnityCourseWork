using System;
using System.Linq;
using DefaultNamespace.Models;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class CircleHitHandler : MonoBehaviour
{
    public LayerMask layer;
    public ControllerHand hand;
    public float maxRayDistance = 25;
    private long pauseDelayMs = 500;
    public float maxNearbyDistance = 1;
    public CircleSpawner circleSpawner;
    private ActionBasedController _controller;
    public UnityEvent onGamePause;
    private bool IsTriggerPressed => _controller.activateActionValue.action.IsPressed();
    private bool IsPausePressed => _controller.selectActionValue.action.IsPressed();
    private long lastPaused = 0;
    private bool CanPause => (DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastPaused) > pauseDelayMs;
    void Start()
    {
        _controller = transform.GetComponent<ActionBasedController>();
    }
    public void OnCircleHit(CircleHit hit) {
        if (hit.Hand != hand) return;
        switch(hit.HitResult) {
            case HitPointResult.MaxPoints: {
                _controller.SendHapticImpulse(0.4f, 0.025f);
            } break;
            case HitPointResult.HalfPoints: {
                _controller.SendHapticImpulse(0.1f, 0.025f);
            } break;
        }   
    }
    void Update()
    {
        if (IsPausePressed && CanPause)
        {
            onGamePause.Invoke();
            Debug.Log("CONTROLLER PAUSE");
            lastPaused = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
        if(Physics.Raycast(transform.position, transform.forward, out var hit, maxRayDistance, layer))
        {
            if (hit.transform.TryGetComponent(out CircleHandler handler) && IsTriggerPressed)
            {
                var nearby = circleSpawner.GetNearbyCircles(handler, maxNearbyDistance);
                if (nearby.Any(ch => ch.IsHittable) && !handler.IsHittable)
                {
                    // We probably wanted to hit a circle above it
                    Debug.Log($"We probably wanted to hit a circle above it {handler.IsHittable}, {nearby.Count()}");
                    return;
                }
                
                handler.OnControllerHit(hand);
            }
        }
    }
}
