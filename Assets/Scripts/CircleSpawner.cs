using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using DefaultNamespace;
using DefaultNamespace.Models;
using JetBrains.Annotations;
using UnityEngine.Networking;
using UnityEngine.XR.Interaction.Toolkit;

public class CircleSpawner : MonoBehaviour
{
    public GameObject circlePrefab;
    public XRInteractionManager interactionManager;
    public AudioSource audioSource;
    private List<GameObject> _circles;
    public long spawnDelay = 1000;
    public float zSpeed = 0.008f;
    private long _playbackStartTime = 0;
    private OsuMap _selectedMap;
    private OsuMapVersion _selectedMapVersion;
    
    [CanBeNull] private AudioClip _currentAudioClip = null;
    private Queue<HitObject> _currentHitObjectQueue = new Queue<HitObject>();
    private long playbackTime => DateTimeOffset.Now.ToUnixTimeMilliseconds() - _playbackStartTime;
    void Start()
    {   
        var maps = OsuMapProvider.GetAvailableMaps();
        _circles = new List<GameObject>();
        _selectedMap = maps.FirstOrDefault()!;
        _selectedMapVersion = _selectedMap.Versions.Values.FirstOrDefault()!;
        StartCoroutine(_selectedMapVersion.LoadClip());
        if (_selectedMapVersion.TryLoadBackground(out var background))
        {
            SkyboxHandler.UpdateSkybox(background);
        }
    }

    void Update()
    {
        if (_currentAudioClip is null)
        {
            if (_selectedMapVersion.TryGetAudioClip(out var audioClip)) _currentAudioClip = audioClip;
            return;
        }
        
        if (audioSource is { clip: null })
        {
            audioSource.clip = _currentAudioClip;
            audioSource.Play();
            _currentHitObjectQueue = _selectedMapVersion.GetHitObjectQueue();
            _playbackStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
        foreach (var circle in _circles)
        {
            var original = circle.transform.position;
            circle.transform.position = new Vector3(original.x, original.y, original.z - zSpeed);
        }
        if (_currentHitObjectQueue.TryPeek(out var obj) && obj.endedAt <= (playbackTime + spawnDelay))
        {
            obj = _currentHitObjectQueue.Dequeue();
            var x = obj.X * 6f;
            var y = obj.Y * 4f; 
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
        handler.OnCircleTriggered += () =>
        {
            DestroyImmediate(circle);
            _circles.Remove(circle);
        };
        _circles.Add(circle);
    }
}
