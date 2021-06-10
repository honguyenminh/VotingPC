using System;
using System.Drawing;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FingerGet
{
    public partial class MainForm : Form
    {
        private static SerialPort serial;
        private static bool isWaitingForSerial;
        public MainForm()
        {
            InitializeComponent();
            submitButton.Enabled = false;
            Init();
        }
        private async void Init()
        {
            while (serial == null) await Task.Run(GetArduinoCOMPort);
            status.Text = "Đã tìm thấy Arduino tại " + serial.PortName;
            status.ForeColor = Color.Green;
            submitButton.Enabled = true;
            isWaitingForSerial = true;
            WaitForSerial();
            CheckArduinoStatus();
        }
        private async void CheckArduinoStatus()
        {
            while (true)
            {
                await Task.Delay(800);
                if (!serial.IsOpen)
                {
                    status.Text = "Đã ngắt kết nối Arduino, vui lòng kết nối lại";
                    status.ForeColor = Color.Red;
                    submitButton.Enabled = false;
                    isWaitingForSerial = false;
                    serial.Close();
                    serial.Dispose();
                    serial = null;
                    Init();
                    break;
                }
            }
        }
        private async void WaitForSerial()
        {
            await Task.Delay(100);
            while (isWaitingForSerial && serial.IsOpen)
            {
                try
                {
                    string input = await Task.Run(serial.ReadLine);
                    if (input == "Internal done")
                    {
                        submitButton.Enabled = true;
                        continue;
                    }
                    serialOutput.AppendText(input + "\r\n");
                }
                catch (Exception)
                {
                    return;
                }
            }
        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {
            serial.Write("S");
            submitButton.Enabled = false;
        }
        private static void GetArduinoCOMPort()
        {
            Thread.Sleep(200);
            string[] COMPorts = SerialPort.GetPortNames();
            foreach (string COMPort in COMPorts)
            {
                SerialPort serialPort = new(COMPort, 115200);
                serialPort.NewLine = "\r\n";
                serialPort.Encoding = System.Text.Encoding.UTF8;
                serialPort.Open();

                serialPort.Write("R");
                for (int i = 0; i < 10; i++)
                {
                    if (serialPort.BytesToRead == 0)
                    {
                        Thread.Sleep(500);
                        serialPort.Write("R");
                    }
                    else break;
                }
                if (serialPort.BytesToRead == 0)
                {
                    serialPort.Dispose();
                    continue;
                }
                string response = serialPort.ReadLine();
                if (response == "​")
                {
                    serial = serialPort;
                    serial.DiscardInBuffer();
                    return;
                }
                serialPort.Dispose();
            }
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            isWaitingForSerial = false;
            if (serial != null)
            {
                if (serial.IsOpen) serial.Close();
                serial.Dispose();
            }
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            serialOutput.Text = "";
        }
    }
}
