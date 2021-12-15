namespace VotingPCNew.Scanner
{
    public readonly struct ScannerSignalTable
    {
        public char Acknowledgement { get; init; }
        public ReceiveSignalTable Receive { get; init; }
        public SendSignalTable Send { get; init; }
    }
    public readonly struct ReceiveSignalTable
    {
        public char FingerFound { get; init; }
    }
    public readonly struct SendSignalTable
    {
        public char StartScanning { get; init; }
        public char AcknowledgedFinger { get; init; }
        public char Close { get; init; }
    }
}
