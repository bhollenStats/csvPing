﻿using System;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

namespace csvPing
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length < 1)
            {
                Console.WriteLine("Syntax: csvPing hostname|IPAddress [OPTIONS]");
                Console.WriteLine("");
                Console.WriteLine("   -t=VALUE [Timeout in milliseconds, default = 5000]");
                Console.WriteLine("   -i=VALUE [Ping interval in milliseconds, default = 1000]");
                Console.WriteLine("   -s=VALUE [Field separator, default = ','");
                Console.WriteLine("");
            }
            else
            {
                string outputSeparator = ",";
                long msBetweenPings = 5000;
                int msTimeout = 5000;

                for (int x = 1; x < args.Length; x++)
                {
                    if (args[x].Contains("-i="))
                        msBetweenPings = Convert.ToInt64(args[x].Substring(3)); 
                    if (args[x].Contains("-s="))
                        outputSeparator = Convert.ToString(args[x].Substring(3)); 
                    if (args[x].Contains("-t="))
                        msTimeout = Convert.ToInt32(args[x].Substring(3)); 
                }

                long respTime = 0;
                PingStats ps = new PingStats(Convert.ToDouble(msTimeout));

                Console.WriteLine("Host" + outputSeparator + 
                    "LocalTime" + outputSeparator +
                    "PingResponseTime" + outputSeparator +
                    "Minimum" + outputSeparator +
                    "Maximum" + outputSeparator +
                    "Average" + outputSeparator +
                    "TimeoutCount_" + msTimeout + "ms");

                while (true)
                {
                    respTime = PingHost(args[0], msTimeout);
                    ps.Add(respTime);
                    string rightNow = string.Format("{0:u}", DateTime.Now);
                    Console.WriteLine(args[0] + outputSeparator +
                        rightNow + outputSeparator + 
                        respTime.ToString("F0", CultureInfo.InvariantCulture) + outputSeparator + 
                        ps.Min.ToString("F0", CultureInfo.InvariantCulture) + outputSeparator + 
                        ps.Max.ToString("F0", CultureInfo.InvariantCulture) + outputSeparator + 
                        ps.Mean.ToString("F3", CultureInfo.InvariantCulture) + outputSeparator + 
                        ps.TimeoutCount.ToString("F0", CultureInfo.InvariantCulture));
                    Thread.Sleep(Convert.ToInt16(msBetweenPings));
                }
            }
        }
        public static long PingHost(string nameOrAddress, int timeout)
        {
            bool pingable = false;
            Ping pinger = null;
            long responseTime = 0;

            PingOptions options = new PingOptions();
            options.DontFragment = true;
            options.Ttl = 300;

            try
            {
                pinger = new Ping();
                string data = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                PingReply reply = pinger.Send(nameOrAddress, timeout, buffer, options);
                responseTime = reply.RoundtripTime;
                pingable = reply.Status == IPStatus.Success;

                if (!pingable)
                {
                    responseTime = 5000;
                }
            }
            catch (PingException)
            {
                responseTime = 5000;
            }
            finally
            {
                if (pinger != null)
                {
                    pinger.Dispose();
                }
            }

            return responseTime;
        }
    }

    class PingStats
    {
        public PingStats(double timeoutValue) 
        {
            m_Count = 0;
            m_Sum = 0;
            m_Min = 1000000.0;
            m_Max = 0;
            m_Sum = 0;
            m_Mean = 0;
            m_TimeoutCount = 0;
            m_Timeout = timeoutValue;
        }
        private double m_Count;
        private double m_Sum;
        private double m_Min;
        private double m_Max;
        private double m_Mean;
        private double m_Timeout;
        private double m_TimeoutCount;
        public double Min { get => m_Min; }
        public double Max { get => m_Max; }
        public double Mean { get => m_Mean; }
        public double TimeoutCount { get => m_TimeoutCount; }
        public void Add(double pingTime)
        {
            if (pingTime < m_Timeout) {
                m_Count++;
                m_Sum += pingTime;
                m_Mean = m_Sum / m_Count;
                if (pingTime < m_Min)
                    m_Min = pingTime;
                if (pingTime > m_Max)
                    m_Max = pingTime;
            } else {
                m_TimeoutCount++;
            } 
        }
    }
}