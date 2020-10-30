using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileServer
{
    public partial class FileServer : Form
    {
        public FileServer()
        {
            InitializeComponent();
            InitTCP();
        }

        public void InitTCP()
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5425);

            TcpListener server = new TcpListener(ip);
        }
    }
}
