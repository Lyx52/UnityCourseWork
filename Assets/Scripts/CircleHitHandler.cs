using System;
using DefaultNamespace.Models;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CircleHitHandler : MonoBehaviour
{
    public LayerMask layer;
    public ControllerHand hand;
    public float maxRayDistance = 25;
    public long triggerDelayMs = 3;
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
                handler.OnControllerHit(hand);
                lastTriggered = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            }
        }
    }
}
