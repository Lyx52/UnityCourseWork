using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using DefaultNamespace;
using UnityEngine.Networking;
using UnityEngine.XR.Interaction.Toolkit;

public class CircleSpawner : MonoBehaviour
{
    public GameObject CirclePrefab;
    public XRInteractionManager _interactionManager;
    private GameObject _camera;
    private AudioSource _audioSource;
    private List<GameObject> _circles;
    public long spawnDelay = 1000;
    public float zSpeed = 0.008f;
    private OsuMapProcessor _mapProcessor;
    private long playbackStartTime = 0;
    private long playbackTime => DateTimeOffset.Now.ToUnixTimeMilliseconds() - playbackStartTime;
    void Start()
    {   
        _circles = new List<GameObject>();
        _mapProcessor= new OsuMapProcessor(
            "C:\\Users\\Students\\Documents\\DVacIMeln\\3DGalaDarbs\\158023 UNDEAD CORPORATION - Everything will freeze","UNDEAD CORPORATION - Everything will freeze (Ekoro) [Normal].osu");
        _mapProcessor.Process();
        StartCoroutine(_mapProcessor.LoadClip());
        _interactionManager = GameObject.Find("XR Interaction Manager").GetComponent<XRInteractionManager>();
        _camera = GameObject.Find("Main Camera");
        _audioSource = _camera.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_audioSource is null || _mapProcessor.audioClip is null) return;
        
        if (_audioSource is { clip: null })
        {
            _audioSource.clip = _mapProcessor.audioClip;
            _audioSource.Play();
            playbackStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
        foreach (var circle in _circles)
        {
            var original = circle.transform.position;
            circle.transform.position = new Vector3(original.x, original.y, original.z - zSpeed);
        }
        if (_mapProcessor._hitObjects.TryPeek(out var obj) && obj.endedAt <= (playbackTime + spawnDelay))
        {
            obj = _mapProcessor._hitObjects.Dequeue();
            var x = obj.X * 6f;
            var y = obj.Y * 4f; 
            SpawnCircle(new Vector3(x, y, 0), playbackTime, obj.endedAt);
        }
    }

    public void SpawnCircle(Vector3 position, long firedAt, long endedAt)
    {
        var circle = Instantiate(CirclePrefab, position, Quaternion.identity);
        var handler = circle.GetComponent<CircleHandler>();
        var interactable = circle.GetComponent<XRSimpleInteractable>();
        interactable.interactionManager = _interactionManager;
        handler.Initialize(firedAt, endedAt);
        handler.OnCircleTriggered += () =>
        {
            //Debug.Log($"Despawned z = {circle.transform.position.z}");
            DestroyImmediate(circle);
            _circles.Remove(circle);
        };
        _circles.Add(circle);
    }
}
