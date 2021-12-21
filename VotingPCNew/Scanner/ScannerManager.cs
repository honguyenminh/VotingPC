using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VotingPCNew.Scanner;

public class ScannerManager : IDisposable
{
    public int BaudRate { get; internal set; }

    // Pre-defined characters, used by scanner to communicate 
    private readonly ScannerSignalTable _signalTable;
    private SerialPort _port;

    public ScannerManager(ScannerSignalTable signalTable)
    {
        _signalTable = signalTable;
    }

    /// <summary>
    /// Get <see cref="SerialPort"/> object linked to the Scanner
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

    public void StartScan()
    {
        _port.Write(_signalTable.Send.StartScan.ToString());
    }

    private bool _disposed;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (_disposed) return;
        _port.Dispose();
        _disposed = true;
    }

    ~ScannerManager()
    {
        if (_disposed) return;
        _port.Dispose();
    }
}