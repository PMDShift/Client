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


using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using Client.Logic.Graphics;
using PMDCP.Core;

using SdlDotNet.Widgets;

namespace Client.Logic.Menus
{
    class mnuOnlineList : Widgets.BorderedPanel, Core.IMenu
    {
        public bool Modal
        {
            get;
            set;
        }

        #region Fields

        Label lblOnlineList;
        Label lblLoading;
        Label lblTotal;
        ListBox lstOnlinePlayers;

        #endregion Fields

        #region Constructors

        public mnuOnlineList(string name)
            : base(name)
        {
            this.Size = new Size(185, 220);
            this.MenuDirection = Enums.MenuDirection.Vertical;
            this.Location = new Point(10, 40);

            lblOnlineList = new Label("lblOnlineList");
            lblOnlineList.Location = new Point(20, 0);
            lblOnlineList.Font = FontManager.LoadFont("PMDCP", 36);
            lblOnlineList.AutoSize = true;
            lblOnlineList.Text = "Online List";
            lblOnlineList.ForeColor = Color.WhiteSmoke;

            lblLoading = new Label("lblLoading");
            lblLoading.Location = new Point(10, 50);
            lblLoading.Font = FontManager.LoadFont("PMDCP", 16);
            lblLoading.AutoSize = true;
            lblLoading.Text = "Loading...";
            lblLoading.ForeColor = Color.WhiteSmoke;

            lblTotal = new Label("lblTotal");
            lblTotal.Location = new Point(10, 34);
            lblTotal.Font = FontManager.LoadFont("PMDCP", 16);
            lblTotal.AutoSize = true;
            lblTotal.ForeColor = Color.WhiteSmoke;

            lstOnlinePlayers = new ListBox("lstOnlinePlayers");
            lstOnlinePlayers.Location = new Point(10, 50);
            lstOnlinePlayers.Size = new Size(this.Width - lstOnlinePlayers.X * 2, this.Height - lstOnlinePlayers.Y - 10);
            lstOnlinePlayers.BackColor = Color.Transparent;
            lstOnlinePlayers.BorderStyle = SdlDotNet.Widgets.BorderStyle.None;

            this.AddWidget(lblOnlineList);
            this.AddWidget(lblLoading);
            this.AddWidget(lblTotal);
            this.AddWidget(lstOnlinePlayers);
        }

        #endregion Constructors
        #region Methods

        public override void OnKeyboardDown(SdlDotNet.Input.KeyboardEventArgs e)
        {
            base.OnKeyboardDown(e);
            switch (e.Key)
            {
                case SdlDotNet.Input.Key.Backspace:
                    {
                        // Show the others menu when the backspace key is pressed
                        MenuSwitcher.ShowOthersMenu();
                        Music.Music.AudioPlayer.PlaySoundEffect("beep3.wav");
                    }
                    break;
            }
        }

        public Widgets.BorderedPanel MenuPanel
        {
            get { return this; }
        }

        public void AddOnlinePlayers(string[] parse)
        {
            lblLoading.Visible = false;
            int count = parse[1].ToInt();

            int n = 2;

            for (int i = 0; i < count; i++)
            {
                // Try not to be too impressed... ;)
                ListBoxTextItem item = new ListBoxTextItem(FontManager.LoadFont("PMDCP", 16), parse[n++]);
                item.ForeColor = Color.FromArgb(parse[n++].ToInt());
                n++;
                lstOnlinePlayers.Items.Add(item);
            }

            lblTotal.Text = count + " Players Online";
        }

        #endregion Methods
    }
}
