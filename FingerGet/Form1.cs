using System.IO.Ports;

namespace FingerGet;

public partial class MainForm : Form
{
    private static SerialPort s_serial;
    private static bool s_isWaitingForSerial;
    public MainForm()
    {
        InitializeComponent();
        submitButton.Enabled = false;
        Init();
    }
    private async void Init()
    {
        await Task.Delay(200);
        while (s_serial == null) await Task.Run(GetArduinoCOMPort);
        status.Text = "Đã tìm thấy Arduino tại " + s_serial.PortName;
        status.ForeColor = Color.Green;
        submitButton.Enabled = true;
        s_isWaitingForSerial = true;
        WaitForSerial();
        CheckArduinoStatus();
    }
    private async void CheckArduinoStatus()
    {
        while (true)
        {
            await Task.Delay(800);
            if (!s_serial.IsOpen)
            {
                await Disconnected();
                Init();
                break;
            }
        }
    }
    private async Task Disconnected()
    {
        await Task.Delay(10);
        status.Text = "Đã ngắt kết nối Arduino, vui lòng kết nối lại";
        status.ForeColor = Color.Red;
        submitButton.Enabled = false;
        s_isWaitingForSerial = false;
        s_serial.Dispose();
        s_serial = null;
    }

    private async void WaitForSerial()
    {
        await Task.Delay(100);
        while (s_isWaitingForSerial && s_serial.IsOpen)
        {
            try
            {
                string input = await Task.Run(s_serial.ReadLine);
                if (input == "Internal done")
                {
                    submitButton.Enabled = true;
                    continue;
                }
                else if (input == "Internal found")
                {
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
        try
        {
            s_serial.Write("S");
            submitButton.Enabled = false;
        }
        catch { _ = Disconnected(); }
    }
    private static void GetArduinoCOMPort()
    {
        string[] COMPorts = SerialPort.GetPortNames();

        foreach (string COMPort in COMPorts)
        {
            SerialPort serialPort = new(COMPort, 115200)
            {
                NewLine = "\r\n",
                Encoding = System.Text.Encoding.UTF8
            };
            try
            {
                serialPort.Open();
                serialPort.Write("F");
                for (int i = 0; i < 10; i++)
                {
                    Thread.Sleep(100);
                    if (serialPort.BytesToRead == 0)
                    {
                        serialPort.Write("F");
                    }
                    else break;
                }
                if (serialPort.BytesToRead == 0)
                {
                    serialPort.Dispose();
                    continue;
                }
                string response = serialPort.ReadLine();
                if (response == "Internal found")
                {
                    s_serial = serialPort;
                    s_serial.DiscardInBuffer();
                    return;
                }
            }
            catch (Exception) { }

            serialPort.Dispose();
        }
    }

    private void Form_FormClosing(object sender, FormClosingEventArgs e)
    {
        s_isWaitingForSerial = false;
        if (s_serial == null) return;
        s_serial.Write("C"); // App closed signal
        if (s_serial.IsOpen) s_serial.Close();
        s_serial.Dispose();
    }

    private void ClearButton_Click(object sender, EventArgs e)
    {
        serialOutput.Text = "";
    }
}