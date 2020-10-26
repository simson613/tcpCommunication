using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileServer
{
    class Program
    {
        public static void Main(string[] args)
        {
            //if (args.Length < 1)
            //{
            //    Console.Write("FileServer.exe <IP Address>");
            //    // args[0] = "127.0.0.1"
            //    // args[1] = @"C:\AiHelper LiveM\AiHelper LiveM\transfer"
            //}

            //string bindIp = "211.218.213.69"; //"127.0.0.1";
            string bindIp = "127.0.0.1";
            const int bindPort = 5425;
            TcpListener server = null;
            Console.WriteLine(bindIp);
            try
            {
                IPEndPoint localAddress = new IPEndPoint(IPAddress.Parse(bindIp), bindPort);
                server = new TcpListener(localAddress);
                server.Start();
                Console.WriteLine("Server Start");

                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();

                    if (client.Connected)
                        Console.WriteLine("Add Client Connection: {0} ", ((IPEndPoint)client.Client.RemoteEndPoint).ToString());

                    Thread tcpHandlerThread = new Thread(new ParameterizedThreadStart(tcpHandler));
                    tcpHandlerThread.Start(client);
                }

            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                server.Stop();
            }

            Console.WriteLine("Server Shutdown");
        }

        private static string GetIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }
            return localIP;
        }

        public static void tcpHandler(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream stream = tcpClient.GetStream();
            //List<string> files = new List<string>(Directory.EnumerateFiles(@"C:\Users\FLEXSYS\Desktop\AutoUpdateTest"));
            List<string> files = new List<string>(Directory.EnumerateFiles(@"C:\Users\simso\OneDrive\바탕 화면\AutoUpdateTest"));

            byte[] bytes = new byte[3];
            bytes = Encoding.Default.GetBytes(files.Count.ToString());
            stream.Write(bytes, 0, bytes.Length);   //Filecount Send

            bytes = new byte[3];
            stream.Read(bytes, 0, 1);   //Request Receive

            foreach (string fileName in files)
            {
                FileInfo file = new FileInfo(fileName);
                bytes = new byte[256];
                bytes = Encoding.Default.GetBytes(file.Name + "\\" + file.Length.ToString());
                stream.Write(bytes, 0, bytes.Length);   //FileInfo Send

                bytes = new byte[3];
                stream.Read(bytes, 0, 1);       //Request Receive

                Console.WriteLine(Encoding.Default.GetString(bytes));

                if (Encoding.Default.GetString(bytes) == "O")
                {
                    byte[] fileBytes = new byte[8192];
                    FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                    int count = 0;

                    while ((count = fs.Read(fileBytes, 0, fileBytes.Length)) > 0)
                    {
                        try
                        {
                            stream.Write(fileBytes, 0, count);      //File Send
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                    fs.Close();
                    Console.WriteLine("File Send Info {0} / {1}", fileName, ((IPEndPoint)tcpClient.Client.RemoteEndPoint).ToString());
                    stream.Read(bytes, 0, 1);       //Request Receive
                }
            }

            Console.WriteLine("Disconnect Client {0}", ((IPEndPoint)tcpClient.Client.RemoteEndPoint).ToString());
            stream.Close();
            tcpClient.Close();
        }
    }
}
