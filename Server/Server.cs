using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public partial class Server : Form
    {

        public Server()
        {
            InitializeComponent();
            Init();
        }

        public void Init()
        {
            string bindIp = "127.0.0.1";
            const int bindPort = 5425;
            TcpListener server = null;

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

        public void tcpHandler(Object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream stream = tcpClient.GetStream();
            //List<string> files = new List<string>(Directory.EnumerateFiles(@"C:\AiHelper LiveM\AiHelper LiveM\transfer"));
            List<string> files = new List<string>(Directory.EnumerateFiles(@"C:\Users\simso\OneDrive\바탕 화면\AutoUpdateTest"));

            byte[] bytes = new byte[3];
            bytes = Encoding.Default.GetBytes(files.Count.ToString());
            stream.Write(bytes, 0, bytes.Length);   //Filecount Send

            bytes = new byte[1];
            stream.Read(bytes, 0, 1);   //Request Receive

            foreach (string fileName in files)
            {
                FileInfo file = new FileInfo(fileName);
                bytes = new byte[256];
                bytes = Encoding.Default.GetBytes(file.Name + "\\" + file.Length.ToString());
                stream.Write(bytes, 0, bytes.Length);   //FileInfo Send

                bytes = new byte[1];
                stream.Read(bytes, 0, 1);       //Request Receive

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
            
            stream.Close();
            tcpClient.Close();
        }
    }
}
