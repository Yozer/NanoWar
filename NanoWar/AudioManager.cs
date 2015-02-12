namespace NanoWar
{
    using System.Collections.Generic;
    using System.Linq;

    using SFML.Audio;

    internal class AudioManager
    {
        private List<SoundData> _sounds = new List<SoundData>();

        public void PlaySound(string name, bool loop = false, int volume = 100)
        {
            for (var i = 0; i < _sounds.Count; i++)
            {
                if (_sounds[i].Sound.Status == SoundStatus.Stopped)
                {
                    _sounds[i].Dispose();
                    _sounds.RemoveAt(i);
                    i--;
                }
            }

            if (name.Contains("background") && _sounds.Any(t => t.Name == name))
            {
                if (_sounds.Single(t => t.Name == name).Sound.Status != SoundStatus.Playing)
                {
                    _sounds.Single(t => t.Name == name).Sound.Play();
                }
            }
            else
            {
                var sound = new Sound(ResourceManager.Instance[name] as SoundBuffer);
                _sounds.Add(new SoundData(name, sound));
                sound.Loop = loop;
                sound.Volume = volume;
                sound.Play();
            }
        }

        public void RemoveSound(string name)
        {
            _sounds.Where(t => t.Name == name).ToList().ForEach(t => t.Dispose());
            _sounds.RemoveAll(t => t.Name == name);
        }

        public void PauseAllBackground()
        {
            _sounds.Where(t => t.Name.Contains("background")).ToList().ForEach(t => t.Sound.Pause());
        }

        public void PauseSound(string name)
        {
            _sounds.Where(t => t.Name == name).ToList().ForEach(t => t.Sound.Pause());
        }

        public void PauseAll()
        {
            _sounds.ForEach(t => t.Sound.Pause());
        }
    }

    internal class SoundData
    {
        public SoundData(string name, Sound sound)
        {
            Sound = sound;
            Name = name;
        }

        public Sound Sound { get; private set; }

        public string Name { get; private set; }

        public void Dispose()
        {
            Sound.Dispose();
        }
    }
}