using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DefaultNamespace;
using DefaultNamespace.Models;
using JetBrains.Annotations;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class CircleSpawner : MonoBehaviour
{
    public GameObject circlePrefab;
    public XRInteractionManager interactionManager;
    public AudioSource audioSource;
    private Dictionary<GameObject, CircleHandler> _circles;
    public long spawnDelay = 1000;
    public float zSpeed = 0.008f;
    public UnityEvent<HitPointResult> onHitResult;
    private long _playbackStartTime = 0;
    [CanBeNull] private AudioClip _currentAudioClip = null;
    private Queue<HitObject> _currentHitObjectQueue = new Queue<HitObject>();
    private long playbackTime => DateTimeOffset.Now.ToUnixTimeMilliseconds() - _playbackStartTime;
    void Start()
    {
        if (OsuMapProvider.ActiveMap is null || OsuMapProvider.ActiveMapVersion is null)
            throw new UnityException("Map not loaded!");
        _circles = new Dictionary<GameObject, CircleHandler>();
    }

    void Update()
    {
        if (!gameObject.activeSelf) return;
        if (_currentAudioClip is null)
        {
            if (OsuMapProvider.ActiveMapVersion!.TryGetAudioClip(out var audioClip)) _currentAudioClip = audioClip;
            return;
        }
        
        if (audioSource is { clip: null })
        {
            audioSource.clip = _currentAudioClip;
            audioSource.Play();
            _currentHitObjectQueue = OsuMapProvider.ActiveMapVersion!.GetHitObjectQueue();
            _playbackStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
        foreach (var kvp in _circles)
        {
            kvp.Value.UpdatePosition(zSpeed, audioSource.transform.position);
        }
        if (_currentHitObjectQueue.TryPeek(out var obj) && obj.endedAt <= (playbackTime + spawnDelay))
        {
            obj = _currentHitObjectQueue.Dequeue();
            var x = obj.X * 24f;
            var y = obj.Y * 18f; 
            SpawnCircle(new Vector3(x, y, 0), playbackTime, obj.endedAt);
        }
    }
    public void SpawnCircle(Vector3 position, long firedAt, long endedAt)
    {
        var circle = Instantiate(circlePrefab, position, Quaternion.identity);
        var handler = circle.GetComponent<CircleHandler>();
        var interactable = circle.GetComponent<XRSimpleInteractable>();
        interactable.interactionManager = interactionManager;
        handler.Initialize(firedAt, endedAt);
        handler.OnCircleTriggered += (hitResult) =>
        {
            onHitResult.Invoke(hitResult);
            handler.StopUpdate();
            DestroyImmediate(circle);
            _circles.Remove(circle);
        };
        _circles.Add(circle, handler);
    }

    public IEnumerable<CircleHandler> GetNearbyCircles(CircleHandler handler, float distance)
    {
        return _circles.Values.Where(ch =>
            Vector2.Distance(ch.transform.position, handler.transform.position) <= distance);
    }
}
