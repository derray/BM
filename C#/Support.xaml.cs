﻿using Microsoft.Win32;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using BeatSaverAPI;

namespace BMBF_Manager
{
    /// <summary>
    /// Interaktionslogik für Support.xaml
    /// </summary>
    public partial class Support : Window
    {
        /////////////////    Settings Now!!!!
        Boolean draggable = true;
        Boolean Running = false;
        String exe = AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.Length - 1);
        Boolean ForThisVersion = true;
        int C = 0;
        String Key = "abcd";
        List<Tuple<String, String, String, String, String, String, Boolean, Tuple<bool, String, String>>> AllModsList = new List<Tuple<String, String, String, String, String, String, Boolean, Tuple<bool, String, String>>>();
        JSONNode BMBFStable = JSON.Parse("{}");
        BeatSaverAPIInteractor interactor = new BeatSaverAPIInteractor();

        public Support()
        {
            InitializeComponent();
            Quest.Text = MainWindow.IP;
            if(MainWindow.CustomProtocols)
            {
                CustomP.Content = "Disable BM Custom Protocol";
            } else
            {
                CustomP.Content = "Enable BM Custom Protocol";
            }
            if (MainWindow.OneClick)
            {
                BSaver.Content = "Disable BeatSaver OneClick install";
            }
            else
            {
                BSaver.Content = "Enable BeatSaver OneClick install";
            }
            UpdateImage();
            if(MainWindow.ShowADB)
            {
                ADB.Content = "Disable ADB output";
            } else
            {
                ADB.Content = "Enable ADB output";
            }
        }

        private void KeepAlive(object sender, RoutedEventArgs e)
        {
            if(MainWindow.KeepAlive)
            {
                MainWindow.KeepAlive = false;
                txtbox.AppendText("Keep Alive has been disabled.");

            } else
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to enable Keep Alive? That will result in your Quest not going to sleep until the program get's closed.\nThis will only work as long as your Quest is reachable via ADB (connected via cable)\nRightly recommended for Quest 2 Users", "BMBF Manager - Settings", MessageBoxButton.YesNo, MessageBoxImage.Information);
                switch (result)
                {
                    case MessageBoxResult.No:
                        txtbox.AppendText("\n\nAborted.");
                        txtbox.ScrollToEnd();
                        Running = false;
                        return;
                }
                MainWindow.KeepAlive = true;
                txtbox.AppendText("Keep Alive has been enabled.");
            }
        }

        private void ADBshow(object sender, RoutedEventArgs e)
        {
            if(MainWindow.ShowADB)
            {
                //Disable
                MainWindow.ShowADB = false;
                txtbox.AppendText("\n\nADB output disabled.");
                ADB.Content = "Enable ADB output";
            } else
            {
                //enable
                MessageBoxResult result = MessageBox.Show("Are you sure you want to enable ADB output? I won't check if your Quest is connected anymore and you will be able to pause the adb process when you click it.\nDo you really want to enable ADB output?", "BMBF Manager - Settings", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                switch (result)
                {
                    case MessageBoxResult.No:
                        txtbox.AppendText("\n\nAborted.");
                        txtbox.ScrollToEnd();
                        Running = false;
                        return;
                }
                MainWindow.ShowADB = true;
                txtbox.AppendText("\n\nADB output enabled.");
                ADB.Content = "Disable ADB output";
            }
        }

        private void ChooseImage(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Pictures (*.jpg, *.png, *.bmp, *.img, *.tif, *.tiff, *.webp)|*.jpg;*.png;*.bmp;*.img;*.tif;*.tiff;*.webp";
            ofd.Multiselect = false;
            bool? result = ofd.ShowDialog();
            if (result == true)
            {
                //Get the path of specified file
                if (File.Exists(ofd.FileName))
                {
                    MainWindow.CustomImageSource = ofd.FileName;
                    MainWindow.CustomImage = true;
                    UpdateImage();
                    txtbox.AppendText("\n\nFor the changes to take effect program wide you have to restert it.");
                }
                else
                {
                    MessageBox.Show("Please select a valid file", "BMBF Manager - Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }
        }

        private void ResetImage(object sender, RoutedEventArgs e)
        {
            MainWindow.CustomImage = false;
            UpdateImage();
        }

        private void UpdateImage()
        {
            if (MainWindow.CustomImage)
            {
                ImageBrush uniformBrush = new ImageBrush();
                uniformBrush.ImageSource = new BitmapImage(new Uri(MainWindow.CustomImageSource, UriKind.Absolute));
                uniformBrush.Stretch = Stretch.UniformToFill;
                this.Background = uniformBrush;
            }
            else
            {
                ImageBrush uniformBrush = new ImageBrush();
                uniformBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Support5.png", UriKind.Absolute));
                uniformBrush.Stretch = Stretch.UniformToFill;
                this.Background = uniformBrush;
            }
        }

        private void Drag(object sender, RoutedEventArgs e)
        {
            bool mouseIsDown = System.Windows.Input.Mouse.LeftButton == MouseButtonState.Pressed;


            if (mouseIsDown)
            {
                if (draggable)
                {
                    this.DragMove();
                }

            }

        }

        public void noDrag(object sender, MouseEventArgs e)
        {
            draggable = false;
        }

        public void doDrag(object sender, MouseEventArgs e)
        {
            draggable = true;
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            CheckIP();
            MainWindow.IP = Quest.Text;
            this.Close();
        }

        private void Mini(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ClearText(object sender, RoutedEventArgs e)
        {
            if (Quest.Text == "Quest IP")
            {
                Quest.Text = "";
            }

        }

        private void QuestIPCheck(object sender, RoutedEventArgs e)
        {
            if (Quest.Text == "")
            {
                Quest.Text = "Quest IP";
            }
        }

        public Boolean CheckIP()
        {
            getQuestIP();
            if (MainWindow.IP == "Quest IP")
            {
                return false;
            }
            MainWindow.IP = MainWindow.IP.Replace(":5000000", "");
            MainWindow.IP = MainWindow.IP.Replace(":500000", "");
            MainWindow.IP = MainWindow.IP.Replace(":50000", "");
            MainWindow.IP = MainWindow.IP.Replace(":5000", "");
            MainWindow.IP = MainWindow.IP.Replace(":500", "");
            MainWindow.IP = MainWindow.IP.Replace(":50", "");
            MainWindow.IP = MainWindow.IP.Replace(":5", "");
            MainWindow.IP = MainWindow.IP.Replace(":", "");
            MainWindow.IP = MainWindow.IP.Replace("/", "");
            MainWindow.IP = MainWindow.IP.Replace("https", "");
            MainWindow.IP = MainWindow.IP.Replace("http", "");
            MainWindow.IP = MainWindow.IP.Replace("Http", "");
            MainWindow.IP = MainWindow.IP.Replace("Https", "");

            int count = MainWindow.IP.Split('.').Count();
            if (count != 4)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate
                {
                    Quest.Text = MainWindow.IP;
                }));
                return false;
            }
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate
            {
                Quest.Text = MainWindow.IP;
            }));
            return true;
        }

        public void getQuestIP()
        {
            MainWindow.IP = Quest.Text;
            return;
        }

        private void EnableBBBUMove(object sender, RoutedEventArgs e)
        {
            MainWindow.BBBUTransfered = false;
            BBBU BBBUWindow = new BBBU();
            BBBUWindow.Show();
        }

        private void EnableQSUMove(object sender, RoutedEventArgs e)
        {
            MainWindow.QSUTransfered = false;
            QSU QSUWindow = new QSU();
            QSUWindow.Show();
        }

        private void EnableCustom(object sender, RoutedEventArgs e)
        {
            if(!MainWindow.CustomProtocols)
            {
                txtbox.AppendText("\n\nChanging Registry to enable BM Custom protocols");
                String regFile = "Windows Registry Editor Version 5.00\n\n[HKEY_CLASSES_ROOT\\bm]\n@=\"URL: bm\"\n\"URL Protocol\"=\"bm\"\n\n[HKEY_CLASSES_ROOT\\bm]\n@=\"" + System.Reflection.Assembly.GetEntryAssembly().Location.Replace("\\", "\\\\") + "\"\n\n[HKEY_CLASSES_ROOT\\bm\\shell]\n\n[HKEY_CLASSES_ROOT\\bm\\shell\\open]\n\n[HKEY_CLASSES_ROOT\\bm\\shell\\open\\command]\n@=\"" + System.Reflection.Assembly.GetEntryAssembly().Location.Replace("\\", "\\\\") + " \\\"%1\\\"\"";
                File.WriteAllText(exe + "\\registry.reg", regFile);
                try
                {
                    Process.Start(exe + "\\registry.reg");
                    txtbox.AppendText("\n\nCustom Links enabled");
                }
                catch
                {
                    txtbox.AppendText("\n\nRegistry was unable to change... no Custom protocol enabled.");
                    return;
                }
                CustomP.Content = "Disable BM Custom Protocol";
                MainWindow.CustomProtocols = true;
            } else
            {
                txtbox.AppendText("\n\nChanging Registry to disable BM Custom protocols");
                String regFile = "Windows Registry Editor Version 5.00\n\n[-HKEY_CLASSES_ROOT\\bm]";
                File.WriteAllText(exe + "\\registry.reg", regFile);
                try
                {
                    Process.Start(exe + "\\registry.reg");
                    txtbox.AppendText("\n\nCustom Links disabled");
                }
                catch
                {
                    txtbox.AppendText("\n\nRegistry was unable to change.");
                    return;
                }
                CustomP.Content = "Enable BM Custom Protocol";
                MainWindow.CustomProtocols = false;
            }
        }

        public void enable_BeatSaver(object sender, RoutedEventArgs e)
        {
            if (!MainWindow.OneClick)
            {
                MessageBoxResult result = MessageBox.Show("This will disable OneClick Install via Mod Assistent.\nDo you wish to continue?", "BMBF Manager - Settings", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                switch (result)
                {
                    case MessageBoxResult.No:
                        txtbox.AppendText("\n\nOneClick Install enabeling aborted");
                        Running = false;
                        txtbox.ScrollToEnd();
                        return;
                }
                txtbox.AppendText("\n\nChanging Registry to enable OneClick Custom protocols");
                String regFile = "Windows Registry Editor Version 5.00\n\n[HKEY_CLASSES_ROOT\\beatsaver]\n@=\"URL: beatsaver\"\n\"URL Protocol\"=\"beatsaver\"\n\n[HKEY_CLASSES_ROOT\\beatsaver]\n@=\"" + System.Reflection.Assembly.GetEntryAssembly().Location.Replace("\\", "\\\\") + "\"\n\n[HKEY_CLASSES_ROOT\\beatsaver\\shell]\n\n[HKEY_CLASSES_ROOT\\beatsaver\\shell\\open]\n\n[HKEY_CLASSES_ROOT\\beatsaver\\shell\\open\\command]\n@=\"" + System.Reflection.Assembly.GetEntryAssembly().Location.Replace("\\", "\\\\") + " \\\"%1\\\"\"";
                File.WriteAllText(exe + "\\registry.reg", regFile);
                try
                {
                    Process.Start(exe + "\\registry.reg");
                    txtbox.AppendText("\n\nOneClick Install via BeatSaver enabled");
                }
                catch
                {
                    txtbox.AppendText("\n\nRegistry was unable to change... no Custom protocol disabled.");
                    return;
                }
                BSaver.Content = "Disable BeatSaver OneClick install";
                MainWindow.OneClick = true;
            }
            else
            {
                MessageBoxResult result = MessageBox.Show("This will disable OneClick Install via BMBF Manager.\nDo you wish to continue?", "BMBF manager - Settings", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                switch (result)
                {
                    case MessageBoxResult.No:
                        txtbox.AppendText("\n\nOneClick disabeling enabeling aborted");
                        Running = false;
                        txtbox.ScrollToEnd();
                        return;
                }
                txtbox.AppendText("\n\nChanging Registry to disable OneClick Custom protocols");
                String regFile = "Windows Registry Editor Version 5.00\n\n[-HKEY_CLASSES_ROOT\\beatsaver]";
                File.WriteAllText(exe + "\\registry.reg", regFile);
                try
                {
                    Process.Start(exe + "\\registry.reg");
                    txtbox.AppendText("\n\nOneClick Install via BeatSaver disabled");
                }
                catch
                {
                    txtbox.AppendText("\n\nRegistry was unable to change.");
                    return;
                }
                BSaver.Content = "Enable BeatSaver OneClick install";
                MainWindow.OneClick = false;
            }

        }

        public void BackupPlaylists()
        {
            try
            {
                Sync();
                txtbox.AppendText("\n\nBacking up Playlist to " + exe + "\\Backup\\Playlists.json");
                txtbox.ScrollToEnd();
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
                String BMBF = "";
                using (TimeoutWebClient client2 = new TimeoutWebClient())
                {
                    BMBF = client2.DownloadString("http://" + MainWindow.IP + ":50000/host/beatsaber/config");
                }
                var json = JSON.Parse(BMBF);
                json["IsCommitted"] = false;
                File.WriteAllText(exe + "\\tmp\\Config.json", json.ToString());

                String Config = exe + "\\tmp\\config.json";

                var j = JSON.Parse(File.ReadAllText(Config));
                File.WriteAllText(exe + "\\tmp\\Playlists.json", j["Config"].ToString());
                txtbox.AppendText("\n\nBacked up Playlists to " + exe + "\\tmp\\Playlists.json");
                txtbox.ScrollToEnd();
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
            }
            catch
            {
                txtbox.AppendText("\n\n\nAn error occured (Code: PL100). Check following:");
                txtbox.AppendText("\n\n- You put in the Quests IP right.");
                txtbox.AppendText("\n\n- You've choosen a Backup Name.");
                txtbox.AppendText("\n\n- Your Quest is on.");

            }
        }

        public void RestorePlaylists()
        {
            System.Threading.Thread.Sleep(5000);
            try
            {
                WebClient client3 = new WebClient();

                String Playlists = exe + "\\tmp\\Playlists.json";

                var j = JSON.Parse(client3.DownloadString("http://" + MainWindow.IP + ":50000/host/beatsaber/config"));
                var p = JSON.Parse(File.ReadAllText(Playlists));

                j["Config"]["Playlists"] = p["Playlists"];
                File.WriteAllText(exe + "\\tmp\\FUCKINBMBF.json", j["Config"].ToString());
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate {
                    postChanges(exe + "\\tmp\\FUCKINBMBF.json");
                }));
            } catch
            {
                txtbox.AppendText("\n\n\nAn error occured (Code: PL100). Check following:");
                txtbox.AppendText("\n\n- You put in the Quests IP right.");
                txtbox.AppendText("\n\n- Your Quest is on.");
            }
        }

        public void resetassets()
        {
            System.Threading.Thread.Sleep(3000);
            TimeoutWebClient client = new TimeoutWebClient();
            client.UploadData("http://" + MainWindow.IP + ":50000/host/mod/resetassets", "POST", new byte[0]);
        }

        public void reloadsongsfolder()
        {
            System.Threading.Thread.Sleep(3000);
            TimeoutWebClient client = new TimeoutWebClient();
            client.QueryString.Add("foo", "foo");
            client.UploadValues("http://" + MainWindow.IP + ":50000/host/beatsaber/reloadsongfolders", "POST", client.QueryString);
        }

        public void postChanges(String Config)
        {
            System.Threading.Thread.Sleep(5000);
            using (TimeoutWebClient client = new TimeoutWebClient())
            {
                client.QueryString.Add("foo", "foo");
                client.UploadFile("http://" + MainWindow.IP + ":50000/host/beatsaber/config", "PUT", Config);
                client.UploadValues("http://" + MainWindow.IP + ":50000/host/beatsaber/commitconfig", "POST", client.QueryString);
            }
        }

        public String InstallMod(String ModName)
        {

            getQuestIP();
            TimeoutWebClientShort client = new TimeoutWebClientShort();

            JSONNode json = JSON.Parse("{}");
            JSONNode BMBF = JSON.Parse("{}");
            Boolean Reaching = true;

            try
            {
                json = SimpleJSON.JSON.Parse(client.DownloadString("http://www.questboard.xyz/api/mods/"));
            }
            catch
            {
                txtbox.AppendText("\n\nError (Code: BM100). Couldn't reach the Quest Boards Website to get some available Mods.");
            }

            try
            {
                BMBF = SimpleJSON.JSON.Parse(client.DownloadString("http://" + MainWindow.IP + ":50000/host/beatsaber/config"));
                MainWindow.GameVersion = BMBF["BeatSaberVersion"];
            }
            catch
            {
                txtbox.AppendText("\n\n\nError (Code: BMBF100). Couldn't acces BMBF Web Interface. Check Following:");
                txtbox.AppendText("\n\n- You've put in the right IP");
                txtbox.AppendText("\n\n- BMBF is opened");
                Reaching = false;
            }
            String[] GameVersion = MainWindow.GameVersion.ToString().Replace("\"", "").Split('.');
            //String[] GameVersion = "1.13.0".Replace("\"", "").Split('.');
            int major = Convert.ToInt32(GameVersion[0]);
            int minor = Convert.ToInt32(GameVersion[1]);
            int patch = Convert.ToInt32(GameVersion[2]);

            foreach (JSONNode mod in json["mods"])
            {
                String Name = mod["name"];
                String Creator = "";

                foreach (JSONNode Creat in mod["creator"])
                {
                    Creator = Creator + Creat + ", ";
                }
                Creator = Creator.Substring(0, Creator.Length - 2);

                foreach (JSONNode download in mod["downloads"])
                {
                    String Version = download["modversion"];
                    bool stop = false;
                    foreach (JSONNode gameversion in download["gameversion"])
                    {
                        String[] MGameVersion = gameversion.ToString().Replace("\"", "").Split('.');
                        int Mmajor = Convert.ToInt32(MGameVersion[0]);
                        int Mminor = Convert.ToInt32(MGameVersion[1]);
                        int Mpatch = 0;
                        if (MGameVersion.Count() == 2)
                        {
                            Mpatch = 0;
                        }
                        else
                        {
                            Mpatch = Convert.ToInt32(MGameVersion[2]);
                        }
                        if (major == Mmajor && minor == Mminor && patch >= Mpatch)
                        {
                            Boolean existent = false;
                            int ListIndex = 0;
                            foreach (Tuple<string, string, string, string, string, string, bool, Tuple<bool, String, String>> t in AllModsList)
                            {
                                if ((String)t.Item1 == Name)
                                {
                                    existent = true;
                                    break;
                                }
                                ListIndex++;
                            }
                            if (existent) continue;
                            Version = download["modversion"];
                            //Name, Version, DownloadLink, Creator, gameVersion, Desciption, Forward, new Tuple (CoreMod, ModID, currentversion, islatest)
                            String BMBFVersion = "0.0.0";
                            foreach (JSONNode BMBFMod in BMBF["Config"]["Mods"])
                            {
                                if (BMBFMod["ID"].ToString().ToLower().Replace("\"", "") == mod["ModID"].ToString().ToLower().Replace("\"", ""))
                                {
                                    BMBFVersion = BMBFMod["Version"];
                                }
                            }
                            AllModsList.Add(new Tuple<string, string, string, string, string, string, bool, Tuple<bool, String, String>>(Name, Version, download["download"].ToString().Replace("\"", ""), Creator, gameversion.ToString().Replace("\"", ""), mod["details"].ToString().Replace("\"", "").Replace("\\r\\n", System.Environment.NewLine), download["forward"].AsBool, new Tuple<bool, String, String>(download["coremod"].AsBool, mod["ModID"], BMBFVersion)));

                            stop = true;
                            break;
                        }
                    }
                    if (stop) break;
                }

            }

            WebClient c = new WebClient();

            //json = JSON.Parse(c.DownloadString("https://raw.githubusercontent.com/ComputerElite/BM/main/testing.json"));
            json = JSON.Parse(c.DownloadString("https://raw.githubusercontent.com/ComputerElite/BM/main/mods.json"));

            foreach (JSONNode mod in json["mods"])
            {
                String Name = mod["name"];
                String Creator = "";

                foreach (JSONNode Creat in mod["creator"])
                {
                    Creator = Creator + Creat + ", ";
                }
                Creator = Creator.Substring(0, Creator.Length - 2);

                foreach (JSONNode download in mod["downloads"])
                {
                    String Version = download["modversion"];
                    bool stop = false;
                    foreach (JSONNode gameversion in download["gameversion"])
                    {
                        String[] MGameVersion = gameversion.ToString().Replace("\"", "").Split('.');
                        int Mmajor = Convert.ToInt32(MGameVersion[0]);
                        int Mminor = Convert.ToInt32(MGameVersion[1]);
                        int Mpatch = 0;
                        if (MGameVersion.Count() == 2)
                        {
                            Mpatch = 0;
                        }
                        else
                        {
                            Mpatch = Convert.ToInt32(MGameVersion[2]);
                        }
                        if (major == Mmajor && minor == Mminor && patch >= Mpatch)
                        {
                            Boolean existent = false;
                            int ListIndex = 0;
                            foreach (Tuple<string, string, string, string, string, string, bool, Tuple<bool, String, String>> t in AllModsList)
                            {
                                if ((String)t.Item1 == Name)
                                {
                                    existent = true;
                                    break;
                                }
                                ListIndex++;
                            }
                            if (!existent)
                            {
                                Version = download["modversion"];
                                //Name, Version, DownloadLink, Creator, gameVersion, Desciption, Forward, new Tuple (CoreMod, ModID, currentversion, islatest)
                                String BMBFVersion = "0.0.0";
                                foreach (JSONNode BMBFMod in BMBF["Config"]["Mods"])
                                {
                                    if (BMBFMod["ID"].ToString().ToLower().Replace("\"", "") == mod["ModID"].ToString().ToLower().Replace("\"", ""))
                                    {
                                        BMBFVersion = BMBFMod["Version"];
                                    }
                                }
                                AllModsList.Add(new Tuple<string, string, string, string, string, string, bool, Tuple<bool, String, String>>(Name, Version, download["download"].ToString().Replace("\"", ""), Creator, gameversion.ToString().Replace("\"", ""), mod["details"].ToString().Replace("\"", "").Replace("\\r\\n", System.Environment.NewLine), download["forward"].AsBool, new Tuple<bool, String, String>(download["coremod"].AsBool, mod["ModID"], BMBFVersion)));
                            }
                            else
                            {
                                //Name, Version, DownloadLink, Creator, gameVersion, Desciption, Forward, new Tuple (CoreMod, ModID, currentversion, islatest)
                                String oldModver = AllModsList[ListIndex].Item2.ToString();
                                String[] allver = oldModver.Replace("\"", "").Split('.');
                                List<int> finishedver = new List<int>();
                                String[] newver = Version.Replace("\"", "").Split('.');
                                Boolean newer = false;
                                foreach (String CV in allver)
                                {
                                    finishedver.Add(Convert.ToInt32(CV));
                                }
                                int e = 0;
                                try
                                {
                                    if ((Convert.ToInt32(newver[0]) >= finishedver[0] && Convert.ToInt32(newver[1]) >= finishedver[1] && Convert.ToInt32(newver[2]) >= finishedver[2]) || (Convert.ToInt32(newver[0]) >= finishedver[0] && Convert.ToInt32(newver[1]) > finishedver[1]) || (Convert.ToInt32(newver[0]) > finishedver[0]))
                                    {
                                        newer = true;
                                    }
                                }
                                catch
                                {
                                    continue;
                                }
                                e++;
                                if (!newer) continue;

                                AllModsList.RemoveAt(ListIndex);

                                //Name, Version, DownloadLink, Creator, gameVersion, Desciption, Forward, new Tuple (CoreMod, ModID, currentversion)
                                String BMBFVersion = "0.0.0";
                                foreach (JSONNode BMBFMod in BMBF["Config"]["Mods"])
                                {
                                    if (BMBFMod["ID"].ToString().ToLower().Replace("\"", "") == mod["ModID"].ToString().ToLower().Replace("\"", ""))
                                    {
                                        BMBFVersion = BMBFMod["Version"];
                                    }
                                }
                                if (BMBFVersion == "0.0.0") BMBFVersion = "N/A";
                                AllModsList.Add(new Tuple<string, string, string, string, string, string, bool, Tuple<bool, String, String>>(Name, Version, download["download"].ToString().Replace("\"", ""), Creator, gameversion.ToString().Replace("\"", ""), mod["details"].ToString().Replace("\"", "").Replace("\\r\\n", System.Environment.NewLine), download["forward"].AsBool, new Tuple<bool, String, String>(download["coremod"].AsBool, mod["ModID"], BMBFVersion)));
                            }

                            stop = true;
                            break;
                        }
                    }
                    if (stop) break;
                }

            }
            if (!Reaching)
            {
                MessageBox.Show("I couldn't reach BMBF. All the mods displayed are for the last Version of BMBF you used while I noticed (" + MainWindow.GameVersion + "). Please check if you can reach BMBF so I can install mods.", "BMBF Manager - Mod Installing", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            foreach (Tuple<string, string, string, string, string, string, bool, Tuple<bool, String, String>> mod in AllModsList)
            {
                if(mod.Item1.ToString().Replace("\"", "").ToLower() == ModName.ToLower())
                {
                    if(mod.Item7)
                    {
                        return "Forward" + mod.Item3;
                    }
                    return mod.Item3;
                }
            }
            return "Error";
        }

        public void installtoquest(String link)
        {
            C = 0;
            while (File.Exists(exe + "\\tmp\\Mod" + C + ".zip"))
            {
                C++;
            }
            txtbox.AppendText("Downloading Mod");
            TimeoutWebClient c = new TimeoutWebClient();
            Uri uri = new Uri(link);
            try
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate
                {
                    c.DownloadFileCompleted += new AsyncCompletedEventHandler(finished_download);
                    c.DownloadFileAsync(uri, exe + "\\tmp\\Mod" + C + ".zip");
                }));
            }
            catch
            {
                txtbox.AppendText("\n\nError (Code: BM200). Couldn't download Mod");
                Running = false;
                return;
            }
        }

        private void finished_download(object sender, AsyncCompletedEventArgs e)
        {
            txtbox.AppendText("\nDownloaded Mod");
            upload(exe + "\\tmp\\Mod" + C + ".zip");
        }

        public void upload(String path)
        {
            getQuestIP();

            TimeoutWebClient client = new TimeoutWebClient();

            txtbox.AppendText("\n\nUploading Mod/Song to BMBF");
            Uri uri = new Uri("http://" + MainWindow.IP + ":50000/host/beatsaber/upload?overwrite");
            try
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate
                {
                    client.UploadFileCompleted += new UploadFileCompletedEventHandler(finished_upload);
                    client.UploadFileAsync(uri, path);
                }));
            }
            catch
            {
                txtbox.AppendText("\n\nA error Occured (Code: BMBF100)");
            }
        }

        private void finished_upload(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate
                {
                    Sync();
                }));
                txtbox.AppendText("\n\nMod/Song was synced to your Quest.");
                if(!ForThisVersion)
                {
                    Process.Start("http://" + MainWindow.IP + ":50000/main/mods");
                    MessageBox.Show("Please enable the mod manually and then sync", "BMBF Manager - Mod Installing", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                Running = false;
            }
            catch
            {
                txtbox.AppendText("\n\nCouldn't sync with BeatSaber. Needs to be done manually.");
                if (!ForThisVersion)
                {
                    Process.Start("http://" + MainWindow.IP + ":50000/main/mods");
                    MessageBox.Show("Please enable the mod manually and then sync", "BMBF Manager - Mod Installing", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                Running = false;
                return;
            }
        }

        public void DownloadSong(String brs)
        {
            Key = brs;
            C = 0;
            while (File.Exists(exe + "\\tmp\\Song" + C + ".zip"))
            {
                C++;
            }
            BeatSaverAPISong song = interactor.GetBeatSaverAPISong(Key);
            if(!song.GoodRequest)
            {
                txtbox.AppendText("The song " + Key + " doesn't exist");
                Running = false;
                return;
            }
            txtbox.AppendText("\nDownloading BeatMap " + Key);
            TimeoutWebClient cl = new TimeoutWebClient();
            cl.Headers.Add("user-agent", "BMBF Manager/1.0");
            Uri keys = new Uri(interactor.BeatSaverLink + song.downloadURL);
            try
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate
                {
                    cl.DownloadFileCompleted += new AsyncCompletedEventHandler(finished_songdownload);
                    cl.DownloadFileAsync(keys, exe + "\\tmp\\Song" + C + ".zip");
                }));
            }
            catch
            {
                txtbox.AppendText("\n\nAn Error Occured");
                Running = false;
                return;
            }
        }

        private void finished_songdownload(object sender, AsyncCompletedEventArgs e)
        {
            txtbox.AppendText("\nDownloaded BeatMap " + Key + "\n");
            upload(exe + "\\tmp\\Song" + C + ".zip");
            Running = false;
        }

        public void Sync()
        {
            System.Threading.Thread.Sleep(2000);
            using (TimeoutWebClient client = new TimeoutWebClient())
            {
                client.QueryString.Add("foo", "foo");
                client.UploadValues("http://" + MainWindow.IP + ":50000/host/beatsaber/commitconfig", "POST", client.QueryString);
            }
        }

        public void SwitchVersion()
        {
            if (!adb("pull /sdcard/Android/data/com.beatgames.beatsaber/files/mods \"" + exe + "\\ModChecks"))
            {
                txtbox.AppendText("\n\n\nAn error Occured (Code: ADB100). Check following");
                txtbox.AppendText("\n\n- Your Quest is connected and USB Debugging enabled.");
                txtbox.AppendText("\n\n- You have adb installed.");
                Running = false;
                return;
            }

            if (!adb("shell am force-stop com.weloveoculus.BMBF"))
            {
                txtbox.AppendText("\n\n\nAn error Occured (Code: ADB100). Check following");
                txtbox.AppendText("\n\n- Your Quest is connected and USB Debugging enabled.");
                txtbox.AppendText("\n\n- You have adb installed.");
                Running = false;
                return;
            }

            if (Directory.Exists(exe + "\\ModChecks\\mods"))
            {
                //game is modded

                if (File.Exists(exe + "\\Backups\\modded.apk"))
                {
                    //Unmodded Beat Saber may be installed
                    MessageBoxResult result = MessageBox.Show("It looks like your last Action was installing unmodded Beat Saber. If you continue and have unmodded Beat Saber installed you must mod Beat Saber By hand.\nDo you wish to continue?", "BMBF Manager - Version Switcher", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    switch (result)
                    {
                        case MessageBoxResult.No:
                            txtbox.AppendText("\n\nAborted.");
                            txtbox.ScrollToEnd();
                            Running = false;
                            return;
                    }
                }
                //Install the unmodded Version of Beat Saber
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate {
                    txtbox.AppendText("\n\nBacking up everything.");
                    txtbox.ScrollToEnd();
                }));
                if (!adb("pull /sdcard/BMBFData/Backups/beatsaber-unmodded.apk \"" + exe + "\\tmp\\unmodded.apk\""))
                {
                    txtbox.AppendText("\n\n\nAn error Occured (Code: ADB100). Check following");
                    txtbox.AppendText("\n\n- Your Quest is connected and USB Debugging enabled.");
                    txtbox.AppendText("\n\n- You have adb installed.");
                    Running = false;
                    return;
                }
                if (!adb("pull /sdcard/Android/data/com.beatgames.beatsaber/files \"" + exe + "\\Backups\""))
                {
                    txtbox.AppendText("\n\n\nAn error Occured (Code: ADB100). Check following");
                    txtbox.AppendText("\n\n- Your Quest is connected and USB Debugging enabled.");
                    txtbox.AppendText("\n\n- You have adb installed.");
                    Running = false;
                    return;
                }
                //Directory.Delete(exe + "\\Backups\\files\\mods", true);
                //Directory.Delete(exe + "\\Backups\\files\\libs", true);

                String moddedBS = adbS("shell pm path com.beatgames.beatsaber").Replace("package:", "").Replace(System.Environment.NewLine, "");
                if (!adb("pull " + moddedBS + " \"" + exe + "\\Backups\\modded.apk\""))
                {
                    txtbox.AppendText("\n\n\nAn error Occured (Code: ADB100). Check following");
                    txtbox.AppendText("\n\n- Your Quest is connected and USB Debugging enabled.");
                    txtbox.AppendText("\n\n- You have adb installed.");
                    Running = false;
                    return;
                }

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate {
                    txtbox.AppendText("\n\nInstalling unmodded Beat Saber.");
                    txtbox.ScrollToEnd();
                }));
                if (!adb("uninstall com.beatgames.beatsaber"))
                {
                    txtbox.AppendText("\n\n\nAn error Occured (Code: ADB100). Check following");
                    txtbox.AppendText("\n\n- Your Quest is connected and USB Debugging enabled.");
                    txtbox.AppendText("\n\n- You have adb installed.");
                    Running = false;
                    return;
                }
                if (!adb("install \"" + exe + "\\tmp\\unmodded.apk\""))
                {
                    txtbox.AppendText("\n\n\nAn error Occured (Code: ADB100). Check following");
                    txtbox.AppendText("\n\n- Your Quest is connected and USB Debugging enabled.");
                    txtbox.AppendText("\n\n- You have adb installed.");
                    Running = false;
                    return;
                }
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate {
                    txtbox.AppendText("\n\nRestoring Scores");
                    txtbox.ScrollToEnd();
                }));
                adb("push \"" + exe + "\\Backups\\files\\LocalDailyLeaderboards.dat\" /sdcard/Android/data/com.beatgames.beatsaber/files/LocalDailyLeaderboards.dat");
                adb("push \"" + exe + "\\Backups\\files\\LocalLeaderboards.dat\" /sdcard/Android/data/com.beatgames.beatsaber/files/LocalLeaderboards.dat");
                adb("push \"" + exe + "\\Backups\\files\\PlayerData.dat\" /sdcard/Android/data/com.beatgames.beatsaber/files/PlayerData.dat");
                adb("push \"" + exe + "\\Backups\\files\\AvatarData.dat\" /sdcard/Android/data/com.beatgames.beatsaber/files/AvatarData.dat");
                adb("push \"" + exe + "\\Backups\\files\\settings.cfg\" /sdcard/Android/data/com.beatgames.beatsaber/files/settings.cfg");


                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate {
                    txtbox.AppendText("\n\nFinished. You can now play vanilla Beat Saber.");
                    txtbox.ScrollToEnd();
                }));

            }
            else
            {
                //game is unmodded
                if (!File.Exists(exe + "\\Backups\\modded.apk"))
                {
                    txtbox.AppendText("\n\nPlease Click \"Install/Update BMBF\" to mod Beat Saber the first time.");
                    Running = false;
                    return;
                }
                adb("pull /sdcard/Android/data/com.beatgames.beatsaber/files/LocalDailyLeaderboards.dat \"" + exe + "\\Backups\\files\\LocalDailyLeaderboards.dat\"");
                adb("pull /sdcard/Android/data/com.beatgames.beatsaber/files/LocalLeaderboards.dat \"" + exe + "\\Backups\\files\\LocalLeaderboards.dat\"");
                adb("pull /sdcard/Android/data/com.beatgames.beatsaber/files/PlayerData.dat \"" + exe + "\\Backups\\files\\PlayerData.dat\"");
                adb("pull /sdcard/Android/data/com.beatgames.beatsaber/files/AvatarData.dat \"" + exe + "\\Backups\\files\\AvatarData.dat\"");
                adb("pull /sdcard/Android/data/com.beatgames.beatsaber/files/settings.cfg \"" + exe + "\\Backups\\files\\settings.cfg\"");
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate {
                    txtbox.AppendText("\n\nUninstalling Beat Saber.");
                    txtbox.ScrollToEnd();
                }));
                if (!adb("uninstall com.beatgames.beatsaber"))
                {
                    txtbox.AppendText("\n\n\nAn error Occured (Code: ADB100). Check following");
                    txtbox.AppendText("\n\n- Your Quest is connected and USB Debugging enabled.");
                    txtbox.AppendText("\n\n- You have adb installed.");
                    Running = false;
                    return;
                }
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate {
                    txtbox.AppendText("\n\nInstalling Modded Beat Saber");
                    txtbox.ScrollToEnd();
                }));
                if (!adb("install \"" + exe + "\\Backups\\modded.apk\""))
                {
                    txtbox.AppendText("\n\n\nAn error Occured (Code: ADB100). Check following");
                    txtbox.AppendText("\n\n- Your Quest is connected and USB Debugging enabled.");
                    txtbox.AppendText("\n\n- You have adb installed.");
                    Running = false;
                    return;
                }
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate {
                    txtbox.AppendText("\n\nRestoring Game Data");
                    txtbox.ScrollToEnd();
                }));
                if (!adb("push \"" + exe + "\\Backups\\files\" /sdcard/Android/data/com.beatgames.beatsaber/files"))
                {
                    txtbox.AppendText("\n\n\nAn error Occured (Code: ADB100). Check following");
                    txtbox.AppendText("\n\n- Your Quest is connected and USB Debugging enabled.");
                    txtbox.AppendText("\n\n- You have adb installed.");
                    Running = false;
                    return;
                }
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate {
                    txtbox.AppendText("\n\nGranting Permissions");
                    txtbox.ScrollToEnd();
                }));
                adb("shell pm grant com.beatgames.beatsaber android.permission.READ_EXTERNAL_STORAGE"); //Grant permission read
                adb("shell pm grant com.beatgames.beatsaber android.permission.WRITE_EXTERNAL_STORAGE"); //Grant permission write
                Directory.Delete(exe + "\\Backups", true);
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate {
                    txtbox.AppendText("\n\nfinished. You can now play your Custom Songs again.");
                    txtbox.ScrollToEnd();
                }));
            }

            if (Directory.Exists(exe + "\\ModChecks\\mods")) Directory.Delete(exe + "\\ModChecks\\mods", true);
        }

        public String adbS(String Argument)
        {
            String User = System.Environment.GetEnvironmentVariable("USERPROFILE");

            foreach (String ADB in MainWindow.ADBPaths)
            {
                ProcessStartInfo s = new ProcessStartInfo();
                s.CreateNoWindow = true;
                s.UseShellExecute = false;
                s.FileName = ADB.Replace("User", User);
                s.WindowStyle = ProcessWindowStyle.Minimized;
                s.Arguments = Argument;
                s.RedirectStandardOutput = true;
                try
                {
                    // Start the process with the info we specified.
                    // Call WaitForExit and then the using statement will close.
                    using (Process exeProcess = Process.Start(s))
                    {
                        String IPS = exeProcess.StandardOutput.ReadToEnd();
                        exeProcess.WaitForExit();
                        if (IPS.Contains("no devices/emulators found"))
                        {
                            txtbox.AppendText("\n\n\nAn error Occured (Code: ADB110). Check following");
                            txtbox.AppendText("\n\n- Your Quest is connected, Developer Mode enabled and USB Debugging enabled.");
                            txtbox.ScrollToEnd();
                            return "Error";
                        }

                        return IPS;
                    }
                }
                catch
                {
                    continue;
                }
            }
            txtbox.AppendText("\n\n\nAn error Occured (Code: ADB100). Check following not");
            txtbox.AppendText("\n\n- You have adb installed.");
            txtbox.ScrollToEnd();
            return "Error";
        }

        public Boolean adb(String Argument)
        {
            String User = System.Environment.GetEnvironmentVariable("USERPROFILE");

            foreach (String ADB in MainWindow.ADBPaths)
            {
                ProcessStartInfo s = new ProcessStartInfo();
                s.CreateNoWindow = true;
                s.UseShellExecute = false;
                s.FileName = ADB.Replace("User", User);
                s.WindowStyle = ProcessWindowStyle.Minimized;
                s.Arguments = Argument;
                s.RedirectStandardOutput = true;
                if (MainWindow.ShowADB)
                {
                    s.RedirectStandardOutput = false;
                    s.CreateNoWindow = false;
                }
                try
                {
                    // Start the process with the info we specified.
                    // Call WaitForExit and then the using statement will close.
                    using (Process exeProcess = Process.Start(s))
                    {
                        if (!MainWindow.ShowADB)
                        {
                            String IPS = exeProcess.StandardOutput.ReadToEnd();
                            exeProcess.WaitForExit();
                            if (IPS.Contains("no devices/emulators found"))
                            {
                                txtbox.AppendText("\n\n\nAn error Occured (Code: ADB110). Check following");
                                txtbox.AppendText("\n\n- Your Quest is connected, Developer Mode enabled and USB Debugging enabled.");
                                txtbox.ScrollToEnd();
                                return false;
                            }
                        }
                        else
                        {
                            exeProcess.WaitForExit();
                        }

                        return true;
                    }
                }
                catch
                {
                    continue;
                }
            }
            txtbox.AppendText("\n\n\nAn error Occured (Code: ADB100). Check following not");
            txtbox.AppendText("\n\n- You have adb installed.");
            txtbox.ScrollToEnd();
            return false;
        }

        private void BMBF_Link()
        {
            using (WebClient client = new WebClient())
            {
                BMBFStable = JSON.Parse(client.DownloadString("https://bmbf.dev/stable/json"));
            }
        }

        private void UpdateBMBF()
        {
            if (Running)
            {
                txtbox.AppendText("\n\nA operation is already running. Please try again after it has finished.");
                return;
            }
            Running = true;
            CheckIP();
            getQuestIP();
            BMBF_Link();
            if (!adb("pull /sdcard/Android/data/com.beatgames.beatsaber/files/mods \"" + exe + "\\ModChecks"))
            {
                Running = false;
                return;
            }

            if (Directory.Exists(exe + "\\ModChecks\\mods"))
            {
                //game is modded
                MessageBoxResult result1 = MessageBox.Show("Modded Beat Saber has been detected. If you press yes I'll uninstall Beat Saber and BMBF and make a Backup of it to restore. If you press no you'll cancle Updating.", "BMBF Manager - BMBF Updater", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                switch (result1)
                {
                    case MessageBoxResult.No:
                        txtbox.AppendText("\n\nBMBF Updating aborted.");
                        txtbox.ScrollToEnd();
                        Running = false;
                        return;
                }
                if (!adb("pull /sdcard/Android/data/com.beatgames.beatsaber/files/ \"" + exe + "\\Backup\""))
                {
                    Running = false;
                    return;
                }

                //Backup Playlists
                try
                {
                    txtbox.AppendText("\n\nBacking up Playlist to " + exe + "\\Backup\\Playlists.json");
                    txtbox.ScrollToEnd();
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));

                    if (!adb("pull /sdcard/BMBFData/Playlists/ \"" + exe + "\\Backup\"")) return;

                    WebClient client2 = new WebClient();

                    var j = JSON.Parse(client2.DownloadString("http://" + MainWindow.IP + ":50000/host/beatsaber/config"));
                    File.WriteAllText(exe + "\\Backup\\Playlists.json", j["Config"].ToString());
                    txtbox.AppendText("\n\nBacked up Playlists to " + exe + "\\Backup\\Playlists.json");
                    txtbox.ScrollToEnd();
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
                }
                catch
                {
                    txtbox.AppendText("\n\n\nAn error occured (Code: PL100). Check following:");
                    txtbox.AppendText("\n\n- You put in the Quests IP right.");
                    txtbox.AppendText("\n\n- You've choosen a Backup Name.");
                    txtbox.AppendText("\n\n- Your Quest is on.");

                }


                if (!adb("uninstall com.beatgames.beatsaber"))
                {
                    Running = false;
                    return;
                }
                if (!adb("uninstall com.weloveoculus.BMBF"))
                {
                    Running = false;
                    return;
                }
                MessageBoxResult result2 = MessageBox.Show("Please download Beat Saber from the oculus store, play a song and then close it. Press OK once you finished.", "BMBF Manager - BMBF Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
                MessageBoxResult result3 = MessageBox.Show("I want to make sure. Do you have unmodded Beat Saber installed and opened it at least once?", "BMBF Manager - BMBF Updater", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                switch (result3)
                {
                    case MessageBoxResult.No:
                        txtbox.AppendText("\n\nBMBF Updating aborted. Please Install unmodded Beat Saber and start it once");
                        txtbox.ScrollToEnd();
                        Running = false;
                        return;
                }
            }
            else
            {
                if (!adb("pull /sdcard/Android/data/com.beatgames.beatsaber/files/ \"" + exe + "\\Backup\""))
                {
                    Running = false;
                    return;
                }
                MessageBoxResult result = MessageBox.Show("Looks like you have unmodded Beat Saber installed. Did you open it at least once?", "BMBF Manager - BMBF Updater", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                switch (result)
                {
                    case MessageBoxResult.No:
                        txtbox.AppendText("\n\nBMBF Updating aborted. Please Install unmodded Beat Saber and start it once");
                        txtbox.ScrollToEnd();
                        Running = false;
                        return;
                }
            }

            if (Directory.Exists(exe + "\\ModChecks\\mods")) Directory.Delete(exe + "\\ModChecks\\mods", true);

            if (Directory.Exists(exe + "\\Backup\\files\\mods")) Directory.Delete(exe + "\\Backup\\files\\mods", true);
            if (Directory.Exists(exe + "\\Backup\\files\\libs")) Directory.Delete(exe + "\\Backup\\files\\libs", true);
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));

            List<String> BadBMBF = new List<String>();
            foreach (JSONNode version in MainWindow.UpdateJSON["BadBMBF"])
            {
                foreach (JSONNode bmbf in BMBFStable)
                {
                    if (bmbf["tag"].ToString().Replace("\"", "") == version.ToString().Replace("\"", ""))
                    {
                        BadBMBF.Add(bmbf["id"].ToString().Replace("\"", ""));
                        break;
                    }
                }
            }

            if (BadBMBF.Contains(BMBFStable[0]["id"].ToString().Replace("\"", "")))
            {
                JSONNode lastBMBF = JSON.Parse("{}");
                foreach (JSONNode bmbf in BMBFStable)
                {
                    if (!BadBMBF.Contains(bmbf["id"].ToString().Replace("\"", "")))
                    {
                        lastBMBF = bmbf;
                        break;
                    }
                }
                MessageBoxResult result4 = MessageBox.Show("The newest BMBF Version (" + BMBFStable[0]["tag"] + ") doesn't work for many people. I'd suggest you update to a more stable version. The last entry that's not listed as not working is BMBF version " + lastBMBF["tag"] + ".\nIf you want to install the recommended version of BMBF press yes. If you want to install the newest one press no. ", "BMBF Manager - BMBF Updater", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                switch (result4)
                {
                    case MessageBoxResult.No:
                        txtbox.AppendText("\n\nDownloading newest BMBF version");
                        txtbox.ScrollToEnd();
                        foreach (JSONNode asset in BMBFStable[0]["assets"])
                        {
                            if (asset["name"].ToString().Replace("\"", "") == "com.weloveoculus.BMBF.apk")
                            {
                                MainWindow.BMBF = "https://bmbf.dev/stable/" + asset["id"];
                                break;
                            }
                        }
                        break;
                    case MessageBoxResult.Yes:
                        txtbox.AppendText("\n\nDownloading recommended BMBF version");
                        txtbox.ScrollToEnd();
                        foreach (JSONNode asset in lastBMBF["assets"])
                        {
                            if (asset["name"].ToString().Replace("\"", "") == "com.weloveoculus.BMBF.apk")
                            {
                                MainWindow.BMBF = "https://bmbf.dev/stable/" + asset["id"];
                                break;
                            }
                        }
                        break;
                }
            }
            else
            {
                foreach (JSONNode asset in BMBFStable[0]["assets"])
                {
                    if (asset["name"].ToString().Replace("\"", "") == "com.weloveoculus.BMBF.apk")
                    {
                        MainWindow.BMBF = "https://bmbf.dev/stable/" + asset["id"];
                        break;
                    }
                }
            }

            using (TimeoutWebClient client2 = new TimeoutWebClient())
            {
                client2.DownloadFileAsync(new Uri(MainWindow.BMBF), exe + "\\tmp\\BMBF.apk");
                client2.DownloadFileCompleted += new AsyncCompletedEventHandler(finishedBMBFDownload);
            }

        }

        private void finishedBMBFDownload(object sender, AsyncCompletedEventArgs e)
        {
            txtbox.AppendText("\nDownload Complete");
            txtbox.ScrollToEnd();
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));


            //Install BMBF
            txtbox.AppendText("\n\nInstalling new BMBF");
            txtbox.ScrollToEnd();
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));


            adb("install -r \"" + exe + "\\tmp\\BMBF.apk\"");

            //Mod Beat Saber
            txtbox.AppendText("\n\nModding Beat Saber. Please wait...");
            txtbox.ScrollToEnd();
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));


            adb("shell am start -n com.weloveoculus.BMBF/com.weloveoculus.BMBF.MainActivity"); //Start BMBF
            adb("shell pm grant com.weloveoculus.BMBF android.permission.READ_EXTERNAL_STORAGE"); //Grant permission read
            adb("shell pm grant com.weloveoculus.BMBF android.permission.WRITE_EXTERNAL_STORAGE"); //Grant permission write
            // Need to add a delay
            System.Threading.Thread.Sleep(6000);
            adb("shell am start -n com.weloveoculus.BMBF/com.weloveoculus.BMBF.MainActivity"); //Start BMBF
            System.Threading.Thread.Sleep(5000);
            adb("shell am start -n com.weloveoculus.BMBF/com.weloveoculus.BMBF.MainActivity"); //Start BMBF
            System.Threading.Thread.Sleep(5000);
            TimeoutWebClient client = new TimeoutWebClient();
            client.UploadDataAsync(new Uri("http://" + MainWindow.IP + ":50000/host/mod/install/step1"), "POST", new byte[0]);
            client.UploadDataCompleted += new UploadDataCompletedEventHandler(finishedstep1);
        }

        private void finishedstep1(object sender, AsyncCompletedEventArgs e)
        {
            TimeoutWebClient client = new TimeoutWebClient();
            adb("uninstall com.beatgames.beatsaber");
            client.UploadDataAsync(new Uri("http://" + MainWindow.IP + ":50000/host/mod/install/step2"), "POST", new byte[0]);
            client.UploadDataCompleted += new UploadDataCompletedEventHandler(finishedstep2);
        }

        private void finishedstep2(object sender, UploadDataCompletedEventArgs e)
        {
            TimeoutWebClient client = new TimeoutWebClient();
            adb("pull /sdcard/Android/data/com.weloveoculus.BMBF/cache/beatsabermod.apk \"" + exe + "\\tmp\\beatsabermod.apk\"");
            adb("install -r \"" + exe + "\\tmp\\beatsabermod.apk\"");
            client.UploadDataAsync(new Uri("http://" + MainWindow.IP + ":50000/host/mod/install/step3"), "POST", new byte[0]);
            client.UploadDataCompleted += new UploadDataCompletedEventHandler(finishedstep3);
        }

        private void finishedstep3(object sender, UploadDataCompletedEventArgs e)
        {
            adb("shell am force-stop com.weloveoculus.BMBF");
            adb("shell am start -n com.weloveoculus.BMBF/com.weloveoculus.BMBF.MainActivity"); //Start BMBF
            adb("shell pm grant com.beatgames.beatsaber android.permission.READ_EXTERNAL_STORAGE"); //Grant permission read
            adb("shell pm grant com.beatgames.beatsaber android.permission.WRITE_EXTERNAL_STORAGE"); //Grant permission write

            if (!adb("push \"" + exe + "\\Backup\\files\" /sdcard/Android/data/com.beatgames.beatsaber"))
            {
                Running = false;
                return;
            }

            System.Threading.Thread.Sleep(6000);

            reloadsongsfolder();

            //restore Playlists
            try
            {
                if (!File.Exists(exe + "\\Backup\\Playlists.json"))
                {
                    txtbox.AppendText("\n\n\nFinished Installing BMBF and modding Beat Saber. Please click \"Reload Songs Folder\" in BMBF to reload your Songs if you Updated BMBF.");
                    txtbox.ScrollToEnd();
                    Running = false;
                    return;
                }

                WebClient client3 = new WebClient();

                String Playlists = exe + "\\Backup\\Playlists.json";

                var j = JSON.Parse(client3.DownloadString("http://" + MainWindow.IP + ":50000/host/beatsaber/config"));
                var p = JSON.Parse(File.ReadAllText(Playlists));

                j["Config"]["Playlists"] = p["Playlists"];
                File.WriteAllText(exe + "\\tmp\\config.json", j["Config"].ToString());

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate {
                    postChanges(exe + "\\tmp\\config.json");
                }));
                txtbox.AppendText("\n\nRestored old Playlists.");
                txtbox.ScrollToEnd();
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
            }
            catch
            {
                txtbox.AppendText("\n\n\nAn error occured (Code: BMBF100). Check following:");
                txtbox.AppendText("\n\n- Your Quest is on and BMBF opened");
                txtbox.AppendText("\n\n- You put in the Quests IP right.");
            }

            txtbox.AppendText("\n\n\nFinished Installing BMBF and modding Beat Saber. Please click \"Reload Songs Folder\" in BMBF to reload your Songs if you Updated BMBF.");
            txtbox.ScrollToEnd();
            Running = false;
        }

        public async void StartSupport(String Link)
        {
            txtbox.AppendText("\n\n" + Link);
            String section = Link.Replace("bm://", "").Replace("%20", " ").ToLower();
            if(Link.ToLower().StartsWith("beatsaver://"))
            {
                String bsr = section.Replace("beatsaver://", "").ToLower();
                DownloadSong(bsr);
            }
            else if(section.StartsWith("support/resetassets"))
            {
                BackupPlaylists();
                resetassets();
                reloadsongsfolder();
                RestorePlaylists();
            } else if(section.StartsWith("support/quickfix"))
            {
                BackupPlaylists();
                resetassets();
                reloadsongsfolder();
                Sync();
                RestorePlaylists();
            } else if(section.StartsWith("mods/install/"))
            {
                
                String DownloadLink = InstallMod(section.Replace("mods/install/", "").ToLower());
                if (DownloadLink == "Error")
                {
                    txtbox.AppendText("\nThe Mod couldn't be found for your GameVersion.");
                    return;
                }
                if(DownloadLink.StartsWith("Forward"))
                {
                    MessageBoxResult result1 = MessageBox.Show("You have to download and install the mod " + section.Replace("mods/install/", "") + " manually. If you click yes I'll redirect you to the download page and open BMBF for you.\nDo you wish to continue?", "BMBF Manager - Mod Installing", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    switch (result1)
                    {
                        case MessageBoxResult.No:
                            txtbox.AppendText("\n\nMod Installing Aborted.");
                            txtbox.ScrollToEnd();
                            Running = false;
                            return;
                    }
                    Process.Start("http://" + MainWindow.IP + ":50000/main/upload");
                    Process.Start(DownloadLink.Replace("Forward", "").Replace("\"", ""));
                }
                installtoquest(DownloadLink);
            } else if(section.StartsWith("songs/install/"))
            {
                String bsr = section.Replace("songs/install/", "").ToLower();
                DownloadSong(bsr);
            } else if(section.StartsWith("bbbu/backup/"))
            {
                String Name = section.Replace("bbbu/backup/", "");
                BBBU BBBU = new BBBU();
                BBBU.Show();
                BBBU.BackupLink(Name);
                this.Close();
            }
            else if (section.StartsWith("bbbu/abackup/"))
            {
                String Name = section.Replace("bbbu/abackup/", "");
                BBBU BBBU = new BBBU();
                BBBU.Show();
                BBBU.BackupLink(Name);
                this.Close();
            }
            else if (section.StartsWith("bbbu/restore"))
            {
                String Name = section.Replace("bbbu/restore/", "");
                BBBU BBBU = new BBBU();
                BBBU.Show();
                BBBU.selectBackup(Name);
                this.Close();
            } else if(section.StartsWith("update"))
            {
                MessageBoxResult result = MessageBox.Show("You have clicked a link to Update/Install BMBF\nDo you wish to continue", "BMBF Manager - BMBF Updater", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                switch (result)
                {
                    case MessageBoxResult.No:
                        txtbox.AppendText("\n\nAborted.");
                        txtbox.ScrollToEnd();
                        Running = false;
                        return;
                }
                UpdateBMBF();
            } else if(section.StartsWith("switchversion"))
            {
                MessageBoxResult result = MessageBox.Show("You have clicked a link to switch from the modded/unmodded to the unmodded/modded version of Beatsaber.\nDo you wish to continue", "BMBF Manager - Version Switcher", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                switch (result)
                {
                    case MessageBoxResult.No:
                        txtbox.AppendText("\n\nAborted.");
                        txtbox.ScrollToEnd();
                        Running = false;
                        return;
                }
                SwitchVersion();
            }
        }
    }
}
