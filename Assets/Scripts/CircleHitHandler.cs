using System;
using System.Linq;
using DefaultNamespace.Models;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CircleHitHandler : MonoBehaviour
{
    public LayerMask layer;
    public ControllerHand hand;
    public float maxRayDistance = 25;
    public long triggerDelayMs = 3;
    public float maxNearbyDistance = 1;
    public CircleSpawner circleSpawner;
    private ActionBasedController _controller;
    private bool IsTriggerPressed => _controller.activateActionValue.action.IsPressed();
    private long lastTriggered = 0;
    private bool CanTrigger => (DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastTriggered) > triggerDelayMs;
    void Start()
    {
        _controller = transform.GetComponent<ActionBasedController>();
    }

    void Update()
    {
        if(Physics.Raycast(transform.position, transform.forward, out var hit, maxRayDistance, layer))
        {
            if (hit.transform.TryGetComponent(out CircleHandler handler) && IsTriggerPressed && CanTrigger)
            {
                var nearby = circleSpawner.GetNearbyCircles(handler, maxNearbyDistance);
                if (nearby.Any(ch => ch.IsHittable) && !handler.IsHittable)
                {
                    // We probably wanted to hit a circle above it
                    Debug.Log($"We probably wanted to hit a circle above it {handler.IsHittable}, {nearby.Count()}");
                    return;
                }
                
                handler.OnControllerHit(hand);
                lastTriggered = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            }
        }
    }
}
