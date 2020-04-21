// This file is part of Mystery Dungeon eXtended.

// Copyright (C) 2015 Pikablu, MDX Contributors, PMU Staff

// Mystery Dungeon eXtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// Mystery Dungeon eXtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with Mystery Dungeon eXtended.  If not, see <http://www.gnu.org/licenses/>.


namespace Client.Logic
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Text;

    using Client.Logic.Windows;
    using Client.Logic.Graphics;
    using Client.Logic.Network;
    using System.IO;
    using System.Threading;
    using System.Text.RegularExpressions;
    using PMDCP.Core;
    using Microsoft.Win32;
    using SharpRaven;
    using Client.Logic.Updater;
    using System.Threading.Tasks;

    /// <summary>
    /// Loader that will load the game data.
    /// </summary>
    public class Loader
    {
        #region Methods

        static RavenClient ravenClient;

        /// <summary>
        /// Checks the folders to see if they exist.
        /// </summary>
        public static void CheckFolders()
        {
            IO.IO.CheckFolders();
        }

        /// <summary>
        /// Initializes the loader.
        /// </summary>
        [STAThread]
        public static void InitLoader(string[] args)
        {
            Globals.CommandLine = PMDCP.Core.CommandProcessor.ParseCommand(System.Environment.CommandLine);
            Globals.GameLoaded = false;

            if (!string.IsNullOrEmpty(CompileConstants.SentryToken))
            {
                ravenClient = new RavenClient(CompileConstants.SentryToken);
            }

            Logic.Globals.GameScreen = new Client.Logic.Windows.Core.GameScreen();
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

#if DEBUG
            if (Globals.CommandLine.ContainsCommandArg("-debug"))
            {
                Logic.Globals.InDebugMode = true;
            }
#endif

            IO.IO.Init();
            if (IsRunningUnderMaintenanceMode(Globals.CommandLine))
            {
                RunMaintenanceMode(Globals.CommandLine);
            }
            else
            {
                RunGame();
            }
        }

        static void RunGame()
        {
#if !DEBUG
            // Naming a Mutex makes it available computer-wide. Use a name that's
            // unique to your company and application (e.g., include your URL).
            using (Mutex mutex = new Mutex(false, Constants.GameName + " (" + Constants.GameNameShort + ")")) {
                // Wait a few seconds if contended, in case another instance
                // of the program is still in the process of shutting down.

                if (!mutex.WaitOne(TimeSpan.FromSeconds(3), false)) {
                    Exceptions.ExceptionHandler.OnException(new Exception("Another instance of " + Constants.GameNameShort + " is already running!"));
                    return;
                }
                InitializeGame();
            }
#else
            InitializeGame();
#endif
        }

        private static void InitializeGame()
        {
            // Create filetype associations
            CheckFileAssociations();

            Sdl.SdlLoader.InitializeSdl();
        }

        static void CheckFileAssociations()
        {
            RegistryKey key = Registry.ClassesRoot.OpenSubKey(".pmdcpskn");
            if (key == null)
            {
                // The association doesn't exist, restart as admin in maintenance mode
                if (!Client.Logic.Security.Admin.IsAdmin())
                {
                    if (Client.Logic.Security.Admin.IsVistaOrHigher())
                    {
                        Process process = Client.Logic.Security.Admin.StartProcessElevated(PMDCP.Core.Environment.StartupPath, "-createfileassociations");
                        process.WaitForExit();
                    }
                }
                else
                {
                    Process process = Process.Start(PMDCP.Core.Environment.StartupPath, "-createfileassociations");
                    process.WaitForExit();
                }
            }
        }

        static void RunMaintenanceMode(Command command)
        {
            if (command.ContainsCommandArg("-installskin"))
            {
                // We are trying to install a new skin
                string skinPackagePath = command["-installskin"];
                if (!string.IsNullOrEmpty(skinPackagePath) && File.Exists(skinPackagePath))
                {
                    bool installed = Skins.SkinManager.InstallSkin(skinPackagePath);
                    if (installed)
                    {
                        System.Windows.Forms.MessageBox.Show("The skin has been installed!", "Installation completed!");
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("The selected file is not a valid skin package.", "Invalid Package");
                    }
                }
            }
            else if (command.ContainsCommandArg("-createfileassociations"))
            {
                // Create associations for the skin loader
                RegistryKey RegKey = Registry.ClassesRoot.CreateSubKey(".pmdcpskn");
                RegKey.SetValue("", "PMDCP.Skin.Loader");
                RegKey.Close();

                RegKey = Registry.ClassesRoot.CreateSubKey("PMDCP.Skin.Loader");
                RegKey.SetValue("", "PMDCP Skin Package");
                RegKey.Close();

                RegKey = Registry.ClassesRoot.CreateSubKey("PMDCP.Skin.Loader" + "\\DefaultIcon");
                RegKey.SetValue("", @"C:\Program Files\Pokemon Mystery Dungeon Community Project\Client\pmdcpicon.ico" + "," + "0");
                RegKey.Close();

                RegKey = Registry.ClassesRoot.CreateSubKey("PMDCP.Skin.Loader" + "\\" + "Shell" + "\\" + "Open");
                RegKey = RegKey.CreateSubKey("Command");
                RegKey.SetValue("", "\"" + PMDCP.Core.Environment.StartupPath + "\" -installskin \"%1\"");
                RegKey.Close();
            }
        }

        static bool IsRunningUnderMaintenanceMode(Command command)
        {
            if (command.ContainsCommandArg("-installskin"))
            {
                return true;
            }
            else if (command.ContainsCommandArg("-createfileassociations"))
            {
                return true;
            }

            return false;
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (ravenClient != null)
            {
                ravenClient.Capture(new SharpRaven.Data.SentryEvent((Exception)e.ExceptionObject));
            }

            Logic.Exceptions.ErrorBox.ShowDialog("Error", ((Exception)e.ExceptionObject).Message, ((Exception)e.ExceptionObject).ToString());
        }

        //static void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e) {
        //    //System.Windows.Forms.MessageBox.Show(e.Exception.ToString());
        //}

        /// <summary>
        /// Loads the game data.
        /// </summary>
        public static void LoadData()
        {
            // Load the main font
            Graphics.FontManager.InitFonts();

            IO.Options.Initialize();
            IO.Options.LoadOptions();
            IO.ControlLoader.LoadControls();

            // Load the initial skin
            Skins.SkinManager.ChangeActiveSkin(IO.Options.ActiveSkin);

            if (Globals.InDebugMode)
            {
                // Init the debug controls
                Globals.GameScreen.InitControls();
            }

            SdlDotNet.Widgets.Widgets.Initialize(
                SdlDotNet.Graphics.Video.Screen,
                SdlDotNet.Widgets.Widgets.ResourceDirectory,
                IO.Paths.FontPath + "tahoma.ttf",
                12
                );
            //SdlDotNet.Widgets.Settings.DefaultFont = ;
            //SdlDotNet.Widgets.WindowManager.Initialize(SdlDotNet.Graphics.Video.Screen);
            //SdlDotNet.Widgets.WindowManager.WindowSwitcherEnabled = false;
            GraphicsCache.LoadCache();
            Input.InputProcessor.Initialize();
            // Switch to the loading window
            SdlDotNet.Widgets.WindowManager.AddWindow(new winLoading());
            ((winLoading)WindowSwitcher.FindWindow("winLoading")).UpdateLoadText("Checking for updates...");
            DoUpdateCheck();
        }

        private static void DoUpdateCheck()
        {

#if !DEBUG && !DISABLEUPDATER
            Thread updateCheckThread = new Thread(new ThreadStart(async delegate ()
            {
                 try
                 {
                     var updater = new GitHubUpdater();

                     DeleteToDeleteFiles();

                     var result = await updater.CheckForUpdates();

                     if (result.Count > 0)
                     {
                         WindowSwitcher.FindWindow("winLoading").Visible = false;
                         SdlDotNet.Widgets.WindowManager.AddWindow(new Updater.winUpdater(updater, result, PostUpdateLoad));
                         Windows.WindowSwitcher.UpdaterWindow.AlwaysOnTop = true;
                     }
                     else
                     {
                         PostUpdateLoad();
                     }
                 }
                 catch (Exception ex)
                 {
                     PostUpdateLoad();
                 }
            }));
            updateCheckThread.Start();
#else
            PostUpdateLoad();
#endif
        }

        private static void PostUpdateLoad()
        {
            Music.Music.Initialize();
            Skins.SkinManager.PlaySkinMusic();
            winLoading winLoading = WindowSwitcher.FindWindow("winLoading") as winLoading;
            winLoading.Visible = true;
            winLoading.UpdateLoadText("Loading game...");
            CheckFolders();
            LoadGuis();
            LoadGraphics();

            if (string.IsNullOrEmpty(IO.Options.ConnectionIP))
            {
                winLoading.Close();
                WindowSwitcher.AddWindow(new winWelcome(PostWelcomeLoad));
            }
            else
            {
                PostWelcomeLoad();
            }
        }

        private static void PostWelcomeLoad()
        {
            winLoading winLoading = WindowSwitcher.FindWindow("winLoading") as winLoading;
            if (winLoading == null)
            {
                winLoading = new winLoading();
                WindowSwitcher.AddWindow(winLoading);
            }

            // TODO: Add encryption key here
            Logic.Globals.Encryption = new Client.Logic.Security.Encryption();
            winLoading.UpdateLoadText("Connecting to server...");
            // Load TCP and connect to the server
            NetworkManager.InitializeTcp();
            NetworkManager.InitializePacketSecurity();
            NetworkManager.Connect();
            winLoading.Close();
            Windows.WindowSwitcher.ShowMainMenu();
            Globals.GameLoaded = true;
        }

        private static void DeleteToDeleteFiles()
        {
            string[] files = Directory.GetFiles(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "*ToDelete", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < files.Length; i++)
            {
                try
                {
                    File.Delete(files[i]);
                }
                catch { }
            }
        }

        /// <summary>
        /// Loads the game graphics.
        /// </summary>
        public static void LoadGraphics()
        {
            Graphics.GraphicsManager.Initialize();

            if (IO.IO.FileExists("GFX\\Items\\Items.png"))
            {
                Graphics.GraphicsManager.Items = Graphics.SurfaceManager.LoadSurface(IO.IO.GetGfxPath("Items\\Items.png"));
                Graphics.GraphicsManager.Items.Transparent = true;
            }

            for (int i = 0; i < Graphics.GraphicsManager.MAX_TILES; i++)
            {
                if (IO.IO.FileExists(IO.Paths.GfxPath + "Tiles\\Tiles" + i.ToString() + ".tile"))
                {
                    Graphics.GraphicsManager.LoadTilesheet(i);
                }
            }
        }

        /// <summary>
        /// Loads the core GUI's.
        /// </summary>
        public static void LoadGuis()
        {
            //Graphics.GuiManager.LoadGui(Graphics.GuiManager.Menu.Menu1);
            //Graphics.GuiManager.LoadGui(Graphics.GuiManager.Menu.Menu2);
        }

        #endregion Methods
    }
}
