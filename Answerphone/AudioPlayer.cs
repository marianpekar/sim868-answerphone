using NAudio.Wave;

namespace Answerphone
{ 
    public class AudioPlayer : IDisposable
    {
        private readonly WaveOutEvent outputDevice = new();
        private readonly Ring<string>[] audioFiles;

        public AudioPlayer(params string[] audioFolders)
        {
            List<Ring<string>> temp = new();
            foreach (var folder in audioFolders)
            {
                string[] files = Directory.GetFiles(folder);

                if (files.Length == 0)
                    throw new FileNotFoundException($"No audio files found. You must provide at least one audio file in {folder}.");

                temp.Add(new Ring<string>(Directory.GetFiles(folder)));
            }
            audioFiles = temp.ToArray();
        }

        public void PlayNextTrack(int group)
        {
            audioFiles[group].PushHead();

            using var audioFile = new AudioFileReader(audioFiles[group].AtHead);
            outputDevice.Init(audioFile);
            outputDevice.Play();
        }

        public void MoveToRandomTrack(int group)
        {
            audioFiles[group].PushHead(DateTime.Now.Second);
        }

        public void Dispose()
        {
            outputDevice.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
