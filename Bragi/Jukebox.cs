using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace LegendaryTools.Bragi
{
    public class Jukebox : SingletonBehaviour<Jukebox>, IPlayable
    {
        public JukeboxConfig Config;

        [ShowInInspector]
        public bool IsMuted => currentHandlers?.Any(item => item.IsMuted) ?? false;
        [ShowInInspector]
        public bool IsPlaying => currentHandlers?.Any(item => item.IsPlaying) ?? false;
        [ShowInInspector]
        public bool IsPaused => currentHandlers?.Any(item => item.IsPaused) ?? false;

        private List<AudioConfigBase> randomOrderTracks;
        private int currentTrackIndex;
        private AudioHandler[] currentHandlers;
        private readonly Random rnGod = new Random();
        
        [Button]
        public void Play()
        {
            switch (Config.PlayMode)
            {
                case JukeboxPlayMode.Sequential:
                {
                    currentHandlers = Config.Tracks[currentTrackIndex].Play(allowFading: Config.Transition == JukeboxTransition.Fade);
                    break;
                }
                case JukeboxPlayMode.Random:
                {
                    if (randomOrderTracks == null)
                    {
                        GenerateShuffledTracks();
                    }
                    else
                    {
                        currentHandlers = randomOrderTracks[currentTrackIndex].Play(allowFading: Config.Transition == JukeboxTransition.Fade);
                    }
                    break;
                }
            }

            foreach (AudioHandler audioHandler in currentHandlers)
            {
                audioHandler.OnFinished += OnAudioHandlerFinished;
            }
        }

        [Button]
        public void Next()
        {
            StopNowAllCurrentHandlers();
            currentTrackIndex = (currentTrackIndex + 1) % Config.Tracks.Length;
            Play();
        }
        
        [Button]
        public void Prev()
        {
            StopNowAllCurrentHandlers();
            currentTrackIndex--;
            if (currentTrackIndex < 0)
            {
                currentTrackIndex = Config.Tracks.Length - 1;
            }
            Play();
        }

        [Button]
        public void Stop()
        {
            foreach (AudioHandler audioHandler in currentHandlers)
            {
                if (audioHandler.IsPlaying)
                {
                    audioHandler.Stop();
                }
            }
        }

        [Button]
        public void Pause()
        {
            foreach (AudioHandler audioHandler in currentHandlers)
            {
                if (audioHandler.IsPlaying)
                {
                    audioHandler.Pause();
                }
            }
        }

        [Button]
        public void UnPause()
        {
            foreach (AudioHandler audioHandler in currentHandlers)
            {
                if (audioHandler.IsPaused)
                {
                    audioHandler.UnPause();
                }
            }
        }

        [Button]
        public void Mute()
        {
            foreach (AudioHandler audioHandler in currentHandlers)
            {
                if (!audioHandler.IsMuted)
                {
                    audioHandler.Mute();
                }
            }
        }

        [Button]
        public void UnMute()
        {
            foreach (AudioHandler audioHandler in currentHandlers)
            {
                if (audioHandler.IsMuted)
                {
                    audioHandler.UnMute();
                }
            }
        }
        
        protected override void Start()
        {
            base.Start();
            if (Config != null)
            {
                if (Config.AutoStart)
                {
                    Play();
                }
            }
        }
        
        void GenerateShuffledTracks()
        {
            randomOrderTracks = new List<AudioConfigBase>(Config.Tracks);
            randomOrderTracks.Shuffle(rnGod);
        }

        void OnAudioHandlerFinished(AudioHandler audioHandler)
        {
            audioHandler.OnFinished -= OnAudioHandlerFinished;
            if (currentHandlers.All(item => !item.IsPlaying))
            {
                currentHandlers = null;

                if (Config.Repeat)
                {
                    Play();
                    return;
                }
                
                currentTrackIndex = (currentTrackIndex + 1) % Config.Tracks.Length;
                if (currentTrackIndex == 0)
                {
                    if (Config.CircularTracks)
                    {
                        if (Config.PlayMode == JukeboxPlayMode.RandomReSeed)
                        {
                            GenerateShuffledTracks();
                        }
                        
                        Play();
                    }
                }
                else
                {
                    Play();
                }
            }
        }
        
        void StopNowAllCurrentHandlers()
        {
            foreach (AudioHandler audioHandler in currentHandlers)
            {
                if (audioHandler.IsPlaying)
                {
                    audioHandler.StopNow();
                }
            }
        }
    }
}