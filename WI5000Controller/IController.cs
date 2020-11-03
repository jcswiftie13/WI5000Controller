using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnumsNET;

using Newtonsoft.Json.Linq;

using Corex.Log;

namespace Corex.Controller
{
    public interface IController
    {
        Enums.State State { get; }

        // 基本Device三大函數.
        bool Startup();
        bool Shutdown();
        bool Reset(); 
    }

    public static class ControllerLog
    {
        public static bool ControllerStartupWithLog(IController controller, string tag)
        {
            bool success = false;
            if (controller != null)
                success = controller.Startup();

            Logger.I(tag, string.Format("Controller:{0}, Startup {1}.", getControllerName(controller), (success ? "Success" : "Fail")));
            return success;
        }

        public static bool ControllerShutdownWithLog(IController controller, string tag)
        {
            bool success = false;
            if (controller != null)
                success = controller.Shutdown();

            Logger.I(tag, string.Format("Controller:{0}, Shutdown {1}.", getControllerName(controller), (success ? "Success" : "Fail")));
            return success;
        }

        private static string getControllerName(IController controller)
        {
            if (controller == null) return "null";
            return controller.GetType().ToString().Split('.').Last();
        }
    }
}
