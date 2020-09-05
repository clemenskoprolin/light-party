using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Q42.HueApi;

namespace LightParty.Connection
{
    /// <summary>
    /// This class contains all the variables that store information about the bridge.
    /// </summary>
    class BridgeInformation
    {
        public static bool demoMode = false; //Determines if the demo mode is on or not.
        // In the demo mode the application doen't need a real bridge or complatitable ligths, because it it simulates them.
        // This mode should only be used for testing purpuses!
        // The demo mode can be activated and deactivated anytime by pressing 5 times F2 while the application is running.
        public static bool showedDemoModeIntroduction = false; //Whether or not the introduction was shown in the demo mode. This is only used by the demo mode!
        public static bool redirectToLightControl = true; //Whether or not BridgeConfig should redirect the frame to BasicLightControl when everything is set up correctly.

        public static bool isConnected = false; //Determines if a bridge is connected or not.
        public static LocalHueClient client; //Client of the current connection.

        public static Light[] lights = { }; //All lights available in the connected brdige.
        public static List<string> usedLights = new List<string>(); //Array of the lights that are used.
        public static Light mainLightTarget; //Light which is the main target.
        // The main target is the light from which attributes like the brightness are displayed by default on the UI.
    }
}
