using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CronusZenMessageScreenStudio
{
    class ZenStudioCommands
    {
        private const int WM_COPYDATA = 0x4A;

        public enum ZenStudioCommand
        {
            GpcTab = 1,
            BuildAndRun = 2
        }


        [StructLayout(LayoutKind.Sequential)]
        private struct COPYDATASTRUCT
        {
            public ZenStudioCommand dwData;
            public int cbData;
            public IntPtr lpData;
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, ref COPYDATASTRUCT lParam);

        public static void SendBuildAndRun(string script) => SendCommand(ZenStudioCommand.BuildAndRun, script);

        public static void SendOpenCompilerTab(string script) => SendCommand(ZenStudioCommand.GpcTab, script);

        private static void SendCommand(ZenStudioCommand messageType, string message)
        {
            COPYDATASTRUCT cds;
            cds.dwData = messageType;
            cds.lpData = Marshal.StringToHGlobalAnsi(message);
            cds.cbData = message.Length;
            foreach (Process zenProc in Process.GetProcessesByName("ZenStudio"))
            {
                SendMessage(zenProc.MainWindowHandle, WM_COPYDATA, 0, ref cds);
            }
            Marshal.FreeHGlobal(cds.lpData);
        }
    }
}
