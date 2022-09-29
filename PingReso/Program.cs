using System.Threading;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Mail;
using System.Reflection.Metadata;
using Microsoft.VisualBasic;
using System.Net.Http;
using System.IO;
using System;
using System.Net.NetworkInformation;

namespace Ping_2
{
    public class Config
    {
        public string Domains { get; set; }
        public string AdminEmail { get; set; }
        public string AdminEmailPassword { get; set; }
    }

    public class Program
    {
        static string ReadJSON()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory
                + @"appsettings.json");
            return path;
        }

        static bool SendEmail(string receiver, string body, string error)
        {
            if (String.IsNullOrEmpty(receiver))
            {
                return false;
            }

            try
            {
                MailMessage mail = new MailMessage();
                mail.To.Add(receiver);
                mail.From = new MailAddress("resopingsend@reso.vn");
                mail.Subject = $"Link : {error}, server down, MAY DAY!";

                mail.Body = body;

                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com"; //Or Your SMTP Server Address
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential("trieuhchse161563@fpt.edu.vn", "0775711152haitrieu"); // ***use valid credentials***
                smtp.Port = 587;

                //Or your Smtp Email ID and Password
                smtp.EnableSsl = true;
                smtp.Send(mail);
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Success");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        static void LogCreate()
        {

        }

        static void PingServer()
        {
            Ping p = new Ping();
            //Read Json file
            bool check = true;
            string path = ReadJSON();

            using (StreamReader sr = new StreamReader(path))
            {

                var json = sr.ReadToEnd();
                var list = JsonConvert.DeserializeObject<Config>(json);

                string[] convertList = list.Domains.Split(",");
                List<string> newList = new List<string>(convertList);
                List<string> ERROR_URL = new List<string>();
                Dictionary<string, int> hashmap = newList.Distinct().ToDictionary(x => x, x => 0);
                while (check)
                {
                    foreach (var item in hashmap)
                    {
                        //Console.WriteLine(list);
                        if (hashmap[item.Key] < 2)
                        {
                            try
                            {
                                PingReply rep = p.Send(item.Key, 1000); // send ping to url 
                                if (rep.Status.ToString() == "Success") // url send back the result
                                {
                                    Console.ForegroundColor = ConsoleColor.Cyan;

                                    Console.WriteLine("Reply from: " + rep.Address + "Bytes=" + rep.Buffer.Length + " Time=" +
                                        rep.RoundtripTime + " TTL=" + rep.Options.Ttl + " Routers=" + (128 - rep.Options.Ttl) + " Status=" +
                                        rep.Status + " Server" + item); //display the information result to console
                                    Thread.Sleep(1000);
                                }
                            }
                            catch (Exception ex)
                            {
                                var receive = "trieuhchse161563@fpt.edu.vn";
                                var body = "";
                                var error = "";

                                Console.ForegroundColor = ConsoleColor.Red;

                                hashmap[item.Key] = item.Value + 1;
                                Console.WriteLine("Error:{0}, {1}", item.Key, hashmap[item.Key]);
                                if (hashmap[item.Key] == 2)
                                {
                                    Console.WriteLine("Error at server: {0}", item.Key);
                                    ERROR_URL.Add(item.Key);
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
                                        foreach (var listerr in ERROR_URL)
                                        {
                                            sw.WriteLine(String.Format("Error occurs at: {0}", now));
                                            sw.WriteLine(String.Format("Error: {0}", ex.Message));
                                            sw.WriteLine(String.Format("Error URL: {0}", listerr));
                                            sw.WriteLine();

                                            //Lỗi ở khúc này, in số lần loạn xà ngầu địt mẹ code lồn

                                            body = $"Error at link {listerr}";
                                            error = listerr;
                                            SendEmail(receive, body, error);
                                        }
                                    }
                                }
                                Thread.Sleep(1500);
                            }
                        }
                        check = true;
                    }

                }

            }
        }

        static void Main(string[] args)
        {
            PingServer();
        }
    }
}
