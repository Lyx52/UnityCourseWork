using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DefaultNamespace;
using DefaultNamespace.Models;
using JetBrains.Annotations;
using UnityEngine.XR.Interaction.Toolkit;

public class CircleSpawner : MonoBehaviour
{
    public GameObject circlePrefab;
    public XRInteractionManager interactionManager;
    public AudioSource audioSource;
    private Dictionary<GameObject, CircleHandler> _circles;
    public long spawnDelay = 1000;
    public float zSpeed = 0.008f;
    public ComboCounter comboCounter;
    private long _playbackStartTime = 0;
    private OsuMap _selectedMap;
    private OsuMapVersion _selectedMapVersion;
    public ActionBasedController rightController;
    public ActionBasedController leftController;
    [CanBeNull] private AudioClip _currentAudioClip = null;
    private Queue<HitObject> _currentHitObjectQueue = new Queue<HitObject>();
    private long playbackTime => DateTimeOffset.Now.ToUnixTimeMilliseconds() - _playbackStartTime;
    void Start()
    {    
        var maps = OsuMapProvider.GetAvailableMaps();
        _circles = new Dictionary<GameObject, CircleHandler>();
        _selectedMap = maps.FirstOrDefault()!;
        _selectedMapVersion = _selectedMap.Versions.Values.FirstOrDefault()!;
        // for (int i = 0; i < _selectedMapVersion.HitObjects.Count; i++)
        // {
        //     _selectedMapVersion.HitObjects[i].endedAt = (uint)(i * 500) + 250;
        //     _selectedMapVersion.HitObjects[i].X = 0;
        //     _selectedMapVersion.HitObjects[i].Y = 0;
        // }
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
        foreach (var kvp in _circles)
        {
            kvp.Value.UpdatePosition(zSpeed, audioSource.transform.position);
        }
        if (_currentHitObjectQueue.TryPeek(out var obj) && obj.endedAt <= (playbackTime + spawnDelay))
        {
            obj = _currentHitObjectQueue.Dequeue();
            var x = obj.X * 28f;
            var y = obj.Y * 21f; 
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
            comboCounter.UpdateDisplay(hitResult);
            handler.StopUpdate();
            DestroyImmediate(circle);
            _circles.Remove(circle);
        };
        _circles.Add(circle, handler);
    }
}
