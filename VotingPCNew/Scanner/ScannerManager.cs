﻿using System;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;

namespace VotingPCNew.Scanner;

public class ScannerManager : IDisposable
{
    public int BaudRate { get; internal set; }

    // Pre-defined characters, used by scanner to communicate 
    private readonly ScannerSignalTable _signalTable;
    private SerialPort _port;
    private volatile bool _isListening;

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

    public async Task StartScan(Action onValidFinger)
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
            if (_port.IsOpen && _port.ReadByte() == _signalTable.Receive.FingerFound)
            {
                _port.Write(_signalTable.Send.AcknowledgedFinger.ToString());
                _isListening = false;
                onValidFinger();
                break;
            }
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