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
    private Dictionary<GameObject, CircleHandler> _circles = new Dictionary<GameObject, CircleHandler>();
    public long spawnEarlyOffset = 1000;
    private float _zSpeed = 0.03f;
    public UnityEvent<HitPointResult> onHitResult;
    public static UnityEvent OnMapFinished;
    private long _playbackStartTime = 0;
    [CanBeNull] private AudioClip _currentAudioClip = null;
    private Queue<HitObject> _currentHitObjectQueue = new Queue<HitObject>();
    private bool isPlaying = false;
    private long playbackTimeOnPause = 0;
    private long playbackTime => DateTimeOffset.Now.ToUnixTimeMilliseconds() - _playbackStartTime;
    void Update()
    {
        if (!isPlaying) return;
        foreach (var kvp in _circles)
        {
            kvp.Value.UpdatePosition(_zSpeed, audioSource.transform.position);
        }
        if (_currentHitObjectQueue.TryPeek(out var obj) && obj.StartTime <= (playbackTime + spawnEarlyOffset))
        {
            obj = _currentHitObjectQueue.Dequeue();
            SpawnCircle(new Vector3(obj.X, obj.Y, 0), obj.StartTime, obj.EndTime);
        } 
        if (_currentHitObjectQueue.Count <= 0)
        {
            SetMapFinished(true);
        }
    }

    public void Init()
    {
        _currentHitObjectQueue = OsuMapProvider.ActiveMapVersion!.GetHitObjectQueue();
        _zSpeed = OsuMapProvider.ActiveMapVersion.ZSpeed;
        playbackTimeOnPause = 0;
        _playbackStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        _circles = new Dictionary<GameObject, CircleHandler>();
        if (OsuMapProvider.ActiveMapVersion!.TryGetAudioClip(out var audioClip))
        {
            _currentAudioClip = audioClip;
            audioSource.clip = _currentAudioClip;
            return;
        }
        
        throw new ApplicationException("Audio clip not loaded!");
    }
    public void SetPause(bool isPaused)
    {
        if (isPaused)
        {
            playbackTimeOnPause = playbackTime;
            audioSource.Pause();
            foreach (var kvp in _circles)
            {
                kvp.Value.IsPaused = true;
            }
        }
        else
        {
            _playbackStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() - playbackTimeOnPause;
            audioSource.Play();
            foreach (var kvp in _circles)
            {
                kvp.Value.IsPaused = false;
            }
        }
        isPlaying = !isPaused;
    }

    public void Reset()
    {
        foreach (var kvp in _circles)
        {
            DestroyImmediate(kvp.Key);
        }
        if (audioSource is not null) audioSource.Pause();
        _circles.Clear();
        _currentHitObjectQueue.Clear();
    }
    
    public void SetMapFinished(bool isFinished) {
        if (OnMapFinished is not null && isFinished) OnMapFinished.Invoke();
        gameObject.SetActive(false);    
    }
    public void SpawnCircle(Vector3 position, long startTime, long endTime)
    {
        var circle = Instantiate(circlePrefab, position, Quaternion.identity);
        var handler = circle.GetComponent<CircleHandler>();
        var interactable = circle.GetComponent<XRSimpleInteractable>();
        interactable.interactionManager = interactionManager;
        handler.Initialize(startTime, endTime);
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
