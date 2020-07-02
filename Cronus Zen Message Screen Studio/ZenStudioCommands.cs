using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CronusZenMessageScreenStudio
{
    class ZenStudioCommands
    {
        private const int WM_COPYDATA = 0x4A;

        [StructLayout(LayoutKind.Sequential)]
        private struct COPYDATASTRUCT
        {
            public int dwData;
            public int cbData;
            public IntPtr lpData;
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, ref COPYDATASTRUCT lParam);

        public static void SendBuildAndRun(string script) => SendCommand(2, script);

        public static void SendOpenCompilerTab(string script) => SendCommand(1, script);

        private static void SendCommand(int messageType, string message)
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
