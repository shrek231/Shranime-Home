using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using RestSharp;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class Render : MonoBehaviour
{
    public static Process process = new Process();
    public static Process proc = new Process();
    public static int serverblendvirs;
    public static int yourblendvirs;
    public Text error;
    public static string blendvirstring;
    public Text output;
    public Text text;
    public static int resp = -1;
    public static string blendpath = null;
    public static string sys = null;

    public static bool running = true;

    // Start is called before the first frame update
    public void Start() {
        Thread thread1 = new Thread(render);
        thread1.Start();
    }
    public void render() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == true) {
            sys = "windows";
            text.text = ("remember to add belnder to path");
        } else {
            sys = "linux";
        }

        string path = "none"; //make dir
        if (sys == "windows") {
            path = AppDomain.CurrentDomain.BaseDirectory + @"\img\";
            blendpath = AppDomain.CurrentDomain.BaseDirectory + @"\ble\";
        } else {
            path = AppDomain.CurrentDomain.BaseDirectory + "/img/";
            blendpath = AppDomain.CurrentDomain.BaseDirectory + "/ble/";
        }

        if (Directory.Exists(path)) {
            //text.text = ("");
        } else {
            DirectoryInfo dire = Directory.CreateDirectory(path);
            DirectoryInfo di = Directory.CreateDirectory(blendpath);
        }

        int timeout = 1000; //start loop
        int fail = 0;
        bool redownload = true;
        while (running) {
            
            if (fail > 1) {
                //error.text = ("waiting 30 seconds because of an error or no new frames");
                timeout = 30000;
                redownload = true;
            }

            Thread.Sleep(timeout);
            text.text = ("\nRequesting new frame");
            var client = new RestClient("https://BlenderRenderServer.youtubeadminist.repl.co/requestFrame");
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            int numb = int.Parse(response.Content);
            resp = numb;
            WebClient Client = new WebClient();
            if (redownload == true) {
                text.text = ("Redownloading");
                Client.DownloadFile("https://BlenderRenderServer.youtubeadminist.repl.co/getBlend", blendpath + "render.blend");
                redownload = false;
            } else {
                text.text = ("Not Redownloading");
            }

            string check = response.Content.ToString();
            if (check.Contains("-")) {
                error.text = ("No new frames");
                fail += 1;
                DirectoryInfo pn = new DirectoryInfo(path);
                //foreach (FileInfo file in pn.GetFiles()) {
                //    file.Delete();
                //}

                DirectoryInfo bl = new DirectoryInfo(blendpath);
                redownload = true;
                //foreach (FileInfo file in bl.GetFiles()) {
                //    file.Delete();
                //}
            } else {
                text.text = ("\nGot new frame " + response.Content);
            } try {
                int num = int.Parse(response.Content);
                var imgpath = String.Format("{0:0000}", num);
                var client2 = new RestClient("https://BlenderRenderServer.youtubeadminist.repl.co/version");
                var request2 = new RestRequest(Method.GET);
                IRestResponse response2 = client2.Execute(request2);
                string vir = response2.Content;
                vir = Regex.Replace(vir, @"\n", "");
                vir = Regex.Replace(vir, "\"", "");
                if (vir == "null") {
                    fail += 1;
                    error.text = ("Unknown Blender Vir or no uploaded blend file");
                } else {
                    if (sys == "windows") {
                        text.text = ("Found Blender Vir " + vir);
                        Process process1 = new Process();
                        process1.StartInfo.FileName = "cmd.exe";
                        process1.StartInfo.Arguments = "/c blender -v";
                        process1.StartInfo.UseShellExecute = false;
                        process1.StartInfo.RedirectStandardOutput = true;
                        process1.StartInfo.RedirectStandardError = true;
                        process1.Start();
                        string output2 = process1.StandardOutput.ReadToEnd();
                        string err = process1.StandardError.ReadToEnd();
                        process1.WaitForExit();
                        string[] myblendvir;
                        if (output2.Contains("Blender")) {
                            myblendvir = output2.Split('\n');
                            blendvirstring = Regex.Replace(myblendvir[0], @"Blender ", "");
                            blendvirstring = Regex.Replace(blendvirstring, @"\n", "");
                            Console.WriteLine(blendvirstring);
                        }
                    } else {
                        text.text = ("Found Blender Vir " + vir);
                        Process process2 = new Process();
                        process2.StartInfo.FileName = "/bin/bash";
                        process2.StartInfo.Arguments = "-c \"blender -v\"";
                        process2.StartInfo.UseShellExecute = false;
                        process2.StartInfo.RedirectStandardOutput = true;
                        process2.StartInfo.RedirectStandardError = true;
                        process2.Start();
                        string output2 = process2.StandardOutput.ReadToEnd();
                        string err = process2.StandardError.ReadToEnd();
                        process2.WaitForExit();
                        string[] myblendvir;
                        if (output2.Contains("Blender")) {
                            myblendvir = output2.Split('\n');
                            blendvirstring = Regex.Replace(myblendvir[0], @"Blender ", "");
                            blendvirstring = Regex.Replace(blendvirstring, @"\n", "");
                            yourblendvirs = int.Parse(blendvirstring);
                            vir = Regex.Replace(vir, @"\n", "");
                            serverblendvirs = int.Parse(vir);
                        }
                    }

                    if (yourblendvirs == serverblendvirs) {
                        text.text = ("correct blender vir");
                        fail = 0;
                    } else {
                        fail += 1;
                        error.text = ("incorrect blender vir, need vir " + vir) + ", your vir " + blendvirstring;
                    }
                } try {
                    if (sys == "windows") {
                        string arguments69 = "/C blender.exe -b " + blendpath + "render.blend -o " + path + " -f " + resp;
                        arguments69= Regex.Replace(arguments69, @"\n", "");
                        ProcessStartInfo startInfo = new ProcessStartInfo("cmd.exe", arguments69);
                        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        //Process process = new Process();
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo = startInfo;
                        process.Start();
                        output.text = "rendering";
                        process.WaitForExit();
                        output.text = "Not rendering...";
                    } else {
                        string arguments3 = "-c \"blender -b " + blendpath + "render.blend -o " + path + " -f " + resp + " > log.txt\"";
                        arguments3 = Regex.Replace(arguments3, @"\n", "");
                        ProcessStartInfo procStartInfo = new ProcessStartInfo("/bin/bash", arguments3);
                        procStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        //procStartInfo.RedirectStandardOutput = false;
                        //procStartInfo.UseShellExecute = false;
                        //procStartInfo.CreateNoWindow = true;
                        //Process proc = new Process();
                        proc.StartInfo = procStartInfo;
                        proc.Start();
                        output.text = "rendering";
                        proc.WaitForExit();
                        output.text = "Not rendering...";
                    }
                } catch (Exception e) {
                    error.text = ("render error " + e);
                    exit();
                }

                text.text = ("Trying to send frame");
                if (sys == "windows") {
                    Process process = new Process();
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.FileName = "cmd.exe";
                    string arguments = $"/C curl -F {response.Content}=@{path}" + imgpath + ".png blenderrenderserver.youtubeadminist.repl.co/sendFrame";
                    arguments = Regex.Replace(arguments, @"\n", "");
                    startInfo.Arguments = arguments;
                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit();
                    DirectoryInfo di = new DirectoryInfo(path);
                    //foreach (FileInfo file in di.GetFiles())
                    //{
                    //    file.Delete();
                    //}
                } else {
                    string arguments2 = "-c \"curl -F " + response.Content + "=@" + path + "" + imgpath + ".png blenderrenderserver.youtubeadminist.repl.co/sendFrame\"";
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
            catch (Exception e) {
                error.text = ("send error " + e);
            }
        }
    }

    public void exit() {
        if (sys == "windows") {
            Process process1 = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            string arguments5 = "/C curl -X POST https://BlenderRenderServer.youtubeadminist.repl.co/cancelFrame -H \"Content-Type: application/json\" -d \"{\\\"frame\\\":\\\"" + resp + "\\\"}\"";
            arguments5 = Regex.Replace(arguments5, @"\n", "");
            startInfo.Arguments = arguments5;
            process1.StartInfo = startInfo;
            process1.Start();
            process1.WaitForExit();
            Application.Quit();
        } else {
            Process proc2 = new Process();
            string arguments2 = "-X POST https://BlenderRenderServer.youtubeadminist.repl.co/cancelFrame -H \"Content-Type: application/json\" -d \'{\"frame\":\"" + resp + "\"}\'";
            arguments2 = Regex.Replace(arguments2, @"\n", "");
            ProcessStartInfo procStartInfo = new ProcessStartInfo("/bin/curl", arguments2);
            procStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc2.StartInfo = procStartInfo;
            proc2.Start();
            proc2.WaitForExit();
            //kill render process
            proc.Kill();
            process.Kill();
            Application.Quit();
        }
    }
}
