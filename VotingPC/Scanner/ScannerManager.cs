using System.IO;
using System.IO.Ports;

namespace VotingPC.Scanner;

public class ScannerManager : IDisposable
{
    // Pre-defined characters, used by scanner to communicate 
    private readonly ScannerSignalTable _signalTable;
    private SerialPort _port;
    private volatile bool _isListening;

    public ScannerManager(ScannerSignalTable signalTable)
    {
        _signalTable = signalTable;
    }

    public int BaudRate { get; private set; }

    /// <summary>
    ///     Get <see cref="SerialPort" /> object linked to the Scanner
    /// </summary>
    /// <param name="baudRate">Baud rate to use to connect</param>
    /// <param name="maxRetry">Times to retry connection before timing out</param>
    /// <exception cref="DriveNotFoundException">Thrown when cannot find scanner</exception>
    public async Task Init(int baudRate, int maxRetry)
    {
        BaudRate = baudRate;
        string[] comPorts = SerialPort.GetPortNames();
        // Send ACK signal to each COM port for given retry count
        foreach (string comPort in comPorts)
        {
            SerialPort serialPort = new(comPort, BaudRate);
            serialPort.Open();
            serialPort.Write(_signalTable.Acknowledgement.ToString());
            for (int i = 0; i < maxRetry; i++)
            {
                if (serialPort.BytesToRead == 0)
                {
                    await Task.Delay(500);
                    serialPort.Write(_signalTable.Acknowledgement.ToString());
                }
                else break;
            }

            if (serialPort.BytesToRead == 0)
            {
                serialPort.Dispose();
                continue;
            }

            int response = serialPort.ReadByte();
            // If received ACK signal back, set the serial port
            if (response == _signalTable.Acknowledgement)
            {
                _port = serialPort;
                _port.DiscardInBuffer();
                return;
            }

            serialPort.Dispose();
        }

        throw new DriveNotFoundException("Cannot find Scanner with given signal table");
    }

    /// <summary>
    ///     Start scanning for finger signal.
    ///     DO NOT AWAIT IF THERE'S STILL WORK AFTER THIS GETS CALLED.
    ///     WILL BLOCK UNTIL SIGNAL FOUND
    /// </summary>
    /// <param name="onValidFinger">Action to run on valid finger signal found</param>
    /// <param name="onInvalidFinger">ASYNC Action to run on invalid finger signal</param>
    /// <param name="stopScanAfterInvalid">Stop scanning after invalid signal is found.</param>
    /// <param name="deleteAfterFound">Delete fingerprint after valid finger is found</param>
    /// <exception cref="InvalidOperationException">Throw if manager is not initialized</exception>
    public async Task StartScan(Action onValidFinger, Func<Task> onInvalidFinger = null, 
        bool stopScanAfterInvalid = true, bool deleteAfterFound = false)
    {
        if (_port is null) throw new InvalidOperationException("No port is opened yet");
        _port.Write(_signalTable.Send.StartScan.ToString());
        _isListening = true;
        while (_isListening && _port.IsOpen)
        {
            while (_isListening && _port.IsOpen && _port.BytesToRead < 1)
            {
                await Task.Delay(100);
            }

            if (!_port.IsOpen) return;
            int incomingSignal = _port.ReadByte();
            if (incomingSignal == _signalTable.Receive.FingerFound)
            {
                onValidFinger?.Invoke();
                if (deleteAfterFound)
                {
                    _port.Write(_signalTable.Send.DeleteFinger.ToString());
                }
                _isListening = false;
                return;
            }

            if (incomingSignal != _signalTable.Receive.InvalidFinger) continue;
            if (onInvalidFinger != null) await onInvalidFinger();
            if (stopScanAfterInvalid)
            {
                _isListening = false;
                return;
            }
            
            _port.Write(_signalTable.Send.StartScan.ToString());
        }
    }

    private bool _disposed;
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (_disposed) return;
        _disposed = true;
        if (_port is null) return;
        _port.Write(_signalTable.Send.Close.ToString());
        _port.Dispose();
    }

    ~ScannerManager()
    {
        if (_disposed) return;
        Dispose();
    }
}