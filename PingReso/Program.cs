using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;


namespace PingReso
{
    class Program
    {

        public class Domain
        {
            public string Domains { get; set; }
        }
       

        static void Main(string[] args)
        {

            Ping p = new Ping();
            bool check = true;

            //Read Json file
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory
                + @"appsettings.json");

            using (StreamReader sr = new StreamReader(path))
            {

                var json = sr.ReadToEnd();
                var list = JsonConvert.DeserializeObject<Domain>(json);

                string[] convertList = list.Domains.Split(",");
                List<string> newList = new List<string>(convertList);


                Dictionary<string, int> hashmap = newList.Distinct().ToDictionary(x => x, x => 0);
                for (; ; )
                {
                    foreach (var item in hashmap)
                    {
                        //Console.WriteLine(list);
                        while (check)
                        {
                            if (hashmap[item.Key] < 2)
                            {


                                try
                                {
                                    PingReply rep = p.Send(item.Key, 1000);
                                    if (rep.Status.ToString() == "Success")
                                    {
                                        Console.ForegroundColor = ConsoleColor.Cyan;

                                        Console.WriteLine("Reply from: " + rep.Address + "Bytes=" + rep.Buffer.Length + " Time=" +
                                            rep.RoundtripTime + " TTL=" + rep.Options.Ttl + " Routers=" + (128 - rep.Options.Ttl) + " Status=" +
                                            rep.Status + " Server" + item);
                                        Thread.Sleep(1000);

                                    }

                                    check = false;


                                }
                                catch (Exception ex)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;

                                    hashmap[item.Key] = item.Value + 1;
                                    Console.WriteLine("Error:{0}, {1}", item.Key, hashmap[item.Key]);
                                    if (hashmap[item.Key] == 2)
                                    {

                                        Console.WriteLine("Error at server: {0}",  item.Key);
                                        


                                        string logFolderName = "logs/";
                                        if (!Directory.Exists(logFolderName))
                                        {
                                            Directory.CreateDirectory(logFolderName);
                                        }
                                        string logFileName = "";
                                        DateTime now = DateTime.Now;
                                        logFileName = String.Format("{0}_{1}_{2}_log.txt", now.Day, now.Month, now.Year);
                                        string fullFileLog = Path.Combine(logFolderName, logFileName);

                                        using (StreamWriter sw = new StreamWriter(fullFileLog))
                                        {
                                            sw.WriteLine(String.Format("Error occurs at: {0}", now));
                                            sw.WriteLine(String.Format("Error: {0}", ex.Message));
                                        }

                                    }



                                    Thread.Sleep(1500);


                                    check = false;


                                }
                            }
                            else
                            {
                                check = false;
                            }
                        }

                        check = true;
                    }

                }

            }

        }
    }
}



