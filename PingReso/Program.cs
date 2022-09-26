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
using System.Net.Mail;
using System.Reflection.Metadata;
using Microsoft.VisualBasic;
using System.Net.Http;

namespace PingReso
{
    public class Domain
    {
        public string Domains { get; set; }
    }


    public class Program
    {

        static string ReadJSON()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory
                + @"appsettings.json");
            return path;
        }

        static void sendEmail(string email, string body, string error)
        {
            if (String.IsNullOrEmpty(email))
                return;
            try
            {
                MailMessage mail = new MailMessage();
                mail.To.Add(email);
                mail.From = new MailAddress("trieuhchse161563@fpt.edu.vn");
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void Main(string[] args)
        {

            Ping p = new Ping();
            bool check = true;
            string URL = "";

            //Read Json file
            string path = ReadJSON();

            using (StreamReader sr = new StreamReader(path))
            {

                var json = sr.ReadToEnd();
                var list = JsonConvert.DeserializeObject<Domain>(json);

                string[] convertList = list.Domains.Split(",");
                List<string> newList = new List<string>(convertList);
                List<string> ERROR_URL = new List<string>();
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
                                        URL = item.ToString();
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
                                            }
                                        }
                                        foreach (var emailsend in ERROR_URL)
                                        {
                                            string email = "trieuhchse161563@fpt.edu.vn";
                                            string body = $"Error at link {emailsend}";
                                            string error = emailsend;
                                            sendEmail(email, body, error);
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


/*
    - dòng 85 tạo 1 list chứa các error URL
    - dòng 120 add các url bị lỗi vô\
    - dòng 133 tới 140 dùng foreach lưu các lỗi vô 1 file.
 */


