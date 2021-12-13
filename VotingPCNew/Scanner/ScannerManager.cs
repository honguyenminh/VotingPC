using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VotingPCNew.Scanner
{
    public class ScannerManager
    {
        public int BaudRate { get; set; } = 115200;
        private readonly ScannerSignalTable _signalTable;

        public ScannerManager(ScannerSignalTable signalTable)
        {
            this._signalTable = signalTable;
        }

        /// <summary>
        /// Get <see cref="SerialPort"/> object linked to the Scanner
        /// </summary>
        /// <returns><see cref="SerialPort"/> object if found device, <see langword="null"/> otherwise</returns>
        private async Task<SerialPort> GetArduinoSerialPort(int maxRetry)
        {
            string[] comPorts = SerialPort.GetPortNames();
            // Send ACK signal to each COM port for given retry count
            foreach (string comPort in comPorts)
            {
                SerialPort serialPort = new(comPort, BaudRate);
                serialPort.Open();
                // Pre-defined character, used by scanner to acknowledge the PC 
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
                // If received ACK signal back, return the serial port
                if (response == _signalTable.Acknowledgement)
                {
                    return serialPort;
                }
                serialPort.Dispose();
            }
            return null;
        }
    }
}
