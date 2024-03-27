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
    private List<GameObject> _circles;
    public long spawnDelay = 1000;
    public float zSpeed = 0.008f;
    private long _playbackStartTime = 0;
    private OsuMap _selectedMap;
    private OsuMapVersion _selectedMapVersion;
    public ActionBasedController rightController;
    public ActionBasedController leftController;
    [CanBeNull] private AudioClip _currentAudioClip = null;
    private Queue<HitObject> _currentHitObjectQueue = new Queue<HitObject>();
    private long playbackTime => DateTimeOffset.Now.ToUnixTimeMilliseconds() - _playbackStartTime;
    private long timeSinceClicked = 0;
    public static bool hasClicked = false;
    void Start()
    {    
        var maps = OsuMapProvider.GetAvailableMaps();
        _circles = new List<GameObject>();
        _selectedMap = maps.FirstOrDefault(m => m.Title == "1674622 Bossfight - Endgame")!;
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
        handler.leftController = leftController;
        handler.rightController = rightController;
        handler.Initialize(firedAt, endedAt);
        handler.OnCircleTriggered += (triggeredOnTime) =>
        {
            timeSinceClicked = playbackTime;
            handler.StopUpdate();
            DestroyImmediate(circle);
            _circles.Remove(circle);
            //Debug.Log($"Circle triggered! {triggeredOnTime}");
        };
        _circles.Add(circle);
    }
}
