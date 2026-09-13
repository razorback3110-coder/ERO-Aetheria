using System;
using System.Collections.Generic;
using UnityEngine;

namespace EternalRealmsOnline.Audio
{
    /// <summary>
    /// ERO adaptive audio director. It is asset-agnostic: approved AudioClips can be dropped into
    /// Resources/ERO/Audio and registered by the profile without hard dependencies on third-party content.
    /// </summary>
    public sealed class EROAudioRuntimeDirector : MonoBehaviour
    {
        public enum MusicState { Menu, Exploration, Combat, EliteCombat, Boss, Dungeon, PvP, Victory }

        [Serializable]
        public sealed class MusicProfile
        {
            public string id;
            public MusicState state;
            public string resourcePath;
            [Range(0f, 1f)] public float volume = .85f;
            public bool loop = true;
        }

        static EROAudioRuntimeDirector instance;
        AudioSource music;
        AudioSource ambience;
        readonly Dictionary<MusicState, MusicProfile> profiles = new Dictionary<MusicState, MusicProfile>();
        MusicState currentState = MusicState.Menu;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            if (instance != null) return;
            var go = new GameObject("ERO_AudioRuntime");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<EROAudioRuntimeDirector>();
        }

        void Awake()
        {
            if (instance != null && instance != this) { Destroy(gameObject); return; }
            instance = this;
            music = CreateSource("Music", true);
            ambience = CreateSource("Ambience", true);
            BuildDefaultProfiles();
        }

        AudioSource CreateSource(string name, bool loop)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 0f;
            return source;
        }

        void BuildDefaultProfiles()
        {
            Add(MusicState.Menu, "ERO/Audio/Music/Menu");
            Add(MusicState.Exploration, "ERO/Audio/Music/Exploration");
            Add(MusicState.Combat, "ERO/Audio/Music/Combat");
            Add(MusicState.EliteCombat, "ERO/Audio/Music/EliteCombat");
            Add(MusicState.Boss, "ERO/Audio/Music/Boss");
            Add(MusicState.Dungeon, "ERO/Audio/Music/Dungeon");
            Add(MusicState.PvP, "ERO/Audio/Music/PvP");
            Add(MusicState.Victory, "ERO/Audio/Music/Victory");
        }

        void Add(MusicState state, string path)
        {
            profiles[state] = new MusicProfile { id = state.ToString(), state = state, resourcePath = path };
        }

        public void SetMusicState(MusicState state)
        {
            currentState = state;
            if (!profiles.TryGetValue(state, out var profile)) return;
            var clip = Resources.Load<AudioClip>(profile.resourcePath);
            if (clip == null) return; // Registry-driven content may be installed later.
            if (music.clip == clip && music.isPlaying) return;
            music.clip = clip;
            music.volume = profile.volume;
            music.loop = profile.loop;
            music.Play();
        }

        public void SetAmbience(AudioClip clip, float volume = .55f)
        {
            if (clip == null) return;
            if (ambience.clip == clip && ambience.isPlaying) return;
            ambience.clip = clip;
            ambience.volume = Mathf.Clamp01(volume);
            ambience.Play();
        }

        public MusicState CurrentState => currentState;
    }
}
