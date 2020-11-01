﻿using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BMBF_Manager
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int MajorV = 1;
        int MinorV = 0;
        int PatchV = 0;
        Boolean Preview = true;

        Boolean draggable = true;
        String exe = AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.Length - 1);
        public static String IP = "";
        String BMBF = "https://bmbf.dev/stable/27153984";
        JSONNode json = JSON.Parse("{}");


        public MainWindow()
        {
            InitializeComponent();
            if (!Directory.Exists(exe + "\\Backups")) Directory.CreateDirectory(exe + "\\Backups");
            if (!Directory.Exists(exe + "\\Backup")) Directory.CreateDirectory(exe + "\\Backup");
            if (!Directory.Exists(exe + "\\ModChecks")) Directory.CreateDirectory(exe + "\\ModChecks");
            if (!Directory.Exists(exe + "\\tmp")) Directory.CreateDirectory(exe + "\\tmp");
            if (File.Exists(exe + "\\BM_Update.exe")) File.Delete(exe + "\\BM_Update.exe");
            UpdateB.Visibility = Visibility.Hidden;
            StartBMBF();
            loadConfig();
            QuestIP();
            Quest.Text = IP;
            txtbox.Text = "Output:";
            //Update();
            BMBF_Link();
        }

        public void loadConfig()
        {
            if(!File.Exists(exe + "\\Config.json"))
            {
                IP = "Quest IP";
                return;
            }
            json = JSON.Parse(File.ReadAllText(exe + "\\Config.json"));
            IP = json["IP"];
            Quest.Text = IP;
        }

        public void saveConfig()
        {
            getQuestIP();
            json["IP"] = IP;
            File.WriteAllText(exe + "\\Config.json", json.ToString());
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



        public static void QuestIP()
        {
            String IPS = adbS("shell ifconfig wlan0");
            int Index = IPS.IndexOf("inet addr:");
            Boolean space = false;
            String FIP = "";
            for (int i = 0; i < IPS.Length; i++)
            {
                if (i > (Index + 9) && i < (Index + 9 + 16))
                {
                    if (IPS.Substring(i, 1) == " ")
                    {
                        space = true;
                    }
                    if (!space)
                    {
                        FIP = FIP + IPS.Substring(i, 1);
                    }
                }
            }

            if (FIP == "")
            {
                IP = "Quest IP";
                return;
            }
            IP = FIP;
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
            try
            {
                if (Directory.Exists(exe + "\\tmp"))
                {
                    Directory.Delete(exe + "\\tmp", true);
                }
            }
            catch
            {
            }
            saveConfig();
            this.Close();
        }

        public void StartBMBF()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate
            {
                adb("shell am start -n com.weloveoculus.BMBF/com.weloveoculus.BMBF.MainActivity");
            }));
        }

        public void getQuestIP()
        {
            IP = Quest.Text;
            return;
        }

        public Boolean CheckIP()
        {
            getQuestIP();
            if (IP == "Quest IP")
            {
                return false;
            }
            IP = IP.Replace(":5000000", "");
            IP = IP.Replace(":500000", "");
            IP = IP.Replace(":50000", "");
            IP = IP.Replace(":5000", "");
            IP = IP.Replace(":500", "");
            IP = IP.Replace(":50", "");
            IP = IP.Replace(":5", "");
            IP = IP.Replace(":", "");
            IP = IP.Replace("/", "");
            IP = IP.Replace("https", "");
            IP = IP.Replace("http", "");
            IP = IP.Replace("Http", "");
            IP = IP.Replace("Https", "");

            int count = 0;
            for (int i = 0; i < IP.Length; i++)
            {
                if (IP.Substring(i, 1) == ".")
                {
                    count++;
                }
            }
            if (count != 3)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate
                {
                    Quest.Text = IP;
                }));
                return false;
            }
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate
            {
                Quest.Text = IP;
            }));
            return true;
        }

        public static Boolean adb(String Argument)
        {
            String User = System.Environment.GetEnvironmentVariable("USERPROFILE");
            ProcessStartInfo s = new ProcessStartInfo();
            s.CreateNoWindow = false;
            s.UseShellExecute = false;
            s.FileName = "adb.exe";
            s.WindowStyle = ProcessWindowStyle.Minimized;
            s.Arguments = Argument;
            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(s))
                {
                    exeProcess.WaitForExit();
                    return true;
                }
            }
            catch
            {

                ProcessStartInfo se = new ProcessStartInfo();
                se.CreateNoWindow = false;
                se.UseShellExecute = false;
                se.FileName = User + "\\AppData\\Roaming\\SideQuest\\platform-tools\\adb.exe";
                se.WindowStyle = ProcessWindowStyle.Minimized;
                se.Arguments = Argument;
                try
                {
                    // Start the process with the info we specified.
                    // Call WaitForExit and then the using statement will close.
                    using (Process exeProcess = Process.Start(se))
                    {
                        exeProcess.WaitForExit();
                        return true;
                    }
                }
                catch
                {
                    // Log error.
                    return false;
                }
            }
        }

        public static String adbS(String Argument)
        {
            String User = System.Environment.GetEnvironmentVariable("USERPROFILE");
            ProcessStartInfo s = new ProcessStartInfo();
            s.CreateNoWindow = false;
            s.UseShellExecute = false;
            s.FileName = "adb.exe";
            s.WindowStyle = ProcessWindowStyle.Minimized;
            s.RedirectStandardOutput = true;
            s.Arguments = Argument;
            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(s))
                {
                    String IPS = exeProcess.StandardOutput.ReadToEnd();
                    exeProcess.WaitForExit();
                    return IPS;
                }
            }
            catch
            {

                ProcessStartInfo se = new ProcessStartInfo();
                se.CreateNoWindow = false;
                se.UseShellExecute = false;
                se.FileName = User + "\\AppData\\Roaming\\SideQuest\\platform-tools\\adb.exe";
                se.WindowStyle = ProcessWindowStyle.Minimized;
                se.RedirectStandardOutput = true;
                se.Arguments = Argument;
                try
                {
                    // Start the process with the info we specified.
                    // Call WaitForExit and then the using statement will close.
                    using (Process exeProcess = Process.Start(se))
                    {
                        String IPS = exeProcess.StandardOutput.ReadToEnd();
                        exeProcess.WaitForExit();
                        return IPS;

                    }
                }
                catch
                {
                    // Log error.
                    return "Error";
                }
            }
        }

        public void Update()
        {
            try
            {
                //Download Update.txt
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        client.DownloadFile("https://raw.githubusercontent.com/ComputerElite/BM/main/Update.txt", exe + "\\tmp\\Update.txt");
                    }
                    catch
                    {
                        txtbox.AppendText("\n\n\nAn error Occured (Code: UD100). Couldn't check for Updates. Check following");
                        txtbox.AppendText("\n\n- Your PC has internet.");
                    }
                }
                StreamReader VReader = new StreamReader(exe + "\\tmp\\Update.txt");

                String line;
                int l = 0;

                int MajorU = 0;
                int MinorU = 0;
                int PatchU = 0;
                while ((line = VReader.ReadLine()) != null)
                {
                    if (l == 0)
                    {
                        String URL = line;
                    }
                    if (l == 1)
                    {
                        MajorU = Convert.ToInt32(line);
                    }
                    if (l == 2)
                    {
                        MinorU = Convert.ToInt32(line);
                    }
                    if (l == 3)
                    {
                        PatchU = Convert.ToInt32(line);
                    }
                    l++;
                }

                if (MajorU > MajorV || MinorU > MinorV || PatchU > PatchV)
                {
                    //Newer Version available
                    UpdateB.Visibility = Visibility.Visible;
                }

                String MajorVS = Convert.ToString(MajorV);
                String MinorVS = Convert.ToString(MinorV);
                String PatchVS = Convert.ToString(PatchV);
                String MajorUS = Convert.ToString(MajorU);
                String MinorUS = Convert.ToString(MinorU);
                String PatchUS = Convert.ToString(PatchU);

                String VersionVS = MajorVS + MinorVS + PatchVS;
                int VersionV = Convert.ToInt32(VersionVS);
                String VersionUS = MajorUS + MinorUS + PatchUS + " ";
                int VersionU = Convert.ToInt32(VersionUS);
                if (VersionV > VersionU)
                {
                    //Newer Version that hasn't been released yet
                    txtbox.AppendText("\n\nLooks like you have a preview version. Downgrade now from " + MajorV + "." + MinorV + "." + PatchV + " to " + MajorU + "." + MinorU + "." + PatchU + " xD");
                    UpdateB.Visibility = Visibility.Visible;
                    UpdateB.Content = "Downgrade Now xD";
                }
                if (VersionV == VersionU && Preview)
                {
                    //User has Preview Version but a release Version has been released
                    txtbox.AppendText("\n\nLooks like you have a preview version. The release version has been released. Please Update now. ");
                    UpdateB.Visibility = Visibility.Visible;
                }
                VReader.Close();
            }
            catch
            {

            }
            try
            {
                File.Delete(exe + "\\tmp\\Update.txt");
            }
            catch
            {
            }
        }

        private void Start_Update(object sender, RoutedEventArgs e)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile("https://github.com/ComputerElite/BM/raw/main/BM_Update.exe", exe + "\\BM_Update.exe");
            }
            //Process.Start(exe + "\\QSU_Update.exe");
            ProcessStartInfo s = new ProcessStartInfo();
            s.CreateNoWindow = false;
            s.UseShellExecute = false;
            s.FileName = exe + "\\BM_Update.exe";
            try
            {
                using (Process exeProcess = Process.Start(s))
                {
                }
                this.Close();
            }
            catch
            {
                // Log error.
                txtbox.AppendText("\n\n\nAn error Occured (Code: UD200). Couldn't download Update.");
            }
        }

        private void Mini(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void InstallSongs(object sender, RoutedEventArgs e)
        {
            Songs SongsWindow = new Songs();
            SongsWindow.Show();
        }

        private void InstallMods (object sender, RoutedEventArgs e)
        {
            CheckIP();
            Mods ModsWindow = new Mods();
            ModsWindow.Show();
        }

        private void BMBF_Link()
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile("https://bmbf.dev/stable/json", exe + "\\tmp\\BMBF.txt");
            }
            StreamReader VReader = new StreamReader(exe + "\\tmp\\BMBF.txt");

            String line;
            String f = "";
            while ((line = VReader.ReadLine()) != null)
            {
                f = f + line;
            }

            var json = SimpleJSON.JSON.Parse(f);
            String id = "";
            String name;
            for (int i = 0; i < 5; i++)
            {
                name = json[0]["assets"][i]["name"].ToString();
                if (name == "\"com.weloveoculus.BMBF.apk\"")
                {
                    id = json[0]["assets"][i]["id"].ToString();
                    break;
                }
            }
            BMBF = "https://bmbf.dev/stable/" + id;
        }

        private void UpdateBMBF(object sender, RoutedEventArgs e)
        {
            CheckIP();
            MessageBoxResult result = MessageBox.Show("Do you have unmodded Beat Saber installed and opened it at least once?", "BMBF Manager BMBF Updater", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.No:
                    txtbox.AppendText("\n\nBMBF Updating aborted. Please Install unmodded Beat Saber and start it once");
                    txtbox.ScrollToEnd();
                    return;
            }

            getQuestIP();
            if (!adb("pull /sdcard/Android/data/com.beatgames.beatsaber/files/ \"" + exe + "\\Backup\""))
            {
                txtbox.AppendText("\n\n\nAn error Occured (Code: ADB100). Check following");
                txtbox.AppendText("\n\n- Your Quest is connected and USB Debugging enabled.");
                txtbox.AppendText("\n\n- You have adb installed.");
                return;
            }
            if (Directory.Exists(exe + "\\Backup\\files\\mods")) Directory.Delete(exe + "\\Backup\\files\\mods", true);
            if (Directory.Exists(exe + "\\Backup\\files\\libs")) Directory.Delete(exe + "\\Backup\\files\\libs", true);
            txtbox.AppendText("\n\nDownloading BMBF");
            txtbox.ScrollToEnd();
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));

            using (TimeoutWebClient client2 = new TimeoutWebClient())
            {
                client2.DownloadFile(BMBF, exe + "\\tmp\\BMBF.apk");
            }
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
            client.UploadData("http://" + IP + ":50000/host/mod/install/step1", "POST", new byte[0]);
            adb("uninstall com.beatgames.beatsaber");
            client.UploadData("http://" + IP + ":50000/host/mod/install/step2", "POST", new byte[0]);
            adb("pull /sdcard/Android/data/com.weloveoculus.BMBF/cache/beatsabermod.apk \"" + exe + "\\tmp\\beatsabermod.apk\"");
            adb("install -r \"" + exe + "\\tmp\\beatsabermod.apk\"");
            client.UploadData("http://" + IP + ":50000/host/mod/install/step3", "POST", new byte[0]);
            adb("shell am force-stop com.weloveoculus.BMBF");
            adb("shell am start -n com.weloveoculus.BMBF/com.weloveoculus.BMBF.MainActivity"); //Start BMBF
            adb("shell pm grant com.beatgames.beatsaber android.permission.READ_EXTERNAL_STORAGE"); //Grant permission read
            adb("shell pm grant com.beatgames.beatsaber android.permission.WRITE_EXTERNAL_STORAGE"); //Grant permission write

            if (!adb("push \"" + exe + "\\Backup\\files\" /sdcard/Android/data/com.beatgames.beatsaber"))
            {
                txtbox.AppendText("\n\n\nAn error Occured (Code: ADB100). Check following");
                txtbox.AppendText("\n\n- Your Quest is connected and USB Debugging enabled.");
                txtbox.AppendText("\n\n- You have adb installed.");
                return;
            }

            txtbox.AppendText("\n\n\nFinished Installing BMBF and modding Beat Saber. Please click \"Reload Songs Folder\" in BMBF to reload your Songs if you Updated BMBF.");
            txtbox.ScrollToEnd();


        }

        private void SwitchVersion(object sender, RoutedEventArgs e)
        {
            if(!adb("pull /sdcard/Android/data/com.beatgames.beatsaber/files/mods \"" + exe + "\\ModChecks"))
            {
                txtbox.AppendText("\n\n\nAn error Occured (Code: ADB100). Check following");
                txtbox.AppendText("\n\n- Your Quest is connected and USB Debugging enabled.");
                txtbox.AppendText("\n\n- You have adb installed.");
                return;
            }

            if(Directory.Exists(exe + "\\ModChecks\\mods"))
            {
                //game is modded
                if (File.Exists(exe + "\\Backups\\modded.apk"))
                {
                    //Unmodded Beat Saber may be installed
                    MessageBoxResult result = MessageBox.Show("It looks like your last Action was installing unmodded Beat Saber. If you continue and have unmodded Beat Saber installed you must mod Beat Saber By hand.\nDo you wish to continue?", "BMBF Manager Version Switcher", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    switch (result)
                    {
                        case MessageBoxResult.No:
                            txtbox.AppendText("\n\nAborted.");
                            txtbox.ScrollToEnd();
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
                    return;
                }
                if (!adb("pull /sdcard/Android/data/com.beatgames.beatsaber/files \"" + exe + "\\Backups\""))
                {
                    txtbox.AppendText("\n\n\nAn error Occured (Code: ADB100). Check following");
                    txtbox.AppendText("\n\n- Your Quest is connected and USB Debugging enabled.");
                    txtbox.AppendText("\n\n- You have adb installed.");
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
                    return;
                }
                if (!adb("install \"" + exe + "\\tmp\\unmodded.apk\""))
                {
                    txtbox.AppendText("\n\n\nAn error Occured (Code: ADB100). Check following");
                    txtbox.AppendText("\n\n- Your Quest is connected and USB Debugging enabled.");
                    txtbox.AppendText("\n\n- You have adb installed.");
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

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate {
                    txtbox.AppendText("\n\nFinished. You can now play vanilla Beat Saber.");
                    txtbox.ScrollToEnd();
                }));

            } else
            {
                //game is unmodded
                if (!File.Exists(exe + "\\Backups\\modded.apk"))
                {
                    txtbox.AppendText("\n\nPlease Click \"Install/Update BMBF\" to mod Beat Saber the first time.");
                    return;
                }
                adb("pull /sdcard/Android/data/com.beatgames.beatsaber/files/LocalDailyLeaderboards.dat \"" + exe + "\\Backups\\files\\LocalDailyLeaderboards.dat\"");
                adb("pull /sdcard/Android/data/com.beatgames.beatsaber/files/LocalLeaderboards.dat \"" + exe + "\\Backups\\files\\LocalLeaderboards.dat\"");
                adb("pull /sdcard/Android/data/com.beatgames.beatsaber/files/PlayerData.dat \"" + exe + "\\Backups\\files\\PlayerData.dat\"");
                adb("pull /sdcard/Android/data/com.beatgames.beatsaber/files/AvatarData.dat \"" + exe + "\\Backups\\files\\AvatarData.dat\"");
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate {
                    txtbox.AppendText("\n\nUninstalling Beat Saber.");
                    txtbox.ScrollToEnd();
                }));
                if (!adb("uninstall com.beatgames.beatsaber"))
                {
                    txtbox.AppendText("\n\n\nAn error Occured (Code: ADB100). Check following");
                    txtbox.AppendText("\n\n- Your Quest is connected and USB Debugging enabled.");
                    txtbox.AppendText("\n\n- You have adb installed.");
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
    }
}   