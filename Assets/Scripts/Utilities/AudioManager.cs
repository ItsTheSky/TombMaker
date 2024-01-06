using UnityEngine.Audio;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

	public static AudioManager instance;

	public AudioMixer audioMixer;

	public Sound[] sounds;
	public Dictionary<AudioSource, Sound> audioSources = new ();

	void Awake()
	{
		if (instance != null)
		{
			Destroy(gameObject);
		}
		else
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}

		foreach (Sound s in sounds)
		{
			s.source = gameObject.AddComponent<AudioSource>();
			s.source.clip = s.clip;
			s.source.loop = s.loop;

			s.source.outputAudioMixerGroup = s.mixerGroup;
		}
	}

	public void Play(string sound, GameObject source = null)
	{
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}

		if (source != null)
		{
			if (audioSources.ContainsKey(source.GetComponent<AudioSource>()))
			{
				Stop(sound, source);
				audioSources[source.GetComponent<AudioSource>()] = s;
			}
			else
			{
				audioSources.Add(source.GetComponent<AudioSource>(), s);
			}
			
			var audioSource = source.GetComponent<AudioSource>();
			if (audioSource == null)
				audioSource = source.AddComponent<AudioSource>();
			
			audioSource.clip = s.clip;
			audioSource.volume = s.volume * (1f + UnityEngine.Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
			audioSource.pitch = s.pitch * (1f + UnityEngine.Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));

			audioSource.Play();
			return;
		}
		
		s.source.volume = s.volume * (1f + UnityEngine.Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
		s.source.pitch = s.pitch * (1f + UnityEngine.Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));

		s.source.Play();
	}

	public void Stop(string sound, GameObject source = null)
	{
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}

		if (source != null)
		{
			var audioSource = source.GetComponent<AudioSource>();
			if (audioSource == null)
				audioSource = source.AddComponent<AudioSource>();
			
			audioSource.clip = s.clip;
			audioSource.volume = s.volume * (1f + UnityEngine.Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
			audioSource.pitch = s.pitch * (1f + UnityEngine.Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));

			audioSource.Stop();
			return;
		}
		
		s.source.volume = s.volume * (1f + UnityEngine.Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
		s.source.pitch = s.pitch * (1f + UnityEngine.Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));

		s.source.Stop();
	}

	public void StopAll()
	{
		foreach (var sound in sounds)
			sound.source.Stop();
	}
	
	public static void PlaySound(string sound, GameObject source = null)
	{
		if (instance == null)
			return;
		
		instance.Play(sound, source);
	}
	
	public static void StopSound(string sound)
	{
		if (instance == null)
			return;
		
		instance.Stop(sound);
	}
}
