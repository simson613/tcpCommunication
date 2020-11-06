using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace AutoUpdate
{
    public partial class AutoUpdate : Form
    {
        TcpClient client;
        NetworkStream stream;

        /// <summary>
        /// 생성자
        /// </summary>
        public AutoUpdate()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Form Shown Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoUpdate_Shown(object sender, EventArgs e)
        {
            int bindPort = 56351;
            while (true)
            {
                if (portAvailable(bindPort))
                    break;
                else
                    bindPort++;
            }

            string serverIp = "192.168.0.8";
            const int serverPort = 5425;

            try
            {
                IPEndPoint clientAddress = new IPEndPoint(GetIPAddress(), bindPort);
                IPEndPoint serverAddress = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
                client = new TcpClient(clientAddress);
                client.Connect(serverAddress);

                stream = client.GetStream();

                byte[] bytes = new byte[10];
                bytes = Encoding.Default.GetBytes("AutoUpdate");
                stream.Write(bytes, 0, bytes.Length);   //flag Send

                InitAutoUpdate();

                MessageBox.Show("AutoUpdate Completed!");
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
            catch (ArgumentNullException ne)
            {
                MessageBox.Show(ne.Message);
            }

            if (stream != null)
            {
                stream.Close();
                stream.Dispose();
                stream = null;
            }
            if (client != null)
            {
                client.Close();
                client = null;
            }
            this.Close();
        }
        
        /// <summary>
        /// AutoUpdate
        /// </summary>
        private void InitAutoUpdate()
        {
            try
            {
                string dir = Directory.GetCurrentDirectory() + @"\";
                //string dir = @"C:\TestFolder\FileReceive\";
                byte[] bytes = new byte[1024];
                int byteLength = stream.Read(bytes, 0, bytes.Length);   //File Info Receive
                string fileInfo = Encoding.Default.GetString(bytes, 0, byteLength);

                if (fileInfo != "NoFiles")
                {
                    string[] files = fileInfo.Split('/');
                    Stack<string> fileStack = new Stack<string>();
                    Dictionary<string, string> dicFile = new Dictionary<string, string>();  //Path, Length
                    int totalSize = 0;
                    int byteSize = Convert.ToInt32(files[files.Length - 1]);
                    long speedCal = byteSize / 8192;
                    this.lblCount.Text = string.Format("0 / {0:N0}", files.Length);

                    for (int i = 0; i < files.Length - 1; i++)
                    {
                        string[] info = files[i].Split('*');

                        if (FileCompare(dir + info[0], info[2]))
                        {
                            dicFile.Add(info[0], info[2]);
                            fileStack.Push(info[0]);
                            totalSize += Convert.ToInt32(info[2]);
                        }
                    }

                    pbrTotal.Maximum = totalSize;
                    this.lblTotal.Text = string.Format("0 / {0:N0}", files.Length);
                    int index = 0;
                    long speed = 0;
                    Stopwatch sw = new Stopwatch();

                    if (dicFile.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> item in dicFile)
                        {
                            bytes = new byte[256];
                            bytes = Encoding.Default.GetBytes(item.Key);
                            stream.Write(bytes, 0, bytes.Length);   //File Request Send

                            int indexCount = (Convert.ToInt32(item.Value) / byteSize) + 1;
                            byte[] fileBytes = new byte[byteSize];  //8192
                            using (FileStream fs = new FileStream(dir + item.Key, FileMode.Create, FileAccess.Write))
                            {
                                int count = 0;
                                pbrNow.Value = 0;
                                pbrNow.Maximum = Convert.ToInt32(item.Value);
                                this.lblNow.Text = string.Format("0 / {0:N0}", pbrNow.Maximum);
                                sw.Reset();

                                for (int j = 0; j < indexCount; j++)
                                {
                                    sw.Start();

                                    count = stream.Read(fileBytes, 0, fileBytes.Length);
                                    fs.Write(fileBytes, 0, count);  //File Receive

                                    sw.Stop();

                                    pbrNow.Value += count;
                                    pbrTotal.Value += count;
                                    speed = sw.ElapsedMilliseconds == 0 ? speed : pbrNow.Value * speedCal / sw.ElapsedMilliseconds;

                                    this.lblNow.Text = string.Format("{0:N0} / {1:N0}", pbrNow.Value, pbrNow.Maximum);
                                    this.lblTotal.Text = string.Format("{0:N0} / {1:N0}", pbrTotal.Value, pbrTotal.Maximum);
                                    this.lblSpeed.Text = string.Format("{0:N2} kbps", speed);
                                    Application.DoEvents();
                                }
                                this.lblCount.Text = string.Format("{0:N0} / {1:N0}", ++index, files.Length);
                                Application.DoEvents();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        /// <summary>
        /// Client의 IP 주소를 가져옵니다.
        /// </summary>
        /// <returns></returns>
        private IPAddress GetIPAddress()
        {
            IPHostEntry host = null;

            try
            {
                host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                        return ip;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                host = null;
            }
            return null;
        }

        /// <summary>
        /// Port를 사용할 수 있는지 확인합니다.
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        private bool portAvailable(int port)
        {
            IPGlobalProperties iPGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = iPGlobalProperties.GetActiveTcpConnections();
            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
            {
                if (tcpi.LocalEndPoint.Port == port)
                    return false;
            }
            return true;
        }

        #region FileCompare
        /// <summary>
        /// File Compare
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="lastAccessTime"></param>
        /// <returns></returns>
        public bool FileCompare(string fileName, string length)
        {
            if (File.Exists(fileName))
            {
                FileInfo file = new FileInfo(fileName);
                if (length.Equals(file.Length.ToString()))
                    return false;
            }
            return true;
        }
        #endregion
    }
}
