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


#region Header

/*
 * Created by SharpDevelop.
 * User: Pikachu
 * Time: 8:48 PM
 *
 */

#endregion Header

namespace Client.Logic
{
    using System;
    using System.Drawing;
    using Client.Logic.Network;
    using PMDCP.Sockets;
    using PMDCP.Core;

    /// <summary>
    /// Description of CommandProcessor.
    /// </summary>
    internal class CommandProcessor
    {
        #region Methods

        public static void ProcessCommand(string command, Enums.ChatChannel chatChannel)
        {
            if (string.IsNullOrEmpty(command))
            {
                return;
            }

            if (Players.PlayerManager.MyPlayer.TempMuteTimer > Globals.Tick)
            {
                return;
            }
            else if (Players.PlayerManager.MyPlayer.TalkTimer > Globals.Tick + 6000)
            {
                Players.PlayerManager.MyPlayer.TempMuteTimer = Globals.Tick + 8000;
                ExpKit.Modules.kitChat chat = (ExpKit.Modules.kitChat)Windows.WindowSwitcher.ExpKit.KitContainer.ModuleSwitcher.FindKitModule(Enums.ExpKitModules.Chat);
                chat.AppendChat("You stop to catch your breath.\n", new SdlDotNet.Widgets.CharRenderOptions(Color.Violet));
                return;
            }
            else if (Players.PlayerManager.MyPlayer.TalkTimer < Globals.Tick)
            {
                Players.PlayerManager.MyPlayer.TalkTimer = Globals.Tick + 3000;
            }
            else
            {
                Players.PlayerManager.MyPlayer.TalkTimer += 3000;
            }

            // Broadcast Message
            if ((command.StartsWith("'") || chatChannel == Enums.ChatChannel.Global) && (command.StartsWith("/") == false && command.StartsWith("!") == false && command.StartsWith("=") == false))
            {
                string message = command;
                if (command.StartsWith("'"))
                {
                    message = command.Substring(1);
                }
                else if (chatChannel == Enums.ChatChannel.Global)
                {
                    message = command;
                }
                Messenger.BroadcastMsg(message);
                return;
            }

            if (command.StartsWith("!"))
            {
                PMDCP.Core.Command com = PMDCP.Core.CommandProcessor.ParseCommand(command);
                if (com.CommandArgs.Count == 2)
                {
                    Messenger.PlayerMsg(com[0].Substring(1), com[1]);
                }
                return;
            }

            //if (command.StartsWith("/edithouse"))
            //{
            //    Messenger.SendPacket(TcpPacket.CreatePacket("requestedithouse"));
            //    return;
            //}

            if (command.StartsWith("/refresh"))
            {
                Messenger.SendRefresh();
                return;
            }

            //if (command.StartsWith("/ping")) {
            //    Messenger.SendPing();
            //    return;
            //}

            if (Ranks.IsAllowed(Players.PlayerManager.MyPlayer, Enums.Rank.Monitor))
            {
                // Global Message
                if (command.StartsWith("/announce"))
                {
                    Messenger.GlobalMsg(command.Substring(9));
                    return;
                }

                // Admin Message
                if ((command.StartsWith("=") || chatChannel == Enums.ChatChannel.Staff) && (command.StartsWith("/") == false))
                {
                    string message = command;
                    if (command.StartsWith("="))
                    {
                        message = command.Substring(1);
                    }
                    else if (chatChannel == Enums.ChatChannel.Staff)
                    {
                        message = command;
                    }
                    Messenger.AdminMsg(message);
                    return;
                }
            }

            if (Ranks.IsAllowed(Players.PlayerManager.MyPlayer, Enums.Rank.Scriptor))
            {
                // Map Editor
                if (command.StartsWith("/editmap"))
                {
                    Messenger.SendPacket(TcpPacket.CreatePacket("requesteditmap"));
                    return;
                }
            }
            if (Ranks.IsAllowed(Players.PlayerManager.MyPlayer, Enums.Rank.Monitor))
            {
                if (command.StartsWith("/loc"))
                {
                    Logic.Graphics.Renderers.Screen.ScreenRenderer.RenderOptions.DisplayLocation = !Logic.Graphics.Renderers.Screen.ScreenRenderer.RenderOptions.DisplayLocation;
                    return;
                }
            }

            if (command.StartsWith("/ping"))
            {
                IO.Options.Ping = !IO.Options.Ping;
                return;
            }

            if (command.StartsWith("/fps"))
            {
                IO.Options.FPS = !IO.Options.FPS;
                return;
            }



            if (Ranks.IsAllowed(Players.PlayerManager.MyPlayer, Enums.Rank.Scriptor))
            {
                if (command == "/editscript")
                {
                    bool syntaxFileExists = System.IO.File.Exists(IO.Paths.StartupPath + "Script/CSharp.syn");
                    Messenger.SendPacket(TcpPacket.CreatePacket("requesteditscript", syntaxFileExists.ToIntString()));
                }
            }

#if DEBUG
            // Just a small test of the music player
            if (Ranks.IsAllowed(Players.PlayerManager.MyPlayer, Enums.Rank.Scriptor))
            {
                if (command == "/test")
                {
                    // TODO: Re-enabletest command
                    //Music.Music.AudioPlayer.RunTest();
                }
            }
#endif

            // Test for scripted commands
            if (command.StartsWith("/"))
            {
                //for (int i = 0; i < MaxInfo.MaxEmoticons; i++)
                //{
                //    if (Emotions.EmotionHelper.Emotions[i].Command != "/" && Emotions.EmotionHelper.Emotions[i].Command == command)
                //    {
                //        Messenger.SendPacket(TcpPacket.CreatePacket("checkemoticons", i.ToString()));
                //    }
                //}
                Messenger.SendPacket(TcpPacket.CreatePacket("checkcommands", command));
                return;
            }

            if (chatChannel == Enums.ChatChannel.Local)
            {
                Messenger.SendMapMsg(command);
            }
            else if (chatChannel == Enums.ChatChannel.Guild)
            {
                Messenger.SendPacket(TcpPacket.CreatePacket("checkcommands", "/g " + command));
            }
        }

        #endregion Methods
    }
}
