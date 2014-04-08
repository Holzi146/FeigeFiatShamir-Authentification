﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Threading;
using System.Xml.Serialization;
using System.IO;

namespace Alice
{
    class Program
    {
        /* Network stuff */
        static Socket socketBob, socketVC;
        static EndPoint endp;
        static Thread thread;
        static MemoryStream stream;
        static XmlSerializer serializer;
        /* constants */
        const string IP_VC = "127.0.0.1";
        const int PORT_BOB = 5555;
        const int PORT_VC = 5554;
        /* Feige-Fiat-Shamir stuff */

        static void Main(string[] args)
        {
            Console.Title = "Alice";
            socketBob = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socketVC = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            /* listen on port 5555 */
            socketBob.Bind(new IPEndPoint(IPAddress.Any, PORT_BOB));
            socketVC.Bind(new IPEndPoint(IPAddress.Any, PORT_VC));

            thread = new Thread(ReceiveFromBob);
            thread.IsBackground = true;
            thread.Start();

            thread = new Thread(ReceiveFromVC);
            thread.IsBackground = true;
            thread.Start();

            Console.ReadLine();
        }

        private static void ReceiveFromBob()
        {
            Console.WriteLine("Waiting for connections...");

            while (true)
            {
                EndPoint endp = new IPEndPoint(IPAddress.Any, 0);

                /* check if valid Data object */
                serializer = new XmlSerializer(typeof(Data));
                byte[] buffer = new byte[1024];
                socketBob.ReceiveFrom(buffer, ref endp);
                stream = new MemoryStream(buffer);
                Data request = (Data)serializer.Deserialize(stream);
                stream.Close();

                IPEndPoint ipendp = (IPEndPoint)endp;
                Console.WriteLine(ipendp.Address + ": Requested authentification");

                /* send request for w's to the authentification center */
                serializer = new XmlSerializer(typeof(Data));
                stream = new MemoryStream();
                serializer.Serialize(stream, request);
                socketBob.SendTo(stream.ToArray(), new IPEndPoint(IPAddress.Parse(IP_VC), 5557));
            }
        }

        private static void ReceiveFromVC()
        {
            while (true)
            {
                /* get the w' of Bob */
            }
        }
    }

    [Serializable]
    public class Data
    {
        public int id { get; set; }
        /* BigInteger is not serializable --> Parse to String */
        public string n { get; set; }
        public string[] w { get; set; }
        public int k { get; set; }
        public int t { get; set; }

        public bool Compare(Data obj)
        {
            if (id == obj.id && n == obj.n && w.SequenceEqual(obj.w) && k == obj.k && t == obj.t)
                return true;
            return false;
        }
    }
}
