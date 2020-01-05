using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenHardwareMonitor.Hardware;
using System.IO.Ports;
using System.Drawing;
using System.Runtime.InteropServices;

namespace MiniOLED
{

    class Program
    {
        //隐藏窗口使用
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]   //找子窗体   
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("User32.dll", EntryPoint = "SendMessage")]   //用于发送信息给窗体   
        private static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, string lParam);

        [DllImport("User32.dll", EntryPoint = "ShowWindow")]   //
        private static extern bool ShowWindow(IntPtr hWnd, int type);


        //OLED显示尺寸
        const int ScreenBufferWidth = 128;
        const int ScreenBufferHeight = 64;

        //显存总量
        static float GPUAllMEM = 0;
        static int AllSYSMEM = 0;

        struct bufferPosition
        {
            public  int BeginXPos;
            public  int BeginYPos;
        }

        public class UpdateVisitor : IVisitor
        {
            public void VisitComputer(IComputer computer)
            {
                computer.Traverse(this);
            }
            public void VisitHardware(IHardware hardware)
            {
                hardware.Update();
                foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
            }
            public void VisitSensor(ISensor sensor) { }
            public void VisitParameter(IParameter parameter) { }
        }
       

        //添加数字到指定位置
        static void AddInfoToBuffer(bufferPosition Position,bool isLargeNum,int Num,ref int[,] screenBuffer)
        {
            string NumBitMapPath = "";
            if (isLargeNum)
            {
                NumBitMapPath = System.IO.Directory.GetCurrentDirectory() + "\\Resource\\Number\\Big\\" + Num + ".bmp";
            }
            else
            {
                NumBitMapPath = System.IO.Directory.GetCurrentDirectory() + "\\Resource\\Number\\Small\\" + Num + ".bmp";
            }

            if (System.IO.File.Exists(NumBitMapPath))
            {
                Bitmap backgroungMap01 = new Bitmap(NumBitMapPath);

                //遍历输出结果看看
                for (int i = 0; i < backgroungMap01.Height; i++)
                {
                    for (int j = 0; j < backgroungMap01.Width; j++)
                    {
                        if (backgroungMap01.GetPixel(j, i).R == 0)
                        {
                            screenBuffer[Position.BeginXPos + j, Position.BeginYPos + i] = 0;
                        }
                        else
                        {
                            screenBuffer[Position.BeginXPos + j, Position.BeginYPos + i] = 1;
                        }
                    }
                }

            }
            else
            {
                Console.WriteLine("加载的图片路径不存在");
            }
        }
        static void GetSystemInfo(ref int[,] screenBuffer)
        {


            UpdateVisitor updateVisitor = new UpdateVisitor();
            Computer computer = new Computer();
            computer.Open();
            computer.CPUEnabled = true;
            computer.GPUEnabled = true;
            computer.RAMEnabled = true;
            computer.Accept(updateVisitor);
            for (int i = 0; i < computer.Hardware.Length; i++)
            {
                //**************************************************************************************************************
                if (computer.Hardware[i].HardwareType == HardwareType.CPU)
                {
                    Console.WriteLine("*******************CPU**************");
                    //计算CPU频率
                    float CPUCoreClock = 0;
                    int CPUCoreCount = 0;

                    for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                    {
                        //CPU温度
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature)
                        {

                            if (computer.Hardware[i].Sensors[j].Name == "CPU Package")
                            {
                                Console.WriteLine("CPU 温度:" + computer.Hardware[i].Sensors[j].Value.ToString() + "\r");

                                //############################################ CPU温度 ##############################################################
                                //CPU温度开始的位置
                                bufferPosition CPUTemperatureBeginPos;           
                                string CPUTemperatureStr = computer.Hardware[i].Sensors[j].Value.ToString();

                                int CPUUsageTemp = 0;
                                //频率小数点左侧的数字
                                if (CPUTemperatureStr.Length == 3)
                                {
                                    CPUTemperatureBeginPos.BeginXPos = 33;
                                    CPUTemperatureBeginPos.BeginYPos = 49;
                                    AddInfoToBuffer(CPUTemperatureBeginPos, false, 10, ref screenBuffer);

                                    CPUTemperatureBeginPos.BeginXPos = 36;
                                    CPUTemperatureBeginPos.BeginYPos = 49;
                                    CPUUsageTemp = int.Parse(CPUTemperatureStr.Substring(1, 1));
                                    AddInfoToBuffer(CPUTemperatureBeginPos, false, CPUUsageTemp, ref screenBuffer);

                                    CPUTemperatureBeginPos.BeginXPos = 43;
                                    CPUTemperatureBeginPos.BeginYPos = 49;
                                    CPUUsageTemp = int.Parse(CPUTemperatureStr.Substring(2, 1));
                                    AddInfoToBuffer(CPUTemperatureBeginPos, false, CPUUsageTemp, ref screenBuffer);
                                }
                                else if (CPUTemperatureStr.Length == 2)
                                {
                                    CPUTemperatureBeginPos.BeginXPos = 36;
                                    CPUTemperatureBeginPos.BeginYPos = 49;
                                    CPUUsageTemp = int.Parse(CPUTemperatureStr.Substring(0, 1));
                                    AddInfoToBuffer(CPUTemperatureBeginPos, false, CPUUsageTemp, ref screenBuffer);

                                    CPUTemperatureBeginPos.BeginXPos = 43;
                                    CPUTemperatureBeginPos.BeginYPos = 49;
                                    CPUUsageTemp = int.Parse(CPUTemperatureStr.Substring(1, 1));
                                    AddInfoToBuffer(CPUTemperatureBeginPos, false, CPUUsageTemp, ref screenBuffer);
                                }
                                else if(CPUTemperatureStr.Length == 1)
                                {
                                    CPUTemperatureBeginPos.BeginXPos = 36;
                                    CPUTemperatureBeginPos.BeginYPos = 49;
                                    AddInfoToBuffer(CPUTemperatureBeginPos, false, 0, ref screenBuffer);

                                    CPUTemperatureBeginPos.BeginXPos = 43;
                                    CPUTemperatureBeginPos.BeginYPos = 49;
                                    CPUUsageTemp = int.Parse(CPUTemperatureStr.Substring(0, 1));
                                    AddInfoToBuffer(CPUTemperatureBeginPos, false, CPUUsageTemp, ref screenBuffer);
                                }
                            }
                        }
                        //CPU频率
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Clock)
                        {
                            if (computer.Hardware[i].Sensors[j].Name.Substring(0, 8) == "CPU Core")
                            {
                                //计算CPU的频率总和 和 CPU核心数
                                CPUCoreClock += (float)computer.Hardware[i].Sensors[j].Value;
                                CPUCoreCount++;
                            }
                        }
                        //CPU使用率
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Load)
                        {
                            if (computer.Hardware[i].Sensors[j].Name == "CPU Total")
                            {
                                Console.WriteLine("CPU 占用:" + computer.Hardware[i].Sensors[j].Value.ToString() + "%\r");

                                //############################################ CPU使用 ##############################################################
                                //CPU使用开始的位置
                                bufferPosition CPUUsageBeginPos;
                                CPUUsageBeginPos.BeginXPos = 64;
                                CPUUsageBeginPos.BeginYPos = 49;
                                string CPUUsageStr = computer.Hardware[i].Sensors[j].Value.ToString();

                                int CPUUsageTemp = 0;
                                //使用的第一位数字
                                if (CPUUsageStr.IndexOf('.') <= 1)
                                {
                                    AddInfoToBuffer(CPUUsageBeginPos, false, 0, ref screenBuffer);
                                }
                                else
                                {
                                    CPUUsageTemp = int.Parse(CPUUsageStr.Substring(CPUUsageStr.IndexOf('.') - 2, 1));
                                    AddInfoToBuffer(CPUUsageBeginPos, false, CPUUsageTemp, ref screenBuffer);
                                }
                                //使用的第二位数字
                                CPUUsageBeginPos.BeginXPos = 71;
                                CPUUsageBeginPos.BeginYPos = 49;
                                CPUUsageStr = computer.Hardware[i].Sensors[j].Value.ToString();

                                if (CPUUsageStr.IndexOf('.') <= 0)
                                {
                                    AddInfoToBuffer(CPUUsageBeginPos, false, 1, ref screenBuffer);
                                }
                                else
                                {
                                    CPUUsageTemp = int.Parse(CPUUsageStr.Substring(CPUUsageStr.IndexOf('.') - 1, 1));
                                    //最小使用率为1%
                                    if (CPUUsageTemp == 0)
                                    {
                                        CPUUsageTemp = 1;
                                    }
                                    AddInfoToBuffer(CPUUsageBeginPos, false, CPUUsageTemp, ref screenBuffer);
                                }
                                
                            }
                        }
                    }
                    //计算CPU频率，多核取平均值
                    CPUCoreClock = CPUCoreClock / CPUCoreCount / 1024;
                    Console.WriteLine("CPU 频率:" + CPUCoreClock + "\r");

                    //############################################ CPU频率 ##############################################################
                    //CPU频率开始的位置
                    bufferPosition CPUColockBeginPos;
                    CPUColockBeginPos.BeginXPos = 89;
                    CPUColockBeginPos.BeginYPos = 49;
                    string CPUClockStr = CPUCoreClock.ToString();

                    //频率小数点左侧的数字
                    int CLockTemp = int.Parse(CPUClockStr.Substring(CPUClockStr.IndexOf('.') - 1, 1));
                    AddInfoToBuffer(CPUColockBeginPos, false, CLockTemp, ref screenBuffer);

                    //频率小数点右侧侧的数字
                    CPUColockBeginPos.BeginXPos = 99;
                    CPUColockBeginPos.BeginYPos = 49;
                    CLockTemp = int.Parse(CPUClockStr.Substring(CPUClockStr.IndexOf('.') + 1, 1));
                    AddInfoToBuffer(CPUColockBeginPos, false, CLockTemp, ref screenBuffer);
                }

                //**************************************************************************************************************

                if (computer.Hardware[i].HardwareType == HardwareType.RAM)
                {
                    //内存总量
                    float MemoryTotal = 0;
                    Console.WriteLine("**************Memory**********************");
                    for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                    {
                        //内存占用
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Load)
                        {
                            Console.WriteLine("内存 占用:" + computer.Hardware[i].Sensors[j].Value.ToString() + "%\r");
                            //############################################ 内存使用率 ##############################################################
                            bufferPosition MEMUsagePos;
                            string MEMUsage = computer.Hardware[i].Sensors[j].Value.ToString();

                            float UsageMEMValue = Convert.ToSingle(computer.Hardware[i].Sensors[j].Value.ToString()) / 100;

                            if (MEMUsage.IndexOf('.') == 3)
                            {
                                MEMUsagePos.BeginXPos = 7;
                                MEMUsagePos.BeginYPos = 38;
                                AddInfoToBuffer(MEMUsagePos, false, 9, ref screenBuffer);

                                MEMUsagePos.BeginXPos = 14;
                                MEMUsagePos.BeginYPos = 38;
                                AddInfoToBuffer(MEMUsagePos, false, 9, ref screenBuffer);
                            }
                            if (MEMUsage.IndexOf('.') == 2)
                            {
                                MEMUsagePos.BeginXPos = 7;
                                MEMUsagePos.BeginYPos = 38;
                                int MEMUsageValue = int.Parse(MEMUsage.Substring(0, 1));
                                AddInfoToBuffer(MEMUsagePos, false, MEMUsageValue, ref screenBuffer);

                                MEMUsagePos.BeginXPos = 14;
                                MEMUsagePos.BeginYPos = 38;
                                MEMUsageValue = int.Parse(MEMUsage.Substring(1, 1));
                                AddInfoToBuffer(MEMUsagePos, false, MEMUsageValue, ref screenBuffer);
                            }
                            if (MEMUsage.IndexOf('.') == 1)
                            {
                                MEMUsagePos.BeginXPos = 7;
                                MEMUsagePos.BeginYPos = 38;
                                AddInfoToBuffer(MEMUsagePos, false, 0, ref screenBuffer);

                                MEMUsagePos.BeginXPos = 14;
                                MEMUsagePos.BeginYPos = 38;
                                int MEMUsageValue = int.Parse(MEMUsage.Substring(0, 1));
                                AddInfoToBuffer(MEMUsagePos, false, MEMUsageValue, ref screenBuffer);
                            }
                            if (MEMUsage.IndexOf('.') == 0)
                            {
                                MEMUsagePos.BeginXPos = 7;
                                MEMUsagePos.BeginYPos = 38;
                                AddInfoToBuffer(MEMUsagePos, false, 0, ref screenBuffer);

                                MEMUsagePos.BeginXPos = 14;
                                MEMUsagePos.BeginYPos = 38;
                                AddInfoToBuffer(MEMUsagePos, false, 0, ref screenBuffer);
                            }


                            //############################################ 内存使用量 ##############################################################
                            UsageMEMValue = AllSYSMEM * UsageMEMValue;
                            string UsageMEMValueStr = UsageMEMValue.ToString();
                            Console.WriteLine(UsageMEMValueStr);
                            if (UsageMEMValueStr.IndexOf('.') == 2)
                            {
                                MEMUsagePos.BeginXPos = 2;
                                MEMUsagePos.BeginYPos = 17;
                                int NowValue = int.Parse(UsageMEMValueStr.Substring(UsageMEMValueStr.IndexOf('.') - 2, 1));
                                AddInfoToBuffer(MEMUsagePos, false, NowValue, ref screenBuffer);

                                MEMUsagePos.BeginXPos = 9;
                                MEMUsagePos.BeginYPos = 17;
                                NowValue = int.Parse(UsageMEMValueStr.Substring(UsageMEMValueStr.IndexOf('.') - 1, 1));
                                AddInfoToBuffer(MEMUsagePos, false, NowValue, ref screenBuffer);

                                MEMUsagePos.BeginXPos = 19;
                                MEMUsagePos.BeginYPos = 17;
                                NowValue = int.Parse(UsageMEMValueStr.Substring(UsageMEMValueStr.IndexOf('.') + 1, 1));
                                AddInfoToBuffer(MEMUsagePos, false, NowValue, ref screenBuffer);
                            }
                            if (UsageMEMValueStr.IndexOf('.') == 1)
                            {
                                MEMUsagePos.BeginXPos = 2;
                                MEMUsagePos.BeginYPos = 17;
                                AddInfoToBuffer(MEMUsagePos, false, 0, ref screenBuffer);

                                MEMUsagePos.BeginXPos = 9;
                                MEMUsagePos.BeginYPos = 17;
                                Console.WriteLine(UsageMEMValueStr.Substring(UsageMEMValueStr.IndexOf('.') - 1, 1));
                                
                                int NowValue = int.Parse(UsageMEMValueStr.Substring(UsageMEMValueStr.IndexOf('.') - 1, 1));
                                Console.WriteLine(NowValue);
                                AddInfoToBuffer(MEMUsagePos, false, NowValue, ref screenBuffer);

                                MEMUsagePos.BeginXPos = 19;
                                MEMUsagePos.BeginYPos = 17;
                                NowValue = int.Parse(UsageMEMValueStr.Substring(UsageMEMValueStr.IndexOf('.') + 1, 1));
                                AddInfoToBuffer(MEMUsagePos, false, NowValue, ref screenBuffer);
                            }
                            if (UsageMEMValueStr.IndexOf('.') == 0)
                            {
                                MEMUsagePos.BeginXPos = 2;
                                MEMUsagePos.BeginYPos = 17;
                                AddInfoToBuffer(MEMUsagePos, false, 0, ref screenBuffer);

                                MEMUsagePos.BeginXPos = 9;
                                MEMUsagePos.BeginYPos = 17;
                                AddInfoToBuffer(MEMUsagePos, false, 0, ref screenBuffer);

                                MEMUsagePos.BeginXPos = 19;
                                MEMUsagePos.BeginYPos = 17;
                                int NowValue = int.Parse(UsageMEMValueStr.Substring(UsageMEMValueStr.IndexOf('.') + 1, 1));
                                AddInfoToBuffer(MEMUsagePos, false, NowValue, ref screenBuffer);
                            }
                        }
                        //内存使用未使用信息
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Data)
                        {
                            MemoryTotal += (float)computer.Hardware[i].Sensors[j].Value;
                        }
                    }

                    //############################################ 内存总量 ##############################################################
                    int ALLMEM = (int)Math.Ceiling(MemoryTotal);
                    AllSYSMEM = ALLMEM;
                    Console.WriteLine("内存 总量:" + ALLMEM + "\r");

                    bufferPosition ALLMEMPos;
                    ALLMEMPos.BeginXPos = 31;
                    ALLMEMPos.BeginYPos = 17;
                    AddInfoToBuffer(ALLMEMPos, false, (ALLMEM / 10), ref screenBuffer);

                    ALLMEMPos.BeginXPos = 38;
                    ALLMEMPos.BeginYPos = 17;
                    AddInfoToBuffer(ALLMEMPos, false, (ALLMEM % 10), ref screenBuffer);
                }

                //**************************************************************************************************************
                if (computer.Hardware[i].HardwareType == HardwareType.GpuNvidia)
                {
                    Console.WriteLine("**************GPU************************");
                    for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                    {
                        //GPU温度
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature)
                        {
                            Console.WriteLine("GPU 温度:" + computer.Hardware[i].Sensors[j].Value.ToString() + "\r");
                            //############################################ GPU温度 ##############################################################
                            //GPU使用开始的位置
                            bufferPosition GPUTemperatureBeginPos;
                            string GPUTemperatureStr = computer.Hardware[i].Sensors[j].Value.ToString();
                            if (GPUTemperatureStr.Length == 3)
                            {
                                GPUTemperatureBeginPos.BeginXPos = 85;
                                GPUTemperatureBeginPos.BeginYPos = 21;
                                AddInfoToBuffer(GPUTemperatureBeginPos, true, 10, ref screenBuffer);

                                GPUTemperatureBeginPos.BeginXPos = 90;
                                GPUTemperatureBeginPos.BeginYPos = 21;
                                int GPUUsageTemp = int.Parse(GPUTemperatureStr.Substring(0, 1));
                                AddInfoToBuffer(GPUTemperatureBeginPos, true, GPUUsageTemp, ref screenBuffer);

                                GPUTemperatureBeginPos.BeginXPos = 103;
                                GPUTemperatureBeginPos.BeginYPos = 21;
                                GPUUsageTemp = int.Parse(GPUTemperatureStr.Substring(1, 1));
                                AddInfoToBuffer(GPUTemperatureBeginPos, true, GPUUsageTemp, ref screenBuffer);
                            }
                            if (GPUTemperatureStr.Length == 2)
                            {
                                GPUTemperatureBeginPos.BeginXPos = 90;
                                GPUTemperatureBeginPos.BeginYPos = 21;
                                int GPUUsageTemp = int.Parse(GPUTemperatureStr.Substring(0, 1));
                                AddInfoToBuffer(GPUTemperatureBeginPos, true, GPUUsageTemp, ref screenBuffer);

                                GPUTemperatureBeginPos.BeginXPos = 103;
                                GPUTemperatureBeginPos.BeginYPos = 21;
                                GPUUsageTemp = int.Parse(GPUTemperatureStr.Substring(1, 1));
                                AddInfoToBuffer(GPUTemperatureBeginPos, true, GPUUsageTemp, ref screenBuffer);
                            }
                            if (GPUTemperatureStr.Length == 1)
                            {
                                GPUTemperatureBeginPos.BeginXPos = 90;
                                GPUTemperatureBeginPos.BeginYPos = 21;
                                AddInfoToBuffer(GPUTemperatureBeginPos, true, 0, ref screenBuffer);

                                GPUTemperatureBeginPos.BeginXPos = 103;
                                GPUTemperatureBeginPos.BeginYPos = 21;
                                int GPUUsageTemp = int.Parse(GPUTemperatureStr.Substring(0, 1));
                                AddInfoToBuffer(GPUTemperatureBeginPos, true, GPUUsageTemp, ref screenBuffer);
                            }


                        }

                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.SmallData)
                        {
                            //显存总量
                            if (computer.Hardware[i].Sensors[j].Name == "GPU Memory Total")
                            {
                                Console.WriteLine("显存 总量:" + computer.Hardware[i].Sensors[j].Value.ToString() + "\r");
                                //############################################ GPU显存总量 ##############################################################
                                bufferPosition GPUALlMEMPos;
                                int GPUMEM = int.Parse(computer.Hardware[i].Sensors[j].Value.ToString()) / 1024;
                                if (GPUMEM > 10)
                                {
                                    GPUALlMEMPos.BeginXPos = 52;
                                    GPUALlMEMPos.BeginYPos = 20;
                                    AddInfoToBuffer(GPUALlMEMPos, false, 10, ref screenBuffer);
                                    GPUALlMEMPos.BeginXPos = 57;
                                    GPUALlMEMPos.BeginYPos = 20;
                                    GPUMEM = GPUMEM % 10;
                                    AddInfoToBuffer(GPUALlMEMPos, false, GPUMEM, ref screenBuffer);
                                }
                                else
                                {
                                    GPUALlMEMPos.BeginXPos = 57;
                                    GPUALlMEMPos.BeginYPos = 20;
                                    AddInfoToBuffer(GPUALlMEMPos, false, GPUMEM, ref screenBuffer);
                                }

                                GPUAllMEM = Convert.ToSingle(computer.Hardware[i].Sensors[j].Value.ToString()) / 1024;
                            }
                        }

                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Load)
                        {
                            //GPU使用率
                            if (computer.Hardware[i].Sensors[j].Name == "GPU Core")
                            {
                                Console.WriteLine("GPU 使用:" + computer.Hardware[i].Sensors[j].Value.ToString() + "\r");

                                //############################################ GPU使用 ##############################################################
                                //GPU使用开始的位置
                                bufferPosition GPUUsageBeginPos;

                                string GPUUsage = computer.Hardware[i].Sensors[j].Value.ToString();
                                if (GPUUsage.Length > 3)
                                {
                                    GPUUsage = "99";
                                }
                                if (GPUUsage.Length == 2)
                                {

                                    GPUUsageBeginPos.BeginXPos = 90;
                                    GPUUsageBeginPos.BeginYPos = 3;
                                    int GPUUsageTemp = int.Parse(GPUUsage.Substring(0, 1));
                                    AddInfoToBuffer(GPUUsageBeginPos, true, GPUUsageTemp, ref screenBuffer);

                                    GPUUsageBeginPos.BeginXPos = 103;
                                    GPUUsageBeginPos.BeginYPos = 3;
                                    GPUUsageTemp = int.Parse(GPUUsage.Substring(1, 1));
                                    AddInfoToBuffer(GPUUsageBeginPos, true, GPUUsageTemp, ref screenBuffer);
                                }
                                if (GPUUsage.Length == 1)
                                {
                                    GPUUsageBeginPos.BeginXPos = 90;
                                    GPUUsageBeginPos.BeginYPos = 3;
                                    AddInfoToBuffer(GPUUsageBeginPos, true, 0, ref screenBuffer);

                                    GPUUsageBeginPos.BeginXPos = 103;
                                    GPUUsageBeginPos.BeginYPos = 3;
                                    int GPUUsageTemp = int.Parse(GPUUsage.Substring(0, 1));
                                    AddInfoToBuffer(GPUUsageBeginPos, true, GPUUsageTemp, ref screenBuffer);
                                }
                            }

                            //显存使用
                            if (computer.Hardware[i].Sensors[j].Name == "GPU Memory")
                            {
                                Console.WriteLine("显存 使用:" + computer.Hardware[i].Sensors[j].Value.ToString() + "\r");
                                //############################################ GPU显存总量 ##############################################################

                                float GPUMEMUsage = Convert.ToSingle(computer.Hardware[i].Sensors[j].Value.ToString()) / 100;
                                GPUMEMUsage = GPUAllMEM * GPUMEMUsage;
                                bufferPosition GPUMEMUsagePos;
                                string GPUMEMUsageStr = GPUMEMUsage.ToString();
                                if (GPUMEMUsageStr.IndexOf('.') > 0)
                                {
                                    GPUMEMUsagePos.BeginXPos = 52;
                                    GPUMEMUsagePos.BeginYPos = 7;
                                    int UsageValue = int.Parse(GPUMEMUsageStr.Substring(GPUMEMUsageStr.IndexOf('.') - 1, 1));
                                    AddInfoToBuffer(GPUMEMUsagePos, false, UsageValue, ref screenBuffer);

                                    GPUMEMUsagePos.BeginXPos = 62;
                                    GPUMEMUsagePos.BeginYPos = 7;
                                    UsageValue = int.Parse(GPUMEMUsageStr.Substring(GPUMEMUsageStr.IndexOf('.') + 1, 1));
                                    AddInfoToBuffer(GPUMEMUsagePos, false, UsageValue, ref screenBuffer);
                                }
                                else if (GPUMEMUsageStr.IndexOf('.') == 0)
                                {
                                    GPUMEMUsagePos.BeginXPos = 52;
                                    GPUMEMUsagePos.BeginYPos = 7;
                                    AddInfoToBuffer(GPUMEMUsagePos, false, 0, ref screenBuffer);

                                    GPUMEMUsagePos.BeginXPos = 62;
                                    GPUMEMUsagePos.BeginYPos = 7;
                                    int UsageValue = int.Parse(GPUMEMUsageStr.Substring(GPUMEMUsageStr.IndexOf('.') + 1, 1));
                                    AddInfoToBuffer(GPUMEMUsagePos, false, UsageValue, ref screenBuffer);
                                }
                                

                            }
                        }
                        
                    }
                }
            }
            computer.Close();
        }

        static void GetBitMapinfo(string Path, ref int[,] screenBuffer)
        {
            //先加载图片
            if (System.IO.File.Exists(Path))
            {
                Bitmap backgroungMap01 = new Bitmap(Path);
                Console.WriteLine("Width" + backgroungMap01.Width);
                Console.WriteLine("Height" + backgroungMap01.Height);
                //遍历输出结果看看
                for (int i = 0; i < backgroungMap01.Height; i++)
                {
                    for (int j = 0; j < backgroungMap01.Width; j++)
                    {
                        //Console.WriteLine("Pixel " + i + " " + j + " " + backgroungMap01.GetPixel(j, i));
                        if (backgroungMap01.GetPixel(j, i).R == 0)
                        {
                            ///Console.Write("0");
                            //Console.Write("\x25a1");
                            //Console.Write("  ");
                            screenBuffer[j, i] = 0;
                        }
                        else
                        {
                            //Console.Write("1");
                            //Console.Write("\x25a0");
                            screenBuffer[j, i] = 1;
                        }
                    }
                    //Console.WriteLine("  " + i);
                }

            }
            else
            {
                Console.WriteLine("加载的图片路径不存在");
            }
        }

        static void ShowScreenBuffet(ref int[,] screenBuffer)
        {
            for (int i = 0; i < ScreenBufferHeight; i++)
            {
                for (int j = 0; j < ScreenBufferWidth; j++)
                {
                    //Console.WriteLine("Pixel " + i + " " + j + " " + backgroungMap01.GetPixel(j, i));
                    if (screenBuffer[j, i] == 0) 
                    {
                        Console.Write("  ");
                    }
                    else
                    {
                        Console.Write("\x25a0");
                    }
                }
                Console.WriteLine("  " + i);
            }
        }

        static void CommitScreenBuffer(ref int[,] screenbuffer, SerialPort sendMessagePort)
        {
            string NeedSendMessage = "@";

            for (int i = 0; i < ScreenBufferHeight; i++) 
            {
                for (int j = 0; j < ScreenBufferWidth; j++) 
                {
                    NeedSendMessage += screenbuffer[j, i].ToString();
                }
            }
            NeedSendMessage += "&";

            try
            {
                sendMessagePort.Open();
                sendMessagePort.Write(NeedSendMessage);

                Console.WriteLine("SendMessage Length:" + NeedSendMessage.Length);

                sendMessagePort.Close();
            }
            catch(Exception ex)
            {
                Console.WriteLine("您所填写的端口号不可用" + ex.Message);
                Console.ReadLine();
            }
            
        }
        static void HideWondows()
        {
            System.Threading.Thread.Sleep(3000);

            //自动隐藏窗口
            Console.Title = "MiniOLED";
            IntPtr ParenthWnd = new IntPtr(0);
            IntPtr et = new IntPtr(0);
            ParenthWnd = FindWindow(null, "MiniOLED");
            ShowWindow(ParenthWnd, 0);
        }
        static void Main(string[] args)
        {

            HideWondows();

            //读取工作路径
            string COMConfigFile = System.IO.Directory.GetCurrentDirectory() + "\\COMCon.txt";
            //判断配置文件是否存在
            if (System.IO.File.Exists(COMConfigFile))
            {
                //读取端口号
                string COMNum = System.IO.File.ReadAllText(COMConfigFile);
                COMNum = "COM" + COMNum;
                //配置好COM端口号
                SerialPort serialPort1 = new SerialPort(COMNum, 1500000, Parity.None, 8, StopBits.One);

                //用于标记读取第几张背景图片
                int BackPictureNum = 0;
                //初始化存储内容的数组
                int [,] screenBuffer =new int [ScreenBufferWidth, ScreenBufferHeight];

                //开始获取信息
                while (true)
                {
                    //测试使用
                    Console.Clear();
                    //读取背景图片
                    BackPictureNum = BackPictureNum % 4;
                    BackPictureNum += 1;
                    COMConfigFile = System.IO.Directory.GetCurrentDirectory() + "\\Resource\\Background\\di" + BackPictureNum + ".bmp";
                    GetBitMapinfo(COMConfigFile, ref screenBuffer);

                    
                    //获取系统参数
                    GetSystemInfo(ref screenBuffer);

                    //显示屏幕内容
                    //ShowScreenBuffet(ref screenBuffer);

                    CommitScreenBuffer(ref screenBuffer, serialPort1);
                    

                    System.Threading.Thread.Sleep(500);
                }
            }
            else
            {
                Console.WriteLine("程序所在目录下找不到配置COM端口号的COMCon.txt文件");
            }

            Console.ReadLine();
        }
    }
}
