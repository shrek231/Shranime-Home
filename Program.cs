using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading;
using RestSharp;

namespace Render {
    class Program {
        public static int resp;
        public static string blendpath = null;
        public static string sys = null;
        public static OperatingSystem os;
        public static bool running = true;
        public static string read = "continue";
        public static void Main() {
            Thread thread1 = new Thread(t1);
            thread1.Start();
            while (true)
            {
                string quit = Console.ReadLine().ToLower();
                if (quit == "q") {
                    exit();
                }
            }
        }
        static void t1() {
            Console.Write("\n");
            Console.WriteLine("------------------------------");
            Console.WriteLine("----------Render@Home---------");
            Console.WriteLine("press 'q' then [ENTER] to quit");
            Console.WriteLine("------------------------------");
            Console.Write("\n");
            os = Environment.OSVersion;//get os
            string ostring = os.ToString();
            string[] osplit = ostring.Split(' ');
            if (osplit[0] == "Microsoft") {
                sys = "windows";
                Console.Write("\n");
                Console.WriteLine("------------------------------");
                Console.WriteLine("----------Render@Home---------");
                Console.WriteLine("remember to add belnder to path");
                Console.WriteLine("------------------------------");
                Console.Write("\n");
            } if (osplit[0] == "Unix") {
                sys = "linux";
            } else {
                Console.WriteLine("OS not recognized, continuing as Unix ");
            }
            string path = "none"; //make dir
            if (sys == "windows") {
                path = AppDomain.CurrentDomain.BaseDirectory+@"img\";
                blendpath = AppDomain.CurrentDomain.BaseDirectory+@"ble\";
            } else {
                path = AppDomain.CurrentDomain.BaseDirectory+"img/";
                blendpath = AppDomain.CurrentDomain.BaseDirectory+"ble/";
            } if (Directory.Exists(path)) {
                Console.WriteLine("");
            } else {
                DirectoryInfo dire = Directory.CreateDirectory(path);
                DirectoryInfo di = Directory.CreateDirectory(blendpath);
            }
            int timeout = 1000; //start loop
            int fail = 0;
            bool redownload = true;
            while (running) {
                if (fail > 1) {
                    Console.Clear();
                    Console.WriteLine("no new frames, waiting 30 seconds.");
                    timeout = 30000;
                }
                Thread.Sleep(timeout);
                Console.WriteLine("\nRequesting new frame");
                var client = new RestClient("https://BlenderRenderServer.youtubeadminist.repl.co/requestFrame");
                var request = new RestRequest(Method.GET);
                IRestResponse response = client.Execute(request); //call at anytime in code
                int numb = int.Parse(response.Content);
                resp = numb;
                WebClient Client = new WebClient();
                if (redownload == true) {
                    Console.WriteLine("Redownloading");
                    Client.DownloadFile("https://BlenderRenderServer.youtubeadminist.repl.co/getBlend", blendpath + "render.blend");
                    redownload = false;
                } else {
                    Console.Clear();
                    Console.WriteLine("Not Redownloading");
                }
                string check = response.Content.ToString();
                if (check.Contains('-')) {
                    Console.Clear();
                    Console.WriteLine("No new frames");
                    fail += 1;
                    DirectoryInfo pn = new DirectoryInfo(path);
                    foreach (FileInfo file in pn.GetFiles()) {
                        file.Delete();
                    }
                    DirectoryInfo bl = new DirectoryInfo(blendpath);
                    redownload = true;
                    foreach (FileInfo file in bl.GetFiles()) {
                        file.Delete();
                    }
                } else {
                    fail = 0;
                    Console.WriteLine("\nGot new frame " + response.Content);
                } try {
                    if (sys == "windows") {
                        Process process = new Process();
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        startInfo.FileName = "cmd.exe";
                        startInfo.Arguments = "/C blender.exe -b " + blendpath + "render.blend -o " + path + " -f " + response.Content;
                        process.StartInfo = startInfo;
                        process.Start();
                        if (process.HasExited) {
                            exit();
                        }
                        process.WaitForExit();
                    } else {
                        string arguments3 = "-c \"blender -b " + blendpath + "render.blend -o " + path + " -f " + response.Content + "\"";
                        arguments3 = Regex.Replace(arguments3, @"\n", "");
                        ProcessStartInfo procStartInfo = new ProcessStartInfo("/bin/bash", arguments3);
                        procStartInfo.RedirectStandardOutput = false; //error true
                        procStartInfo.UseShellExecute = true; //false
                        procStartInfo.CreateNoWindow = true;
                        Process proc = new Process();
                        proc.StartInfo = procStartInfo;
                        proc.Start();
                        if (proc.HasExited) {
                            exit();
                        }
                        proc.WaitForExit();
                    }
                } catch (Exception e) {
                    exit();
                    Console.WriteLine(e);
                    Console.Clear();
                    Console.WriteLine("Exited Safely");
                    throw;
                }
                Console.Clear();
                Console.WriteLine("Trying to send frame");
                if (sys == "windows") {
                    Process process = new Process();
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.FileName = "cmd.exe";
                    int num = int.Parse(response.Content);
                    string arguments = $"/C curl -F {response.Content}=@{path}" + String.Format("{0:0000}", num) + ".png blenderrenderserver.youtubeadminist.repl.co/sendFrame";
                    arguments = Regex.Replace(arguments, @"\n", "");
                    startInfo.Arguments = arguments;
                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit();
                    DirectoryInfo di = new DirectoryInfo(path);
                    foreach (FileInfo file in di.GetFiles()) {
                        file.Delete();
                    }
                } else {
                    int num = int.Parse(response.Content);
                    string arguments2 = "-c \"curl -F " + response.Content + "=@" + path + "" + String.Format("{0:0000}", num) + ".png blenderrenderserver.youtubeadminist.repl.co/sendFrame\"";
                    arguments2 = Regex.Replace(arguments2, @"\n", "");
                    ProcessStartInfo procStartInfo = new ProcessStartInfo("/bin/bash", arguments2);
                    procStartInfo.RedirectStandardOutput = true;
                    procStartInfo.UseShellExecute = false;
                    procStartInfo.CreateNoWindow = true;
                    Process proc = new Process();
                    proc.StartInfo = procStartInfo;
                    proc.Start();
                    proc.WaitForExit();
                }
            }
        }
        public static void exit() {
            if (sys == "windows") {
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                string arguments5 = "/C curl -X POST https://BlenderRenderServer.youtubeadminist.repl.co/cancelFrame -H \"Content-Type: application/json\" -d \"{\"frame\":\""+resp+"\"}";
                arguments5 = Regex.Replace(arguments5, @"\n", "");
                startInfo.Arguments = arguments5;
                process.StartInfo = startInfo;
                Console.WriteLine("exiting");
                process.Start();
                process.WaitForExit();
                Environment.Exit(1);
            } else {
                string arguments2 = "-c \"curl -X POST https://BlenderRenderServer.youtubeadminist.repl.co/cancelFrame -H \"Content-Type: application/json\" -d \"{\"frame\":\""+resp+"\"}";
                arguments2 = Regex.Replace(arguments2, @"\n", "");
                Console.WriteLine(arguments2);
                ProcessStartInfo procStartInfo = new ProcessStartInfo("/bin/bash", arguments2);
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;
                Process proc = new Process();
                proc.StartInfo = procStartInfo;
                Console.WriteLine("exiting");
                proc.Start();
                proc.WaitForExit();
                Environment.Exit(1);
            }
        }
    }
}
