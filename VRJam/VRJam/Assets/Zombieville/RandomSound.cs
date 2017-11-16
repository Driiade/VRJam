using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// [ExecuteInEditMode]
[RequireComponent(typeof(AudioSource))]
public class RandomSound : MonoBehaviour {
    public bool stopped;
    public float minDelay = 1;
    public float maxDelay = 10;
    public enum RepeatMode {
        normal,
        avoidRepeats,
        coverAll,
    }
    public enum PlayMode {
        normal,
        singleCycle,
        singleSound,
    }
    public RepeatMode repeat;
    public PlayMode play;
    
    AudioSource[] sources;
    List<AudioSource> lastPlayedSources;
    Coroutine runningCoroutine;
    
	void Start () {
        sources = GetComponents<AudioSource>();
        if (!stopped && runningCoroutine == null)
            runningCoroutine = StartCoroutine(waitForNextSound());
	}
    
    IEnumerator waitForNextSound()
    {
        float nextTimeSeconds = Random.Range(minDelay, maxDelay);
        yield return new WaitForSeconds(nextTimeSeconds);
        runningCoroutine = null;
        if (!stopped)
            PlayNext();
    }
    
    void PlayNext()
    {
        int index;
        bool wasCycle = false;
        if (repeat == RepeatMode.normal)
        {
            index = Random.Range(0, sources.Length);
            sources[index].Play();
        }
        else if (repeat == RepeatMode.avoidRepeats || repeat == RepeatMode.coverAll)
        {
            if (lastPlayedSources == null)
                lastPlayedSources = new List<AudioSource>();
            AudioSource sourceToPlay;
            do {
                index = Random.Range(0, sources.Length);
                sourceToPlay = sources[index];
            } while (lastPlayedSources.Contains(sourceToPlay));
            sourceToPlay.Play();
            if (lastPlayedSources.Count >= sources.Length-2 || repeat == RepeatMode.avoidRepeats)
            {
                lastPlayedSources.Clear();
                wasCycle = true;
            }
            lastPlayedSources.Add(sourceToPlay);
        }
        
        if (play == PlayMode.normal || (play == PlayMode.singleCycle && !wasCycle) && !stopped)
            runningCoroutine = StartCoroutine(waitForNextSound());
    }
}
