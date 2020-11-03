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

namespace WI5000_Controller
{
    class WI5000Controller : ICameraController
    {
        private AxWILib.AxWI WI = new AxWILib.AxWI();
        private string tag = "WI-5000 Controller";
        private string baseFolder = @"C:\work";
        private double[] size = new double[2];

        private void initLog()
        {
            // Logger
            FileProxy fileProxy = new FileProxy(@".\logs\WI-5000.log", 50000, 100);

            if (Corex.Env.DEBUG)
            {
                Logger.Init(true, fileProxy);
            }
            else
            {
                Logger.Init(false, fileProxy);
            }
            Logger.EnableCollapsing(15, 1.0);

        }

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
        }
        public WI5000Controller()
        {
            initLog();
            int state = WI.Initialize();
            if (state == 0)
            {
                addLog("Initialized", LogType.Info);
                MessageBox.Show("Initialized");
            }
            else
            {
                addLog("Initialization failed", LogType.Info);
                MessageBox.Show("Initialization failed");
            } 
        }

        public bool Startup()
        {
            //make connection
            WI.Address = "192.168.10.20";
            WI.Port = 8500;
            int state = WI.Connect();

            if (state == 0)
            {
                string response = null;
                state = WI.ExecuteCommand("R0", ref response);

                if (state == 0)
                {
                    //get image size
                    Bitmap src = null;
                    state = WI.StartImageLog(baseFolder);
                    src = new Bitmap(baseFolder + "圖像輸出時間點的年月日_時分秒_測量次數_IMG_圖像類別_綜合判定.bmp");
                    size[0] = src.Height;
                    size[1] = src.Width;
                    if (state == 0)
                    {
                        addLog("Startup and image capture succeeded", LogType.Info);
                        return true;
                    }
                    else
                    {
                        addLog("Image caption error: " + state, LogType.Error);
                        MessageBox.Show("Image caption error: " + state);
                        return false;
                    }
                }
                else
                {
                    addLog("Not in operation mode", LogType.Error);
                    MessageBox.Show("Not in operation mode");
                    return false;
                }
            }
            else
            {
                addLog("Connection Error:" + state, LogType.Error);
                MessageBox.Show("Connection error: " + state);
                return false;
            }
        }

        public bool Shutdown()
        {
            WI.Disconnect();
            addLog($"Device disconnected: {!WI.Connected}", LogType.Info);
            return !WI.Connected;
        }

        public bool Reset()
        {
            addLog($"reset: {Shutdown() && Startup()}", LogType.Info);
            return Shutdown() && Startup();
        }

        public bool IsValid()
        {
            addLog($"Check connection, connected: {WI.Connected}", LogType.Info);
            return WI.Connected;
        }

        public string GetProperty()
        {
            string response = null;
            int state = WI.ExecuteCommand("PR\r", ref response);
            if (state == 0)
            {
                string[] analyzed = response.Split(',');
                response = analyzed[2];
                addLog($"Device property: {response}", LogType.Info);
                return response;
            }
            else if (state == 1100)
            {
                addLog($"Communication error in getting property", LogType.Error);
                MessageBox.Show("Communication error");
                return response;
            }
            else
            {
                addLog($"Execution error in getting property", LogType.Error);
                MessageBox.Show("Execution error");
                return response;
            }
        }

        public string GetPropertyWithoutDeviceInfo()
        {
            string response = null;
            int state = WI.ExecuteCommand("PR\r", ref response);
            if (state == 0)
            {
                string[] analyzed = response.Split(',');
                response = analyzed[2];
                addLog($"Device property: {response}", LogType.Info);
                return response;
            }
            else if (state == 1100)
            {
                addLog($"Communication error in getting property", LogType.Error);
                MessageBox.Show("Communication error");
                return response;
            }
            else
            {
                addLog($"Execution error in getting property", LogType.Error);
                MessageBox.Show("Execution error");
                return response;
            }
        }

        public void SetProperty(string property, bool update)
        {
            string response = null;
            int state = WI.ExecuteCommand("PW," + $"{property.Split()[0]},{property.Split()[1]}\r", ref response);
            if (state == 0)
            {
                addLog("Set property succeeded", LogType.Info);
            }
            else if (state == 1100)
            {
                addLog("Communication error in setting property", LogType.Error);
                MessageBox.Show("Communication error");
            }
            else
            {
                addLog("Execution error in setting property", LogType.Error);
                MessageBox.Show("Execution error");
            }
        }

        public void SetTriggerFunc(DTriggerImage triggerImage)
        {
        }
        public bool GetTriggerMode()
        {
            string response = null;
            int state = WI.ExecuteCommand("PR\r", ref response);
            if (state == 0)
            {
                string[] analyzed = response.Split(',');
                response = analyzed[2];
                addLog("Get trigger mode succeeded", LogType.Info);
                return true;
            }
            else if (state == 1100)
            {
                addLog("Communication error in getting trigger mode", LogType.Error);
                MessageBox.Show("Communication error");
                return false;
            }
            else
            {
                addLog("Execution error in getting trigger mode", LogType.Error);
                MessageBox.Show("Execution error");
                return false;
            }
        }
        public bool SetTrigger(bool triggerMode)
        {
            //假設內部和外部觸發兩兩一組編號, 內部觸發是單數, 外部觸發雙數, 而triggermode true為外部觸發
            if (triggerMode == true)
            {
                string response = null;
                int state = WI.ExecuteCommand("PR\r", ref response);
                if (state == 0)
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
                        state = WI.ExecuteCommand("PW," + $"1,{Convert.ToInt32(response) + 1}\r", ref response);
                        if (state == 0)
                        {
                            addLog("Trigger mode set to True", LogType.Info);
                            return true;
                        }
                        else if (state == 1100)
                        {
                            addLog("Communication error in setting trigger mode", LogType.Error);
                            MessageBox.Show("Communication error");
                            return false;
                        }
                        else
                        {
                            addLog("Execution error in setting trigger mode", LogType.Error);
                            MessageBox.Show("Execution error");
                            return false;
                        }
                    }
                }
                else if (state == 1100)
                {
                    addLog("Communication error in getting trigger mode", LogType.Error);
                    MessageBox.Show("Communication error");
                    return false;
                }
                else
                {
                    addLog("Execution error in getting trigger mode", LogType.Error);
                    MessageBox.Show("Execution error");
                    return false;
                }
            }
            else
            {
                string response = null;
                int state = WI.ExecuteCommand("PR\r", ref response);
                if (state == 0)
                {
                    string[] analyzed = response.Split(',');
                    response = analyzed[2];
                    if (Convert.ToInt32(response) % 2 != 0)
                    {
                        return true;
                    }
                    else
                    {
                        state = WI.ExecuteCommand("PW," + $"1,{Convert.ToInt32(response) - 1}\r", ref response);
                        if (state == 0)
                        {
                            return true;
                        }
                        else if (state == 1100)
                        {
                            addLog("Communication error in setting trigger mode", LogType.Error);
                            MessageBox.Show("Communication error");
                            return false;
                        }
                        else
                        {
                            addLog("Execution error in setting trigger mode", LogType.Error);
                            MessageBox.Show("Execution error");
                            return false;
                        }
                    }
                }
                else if (state == 1100)
                {
                    addLog("Communication error in getting trigger mode", LogType.Error);
                    MessageBox.Show("Communication error");
                    return false;
                }
                else
                {
                    addLog("Execution error in getting trigger mode", LogType.Error);
                    MessageBox.Show("Execution error");
                    return false;
                }
            }
        }

        public bool IsGrabbing()
        {
            string response = null;
            int state = WI.ExecuteCommand("RM\r", ref response);
            if (state == 0)
            {
                if (response == "1")
                {
                    addLog("Device is grabbing", LogType.Info);
                    return true;
                }
                else
                {
                    addLog("Device is not grabbing", LogType.Info);
                    return false;
                }
            }
            else if (state == 1100)
            {
                addLog("Communication error in \"IsGrabbing\" function", LogType.Error);
                MessageBox.Show("Communication error");
                return false;
            }
            else
            {
                addLog($"Execution error: {state} in \"IsGrabbing\" function", LogType.Error);
                MessageBox.Show($"Execution error: {state}");
                return false;
            }

        }

        public void StartGrab()
        {
            String response = null;
            int state = WI.ExecuteCommand("TE,1\r", ref response);
            if (state == 0)
            {
                addLog("Start grabbing", LogType.Info);
            }
            else if (state == 1100)
            {
                addLog("Communication error in \"StartGrab\" method", LogType.Error);
                MessageBox.Show("Communication error");
            }
            else
            {
                addLog($"Execution error: {state} in \"StartGrab\" method", LogType.Error);
                MessageBox.Show($"Execution error: {state}");
            }
        }

        public void StopGrab()
        {
            String response = null;
            int state = WI.ExecuteCommand("TE,0\r", ref response);
            if (state == 0)
            {
                addLog("Stop grabbing", LogType.Info);
            }
            else if (state == 1100)
            {
                addLog("Communication error in \"StopGrab\" method", LogType.Error);
                MessageBox.Show("Communication error");
            }
            else
            {
                addLog($"Execution error: {state} in \"SttopGrab\" method", LogType.Error);
                MessageBox.Show($"Execution error: {state}");
            }
        }

        public double[] GetFrameSize()
        {
            addLog($"Retrieved frame size: {size}", LogType.Info);
            return size;
        }

        public double GetFrameHeight()
        {
            addLog($"Retrieved frame height: {size[0]}", LogType.Info);
            return size[0];
        }

        public double GetFrameWidth()
        {
            addLog($"Retrieved frame width: {size[1]}", LogType.Info);
            return size[1];
        }

        public Emgu.CV.Image<Bgra, Byte> GetImage()
        {
            Bitmap src = null;
            int state = WI.StartImageLog(baseFolder);
            if (state == 0)
            {
                src = new Bitmap(baseFolder + "圖像輸出時間點的年月日_時分秒_測量次數_IMG_圖像類別_綜合判定.bmp");
                Emgu.CV.Image<Bgra, Byte> img = src.ToImage<Bgra, Byte>();
                addLog("Get image succeeded", LogType.Info);
                return img;
            }
            else if (state == 1100)
            {
                addLog("Communication error in getting image", LogType.Error);
                MessageBox.Show("Communication error");
                return null;
            }
            else
            {
                addLog("Execution error in getting image", LogType.Error);
                MessageBox.Show("Execution error");
                return null;
            }
        }

        public void SoftwareTrigger()
        {
            string response = null;
            int state = WI.ExecuteCommand("TG\r", ref response);
            if (state == 0)
            {
                addLog("Software triggered", LogType.Info);
                MessageBox.Show("Triggered");
            }
            else if (state == 1100)
            {
                addLog("Communication error in software triggering", LogType.Error);
                MessageBox.Show("Communication error");
            }
            else
            {
                addLog("Execution error in software triggering", LogType.Error);
                MessageBox.Show("Execution error");
            }
        }
    }
}
