using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MultiClip.Utilities
{
    /// <summary>
    /// A wrapper utility class which exposes
    /// low level utilities for sending virtual input to other windows.
    /// </summary>
    public static class VirtualInput
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct GUITHREADINFO
        {
            public int cbSize;
            public int flags;
            public IntPtr hwndActive;
            public IntPtr hwndFocus;
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCaret;
            public System.Drawing.Rectangle rcCaret;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint Type;
            public MOUSEKEYBDHARDWAREINPUT Data;
        }
        
        [StructLayout(LayoutKind.Explicit)]
        private struct MOUSEKEYBDHARDWAREINPUT
        {
            [FieldOffset(0)]
            public HARDWAREINPUT Hardware;
            [FieldOffset(0)]
            public KEYBDINPUT Keyboard;
            [FieldOffset(0)]
            public MOUSEINPUT Mouse;
        }
        
        [StructLayout(LayoutKind.Sequential)]
        private struct HARDWAREINPUT
        {
            public uint Msg;
            public ushort ParamL;
            public ushort ParamH;
        }
        
        [StructLayout(LayoutKind.Sequential)]
        internal struct KEYBDINPUT
        {
            public ushort Vk;
            public ushort Scan;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }
        
        [StructLayout(LayoutKind.Sequential)]
        internal struct MOUSEINPUT
        {
            public int X;
            public int Y;
            public uint MouseData;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint numberOfInputs, INPUT[] inputs, int sizeOfInputStructure);

        private const uint MAPVK_VK_TO_VSC = 0x00;
        private const uint MAPVK_VSC_TO_VK = 0x01;
        private const uint MAPVK_VK_TO_CHAR = 0x02;
        private const uint MAPVK_VSC_TO_VK_EX = 0x03;
        private const uint MAPVK_VK_TO_VSC_EX = 0x04;

        private const uint INPUT_MOUSE = 0;
        private const uint INPUT_KEYBOARD = 1;
        private const uint INPUT_HARDWARE = 2;

        private const uint KEYEVENTF_KEYUP = 0x0002;

        private const ushort VK_CONTROL = 0x11;
        private const ushort VK_C = 0x43;
        private const ushort VK_V = 0x56;


#if false

        /// <summary>
        /// Sends Ctrl+C to the target window using the SendInput API.
        /// </summary>
        public static void SendInputCtrlC(IntPtr hWnd)
        {
            uint tid = Win32.GetCurrentThreadId();
            uint targetTid = Win32.GetWindowThreadProcessId(hWnd, out uint targetPid);
            if (Win32.AttachThreadInput(tid, targetTid, true))
            {
                Win32.SetFocus(hWnd);
                SendInputCtrlC();
                Win32.AttachThreadInput(tid, targetTid, false);
            }
        }

        /// <summary>
        /// Sends Ctrl+V to the target window using the SendInput API.
        /// </summary>
        public static void SendInputCtrlV(IntPtr hWnd)
        {
            uint tid = Win32.GetCurrentThreadId();
            uint targetTid = Win32.GetWindowThreadProcessId(hWnd, out uint targetPid);
            if (Win32.AttachThreadInput(tid, targetTid, true))
            {
                Win32.SetFocus(hWnd);
                SendInputCtrlV();
                Win32.AttachThreadInput(tid, targetTid, false);
            }
        }

#endif

        /// <summary>
        /// Sends Ctrl+C to the foreground window using the SendInput API.
        /// </summary>
        public static void SendInputCtrlC()
        {
            int inputCount = 4;
            INPUT[] inputs = new INPUT[inputCount];

            for (int i = 0; i < inputCount; i++)
            {
                inputs[i].Type = INPUT_KEYBOARD;
                inputs[i].Data.Keyboard = new KEYBDINPUT
                {
                    Flags = 0,
                };
            }

            inputs[0].Data.Keyboard.Vk = VK_CONTROL;
            inputs[0].Data.Keyboard.Scan = (ushort)MapVirtualKey(VK_CONTROL, MAPVK_VK_TO_VSC);
            inputs[1].Data.Keyboard.Vk = VK_C;
            inputs[1].Data.Keyboard.Scan = (ushort)MapVirtualKey(VK_C, MAPVK_VK_TO_VSC);
            inputs[2].Data.Keyboard.Flags = KEYEVENTF_KEYUP;
            inputs[2].Data.Keyboard.Vk = inputs[0].Data.Keyboard.Vk;
            inputs[2].Data.Keyboard.Scan = inputs[0].Data.Keyboard.Scan;
            inputs[3].Data.Keyboard.Flags = KEYEVENTF_KEYUP;
            inputs[3].Data.Keyboard.Vk = inputs[1].Data.Keyboard.Vk;
            inputs[3].Data.Keyboard.Scan = inputs[1].Data.Keyboard.Scan;

            if (SendInput((uint)inputCount, inputs, Marshal.SizeOf(typeof(INPUT))) != inputCount)
            {
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// Sends Ctrl+V to the foreground window using the SendInput API.
        /// </summary>
        public static void SendInputCtrlV()
        {
            int inputCount = 4;
            INPUT[] inputs = new INPUT[inputCount];

            for (int i = 0; i < inputCount; i++)
            {
                inputs[i].Type = INPUT_KEYBOARD;
                inputs[i].Data.Keyboard = new KEYBDINPUT
                {
                    Flags = 0,
                };
            }

            inputs[0].Data.Keyboard.Vk = VK_CONTROL;
            inputs[0].Data.Keyboard.Scan = (ushort)MapVirtualKey(VK_CONTROL, MAPVK_VK_TO_VSC);
            inputs[1].Data.Keyboard.Vk = VK_V;
            inputs[1].Data.Keyboard.Scan = (ushort)MapVirtualKey(VK_V, MAPVK_VK_TO_VSC);
            inputs[2].Data.Keyboard.Flags = KEYEVENTF_KEYUP;
            inputs[2].Data.Keyboard.Vk = inputs[0].Data.Keyboard.Vk;
            inputs[2].Data.Keyboard.Scan = inputs[0].Data.Keyboard.Scan;
            inputs[3].Data.Keyboard.Flags = KEYEVENTF_KEYUP;
            inputs[3].Data.Keyboard.Vk = inputs[1].Data.Keyboard.Vk;
            inputs[3].Data.Keyboard.Scan = inputs[1].Data.Keyboard.Scan;

            if (SendInput((uint)inputCount, inputs, Marshal.SizeOf(typeof(INPUT))) != inputCount)
            {
                throw new Win32Exception();
            }
        }
    }
}
