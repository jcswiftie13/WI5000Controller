using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Corex.Controller.Camera;
using AxWILib;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using Emgu.CV;

namespace WI5000_Controller
{
    class WI5000Controller : ICameraController
    {
        private AxWILib.AxWI WI = new AxWILib.AxWI();
        private string baseFolder = @"C:\work";
        private double[] size = new double[2];
        public WI5000Controller()
        {
            WI.Initialize();
        }

        public bool Startup(string IP, int port)
        {
            //make connection
            WI.Address = IP;
            WI.Port = port;
            int state = WI.Connect();

            //get image size
            Bitmap src = null;
            WI.StartImageLog(baseFolder);
            src = new Bitmap("");
            size[0] = src.Height;
            size[1] = src.Width;

            if (state == 0) return true;
            else return false;
        }

        public void Shutdown()
        {
            WI.Disconnect();
        }

        public bool Reset()
        {
            WI.Disconnect();
            WI.Connect();
            return WI.Connected;
        }

        public bool IsValid()
        {
            return WI.Connected;
        }

        public bool IsGrabbing()
        {
            string response = null;
            WI.ExecuteCommand("RM", ref response);
            if (response == "1") return true;
            else return false;

        }

        public void StartGrab()
        {
            String response = null;
            WI.ExecuteCommand("TE, 1", ref response);
        }

        public void StopGrab()
        {
            String response = null;
            WI.ExecuteCommand("TE, 0", ref response);
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
            int state = WI.StartImageLog(baseFolder);
            src = new Bitmap("");
            Emgu.CV.Image<Bgra, Byte> img = src.ToImage<Bgra, Byte>();
            return img;
        }
    }
}
