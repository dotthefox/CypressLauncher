using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using CypressLauncher.Properties;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace CypressLauncher;

public class LaunchWindow : Form
{
    private enum PVZGame
    {
        GW1,
        GW2,
        BFN
    }

    private static string s_destDLLName = "dinput8.dll";

    private static string s_patchedGW2ExeName = "GW2.PreEAAC.exe";

    private static string s_patchedBFNExeName = "BFN.PreEAAC.exe";

    private static string s_launcherSavedataFilename = "launcherdata.json";

    private static Dictionary<PVZGame, string> s_gameToExecutableName = new Dictionary<PVZGame, string>
    {
        {
            PVZGame.GW1,
            "PVZ.Main_Win64_Retail.exe"
        },
        {
            PVZGame.GW2,
            "GW2.Main_Win64_Retail.exe"
        },
        {
            PVZGame.BFN,
            "PVZBattleforNeighborville.exe"
        }
    };

    private static Dictionary<PVZGame, string> s_gameToPatchedExecutableName = new Dictionary<PVZGame, string>
    {
        {
            PVZGame.GW2,
            s_patchedGW2ExeName
        },
        {
            PVZGame.BFN,
            s_patchedBFNExeName
        }
    };

    private static Dictionary<PVZGame, string> s_gameToGameName = new Dictionary<PVZGame, string>
    {
        {
            PVZGame.GW1,
            "Garden Warfare 1"
        },
        {
            PVZGame.GW2,
            "Garden Warfare 2"
        },
        {
            PVZGame.BFN,
            "Battle for Neighborville"
        }
    };

    private static Dictionary<PVZGame, string> s_specialLaunchArgsForGame = new Dictionary<PVZGame, string> {
    {
        PVZGame.GW1,
        "-GameTime.MaxSimFps -1"
    },

    {

        PVZGame.GW2,
        "-GameMode.SkipIntroHubNIS true"
    }

    };

    private static Dictionary<PVZGame, string> s_serverLaunchArgsForGame = new Dictionary<PVZGame, string> {
    {
        PVZGame.GW1,
        "-Online.ClientIsPresenceEnabled false -Online.ServerIsPresenceEnabled false -SyncedBFSettings.AllUnlocksUnlocked true -PingSite ams -name \"PVZGW Dedicated Server\" -BFServer.MapSequencerEnabled false "
    },

    {

        PVZGame.GW2,
        "-enableServerLog -platform Win32 -console -Game.Platform GamePlatform_Win32 -Game.EnableServerLog true -GameMode.SkipIntroHubNIS true -Online.Backend Backend_Local -Online.PeerBackend Backend_Local -PVZServer.MapSequencerEnabled false "

    }
    };

    private PVZGame m_selectedGame = PVZGame.GW2;

    private ResourceManager m_resourceManager = new ResourceManager("CypressLauncher.Properties.Resources", Assembly.GetExecutingAssembly());

    private IContainer components;

    private TextBox ServerIPTextBox;

    private Label label1;

    private Button JoinButton;

    private Label GameStatusLabel;

    private Label label2;

    private TextBox UsernameTextbox;

    private Label GameDirectoryLabel;

    private Button SelectGameDirectoryButton;

    private Label label3;

    private Label label4;

    private TextBox AdditionalLaunchArgumentsBox;

    private ComboBox GameSelectorComboBox;

    private Label label5;

    private Button AutoFindGameDirButton;

    private CheckBox UseModsCheckbox;

    private ComboBox ModPackCombobox;

    private Label label6;

    private TextBox ServerPasswordTextBox;
    private Label label8;
    private Label label9;
    private Label DedicatedServerPasswordLabel;
    private TextBox DedicatedServerPasswordTextBox;
    private Label InclusionLabel;
    private TextBox InclusionTextBox;
    private Label DeviceIPLabel;
    private TextBox DeviceIPTextBox;
    private Button StartServerButton;
    private Label LevelLabel;
    private TextBox LevelTextBox;
    private Label PlayerCountLabel;
    private TextBox PlayerCountTextBox;
    private Label AdditionalServerLaunchArgumentsLabel;
    private TextBox AdditionalServerLaunchArgumentsTextBox;
    private Label PlaylistLabel;
    private ComboBox PlaylistComboBox;
    private CheckBox PlaylistCheckBox;
    private Label ServerPasswordLabel;

    public LaunchWindow()
    {
        InitializeComponent();
    }

    private string GetGameDir()
    {
        return GameDirectoryLabel.Text;
    }

    private string GetServerDLLName()
    {
        return $"cypress_{m_selectedGame}.dll";
    }

    private string GetAppdataDir()
    {
        string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Cypress");
        Directory.CreateDirectory(text);
        return text;
    }

    private bool GameRequiresPatchedExe(string filepath, ref bool failed)
    {
        try
        {
            GameStatusLabel.Text = "Checking executable";
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(PEFile.GetNTHeaderFromPE(filepath).TimeDateStamp);
            failed = false;
            switch (m_selectedGame)
            {
                case PVZGame.GW1:
                    return false;
                case PVZGame.GW2:
                case PVZGame.BFN:
                    return dateTimeOffset.Year >= 2024;
                default:
                    return false;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Exception while checking game executable: " + ex.Message);
            failed = true;
            return false;
        }
    }

    private string GetRtPLaunchCode()
    {
        uint num = 0u;
        DateTime utcNow = DateTime.UtcNow;
        int month = utcNow.Month;
        int day = utcNow.Day;
        num = (uint)((utcNow.Year * 104729) ^ (month * 224737) ^ (day * 350377));
        return (num ^ ((num << 16) ^ (num >> 16))).ToString();
    }

    private bool VerifyLaunch()
    {
        if (string.IsNullOrWhiteSpace(GameDirectoryLabel.Text))
        {
            GameStatusLabel.Text = "Game directory not set.";
            return false;
        }
        if (!File.Exists(GetServerDLLName()))
        {
            GameStatusLabel.Text = "Server DLL not found. Verify that " + GetServerDLLName() + " is in the launcher's folder.";
            return false;
        }
        if (string.IsNullOrEmpty(ServerIPTextBox.Text))
        {
            GameStatusLabel.Text = "Must enter a server address.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(UsernameTextbox.Text))
        {
            GameStatusLabel.Text = "Username cannot be empty.";
            return false;
        }
        if (UsernameTextbox.Text.Length < 3)
        {
            GameStatusLabel.Text = "Username must be longer than 4 characters.";
            return false;
        }
        if (UsernameTextbox.Text.Length > 32)
        {
            GameStatusLabel.Text = "Username cannot be longer than 32 characters.";
            return false;
        }
        //Process[] processesByName3 = Process.GetProcessesByName(s_gameToExecutableName[m_selectedGame].Replace(".exe", string.Empty));
        //if (processesByName3.Length != 0)
        //{
        //    GameStatusLabel.Text = $"Game is already running. (PID {processesByName3[0].Id})";
        //    return false;
        //}
        return true;
    }

    private bool DedicatedVerifyLaunch()
    {
        if (string.IsNullOrWhiteSpace(GameDirectoryLabel.Text))
        {
            GameStatusLabel.Text = "Game directory not set.";
            return false;
        }
        if (!File.Exists(GetServerDLLName()))
        {
            GameStatusLabel.Text = "Server DLL not found. Verify that " + GetServerDLLName() + " is in the launcher's folder.";
            return false;
        }
        if (string.IsNullOrEmpty(DeviceIPTextBox.Text))
        {
            GameStatusLabel.Text = "Must enter a Device IP.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(LevelTextBox.Text))
        {
            GameStatusLabel.Text = "Level not set.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(InclusionTextBox.Text))
        {
            GameStatusLabel.Text = "Level's Inclusion not set.";
            return false;
        }
        //Process[] processesByName3 = Process.GetProcessesByName(s_gameToExecutableName[m_selectedGame].Replace(".exe", string.Empty));
        //if (processesByName3.Length != 0)
        //{
        //    GameStatusLabel.Text = $"Game is already running. (PID {processesByName3[0].Id})";
        //    return false;
        //}
        return true;
    }

    private void SaveUserData()
    {
        string path = Path.Combine(GetAppdataDir(), s_launcherSavedataFilename);
        try
        {
            JObject jObject = new JObject();
            if (File.Exists(path))
            {
                jObject = JObject.Parse(File.ReadAllText(path));
            }
            JObject jObject2 = new JObject();
            jObject2["Username"] = UsernameTextbox.Text;
            jObject2["ServerIP"] = ServerIPTextBox.Text;
            jObject2["GameDirectory"] = GameDirectoryLabel.Text;
            jObject2["ServerPassword"] = ServerPasswordTextBox.Text;
            jObject2["AdditionalLaunchArgs"] = AdditionalLaunchArgumentsBox.Text;
            jObject2["DeviceIP"] = DeviceIPTextBox.Text;
            jObject2["Level"] = LevelTextBox.Text;
            jObject2["Inclusion"] = InclusionTextBox.Text;
            jObject2["DedicatedServerPassword"] = DedicatedServerPasswordTextBox.Text;
            jObject2["PlayerCount"] = PlayerCountTextBox.Text;
            jObject2["AdditionalServerLaunchArguments"] = AdditionalServerLaunchArgumentsTextBox.Text;
            string text = GameSelectorComboBox.SelectedItem.ToString();
            jObject["SelectedGame"] = text;
            jObject[text] = jObject2;
            string contents = jObject.ToString();
            File.WriteAllText(path, contents);
        }
        catch (Exception)
        {
        }
    }

    private void GetLastSelectedGame(out PVZGame selectedGame)
    {
        string path = Path.Combine(GetAppdataDir(), s_launcherSavedataFilename);
        if (File.Exists(path))
        {
            try
            {
                JObject jObject = new JObject();
                jObject = JObject.Parse(File.ReadAllText(path));
                PVZGame result = PVZGame.GW2;
                if (jObject.ContainsKey("SelectedGame") && Enum.TryParse<PVZGame>((string?)jObject["SelectedGame"], out result))
                {
                    selectedGame = result;
                    return;
                }
            }
            catch (Exception)
            {
            }
        }
        selectedGame = PVZGame.GW2;
    }

    private void LoadUserData(string? profileName = null)
    {
        string path = Path.Combine(GetAppdataDir(), s_launcherSavedataFilename);
        if (!File.Exists(path))
        {
            return;
        }
        try
        {
            JObject jObject = new JObject();
            jObject = JObject.Parse(File.ReadAllText(path));
            PVZGame result;
            if (profileName != null && jObject.ContainsKey(profileName))
            {
                JObject jObject2 = (JObject)jObject[profileName];
                if (jObject2.ContainsKey("Username"))
                {
                    UsernameTextbox.Text = (string?)jObject2["Username"];
                }
                if (jObject2.ContainsKey("ServerIP"))
                {
                    ServerIPTextBox.Text = (string?)jObject2["ServerIP"];
                }
                if (jObject2.ContainsKey("GameDirectory"))
                {
                    GameDirectoryLabel.Text = (string?)jObject2["GameDirectory"];
                }
                if (jObject2.ContainsKey("ServerPassword"))
                {
                    ServerPasswordTextBox.Text = (string?)jObject2["ServerPassword"];
                }
                if (jObject2.ContainsKey("AdditionalLaunchArgs"))
                {
                    AdditionalLaunchArgumentsBox.Text = (string?)jObject2["AdditionalLaunchArgs"];
                }
                if (jObject2.ContainsKey("DeviceIP"))
                {
                    DeviceIPTextBox.Text = (string?)jObject2["DeviceIP"];
                }
                if (jObject2.ContainsKey("Level"))
                {
                    LevelTextBox.Text = (string?)jObject2["Level"];
                }
                if (jObject2.ContainsKey("Inclusion"))
                {
                    InclusionTextBox.Text = (string?)jObject2["Inclusion"];
                }
                if (jObject2.ContainsKey("DedicatedServerPassword"))
                {
                    DedicatedServerPasswordTextBox.Text = (string?)jObject2["DedicatedServerPassword"];
                }
                if (jObject2.ContainsKey("PlayerCount"))
                {
                    PlayerCountTextBox.Text = (string?)jObject2["PlayerCount"];
                }
                if (jObject2.ContainsKey("AdditionalServerLaunchArguments"))
                {
                    AdditionalServerLaunchArgumentsTextBox.Text = (string?)jObject2["AdditionalServerLaunchArguments"];
                }
            }
            else if (profileName == null && jObject.ContainsKey("SelectedGame") && Enum.TryParse<PVZGame>((string?)jObject["SelectedGame"], out result))
            {
                GameSelectorComboBox.SelectedIndex = (int)result;
                string propertyName = result.ToString();
                if (jObject.ContainsKey(propertyName))
                {
                    JObject jObject3 = (JObject)jObject[propertyName];
                    if (jObject3.ContainsKey("Username"))
                    {
                        UsernameTextbox.Text = (string?)jObject3["Username"];
                    }
                    if (jObject3.ContainsKey("ServerIP"))
                    {
                        ServerIPTextBox.Text = (string?)jObject3["ServerIP"];
                    }
                    if (jObject3.ContainsKey("GameDirectory"))
                    {
                        GameDirectoryLabel.Text = (string?)jObject3["GameDirectory"];
                    }
                    if (jObject3.ContainsKey("ServerPassword"))
                    {
                        ServerPasswordTextBox.Text = (string?)jObject3["ServerPassword"];
                    }
                    if (jObject3.ContainsKey("AdditionalLaunchArgs"))
                    {
                        AdditionalLaunchArgumentsBox.Text = (string?)jObject3["AdditionalLaunchArgs"];
                    }
                    if (jObject3.ContainsKey("DeviceIP"))
                    {
                        DeviceIPTextBox.Text = (string?)jObject3["DeviceIP"];
                    }
                    if (jObject3.ContainsKey("Level"))
                    {
                        LevelTextBox.Text = (string?)jObject3["Level"];
                    }
                    if (jObject3.ContainsKey("Inclusion"))
                    {
                        InclusionTextBox.Text = (string?)jObject3["Inclusion"];
                    }
                    if (jObject3.ContainsKey("DedicatedServerPassword"))
                    {
                        DedicatedServerPasswordTextBox.Text = (string?)jObject3["DedicatedServerPassword"];
                    }
                    if (jObject3.ContainsKey("PlayerCount"))
                    {
                        PlayerCountTextBox.Text = (string?)jObject3["PlayerCount"];
                    }
                    if (jObject3.ContainsKey("AdditionalServerLaunchArguments"))
                    {
                        AdditionalServerLaunchArgumentsTextBox.Text = (string?)jObject3["AdditionalServerLaunchArguments"];
                    }
                }
            }
            else
            {
                UsernameTextbox.Text = string.Empty;
                ServerIPTextBox.Text = string.Empty;
                ServerPasswordTextBox.Text = string.Empty;
                AdditionalLaunchArgumentsBox.Text = string.Empty;
                DeviceIPTextBox.Text = string.Empty;
                LevelTextBox.Text = string.Empty;
                InclusionTextBox.Text = string.Empty;
                DedicatedServerPasswordTextBox.Text = string.Empty;
                PlayerCountTextBox.Text = string.Empty;
                AdditionalServerLaunchArgumentsTextBox.Text = string.Empty;
                GameDirectoryLabel.Text = string.Empty;
            }
        }
        catch (Exception)
        {
            File.Delete(path);
        }
    }

    private void LaunchWindow_Load(object sender, EventArgs e)
    {
        GameSelectorComboBox.Items.Clear();
        GameSelectorComboBox.Items.Add(PVZGame.GW1);
        GameSelectorComboBox.Items.Add(PVZGame.GW2);
        GameSelectorComboBox.Items.Add(PVZGame.BFN);
        GetLastSelectedGame(out m_selectedGame);
        GameSelectorComboBox.SelectedIndex = (int)m_selectedGame;
    }

    private void JoinButton_Click(object sender, EventArgs e)
    {
        if (!VerifyLaunch())
        {
            return;
        }
        SaveUserData();
        string path = s_gameToExecutableName[m_selectedGame];
        bool failed = false;
        if (GameRequiresPatchedExe(Path.Combine(GetGameDir(), path), ref failed) && !failed)
        {
            path = s_gameToPatchedExecutableName[m_selectedGame];
            if (!File.Exists(Path.Combine(GetGameDir(), s_gameToPatchedExecutableName[m_selectedGame])))
            


            {
                GameStatusLabel.Text = "First time launch - Creating patched executable (this might take a while)";

                string courgetteApplyCommand = "-apply";
                if (m_selectedGame == PVZGame.BFN)
                {
                    courgetteApplyCommand = "-applybsdiff";
                }

                ProcessStartInfo startInfo = new ProcessStartInfo

                {
                    FileName = "courgette.exe",
                    Arguments = $"{courgetteApplyCommand} \"{Path.Combine(GetGameDir(), s_gameToExecutableName[m_selectedGame])}\" {m_selectedGame}.patch \"{Path.Combine(GetGameDir(), s_gameToPatchedExecutableName[m_selectedGame])}\"",
                    Verb = "runas",
                    UseShellExecute = true
                };
                Process process = new Process
                {
                    StartInfo = startInfo,
                    EnableRaisingEvents = true
                };
                try
                {
                    process.Start();
                    process.WaitForExit();
                    if (process.ExitCode != 0)
                    {
                        GameStatusLabel.Text = "Patcher failed (Code: " + process.ExitCode.ToString("X") + ")";
                        return;
                    }
                    GameStatusLabel.Text = "Success - Created patched executable " + s_gameToPatchedExecutableName[m_selectedGame];
                }
                catch (Exception ex)
                {
                    GameStatusLabel.Text = "Failed to start courgette";
                    MessageBox.Show("Exception while starting courgette: " + ex.Message);
                    return;
                }
            }
        }
        if (failed)
        {
            return;
        }
        string gameDir = GetGameDir();
        Environment.SetEnvironmentVariable("EARtPLaunchCode", GetRtPLaunchCode());
        Environment.SetEnvironmentVariable("ContentId", "1026482");
        bool flag = UseModsCheckbox.Checked && !string.IsNullOrEmpty(ModPackCombobox.Text);
        Environment.SetEnvironmentVariable("GAME_DATA_DIR", flag ? Path.Combine(gameDir, "ModData", ModPackCombobox.Text) : null);
        string text = "-playerName \"" + UsernameTextbox.Text + "\" -Client.ServerIp " + ServerIPTextBox.Text + " -allowMultipleInstances";
        if (!string.IsNullOrWhiteSpace(ServerPasswordTextBox.Text))
        {
            text = text + " -password " + ServerPasswordTextBox.Text;
        }
        if (s_specialLaunchArgsForGame.ContainsKey(m_selectedGame))
        {
            text = text + " " + s_specialLaunchArgsForGame[m_selectedGame];
        }
        if (!string.IsNullOrWhiteSpace(AdditionalLaunchArgumentsBox.Text))
        {
            text = text + " " + AdditionalLaunchArgumentsBox.Text;
        }
        Environment.SetEnvironmentVariable("GW_LAUNCH_ARGS", text);
        if (!File.Exists(Path.Combine(gameDir, s_destDLLName)))
        {

            try
            {
                File.Copy(GetServerDLLName(), Path.Combine(gameDir, s_destDLLName), overwrite: true);
            }
            catch (Exception ex2)
            {
                MessageBox.Show("Exception when attempting to copy " + GetServerDLLName() + ": " + ex2.Message);
                return;
            }

        }
        ProcessStartInfo startInfo2 = new ProcessStartInfo
        {
            FileName = Path.Combine(gameDir, path),
            Arguments = text,
            UseShellExecute = false
        };
        Process process2 = new Process
        {
            StartInfo = startInfo2,
            EnableRaisingEvents = true
        };
        process2.Exited += GameProcess_Exited;
        try
        {
            process2.Start();
            GameStatusLabel.Text = $"Game launched (PID {process2.Id})";
            GameStatusLabel.ForeColor = Color.LightGreen;
        }
        catch (Win32Exception ex3)
        {
            if (ex3.NativeErrorCode == 2)
            {
                GameStatusLabel.Text = "Game executable not found.";
            }
            else
            {
                MessageBox.Show("Exception: " + ex3.Message);
            }
        }
        catch (Exception ex4)
        {
            MessageBox.Show("Exception: " + ex4.Message);
        }
    }

    private void GameProcess_Exited(object sender, EventArgs e)
    {
        Invoke((System.Windows.Forms.MethodInvoker)delegate
        {
            GameStatusLabel.Text = "Game exited with code " + (sender as Process)?.ExitCode.ToString("X"); 
            GameStatusLabel.ForeColor = Color.White;
            try
            {
                File.Delete(Path.Combine(GetGameDir(), s_destDLLName));
            }
            catch (Exception)
            {
            }
            Environment.SetEnvironmentVariable("EARtPLaunchCode", null);
            Environment.SetEnvironmentVariable("ContentId", null);
            Environment.SetEnvironmentVariable("GW_LAUNCH_ARGS", null);
            if (m_selectedGame < PVZGame.BFN)
            {
                try
                {
                    File.Delete(Path.Combine(GetGameDir(), "CryptBase.dll"));
                }
                catch
                {
                }
            }
            JoinButton.Enabled = true;
            base.WindowState = FormWindowState.Normal;
        });
    }

    private void SelectGameDirectoryButton_Click(object sender, EventArgs e)
    {
        bool flag = false;
        while (!flag)
        {
            using FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Select " + s_gameToGameName[m_selectedGame] + "'s directory";
            folderBrowserDialog.ShowNewFolderButton = false;
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
            {
                if (File.Exists(Path.Combine(folderBrowserDialog.SelectedPath, s_gameToExecutableName[m_selectedGame])))
                {
                    GameDirectoryLabel.Text = folderBrowserDialog.SelectedPath;
                    flag = true;
                }
                else
                {
                    MessageBox.Show("Selected folder does not contain " + s_gameToExecutableName[m_selectedGame] + ". Please select the folder that contains " + s_gameToExecutableName[m_selectedGame]);
                }
                continue;
            }
            break;
        }
    }

    private void GameSelectorComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        ComboBox comboBox = (ComboBox)sender;
        m_selectedGame = (PVZGame)comboBox.SelectedItem;
        Image image = (Image)m_resourceManager.GetObject(m_selectedGame.ToString());
        if (image != null)
        {
            BackgroundImage = image;
        }
        LoadUserData(m_selectedGame.ToString());
    }

    private void AutoFindGameDirButton_Click(object sender, EventArgs e)
    {
        RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\PopCap") ?? Registry.LocalMachine.OpenSubKey("SOFTWARE\\PopCap");
        if (registryKey != null)
        {
            RegistryKey registryKey2 = null;
            switch (m_selectedGame)
            {
                case PVZGame.GW1:
                    registryKey2 = registryKey.OpenSubKey("Plants vs Zombies Garden Warfare");
                    break;
                case PVZGame.GW2:
                    registryKey2 = registryKey.OpenSubKey("Plants vs Zombies GW2");
                    break;
                case PVZGame.BFN:
                    registryKey2 = registryKey.OpenSubKey("PVZ Battle for Neighborville");
                    break;
            }
            if (registryKey2 != null && registryKey2.GetValue("Install Dir") is string text && Directory.Exists(text) && File.Exists(Path.Combine(text, s_gameToExecutableName[m_selectedGame])))
            {
                GameDirectoryLabel.Text = text;
                GameStatusLabel.Text = $"Found directory for {m_selectedGame}: {text}";
                return;
            }
        }
        GameStatusLabel.Text = "Could not automatically find directory";
    }

    private void ModPackCombobox_OnDropDown(object sender, EventArgs e)
    {
        ComboBox comboBox = sender as ComboBox;
        comboBox.Items.Clear();
        string gameDir = GetGameDir();
        string path = Path.Combine(gameDir, "ModData");
        if (Directory.Exists(gameDir) && Directory.Exists(path))
        {
            string[] directories = Directory.GetDirectories(path);
            foreach (string text in directories)
            {
                comboBox.Items.Add(text.Split('\\').Last());
            }
        }
    }

    private void PlaylistCombobox_OnDropDown(object sender, EventArgs e)
    {
        ComboBox comboBox = sender as ComboBox;
        comboBox.Items.Clear();
        string path = Path.Combine(GetGameDir(), "Playlists");
        if (Directory.Exists(path))
        {
            string[] directories = Directory.GetFiles(path);
            foreach (string text in directories)
            {
                comboBox.Items.Add(text.Split('\\').Last());
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        ServerIPTextBox = new TextBox();
        label1 = new Label();
        JoinButton = new Button();
        GameStatusLabel = new Label();
        label2 = new Label();
        UsernameTextbox = new TextBox();
        GameDirectoryLabel = new Label();
        SelectGameDirectoryButton = new Button();
        label3 = new Label();
        label4 = new Label();
        AdditionalLaunchArgumentsBox = new TextBox();
        GameSelectorComboBox = new ComboBox();
        label5 = new Label();
        AutoFindGameDirButton = new Button();
        UseModsCheckbox = new CheckBox();
        ModPackCombobox = new ComboBox();
        label6 = new Label();
        ServerPasswordTextBox = new TextBox();
        label8 = new Label();
        label9 = new Label();
        DedicatedServerPasswordLabel = new Label();
        DedicatedServerPasswordTextBox = new TextBox();
        InclusionLabel = new Label();
        InclusionTextBox = new TextBox();
        DeviceIPLabel = new Label();
        DeviceIPTextBox = new TextBox();
        StartServerButton = new Button();
        LevelLabel = new Label();
        LevelTextBox = new TextBox();
        ServerPasswordLabel = new Label();
        PlayerCountLabel = new Label();
        PlayerCountTextBox = new TextBox();
        AdditionalServerLaunchArgumentsLabel = new Label();
        AdditionalServerLaunchArgumentsTextBox = new TextBox();
        PlaylistLabel = new Label();
        PlaylistComboBox = new ComboBox();
        PlaylistCheckBox = new CheckBox();
        SuspendLayout();
        // 
        // ServerIPTextBox
        // 
        ServerIPTextBox.Anchor = AnchorStyles.Bottom;
        ServerIPTextBox.Location = new Point(150, 183);
        ServerIPTextBox.Name = "ServerIPTextBox";
        ServerIPTextBox.Size = new Size(215, 23);
        ServerIPTextBox.TabIndex = 6;
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.BackColor = SystemColors.Control;
        label1.Location = new Point(150, 165);
        label1.Name = "label1";
        label1.Size = new Size(52, 15);
        label1.TabIndex = 1;
        label1.Text = "Server IP";
        // 
        // JoinButton
        // 
        JoinButton.Location = new Point(150, 313);
        JoinButton.Name = "JoinButton";
        JoinButton.Size = new Size(215, 23);
        JoinButton.TabIndex = 9;
        JoinButton.Text = "Join Server!";
        JoinButton.UseVisualStyleBackColor = true;
        JoinButton.Click += JoinButton_Click;
        // 
        // GameStatusLabel
        // 
        GameStatusLabel.BackColor = Color.Transparent;
        GameStatusLabel.Font = new Font("Segoe UI", 16F);
        GameStatusLabel.ForeColor = Color.White;
        GameStatusLabel.Location = new Point(12, 339);
        GameStatusLabel.Name = "GameStatusLabel";
        GameStatusLabel.Size = new Size(776, 102);
        GameStatusLabel.TabIndex = 3;
        GameStatusLabel.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // label2
        // 
        label2.AutoSize = true;
        label2.BackColor = SystemColors.Control;
        label2.Location = new Point(150, 117);
        label2.Name = "label2";
        label2.Size = new Size(60, 15);
        label2.TabIndex = 5;
        label2.Text = "Username";
        // 
        // UsernameTextbox
        // 
        UsernameTextbox.Anchor = AnchorStyles.Bottom;
        UsernameTextbox.Location = new Point(150, 135);
        UsernameTextbox.MaxLength = 32;
        UsernameTextbox.Name = "UsernameTextbox";
        UsernameTextbox.Size = new Size(215, 23);
        UsernameTextbox.TabIndex = 5;
        // 
        // GameDirectoryLabel
        // 
        GameDirectoryLabel.AutoSize = true;
        GameDirectoryLabel.BackColor = Color.Transparent;
        GameDirectoryLabel.ForeColor = Color.Black;
        GameDirectoryLabel.Location = new Point(5, 2);
        GameDirectoryLabel.Name = "GameDirectoryLabel";
        GameDirectoryLabel.Size = new Size(127, 15);
        GameDirectoryLabel.TabIndex = 6;
        GameDirectoryLabel.Text = "No Game Directory Set";
        // 
        // SelectGameDirectoryButton
        // 
        SelectGameDirectoryButton.Location = new Point(5, 20);
        SelectGameDirectoryButton.Name = "SelectGameDirectoryButton";
        SelectGameDirectoryButton.Size = new Size(133, 23);
        SelectGameDirectoryButton.TabIndex = 0;
        SelectGameDirectoryButton.Text = "Select Game Directory";
        SelectGameDirectoryButton.UseVisualStyleBackColor = true;
        SelectGameDirectoryButton.Click += SelectGameDirectoryButton_Click;
        // 
        // label3
        // 
        label3.AutoSize = true;
        label3.BackColor = Color.Transparent;
        label3.Location = new Point(134, 75);
        label3.Name = "label3";
        label3.Size = new Size(521, 15);
        label3.TabIndex = 8;
        label3.Text = "Note: To prevent unnecessary issues, please leave this window open until you've closed the game.";
        // 
        // label4
        // 
        label4.AutoSize = true;
        label4.BackColor = SystemColors.Control;
        label4.Location = new Point(150, 266);
        label4.Name = "label4";
        label4.Size = new Size(166, 15);
        label4.TabIndex = 10;
        label4.Text = "Additional Launch Arguments";
        // 
        // AdditionalLaunchArgumentsBox
        // 
        AdditionalLaunchArgumentsBox.Anchor = AnchorStyles.Bottom;
        AdditionalLaunchArgumentsBox.Location = new Point(150, 284);
        AdditionalLaunchArgumentsBox.Name = "AdditionalLaunchArgumentsBox";
        AdditionalLaunchArgumentsBox.Size = new Size(215, 23);
        AdditionalLaunchArgumentsBox.TabIndex = 8;
        // 
        // GameSelectorComboBox
        // 
        GameSelectorComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        GameSelectorComboBox.FormattingEnabled = true;
        GameSelectorComboBox.Location = new Point(5, 117);
        GameSelectorComboBox.Name = "GameSelectorComboBox";
        GameSelectorComboBox.Size = new Size(121, 23);
        GameSelectorComboBox.TabIndex = 2;
        GameSelectorComboBox.SelectedIndexChanged += GameSelectorComboBox_SelectedIndexChanged;
        // 
        // label5
        // 
        label5.AutoSize = true;
        label5.BackColor = SystemColors.Control;
        label5.Location = new Point(5, 99);
        label5.Name = "label5";
        label5.Size = new Size(72, 15);
        label5.TabIndex = 12;
        label5.Text = "Select Game";
        // 
        // AutoFindGameDirButton
        // 
        AutoFindGameDirButton.Location = new Point(5, 49);
        AutoFindGameDirButton.Name = "AutoFindGameDirButton";
        AutoFindGameDirButton.Size = new Size(133, 23);
        AutoFindGameDirButton.TabIndex = 1;
        AutoFindGameDirButton.Text = "Scan for Directory";
        AutoFindGameDirButton.UseVisualStyleBackColor = true;
        AutoFindGameDirButton.Click += AutoFindGameDirButton_Click;
        // 
        // UseModsCheckbox
        // 
        UseModsCheckbox.AutoSize = true;
        UseModsCheckbox.BackColor = SystemColors.Control;
        UseModsCheckbox.CheckAlign = ContentAlignment.MiddleRight;
        UseModsCheckbox.Location = new Point(5, 153);
        UseModsCheckbox.Name = "UseModsCheckbox";
        UseModsCheckbox.Size = new Size(78, 19);
        UseModsCheckbox.TabIndex = 3;
        UseModsCheckbox.Text = "Use Mods";
        UseModsCheckbox.UseVisualStyleBackColor = false;
        // 
        // ModPackCombobox
        // 
        ModPackCombobox.DropDownStyle = ComboBoxStyle.DropDownList;
        ModPackCombobox.FormattingEnabled = true;
        ModPackCombobox.Location = new Point(5, 203);
        ModPackCombobox.Name = "ModPackCombobox";
        ModPackCombobox.Size = new Size(121, 23);
        ModPackCombobox.TabIndex = 4;
        ModPackCombobox.DropDown += ModPackCombobox_OnDropDown;
        // 
        // label6
        // 
        label6.AutoSize = true;
        label6.BackColor = SystemColors.Control;
        label6.Location = new Point(5, 185);
        label6.Name = "label6";
        label6.Size = new Size(91, 15);
        label6.TabIndex = 16;
        label6.Text = "Select ModPack";
        // 
        // ServerPasswordTextBox
        // 
        ServerPasswordTextBox.Anchor = AnchorStyles.Bottom;
        ServerPasswordTextBox.Location = new Point(151, 235);
        ServerPasswordTextBox.Name = "ServerPasswordTextBox";
        ServerPasswordTextBox.Size = new Size(215, 23);
        ServerPasswordTextBox.TabIndex = 7;
        // 
        // label8
        // 
        label8.AutoSize = true;
        label8.BackColor = SystemColors.Control;
        label8.Location = new Point(216, 99);
        label8.Name = "label8";
        label8.Size = new Size(74, 15);
        label8.TabIndex = 19;
        label8.Text = "Join a server!";
        // 
        // label9
        // 
        label9.AutoSize = true;
        label9.BackColor = SystemColors.Control;
        label9.Location = new Point(526, 99);
        label9.Name = "label9";
        label9.Size = new Size(78, 15);
        label9.TabIndex = 20;
        label9.Text = "Host a server!";
        // 
        // DedicatedServerPasswordLabel
        // 
        DedicatedServerPasswordLabel.AutoSize = true;
        DedicatedServerPasswordLabel.BackColor = SystemColors.Control;
        DedicatedServerPasswordLabel.Location = new Point(456, 266);
        DedicatedServerPasswordLabel.Name = "DedicatedServerPasswordLabel";
        DedicatedServerPasswordLabel.Size = new Size(111, 15);
        DedicatedServerPasswordLabel.TabIndex = 29;
        DedicatedServerPasswordLabel.Text = "Set Server Password";
        // 
        // DedicatedServerPasswordTextBox
        // 
        DedicatedServerPasswordTextBox.Anchor = AnchorStyles.Bottom;
        DedicatedServerPasswordTextBox.Location = new Point(456, 284);
        DedicatedServerPasswordTextBox.Name = "DedicatedServerPasswordTextBox";
        DedicatedServerPasswordTextBox.Size = new Size(215, 23);
        DedicatedServerPasswordTextBox.TabIndex = 13;
        // 
        // InclusionLabel
        // 
        InclusionLabel.AutoSize = true;
        InclusionLabel.BackColor = SystemColors.Control;
        InclusionLabel.Location = new Point(456, 217);
        InclusionLabel.Name = "InclusionLabel";
        InclusionLabel.Size = new Size(55, 15);
        InclusionLabel.TabIndex = 27;
        InclusionLabel.Text = "Inclusion";
        // 
        // InclusionTextBox
        // 
        InclusionTextBox.Anchor = AnchorStyles.Bottom;
        InclusionTextBox.Location = new Point(456, 235);
        InclusionTextBox.Name = "InclusionTextBox";
        InclusionTextBox.Size = new Size(215, 23);
        InclusionTextBox.TabIndex = 12;
        // 
        // DeviceIPLabel
        // 
        DeviceIPLabel.AutoSize = true;
        DeviceIPLabel.BackColor = SystemColors.Control;
        DeviceIPLabel.Location = new Point(456, 117);
        DeviceIPLabel.Name = "DeviceIPLabel";
        DeviceIPLabel.Size = new Size(55, 15);
        DeviceIPLabel.TabIndex = 25;
        DeviceIPLabel.Text = "Device IP";
        // 
        // DeviceIPTextBox
        // 
        DeviceIPTextBox.Anchor = AnchorStyles.Bottom;
        DeviceIPTextBox.Location = new Point(456, 135);
        DeviceIPTextBox.Name = "DeviceIPTextBox";
        DeviceIPTextBox.Size = new Size(215, 23);
        DeviceIPTextBox.TabIndex = 10;
        // 
        // StartServerButton
        // 
        StartServerButton.Location = new Point(456, 313);
        StartServerButton.Name = "StartServerButton";
        StartServerButton.Size = new Size(215, 23);
        StartServerButton.TabIndex = 14;
        StartServerButton.Text = "Start Server!";
        StartServerButton.UseVisualStyleBackColor = true;
        StartServerButton.Click += StartServerButton_Click;
        // 
        // LevelLabel
        // 
        LevelLabel.AutoSize = true;
        LevelLabel.BackColor = SystemColors.Control;
        LevelLabel.Location = new Point(456, 165);
        LevelLabel.Name = "LevelLabel";
        LevelLabel.Size = new Size(34, 15);
        LevelLabel.TabIndex = 22;
        LevelLabel.Text = "Level";
        // 
        // LevelTextBox
        // 
        LevelTextBox.Anchor = AnchorStyles.Bottom;
        LevelTextBox.Location = new Point(456, 183);
        LevelTextBox.Name = "LevelTextBox";
        LevelTextBox.Size = new Size(215, 23);
        LevelTextBox.TabIndex = 11;
        // 
        // ServerPasswordLabel
        // 
        ServerPasswordLabel.AutoSize = true;
        ServerPasswordLabel.BackColor = SystemColors.Control;
        ServerPasswordLabel.Location = new Point(150, 217);
        ServerPasswordLabel.Name = "ServerPasswordLabel";
        ServerPasswordLabel.Size = new Size(122, 15);
        ServerPasswordLabel.TabIndex = 30;
        ServerPasswordLabel.Text = "Enter Server Password";
        // 
        // PlayerCountLabel
        // 
        PlayerCountLabel.AutoSize = true;
        PlayerCountLabel.BackColor = SystemColors.Control;
        PlayerCountLabel.Location = new Point(692, 243);
        PlayerCountLabel.Name = "PlayerCountLabel";
        PlayerCountLabel.Size = new Size(75, 15);
        PlayerCountLabel.TabIndex = 32;
        PlayerCountLabel.Text = "Player Count";
        // 
        // PlayerCountTextBox
        // 
        PlayerCountTextBox.Anchor = AnchorStyles.Bottom;
        PlayerCountTextBox.Location = new Point(692, 261);
        PlayerCountTextBox.Name = "PlayerCountTextBox";
        PlayerCountTextBox.Size = new Size(48, 23);
        PlayerCountTextBox.TabIndex = 18;
        // 
        // AdditionalServerLaunchArgumentsLabel
        // 
        AdditionalServerLaunchArgumentsLabel.AutoSize = true;
        AdditionalServerLaunchArgumentsLabel.BackColor = SystemColors.Control;
        AdditionalServerLaunchArgumentsLabel.Location = new Point(456, 31);
        AdditionalServerLaunchArgumentsLabel.Name = "AdditionalServerLaunchArgumentsLabel";
        AdditionalServerLaunchArgumentsLabel.Size = new Size(201, 15);
        AdditionalServerLaunchArgumentsLabel.TabIndex = 20;
        AdditionalServerLaunchArgumentsLabel.Text = "Additional Server Launch Arguments";
        // 
        // AdditionalServerLaunchArgumentsTextBox
        // 
        AdditionalServerLaunchArgumentsTextBox.Anchor = AnchorStyles.Bottom;
        AdditionalServerLaunchArgumentsTextBox.Location = new Point(456, 49);
        AdditionalServerLaunchArgumentsTextBox.Name = "AdditionalServerLaunchArgumentsTextBox";
        AdditionalServerLaunchArgumentsTextBox.Size = new Size(272, 23);
        AdditionalServerLaunchArgumentsTextBox.TabIndex = 19;
        // 
        // PlaylistLabel
        // 
        PlaylistLabel.AutoSize = true;
        PlaylistLabel.BackColor = SystemColors.Control;
        PlaylistLabel.Location = new Point(692, 191);
        PlaylistLabel.Name = "PlaylistLabel";
        PlaylistLabel.Size = new Size(78, 15);
        PlaylistLabel.Text = "Select Playlist";
        // 
        // PlaylistCheckBox
        // 
        PlaylistCheckBox.AutoSize = true;
        PlaylistCheckBox.BackColor = SystemColors.Control;
        PlaylistCheckBox.CheckAlign = ContentAlignment.MiddleRight;
        PlaylistCheckBox.Location = new Point(692, 161);
        PlaylistCheckBox.Name = "PlaylistCheckBox";
        PlaylistCheckBox.Size = new Size(85, 19);
        PlaylistCheckBox.TabIndex = 16;
        PlaylistCheckBox.Text = "Use Playlist";
        PlaylistCheckBox.UseVisualStyleBackColor = false;
        // 
        // PlaylistComboBox
        // 
        PlaylistComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        PlaylistComboBox.FormattingEnabled = true;
        PlaylistComboBox.Location = new Point(692, 209);
        PlaylistComboBox.Name = "PlaylistComboBox";
        PlaylistComboBox.Size = new Size(154, 23);
        PlaylistComboBox.TabIndex = 17;
        PlaylistComboBox.DropDown += PlaylistCombobox_OnDropDown;
        // 
        // LaunchWindow
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackgroundImageLayout = ImageLayout.Stretch;
        ClientSize = new Size(867, 450);
        Controls.Add(PlaylistLabel);
        Controls.Add(PlaylistComboBox);
        Controls.Add(PlaylistCheckBox);
        Controls.Add(AdditionalServerLaunchArgumentsLabel);
        Controls.Add(AdditionalServerLaunchArgumentsTextBox);
        Controls.Add(PlayerCountLabel);
        Controls.Add(PlayerCountTextBox);
        Controls.Add(ServerPasswordLabel);
        Controls.Add(DedicatedServerPasswordLabel);
        Controls.Add(DedicatedServerPasswordTextBox);
        Controls.Add(InclusionLabel);
        Controls.Add(InclusionTextBox);
        Controls.Add(DeviceIPLabel);
        Controls.Add(DeviceIPTextBox);
        Controls.Add(StartServerButton);
        Controls.Add(LevelLabel);
        Controls.Add(LevelTextBox);
        Controls.Add(label9);
        Controls.Add(label8);
        Controls.Add(ServerPasswordTextBox);
        Controls.Add(label6);
        Controls.Add(ModPackCombobox);
        Controls.Add(UseModsCheckbox);
        Controls.Add(AutoFindGameDirButton);
        Controls.Add(label5);
        Controls.Add(GameSelectorComboBox);
        Controls.Add(label4);
        Controls.Add(AdditionalLaunchArgumentsBox);
        Controls.Add(label3);
        Controls.Add(SelectGameDirectoryButton);
        Controls.Add(GameDirectoryLabel);
        Controls.Add(label2);
        Controls.Add(UsernameTextbox);
        Controls.Add(GameStatusLabel);
        Controls.Add(JoinButton);
        Controls.Add(label1);
        Controls.Add(ServerIPTextBox);
        DoubleBuffered = true;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        Name = "LaunchWindow";
        Text = "Cypress Launcher";
        Load += LaunchWindow_Load;
        ResumeLayout(false);
        PerformLayout();
    }

    private void StartServerButton_Click(object sender, EventArgs e)
    {
        if (!DedicatedVerifyLaunch())
        {
            return;
        }
        SaveUserData();
        string path = s_gameToExecutableName[m_selectedGame];
        bool failed = false;
        if (GameRequiresPatchedExe(Path.Combine(GetGameDir(), path), ref failed) && !failed)
        {
            path = s_gameToPatchedExecutableName[m_selectedGame];
            if (!File.Exists(Path.Combine(GetGameDir(), s_gameToPatchedExecutableName[m_selectedGame])))
            {
                GameStatusLabel.Text = "First time launch - Creating patched executable (this might take a while)";

                string courgetteApplyCommand = "-apply";
                if (m_selectedGame == PVZGame.BFN)
                {
                    courgetteApplyCommand = "-applybsdiff";
                }

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "courgette.exe",
                    Arguments = $"{courgetteApplyCommand} \"{Path.Combine(GetGameDir(), s_gameToExecutableName[m_selectedGame])}\" {m_selectedGame}.patch \"{Path.Combine(GetGameDir(), s_gameToPatchedExecutableName[m_selectedGame])}\"",
                    Verb = "runas",
                    UseShellExecute = true
                };
                Process process = new Process
                {
                    StartInfo = startInfo,
                    EnableRaisingEvents = true
                };
                try
                {
                    process.Start();
                    process.WaitForExit();
                    if (process.ExitCode != 0)
                    {
                        GameStatusLabel.Text = "Patcher failed (Code: " + process.ExitCode.ToString("X") + ")";
                        return;
                    }
                    GameStatusLabel.Text = "Success - Created patched executable " + s_gameToPatchedExecutableName[m_selectedGame];
                }
                catch (Exception ex)
                {
                    GameStatusLabel.Text = "Failed to start courgette";
                    MessageBox.Show("Exception while starting courgette: " + ex.Message);
                    return;
                }
            }
        }
        if (failed)
        {
            return;
        }
        string gameDir = GetGameDir();
        Environment.SetEnvironmentVariable("EARtPLaunchCode", GetRtPLaunchCode());
        Environment.SetEnvironmentVariable("ContentId", "1026482");
        bool flag = UseModsCheckbox.Checked && !string.IsNullOrEmpty(ModPackCombobox.Text);
        Environment.SetEnvironmentVariable("GAME_DATA_DIR", flag ? Path.Combine(gameDir, "ModData", ModPackCombobox.Text) : null);
        bool playlistflag = PlaylistCheckBox.Checked && !string.IsNullOrEmpty(PlaylistCheckBox.Text);
        string text = "-server -level " + LevelTextBox.Text + " -listen " + DeviceIPTextBox.Text + " -inclusion " + InclusionTextBox.Text + " -allowMultipleInstances " + " -Network.ServerAddress " + DeviceIPTextBox.Text + " ";
        if (!string.IsNullOrWhiteSpace(DedicatedServerPasswordTextBox.Text))
        {
            text = text + " -password " + DedicatedServerPasswordTextBox.Text;
        }
        if (playlistflag)
        {
            text = text + " -usePlaylist -playlistFilename " + Path.Combine(GetGameDir(), "Playlists", PlaylistComboBox.Text);
        }
        if (s_serverLaunchArgsForGame.ContainsKey(m_selectedGame))
        {
            text = text + " " + s_serverLaunchArgsForGame[m_selectedGame];
        }
        if (!string.IsNullOrWhiteSpace(AdditionalServerLaunchArgumentsTextBox.Text))
        {
            text = text + " " + AdditionalServerLaunchArgumentsTextBox.Text;
        }
        if (!string.IsNullOrWhiteSpace(PlayerCountTextBox.Text))
        {
            text = text + " -Network.MaxClientCount " + PlayerCountTextBox.Text;
        }
        Environment.SetEnvironmentVariable("GW_LAUNCH_ARGS", text);
        if (!File.Exists(Path.Combine(gameDir, s_destDLLName)))
        {

            try
            {
                File.Copy(GetServerDLLName(), Path.Combine(gameDir, s_destDLLName), overwrite: true);
            }
            catch (Exception ex2)
            {
                MessageBox.Show("Exception when attempting to copy " + GetServerDLLName() + ": " + ex2.Message);
                return;
            }

        }
        ProcessStartInfo startInfo2 = new ProcessStartInfo
        {
            FileName = Path.Combine(gameDir, path),
            Arguments = text,
            UseShellExecute = false
        };
        Process process2 = new Process
        {
            StartInfo = startInfo2,
            EnableRaisingEvents = true
        };
        process2.Exited += GameProcess_Exited;
        try
        {
            process2.Start();
            GameStatusLabel.Text = $"Game launched (PID {process2.Id})";
            GameStatusLabel.ForeColor = Color.LightGreen;
        }
        catch (Win32Exception ex3)
        {
            if (ex3.NativeErrorCode == 2)
            {
                GameStatusLabel.Text = "Game executable not found.";
            }
            else
            {
                MessageBox.Show("Exception: " + ex3.Message);
            }
        }
        catch (Exception ex4)
        {
            MessageBox.Show("Exception: " + ex4.Message);
        }
    }

}
