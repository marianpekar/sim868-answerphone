using NAudio.Wave;

namespace Answerphone
{
    public class AudioRecorder : IDisposable
    {
        private readonly WaveInEvent micIn = new();
        private readonly WaveFileWriter micWriter;

        private readonly WasapiLoopbackCapture loopbackCapture = new();
        private readonly WaveFileWriter loopbackWriter;

        public AudioRecorder(string micOutput, string loopbackOutput)
        {
            // For mixing, both files must have the same wave format
            micIn.WaveFormat = loopbackCapture.WaveFormat;

            micWriter = new(micOutput, micIn.WaveFormat);
            loopbackWriter = new(loopbackOutput, loopbackCapture.WaveFormat);

            micIn.DataAvailable += (s, args) => 
            { 
                micWriter.Write(args.Buffer, 0, args.BytesRecorded); 
            };

            loopbackCapture.DataAvailable += (s, args) => 
            { 
                loopbackWriter.Write(args.Buffer, 0, args.BytesRecorded); 
            };
        }

        public void Record()
        {
            micIn.StartRecording();
            loopbackCapture.StartRecording();
        }
        
        public void Stop()
        {
            micIn.StopRecording();
            loopbackCapture.StopRecording();
        }


        public void Dispose()
        {
            micIn.Dispose();
            micWriter.Dispose();
            loopbackCapture.Dispose();
            loopbackWriter.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
