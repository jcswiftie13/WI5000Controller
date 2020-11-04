using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Windows.Forms;

using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.CV;

using AxWILib;

using Corex.UI;
using Corex.UI.Widget;
using Corex.Log;
using Corex.Log.ANLog.FileLog;
using Corex.Controller.Camera;
using System.Windows.Forms.VisualStyles;
using WILib;

namespace WI5000_Controller
{
    class WI5000Controller : ICameraController
    {
        private AxWILib.AxWI WI = new AxWILib.AxWI();
        private string tag = "WI-5000 Controller";
        private string baseFolder = @"C:\work";
        private double[] size = new double[2];
        private static int imgCount = 0;

        enum LogType
        {
            Info,
            Error,
            Warning,
        }

        private void addLog(string message, LogType logType)
        {
            if (logType == LogType.Info)
            {
                Logger.I(tag, message);
            }
            else if (logType == LogType.Warning)
            {
                Logger.W(tag, message);
            }
            else if (logType == LogType.Error)
            {
                Logger.E(tag, message);
            }
            else
            {
                Logger.E(tag, "The loggerResult's logType is incorrect.");
            }
            Console.WriteLine(message);
        }

        private bool StateProcessor(int state, string func)
        {
            if (state == 0)
            {
                addLog($"Succeeded in {func}", LogType.Info);
                return true;
            }
            else if (state == 1100)
            {
                addLog($"Communication error in {func}, code: {state}", LogType.Error);
                MessageBox.Show($"Communication error in {func}, code: {state}");
                return false;
            }
            else
            {
                addLog($"Execution error in {func}, code: {state}", LogType.Error);
                MessageBox.Show($"Execution error in {func}, code: {state}");
                return false;
            }
        }

        private void axWI1_OnImageLogDataReceived(object sender, AxWILib._DWIEvents_OnImageLogDataReceivedEvent e)
        {
            if (StateProcessor(e.state, "Image received"))
            {
                imgCount += 1;
            }
        }

        public WI5000Controller(string ip = "192.168.10.20", int port = 8500) //設備預設的ip和port
        {
            int state = WI.Initialize();
            StateProcessor(state, "Initialize()");
            WI.Address = ip;
            WI.Port = port;
            
        }

        public enum State : int
        {
            Off, Ready, Running, Error, Pause
        }

        public bool Startup()
        {
            //make connection
            if (StateProcessor(WI.Connect(), "Startup()"))
            {
                //切換到運轉模式
                string response = null;
                if (StateProcessor(WI.ExecuteCommand("R0", ref response), "Operation mode"))
                {
                    if (GetTriggerMode())
                    {
                        SoftwareTrigger();
                    }
                    if (StateProcessor(WI.StartImageLog(baseFolder), "StartImageLog()"))
                    {
                        Bitmap src = null;
                        src = new Bitmap(baseFolder + $"{imgCount}_OUTPUT_IMG_HEIGHT_OK.bmp"); //文件名格式: 連號_指定字串_IMG_圖像類別*_綜合判定.bmp
                        size[0] = src.Height;
                        size[1] = src.Width;

                        return true;
                    }
                }
            }
            return false;
        }

        public bool Shutdown()
        {
            WI.Disconnect();
            bool connected = WI.Connected;
            addLog($"Device disconnected: {!connected}", LogType.Info);
            return !connected;
        }

        public bool Reset()
        {
            bool reset = Shutdown() && Startup();
            addLog($"reset: {reset}", LogType.Info);
            return reset;
        }

        public bool IsValid()
        {
            bool connected = WI.Connected;
            addLog($"Check connection, connected: {connected}", LogType.Info);
            return connected;
        }

        public string GetProperty()
        {
            string response = null;
            if (StateProcessor(WI.ExecuteCommand("PR\r", ref response), "GetProperty()"))
            {
                string[] analyzed = response.Split(',');
                response = analyzed[2];
                addLog($"Device property: {response}", LogType.Info);
                return response;
            }
            return null;
        }

        public string GetPropertyWithoutDeviceInfo()
        {
            return GetProperty();
        }

        public void SetProperty(string property, bool update)
        {
            string response = null;
            if (StateProcessor(WI.ExecuteCommand("PW," + $"{property.Split()[0]},{property.Split()[1]}\r", ref response), "SetProperty()"))
            {
                addLog($"Property set to {property.Split()[1]}", LogType.Info);
            }
        }

        public void SetTriggerFunc(DTriggerImage triggerImage)
        {

        }

        //假設內部和外部觸發兩兩一組編號, 內部觸發是單數, 外部觸發雙數, 而triggermode true為外部觸發
        public bool GetTriggerMode()
        {
            string response = null;
            if (StateProcessor(WI.ExecuteCommand("PR\r", ref response), "GetTriggerMode()"))
            {
                string[] analyzed = response.Split(',');
                response = analyzed[2];
                if (Convert.ToInt32(response) % 2 == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public bool SetTrigger(bool triggerMode)
        {
            if (triggerMode == true)
            {
                string response = null;
                if (StateProcessor(WI.ExecuteCommand("PR\r", ref response), "GetTrigger()"))
                {
                    string[] analyzed = response.Split(',');
                    response = analyzed[2];
                    if (Convert.ToInt32(response) % 2 == 0)
                    {
                        addLog("Trigger mode was already in true", LogType.Info);
                        return true;
                    }
                    else
                    {
                        if (StateProcessor(WI.ExecuteCommand("PW," + $"1,{Convert.ToInt32(response) + 1}\r", ref response), "SetTrigger()"))
                        {
                            addLog("Trigger mode set to true", LogType.Info);
                            return true;
                        }
                    }
                }
                return false;
            }
            else
            {
                string response = null;
                if (StateProcessor(WI.ExecuteCommand("PR\r", ref response), "GetTrigger()"))
                {
                    string[] analyzed = response.Split(',');
                    response = analyzed[2];
                    if (Convert.ToInt32(response) % 2 == 0)
                    {
                        addLog("Trigger mode was already in false", LogType.Info);
                        return true;
                    }
                    else
                    {
                        if (StateProcessor(WI.ExecuteCommand("PW," + $"1,{Convert.ToInt32(response) - 1}\r", ref response), "SetTrigger()"))
                        {
                            addLog("Trigger mode set to false", LogType.Info);
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public bool IsGrabbing()
        {
            if (WI.ImageLogStarted)
            {
                return true;
            }
            return false;
        }

        public void StartGrab()
        {
            if (StateProcessor(WI.StartImageLog(baseFolder), "StartGrab()"))
            {
                addLog("Start grabbing", LogType.Info);
            }
        }

        public void StopGrab()
        {
            if (StateProcessor(WI.StopImageLog(), "StopGrab()"))
            {
                addLog("Stop grabbing", LogType.Info);
            }
        }

        public double[] GetFrameSize()
        {
            return size;
        }

        public double GetFrameHeight()
        {
            return size[0];
        }

        public double GetFrameWidth()
        {
            return size[1];
        }

        public Emgu.CV.Image<Bgra, Byte> GetImage()
        {
            Bitmap src = null;
            src = new Bitmap(baseFolder + $"{imgCount}_OUTPUT_IMG_HEIGHT_NG.bmp"); //文件名格式: 連號_指定字串_IMG_圖像類別*_綜合判定.bmp
            Emgu.CV.Image<Bgra, Byte> img = src.ToImage<Bgra, Byte>();
            addLog("Succeeded in GetImage()", LogType.Info);
            return img;
        }

        public void SoftwareTrigger()
        {
            string response = null;
            StateProcessor(WI.ExecuteCommand("TG\r", ref response), "SoftwareTrigger()");
        }
    }
}
