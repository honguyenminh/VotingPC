using System.IO.Ports;
using System.Threading;

namespace VotingPC
{
    public class ArduinoInteract
    {
        /// <summary>
        /// Get SerialPort object linked to the Arduino. Return null if no Arduino found.
        /// </summary>
        /// <returns>SerialPort object linked to the Arduino</returns>
        public static SerialPort GetArduinoCOMPort()
        {
            string[] COMPorts = SerialPort.GetPortNames();
            foreach (string COMPort in COMPorts)
            {
                SerialPort serialPort = new(COMPort, 115200);
                serialPort.Open();
                serialPort.Write("V"); // Pre-defined character, used by the Arduino to acknowledge the PC
                // Send signle to each COMPort in PC for 5 seconds, if nothing received, dispose serialPort
                for (int i = 0; i < 10; i++)
                {
                    if (serialPort.BytesToRead == 0)
                    {
                        Thread.Sleep(500);
                        serialPort.Write("V");
                    }
                    else break;
                }
                if (serialPort.BytesToRead == 0)
                {
                    serialPort.Dispose();
                    continue;
                }
                int response = serialPort.ReadByte();
                if (response == 'K')
                {
                    return serialPort;
                }
                serialPort.Dispose();
            }
            return null;
        }
    }
}
