using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Answerphone
{
    public static class AudioMixer
    {
        public static void MixTwo(string fileOne, string fileTwo, string output)
        {
            using var readerOne = new AudioFileReader(fileOne);
            using var readerTwo = new AudioFileReader(fileTwo);
            var mixer = new MixingSampleProvider(new[] { readerOne, readerTwo });
            WaveFileWriter.CreateWaveFile16(output, mixer);
        }
    }
}
