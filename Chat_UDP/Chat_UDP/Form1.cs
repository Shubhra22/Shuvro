using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace Chat_UDP
{
    public partial class Form1 : Form
    {
        Socket sckCommunication;
        EndPoint epLocal, epRemote;
        byte[] buffer;
        public Form1()
        {
            InitializeComponent();
            // set up socket
            sckCommunication = new Socket(AddressFamily.InterNetwork,
                                SocketType.Dgram, ProtocolType.Udp);
            sckCommunication.SetSocketOption(SocketOptionLevel.Socket,
                                SocketOptionName.ReuseAddress, true);

            // get own ip
            txtLocalIp.Text = getIp();
            txtFriendsIp.Text = getIp();
        }
        /*static string getIp()
         {
             string hostname = System.Net.Dns.GetHostName();
             IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(hostname);
             IPAddress[] addr = ipEntry.AddressList;
             return addr[addr.Length - 1].ToString();
         }*/
        // return the own ip
        private string getIp()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }



        private void btnStart_Click(object sender, EventArgs e)
        {
            // bind socket                        
            epLocal = new IPEndPoint(IPAddress.Parse(txtLocalIp.Text),
                                    Convert.ToInt32(txtLocalPort.Text));
            sckCommunication.Bind(epLocal);

            // connect to remote ip and port 
            epRemote = new IPEndPoint(IPAddress.Parse(txtFriendsIp.Text),
                                    Convert.ToInt32(txtFriendsPort.Text));
            sckCommunication.Connect(epRemote);

            // starts to listen to an specific port
            buffer = new byte[1464];
            sckCommunication.BeginReceiveFrom(buffer, 0, buffer.Length,
                                     SocketFlags.None, ref epRemote,
                            new AsyncCallback(OperatorCallBack), buffer);

            // release button to send message
            btnSend.Enabled = true;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            // converts from string to byte[]
            System.Text.ASCIIEncoding enc =
                    new System.Text.ASCIIEncoding();
            byte[] msg = new byte[1464];
            msg = enc.GetBytes(txtMessage.Text);

            // sending the message
            sckCommunication.Send(msg);

            // add to listbox
            //listBox1.Items.Add("You: " + txtMessage.Text);
            textBox1.Text = "You :" + txtMessage.Text;
            // clear txtMessage
            txtMessage.Clear();
        }
       private void OperatorCallBack(IAsyncResult ar)
{
    try
    {
        int size = sckCommunication.EndReceiveFrom(ar, ref epRemote);

        // check if theres actually information
        if (size > 0)
        {
            // used to help us on getting the data
            byte[] aux = new byte[1464];

            // gets the data
            aux = (byte[])ar.AsyncState;

            // converts from data[] to string
            System.Text.ASCIIEncoding enc = 
                                    new System.Text.ASCIIEncoding();
            string msg = enc.GetString(aux);

            // adds to listbox
           // listBox1.Items.Add("Friend: " + msg);  
            textBox1.Text="Friend: " + msg;    
        }

        // starts to listen again
        buffer = new byte[1464];
        sckCommunication.BeginReceiveFrom(buffer, 0, 
                            buffer.Length, SocketFlags.None,
            ref epRemote, new AsyncCallback(OperatorCallBack), buffer);
    }
    catch (Exception exp)
    {
        MessageBox.Show(exp.ToString());
    }
}

        }
    }

