using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Corex.Controller
{
    public interface IController
    {
        // 基本Device三大函數.
        bool Startup(string IP, int port); //Initialize -> Connect
        void Shutdown(); //Disconnect
        bool Reset(); //Shutdown() -> Startup()
    }

    public static class ControllerLog
    {
        public static bool ControllerStartupWithLog(IController controller, string tag)
        {
            bool success = false;
            if (controller != null)
                success = controller.Startup();
            return success;
        }

        public static bool ControllerShutdownWithLog(IController controller, string tag)
        {
            bool success = false;
            if (controller != null)
                success = controller.Shutdown();
            return success;
        }

        private static string getControllerName(IController controller)
        {
            if (controller == null) return "null";
            return controller.GetType().ToString().Split('.').Last();
        }
    }
}
