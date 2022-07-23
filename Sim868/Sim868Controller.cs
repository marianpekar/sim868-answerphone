using System.IO.Ports;

namespace Sim868
{
    public class Sim868Controller : IDisposable
    {
        private readonly SerialPort serialPort;
        public bool IsReading { get; private set; }

        public Action<string>? OnRead;
        public Action<string>? OnTextMessageRead;
        public Action? OnRing;
        public Action? OnAnswer;
        public Action? OnHangUp;
        public Action? OnOk;

        // Calling
        private const string InquiryStateCode = "AT";
        private const string AnswerCode = "ATA";
        private const string HangCode = "ATH";
        private const string DialCode = "ATD<phone_number>;";

        // Texting
        private const string SetMessageFormatCode = "AT+CMGF=<format_code>";
        private const string SendMessageCode = "AT+CMGS=\"<phone_number>\"";
        private readonly byte[] EndMark = new byte[] { 0x1A };
        private const string ReadMessageCode = "AT+CMGR=<slot>";
        private const string DeleteMessageCode = "AT+CMGD=<slot>";
        private const string ReadAllMessagesCode = "AT+CMGL=\"ALL\"";

        public enum MessageFormat
        {
            PDUMode = 0,
            TextMode = 1
        }

        public Sim868Controller(string comPortName)
        {
            serialPort = new()
            {
                PortName = comPortName,
                BaudRate = 115200,
                DataBits = 8,
                StopBits = StopBits.One,
                Parity = Parity.None,
            };

            serialPort.Open();
        }

        public Task BeginRead()
        {
            return Task.Run(Read);
        }

        public void CancelRead()
        {
            IsReading = false;
        }

        private void Read()
        {
            IsReading = true;
            while (IsReading)
            {
                try
                {
                    string message = serialPort.ReadLine();
                    OnRead?.Invoke(message);
                    switch (message)
                    {
                        case "RING\r":
                            OnRing?.Invoke();
                            break;
                        case "OK\r":
                            OnOk?.Invoke();
                            break;
                        case "ATA\r":
                            OnAnswer?.Invoke();
                            break;
                        case "NO CARRIER\r":
                            OnHangUp?.Invoke();
                            break;
                        default:
                            break;
                    }

                    if(message.StartsWith("+CMGL:") || message.StartsWith("+CMGR:"))
                    {
                        string header = message;
                        message = serialPort.ReadLine();
                        OnTextMessageRead?.Invoke(header + Environment.NewLine + message);
                    }
                }
                catch (TimeoutException) { }
            }
        }

        public void InquiryState()
        {
            serialPort.WriteLine(InquiryStateCode);
        }

        public void Answer()
        {
            serialPort.WriteLine(AnswerCode);
        }

        public void HangUp()
        {
            serialPort.WriteLine(HangCode);
        }

        public void Dial(int number)
        {
            serialPort.WriteLine(DialCode.Replace("<phone_number>", number.ToString()));
        }

        public void SetMessageFormat(MessageFormat format)
        {
            serialPort.WriteLine(SetMessageFormatCode.Replace("<format_code>", ((int)format).ToString()));
        }

        public void SendMessage(int number, string message)
        {
            serialPort.WriteLine(SendMessageCode.Replace("<phone_number>", number.ToString()));
            serialPort.WriteLine(message);
            serialPort.Write(EndMark, 0, 1);
        }

        public void ReadMessage(int slot)
        {
            serialPort.WriteLine(ReadMessageCode.Replace("<slot>", slot.ToString()));
        }

        public void ReadAllMessages()
        {
            serialPort.WriteLine(ReadAllMessagesCode);
        }

        public void DeleteMessage(int slot)
        {
            serialPort.WriteLine(DeleteMessageCode.Replace("<slot>", slot.ToString()));
        }

        public void Dispose()
        {
            serialPort.Close();
            GC.SuppressFinalize(this);
        }
    }
}
