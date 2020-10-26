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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoUpdate_LM
{
    public partial class AutoUpdate : Form
    {
        public AutoUpdate()
        {
            InitializeComponent();
        }

        private void AutoUpdate_Shown(object sender, EventArgs e)
        {
            InitControls();
            Init();
        }

        private void InitControls()
        {
            pbrFileUpdate.Style = ProgressBarStyle.Continuous;
            pbrFileUpdate.Minimum = 0;
            pbrFileUpdate.Maximum = 100;
            pbrFileUpdate.Value = 0;
            pbrFileUpdate.Enabled = true;
        }

        private string GetIPAddress()
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

        private void Init()
        {
            //string bindIP = "14.32.21.123";
            string bindIP = "127.0.0.1"; // GetIPAddress();
            int bindPort = 5605;
            ////string serverIp = "14.32.21.123";
            string serverIp = "127.0.0.1"; // GetIPAddress();
            const int serverPort = 5425;

            try
            {
                IPEndPoint clientAddress = new IPEndPoint(IPAddress.Parse(bindIP), bindPort);
                IPEndPoint serverAddress = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
                TcpClient client = new TcpClient(clientAddress);
                client.Connect(serverAddress);

                NetworkStream stream = client.GetStream();
                string dir = Directory.GetCurrentDirectory();

                byte[] bytes = new byte[3];
                int byteLength = stream.Read(bytes, 0, bytes.Length);   //FileCount Receive
                int fileCount = Convert.ToInt32(Encoding.Default.GetString(bytes, 0, byteLength));

                if (fileCount < 1)
                {
                    Console.WriteLine("No files");
                }
                
                pbrFileUpdate.Step = 100 / fileCount;
                lblUpdate.Text = "Updating... (0 / " + fileCount + ")";

                bytes = Encoding.Default.GetBytes("O");
                stream.Write(bytes, 0, 1);   //Request Send

                for (int i = 0; i < fileCount; i++)
                {
                    string[] serverFile = null;
                    bytes = new byte[256];
                    byteLength = stream.Read(bytes, 0, bytes.Length);   //Fileinfo Receive
                    serverFile = Encoding.Default.GetString(bytes, 0, byteLength).Split('\\');
                    string fileName = dir + @"\" + serverFile[0];

                    bytes = Encoding.Default.GetBytes(FileCompare(fileName, serverFile[1]));
                    stream.Write(bytes, 0, 1);   //Request Send

                    if (Encoding.Default.GetString(bytes) == "O")
                    {
                        int byteCount = 8192;
                        int indexCount = (Convert.ToInt32(serverFile[1]) / byteCount) + 1;
                        byte[] fileBytes = new byte[byteCount];  //8192
                        FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                        int count = 0;

                        for (int j = 0; j < indexCount; j++)
                        {
                            count = stream.Read(fileBytes, 0, fileBytes.Length);
                            fs.Write(fileBytes, 0, count);  //File Receive
                        }

                        fs.Close();
                        stream.Write(bytes, 0, 1);   //Request Send
                    }
                    lblUpdate.Text = "Updating... (" + (i + 1) + " / " + fileCount + ")";
                    pbrFileUpdate.PerformStep();
                    Application.DoEvents();
                    //lblUpdate.Update();
                }

                pbrFileUpdate.Value = 100;
                pbrFileUpdate.Enabled = false;
                stream.Close();
                client.Close();
                this.Close();
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
            
            Console.WriteLine("클라이언트를 종료합니다.");
        }

        private string FileCompare(string fileName, string serverFile)
        {
            if (File.Exists(fileName))
            {
                FileInfo file = new FileInfo(fileName);
                if (serverFile == file.Length.ToString())
                {
                    return "X"; //동일버전
                }
            }
            return "O"; //다른버전
        }
    }
}
