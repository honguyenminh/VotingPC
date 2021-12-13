namespace VotingPCNew.Scanner
{
    public class ScannerSignalTable
    {
        public char Acknowledgement { get; init; }
        public ReceiveSignalTable Receive { get; init; }
        public SendSignalTable Send { get; init; }
    }
    public class ReceiveSignalTable
    {
        public char FingerFound { get; init; }
    }
    public class SendSignalTable
    {
        public char StartScanning { get; init; }
        public char AcknowledgedFinger { get; init; }
        public char Close { get; init; }
    }
}
