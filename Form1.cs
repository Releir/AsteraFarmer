using Binarysharp.MemoryManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AsteraFarmer
{
    public partial class Form1 : Form
    {

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        public delegate bool Win32Callback(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.Dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr parentHandle, Win32Callback callback, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern string GetWindowText(IntPtr hWnd);

        public static IntPtr FindWindow(IntPtr parentHandle, Predicate<IntPtr> target)
        {
            var result = IntPtr.Zero;
            if (parentHandle == IntPtr.Zero)
                parentHandle = Process.GetCurrentProcess().MainWindowHandle;
            EnumChildWindows(parentHandle, (hwnd, param) => {
                if (target(hwnd))
                {
                    result = hwnd;
                    return false;
                }
                return true;
            }, IntPtr.Zero);
            return result;
        }

        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hwnd);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SetFocus(IntPtr hwnd);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);
        const uint WM_KEYDOWN = 0x100;
        const uint WM_KEYUP = 0x101;
        const uint WM_CHAR = 0x102;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);
        private const uint WM_LBUTTONDOWN = 0x0201;
        private const uint WM_LBUTTONUP = 0x0202;
        private const uint WM_MOUSEMOVE = 0x0200;
        private const uint WM_PASTE = 0x0302;
        //private const uint WM_CHAR = 0x0102;



        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(int hWnd, int msg, string wParam, IntPtr lParam);

        const int PROCESS_WM_READ = 0x0010;
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(
                IntPtr hProcess,
                IntPtr lpBaseAddress,
                byte[] lpBuffer,
                int dwSize,
                out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern Int32 CloseHandle(IntPtr hProcess);

        [DllImport("kernel32.dll")]
        public static extern Int32 ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
            [In, Out] byte[] buffer, UInt32 size, out IntPtr lpNumberOfBytesRead);

        private static byte[] ReadBytes(IntPtr handle, long address, uint bytesToRead)
        {
            IntPtr ptrBytesRead;
            byte[] buffer = new byte[bytesToRead];

            ReadProcessMemory(handle, new IntPtr(address), buffer, bytesToRead, out ptrBytesRead);

            return buffer;
        }

        private static void WriteBytes(IntPtr handle, long address, int value)
        {

            byte[] dataBuffer = BitConverter.GetBytes(value);
            IntPtr bytesWritten = IntPtr.Zero;

            WriteProcessMemory(handle, (IntPtr)address, dataBuffer, dataBuffer.Length, out bytesWritten);
        }

        private static int ReadInt32(IntPtr handle, long address)
        {
            return BitConverter.ToInt32(ReadBytes(handle, address, 4), 0);
        }

        string ReadString(Process proc, int addr)
        {
            byte[] buffer = ReadBytes(proc.Handle, addr, 100);
            int firstNullIndex = Array.FindIndex(buffer, b => b == 0);
            string msg = Encoding.Default.GetString(buffer, 0, firstNullIndex);
            string formattedMsg = msg.Trim().ToLower();
            return formattedMsg;
        }

        public Form1()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public static void populateBuffs(Process actualProcess, List<int> buffArr)
        {
            int buffStart = 0x010DD284;
            //int buffStart = 0x010183AC;
            for (int ctr = 0; ctr <= 16; ctr = ctr + 4)
            {
                int buffAddr = buffStart + ctr;
                int buffVal = ReadInt32(actualProcess.Handle, buffAddr);
                buffArr.Add(buffVal);
            }
        }

        static List<Thread> threadList = new List<Thread>();
        static Stopwatch sw = new Stopwatch();
        Thread timerThd;
        static List<Process> procList = new List<Process>();
        private void startBtn_Click(object sender, EventArgs e)
        {
            threadList = new List<Thread>();
            sw = new Stopwatch();
            sw.Reset();
            int delay = Convert.ToInt32(delayTime.Text);
            int timerEnd = Convert.ToInt32(timerBox.Text) * 60000; //Minute to ms
            //int timerEnd = 5000; //Minute to ms

            foreach (Process proc in procList)
            {
                Thread thd = new Thread(() => doFarming(proc, delay));
                Thread thd2 = new Thread(() => autoPots(proc));
                //Thread thd = new Thread(() => exchangeArc(proc));
                thd.IsBackground = true;
                threadList.Add(thd);
                threadList.Add(thd2);
            }
            foreach (Thread th in threadList)
            {
                th.Start();
            }
            //if(timerThd == null)
            //{
            //    timerThd = new Thread(() => timer(timerEnd));
            //    timerThd.Start();
            //}
            //sw.Start();
            startBtn.Enabled = false;
        }

        public void timer(int timerEnd)
        {
            sw.Start();
            while (sw.ElapsedMilliseconds < timerEnd)
            {
                Console.WriteLine("ELAPSED TIME :" + sw.ElapsedMilliseconds);
                if (sw.ElapsedMilliseconds >= timerEnd)
                {
                    Console.WriteLine("ENTERING LMAO");
                    sw.Stop();
                    sw.Reset();
                    stopThreads();
                }
            }
        }

        public static void sendMail(Process proc)
        {
            SetForegroundWindow(proc.MainWindowHandle);
            SetFocus(proc.MainWindowHandle);
            Thread.Sleep(1000);
            SetCursorPos(599, 433);
            Thread.Sleep(500);
            mouseClick(proc, (int)Keys.LButton);
            Thread.Sleep(500);
            SetCursorPos(1031, 375);
            Thread.Sleep(500);
            mouseClick(proc, (int)Keys.LButton);
            mouseClick(proc, (int)Keys.LButton);
            Thread.Sleep(500);
            SetCursorPos(823, 409);
            Thread.Sleep(500);
            mouseClick(proc, (int)Keys.LButton);
            Thread.Sleep(1000);
            pasteMessage(proc);
            
            //SetCursorPos(538, 539); //Citrine Position
            //mouseClick(proc, (int)Keys.LButton);
            //mouseClick(proc, (int)Keys.LButton);
        }

        public void exchangeArc(Process proc)
        {
            lock (this)
            {
                SetForegroundWindow(proc.MainWindowHandle);
                SetFocus(proc.MainWindowHandle);
                Thread.Sleep(1000);
                pressKey(proc, (int)Keys.F8);
                Thread.Sleep(4000);
                SetCursorPos(805, 574);
                Thread.Sleep(500);
                mouseClick(proc, (int)Keys.LButton);
                mouseClick(proc, (int)Keys.LButton);
                Thread.Sleep(500);
                pressKey(proc, (int)Keys.Enter);
                Thread.Sleep(500);
                pressKey(proc, (int)Keys.Enter);
                Thread.Sleep(500);
                pressNumerics(proc, (int)Keys.D2);
                pressNumerics(proc, (int)Keys.D0);
                pressNumerics(proc, (int)Keys.D0);
                Thread.Sleep(500);
                pressKey(proc, (int)Keys.Enter);
                Thread.Sleep(500);
                pressKey(proc, (int)Keys.Enter);
                Thread.Sleep(500);
                pressKey(proc, (int)Keys.Enter);
                Thread.Sleep(500);
                SetCursorPos(815, 450);
                Thread.Sleep(500);
                mouseClick(proc, (int)Keys.LButton);
                mouseClick(proc, (int)Keys.LButton);
                Thread.Sleep(500);
                pressKey(proc, (int)Keys.Enter);
                Thread.Sleep(500);
                pressKey(proc, (int)Keys.Enter);
                Thread.Sleep(2500);
            }
        }

        private static void KillProcessAndChildrens(int pid)
        {
            ManagementObjectSearcher processSearcher = new ManagementObjectSearcher
              ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection processCollection = processSearcher.Get();

            try
            {
                Process proc = Process.GetProcessById(pid);
                if (!proc.HasExited) proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }

            if (processCollection != null)
            {
                foreach (ManagementObject mo in processCollection)
                {
                    KillProcessAndChildrens(Convert.ToInt32(mo["ProcessID"])); //kill child processes(also kills childrens of childrens etc.)
                }
            }
        }

        public static void doFarming(Process proc, int delay)
        {
            while (true)
            {
                //var foundHandle = FindWindow(IntPtr.Zero, ptr => GetWindowText(ptr) == "GS 3.0 :: 2021110101");
                int pid = proc.Id;
                int newCount = WinUtil.GetWindowCount(pid);
                if (newCount > 1)
                {
                    Process[] gepProc = Process.GetProcessesByName("GS 3.0 :: 2021110101");
                    KillProcessAndChildrens(proc.Id);
                    //WinUtil.CloseWindow(WinUtil.GetForegroundWindow());
                }

                autoRepack(proc);
                //wizFarm(proc);
                //autoStorage(proc);
                //Thread.Sleep(delay + RandomNumber());
                Thread.Sleep(delay);
            }
        }

        public static void wizFarm(Process proc)
        {
            IntPtr xAddr = (IntPtr)0x00E2EC74;
            IntPtr yAddr = (IntPtr)0x00E2EC78;
            pressKey(proc, (int)Keys.F9);
            //Thread.Sleep(200);
            //pressKey(proc, (int)Keys.Enter);
            //Thread.Sleep(50);
            pressKey(proc, (int)Keys.F1);
            //New Era, Kratos IDS
            WriteBytes(proc.Handle, 0x00E2EC74, 509); //X
            WriteBytes(proc.Handle, 0x00E2EC78, 386); //Y
            //Thread.Sleep(50); //Was 200
            for (int i = 0; i < 6; i++)
            {
                mouseClick(proc, (int)Keys.LButton);
            }
            //mouseClick(proc, (int)Keys.LButton);
            //mouseClick(proc, (int)Keys.LButton);

            //pressKey(proc, (int)Keys.F1);
            //////////////////////////////////////
            pressKey(proc, (int)Keys.F1);
            for (int i = 0; i < 6; i++)
            {
                mouseClick(proc, (int)Keys.LButton);
            }
            ////////////////////////////////////////
            //Thread.Sleep(200);
            //mouseClick(proc, (int)Keys.LButton);
            //mouseClick(proc, (int)Keys.LButton);
        }

        public static void autoStorage(Process proc)
        {
                    int maxWeight = 0x010D94AC;
                    int currWeight = 0x010D94B0;
                    int maxWVal = ReadInt32(proc.Handle, maxWeight);
                    int currWVal = ReadInt32(proc.Handle, currWeight); ;
                    double weightPerc = ((double)currWVal / (double)maxWVal) * 100;
                    double WeightToStore = 60;

                    if (weightPerc > WeightToStore)
                    {
                        IntPtr xAddr = (IntPtr)0x00E2EC74;
                        IntPtr yAddr = (IntPtr)0x00E2EC78;
                        WriteBytes(proc.Handle, 0x00E2EC74, 54); //X
                        WriteBytes(proc.Handle, 0x00E2EC78, 175); //Y

                        SendMessage(proc.MainWindowHandle, WM_LBUTTONDOWN, (int)Keys.LButton, 0);
                        Thread.Sleep(55);
                        WriteBytes(proc.Handle, 0x00E2EC74, 344); //X
                        WriteBytes(proc.Handle, 0x00E2EC78, 191); //Y
                        Thread.Sleep(55);
                        SendMessage(proc.MainWindowHandle, WM_LBUTTONUP, (int)Keys.LButton, 0);
                        Thread.Sleep(55);
                        pressKey(proc, (int)Keys.Enter);
                        Thread.Sleep(500);
                    }
        }

        public static void autoRepack(Process proc)
        {
            IntPtr xAddr = (IntPtr)0x00E2EC74;
            IntPtr yAddr = (IntPtr)0x00E2EC78;


            //WriteBytes(proc.Handle, 0x00E2EC74, 54); //X
            //WriteBytes(proc.Handle, 0x00E2EC78, 175); //Y
            Thread.Sleep(50);
            pressKey(proc, (int)Keys.F8);  //Storage
            Thread.Sleep(100);
            moveMouse(proc, 334, 175); //berry position in storage
            Thread.Sleep(100);
            dragoTo(proc, 214, 213); //drag to inventory
            Thread.Sleep(50);
            //Get 500 Berry
            pressNumerics(proc, (int)Keys.D5);
            pressNumerics(proc, (int)Keys.D0);
            pressNumerics(proc, (int)Keys.D0);
            Thread.Sleep(50);
            pressKey(proc, (int)Keys.Enter);
            Thread.Sleep(50);
            moveMouse(proc, 516, 647); //close button
            Thread.Sleep(100);
            mouseClick(proc, (int)Keys.LButton);
            Thread.Sleep(100);
            mouseClick(proc, (int)Keys.LButton);
            Thread.Sleep(200);
            moveMouse(proc, 666, 377); //Berry Trader
            Thread.Sleep(200);
            mouseClick(proc, (int)Keys.LButton);
            Thread.Sleep(100);
            mouseClick(proc, (int)Keys.LButton);
            Thread.Sleep(200);
            pressKey(proc, (int)Keys.Enter);
            Thread.Sleep(200);
            moveMouse(proc, 613, 149); //Berry Box pos in shop
            Thread.Sleep(200);
            dragoTo(proc, 442, 317); //random pos in buy shop
            Thread.Sleep(200);
            pressNumerics(proc, (int)Keys.D5);
            Thread.Sleep(200);
            pressKey(proc, (int)Keys.Enter);
            Thread.Sleep(200);
            moveMouse(proc, 473, 381);  //Buy Button
            Thread.Sleep(200);
            mouseClick(proc, (int)Keys.LButton);
            Thread.Sleep(200);
            mouseClick(proc, (int)Keys.LButton);
            Thread.Sleep(200);
            pressKey(proc, (int)Keys.Enter);
            Thread.Sleep(200);
            pressKey(proc, (int)Keys.Enter);
            Thread.Sleep(200);
            pressKey(proc, (int)Keys.Enter);
            Thread.Sleep(200);
            pressKey(proc, (int)Keys.Enter);
            Thread.Sleep(500);
            moveMouse(proc, 334, 175);
        }

        public static void autoPots(Process proc)
        {
            while (true)
            {
                int hpAddr = 0x010DCE10;  //New Era Addr
                                          //int hpAddr = 0x00E4CAF4;  //Vicious
                int hpMaxAddr = hpAddr + 4;
                int spAddr = hpAddr + 8;
                int spMaxAddr = hpAddr + 12;
                double spPercToPot = 10;
                double hpPercToPot = 1;
                int spVal = ReadInt32(proc.Handle, spAddr);
                int spMax = ReadInt32(proc.Handle, spMaxAddr);
                int hpVal = ReadInt32(proc.Handle, hpAddr); ;
                int hpMax = ReadInt32(proc.Handle, hpMaxAddr);
                double hpPerc = ((double)hpVal / (double)hpMax) * 100;
                double spPerc = ((double)spVal / (double)spMax) * 100;
                if (spPerc < spPercToPot)
                {
                    Thread.Sleep(100);
                    pressKey(proc, (int)Keys.F7);
                }
                //if (hpPerc < hpPercToPot)
                //{
                //    Thread.Sleep(100);
                //    pressKey(proc, (int)Keys.F8);
                //}
            }
        }

        public static void autoHide(Process proc)
        {
            //int playdeadStat = 29;
            //List<int> buffArr = new List<int>();
            //populateBuffs(proc, buffArr);
            //Thread.Sleep(400);
            //if (!buffArr.Contains(playdeadStat))
            //{
            //    Thread.Sleep(400);
            //    pressKey(proc, (int)Keys.F7);
            //}
            //Thread.Sleep(100);
            int weightAddr = 0x010D94B0;
            double weight = ReadInt32(proc.Handle, weightAddr);

            if (weight > 8000)
            {
                Thread.Sleep(400);
                pressKey(proc, (int)Keys.F6);
            }
        }

        public static void asterFarm(Process proc)
        {
            int frozenStatus = 876;
            int ghostMild = 148;
            pressKey(proc, (int)Keys.F9);
            Thread.Sleep(200);
            pressKey(proc, (int)Keys.F1);
            Thread.Sleep(200);
            pressKey(proc, (int)Keys.F1);
            pressKey(proc, (int)Keys.F1);

            List<int> buffArr = new List<int>();
            populateBuffs(proc, buffArr);
            if (!buffArr.Contains(ghostMild))
            {
                Thread.Sleep(400);
                pressKey(proc, (int)Keys.F2);
            }
            if (buffArr.Contains(frozenStatus))
            {
                Thread.Sleep(60000);
                pressKey(proc, (int)Keys.F8);
                Thread.Sleep(200);
            }
        }

        public void stopThreads()
        {
            foreach (Thread th in threadList)
            {
                th.Abort();
            }
            startExchange();
        }

        public void startExchange()
        {
            foreach (Process proc in procList)
            {
                Thread thd = new Thread(() => exchangeArc(proc));
                thd.Start();
                thd.Join();
            }
            startBtn_Click(null, null);
        }

        public static void pressKey(Process proc, int keybind)
        {
            PostMessage(proc.MainWindowHandle, WM_KEYDOWN, keybind, 0);
            PostMessage(proc.MainWindowHandle, WM_KEYUP, keybind, 0);
        }

        //Hacking this one since it sends double chars on pressKey
        public static void pressNumerics(Process proc, int keybind)
        {
            PostMessage(proc.MainWindowHandle, WM_KEYDOWN, keybind, 0);
        }

        public static void mouseClick(Process proc, int keybind)
        {
            SendMessage(proc.MainWindowHandle, WM_LBUTTONDOWN, keybind, 0);
            SendMessage(proc.MainWindowHandle, WM_LBUTTONUP, keybind, 0);
        }

        public static void dragoTo(Process proc, int x, int y)
        {
            SendMessage(proc.MainWindowHandle, WM_LBUTTONDOWN, (int)Keys.LButton, 0);
            Thread.Sleep(11);
            WriteBytes(proc.Handle, 0x00E2EC74, x); //X
            WriteBytes(proc.Handle, 0x00E2EC78, y); //Y
            Thread.Sleep(11);
            SendMessage(proc.MainWindowHandle, WM_LBUTTONUP, (int)Keys.LButton, 0);
        }

        public static void moveMouse(Process proc, int x, int y)
        {
            WriteBytes(proc.Handle, 0x00E2EC74, x); //X
            WriteBytes(proc.Handle, 0x00E2EC78, y); //Y
        }

        public static void pasteMessage(Process proc)
        {
            SendMessage(proc.MainWindowHandle, WM_KEYDOWN, (int)Keys.Control, 0);
            Thread.Sleep(500);
            SendMessage(proc.MainWindowHandle, WM_KEYDOWN, (int)Keys.V, 0);
            SendMessage(proc.MainWindowHandle, WM_KEYUP, (int)Keys.V, 0);
            Thread.Sleep(500);
            SendMessage(proc.MainWindowHandle, WM_KEYUP, (int)Keys.Control, 0);
        }

        public static void mouseDragTo(Process proc, int xTo, int yTo)
        {
            SendMessage(proc.MainWindowHandle, WM_LBUTTONDOWN, (int)Keys.LButton, 0);
            SetCursorPos(xTo, yTo);
            SendMessage(proc.MainWindowHandle, WM_LBUTTONUP, (int)Keys.LButton, 0);
        }

        private void stopBtn_Click(object sender, EventArgs e)
        {
            //stopThreads();
            foreach (Thread th in threadList)
            {
                th.Abort();
            }
            threadList.Clear();
            startBtn.Enabled = true;
            //timerThd.Abort();
            timerThd = null;
        }

        private void resetBtn_Click(object sender, EventArgs e)
        {
            sw.Reset();
        }

        private void initBtn_Click(object sender, EventArgs e)
        {
            //string[] charNames = { "Murdock", "Hakdog"};
            Process[] initialProcs = Process.GetProcessesByName("Mining Ragnarok Online");
            List<Process> procLists = new List<Process>();
            foreach (Process proc in initialProcs)
            {
                //string charName = ReadString(proc, 0x010DF5D8);
                //if (charNames.Contains(charName))
                //{
                procLists.Add(proc);
                //}

            }
            procList = procLists;


            //procList = Process.GetProcessesByName("Requiem-RO");
            //procList = Process.GetProcessesByName("KratosRO");
            //procList = Process.GetProcessesByName("AquaRO");
            //procList = Process.GetProcessesByName("New Era Ragnarok");
            //procList = Process.GetProcessesByName("AlerionRO");


            //Process ragnaProc = new Process();
            //// Configure the process using the StartInfo properties.
            //ragnaProc.StartInfo.FileName = "D:\\Games\\RequiemRO\\RequiemRO.lnk";
            //ragnaProc.Start();
            //procList.Add(ragnaProc);

        }

            public static int RandomNumber()
        {
            Random random = new Random();  
            return random.Next(1, 8) * 1000;
        }

        private void timerBox_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
