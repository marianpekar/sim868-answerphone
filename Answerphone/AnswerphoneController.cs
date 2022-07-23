using Sim868;

namespace Answerphone
{
    public class AnswerphoneController : IDisposable
    {
        private readonly AudioPlayer player;
        private readonly Sim868Controller sim868;
        
        public TimeSpan MaxCallLength { get; set; } = TimeSpan.FromMinutes(1);
        public bool IsRinging { get; private set; }
        public bool IsCalling { get; private set; }

        private const int GreetingsAudioFilesGroupIndex = 0;
        private const int FollowUpAudioFilesGroupIndex = 1;

        private const string MicOutputFilename = "temp_mic.wav";
        private const string LoopbackOutputFilename = "temp_loopback.wav";

        public AnswerphoneController(string comPortName, string greetingsAudioFilesDirectory, string followUpAudioFilesDirectory)
        {
            player = new(greetingsAudioFilesDirectory, followUpAudioFilesDirectory);
            sim868 = new(comPortName);

            sim868.OnRing += HandleRing;
            sim868.OnAnswer += HandleAnswer;
            sim868.OnHangUp += HandleHangUp;
            sim868.OnRead += Console.WriteLine;

            sim868.InquiryState();
        }

        private void HandleRing()
        {
            if (IsRinging || IsCalling)
                return;

            IsRinging = true;

            Console.WriteLine("Phone is ringing, answer in 3 seconds.");
            Thread.Sleep(TimeSpan.FromSeconds(3));

            sim868.Answer();
            IsRinging = false;          
        }

        private void HandleAnswer()
        {            
            IsCalling = true;

            player.MoveToRandomTrack(GreetingsAudioFilesGroupIndex);
            player.MoveToRandomTrack(FollowUpAudioFilesGroupIndex);

            Task.Run(() =>
            {
                RunMaxCallLengthGuard(DateTime.Now);
            });

            Console.WriteLine("Call started");

            Task.Run(() =>
            {    
                AudioRecorder recorder = new(MicOutputFilename, LoopbackOutputFilename);
                recorder.Record();

                Thread.Sleep(TimeSpan.FromSeconds(1));

                Console.WriteLine("Greeting...");
                player.PlayNextTrack(GreetingsAudioFilesGroupIndex);
                while (IsCalling)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(7));
                    Console.WriteLine("Speaking...");
                    player.PlayNextTrack(FollowUpAudioFilesGroupIndex);            
                }

                recorder.Stop();
                recorder.Dispose();

                string mixedOutputFilename = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".wav";
                AudioMixer.MixTwo(MicOutputFilename, LoopbackOutputFilename, mixedOutputFilename);
                Console.WriteLine($"Recording {mixedOutputFilename} saved");
            });
        }

        private void RunMaxCallLengthGuard(DateTime callStartTime)
        {
            while(DateTime.Now < callStartTime + MaxCallLength && IsCalling)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }

            IsCalling = false;
            sim868.HangUp();
        }

        private void HandleHangUp()
        {
            Console.WriteLine("Call ended");
            IsCalling = false;
        }

        public void Run()
        {
            sim868.BeginRead().Wait();
        }

        public void Dispose()
        {
            player.Dispose();
            sim868.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
