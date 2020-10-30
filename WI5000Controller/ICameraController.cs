using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Emgu.CV;
using Emgu.CV.Structure;

using Corex.UI;

namespace Corex.Controller.Camera
{
    public delegate void DTriggerImage(Image<Bgra, byte> image);

    public interface ICameraController : IController
    {
        double[] GetFrameSize(); //ExecuteCommand("BC") -> Emgu.CV
        double GetFrameHeight(); //ExecuteCommand("BC") -> Emgu.CV
        double GetFrameWidth(); //ExecuteCommand("BC") -> Emgu.CV
        double GetFPS();
        bool IsValid(); //Connected

        Image<Bgra, byte> GetImage(); //ExecuteCommand("BC") -> Emgu.CV
        Image<Bgra, byte> CutTriggerBuffer();
        List<Image<Bgra, byte>> GetTriggerBuffer();

        void ShowPropertyDialog(System.Windows.Forms.Form owner);

        string GetProperty();
        string GetPropertyWithoutDeviceInfo();
        void SetProperty(string property, bool update);

        void SetTriggerFunc(DTriggerImage triggerImage);
        bool GetTriggerMode();
        bool SetTrigger(bool triggerMode);

        bool IsGrabbing(); //ExecuteCommand("RM") -> if return value == 0 && command response == 1 -> true
        void StartGrab(); //ExecuteCommand("TE,1")
        void StopGrab(); //ExecuteCommand("TE,0")

        void SoftwareTrigger();
    }

    // ISSUE: Duplicate with Corex.Vision.Util.ImageUtility.Dispose and Mirror.
    class Utility
    {
        public static void Dispose<TColor, TDepth>(ref Image<TColor, TDepth> image)
            where TColor : struct, IColor where TDepth : new()
        {
            if (image != null)
            {
                image.Dispose();
                image = null;
            }
        }

        public static void Mirror<TColor, TDepth>(ref Image<TColor, TDepth> image, bool h, bool v)
            where TColor : struct, IColor
            where TDepth : new()
        {
            if (h)
            {
                image._Flip(Emgu.CV.CvEnum.FlipType.Horizontal);
            }

            if (v)
            {
                image._Flip(Emgu.CV.CvEnum.FlipType.Vertical);
            }
        }

        public static void Preprocess<TColor, TDepth>(ref Image<TColor, TDepth> image, bool h, bool v, int rotation)
            where TColor : struct, IColor
            where TDepth : new()
        {
            Utility.Mirror(ref image, h, v);

            if (rotation != 0)
            {
                Image<TColor, TDepth> tmp = image.Rotate(rotation, new System.Drawing.PointF(0, 0), Emgu.CV.CvEnum.Inter.Linear, default(TColor), false);
                Utility.Dispose(ref image);
                image = tmp;
            }
        }

        private const string PROPERTY = "property";

        public static void SetProperty(ControllerConfig config, string property)
        {
            config.Properties.Set(PROPERTY, property);
        }

        public static string GetProperty(ControllerConfig config)
        {
            return config.Properties.Get(PROPERTY, "");
        }

        public static bool CameraSetProperty(ControllerConfig config, string propStr, bool setupOk)
        {
            if (setupOk)
            {
                // Save the setting
                SetProperty(config, propStr);
                System.Windows.Forms.MessageBox.Show(Lang.GetText("@M_camera_setup_ok"));
                return true;
            }
            else System.Windows.Forms.MessageBox.Show(Lang.GetText("@M_camera_setup_error"));
            return false;
        }

        public static bool IsColorFormat(string pixelFormat)
        {
            if (!pixelFormat.Contains("Mono") || pixelFormat.Contains("Bayer"))
                return true;
            else
                return false;
        }
    }
}
