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
                var list = JsonConvert.DeserializeObject<List<string>>(json);
                //var hashmap = new Dictionary<string, int>();
                //var hashmap = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                var res = list.Distinct().ToDictionary(x => x, x => x);
                //foreach (var item in list)
                //{
                //    hashmap.Add(item, 0);
                //}

                for (int j = 0; j< 2; j++ )
                {
                    foreach (var item in res)
                    {
                        //Console.WriteLine(list);
                        while (check)
                        {
                            try
                            {
                                PingReply rep = p.Send(res.ToString(), 1000);
                                if (rep.Status.ToString() == "Success")
                                {
                                    Console.ForegroundColor = ConsoleColor.Cyan;

                                    Console.WriteLine("Reply from: " + rep.Address + "Bytes=" + rep.Buffer.Length + " Time=" +
                                        rep.RoundtripTime + " TTL=" + rep.Options.Ttl + " Routers=" + (128 - rep.Options.Ttl) + " Status=" +
                                        rep.Status + " Server" + res);
                                    Thread.Sleep(1000);

                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("Request Timed Out");
                                    Thread.Sleep(100);
                                }

                            check = false;
                                

                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Server do not exist!!!!");
                               
                               

                                //string logFolderName = "logs/";
                                //if (!Directory.Exists(logFolderName))
                                //{
                                //    Directory.CreateDirectory(logFolderName);
                                //}
                                //string logFileName = "";
                                //DateTime now = DateTime.Now;
                                //logFileName = String.Format("{0}_{1}_{2}_log.txt", now.Day, now.Month, now.Year);
                                //string fullFileLog = Path.Combine(logFolderName, logFileName);

                                //using (StreamWriter sw = new StreamWriter(fullFileLog))
                                //{
                                //    sw.WriteLine(String.Format("Error occurs at: {0}", now));
                                //    sw.WriteLine(String.Format("Error: {0}", ex.Message));
                                //}
                                Thread.Sleep(1500);
                                //lưu vô 1 list
                      
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

