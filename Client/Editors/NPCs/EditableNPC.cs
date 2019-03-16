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
using System.Text;

namespace Client.Logic.Editors.NPCs
{
    class EditableNPC
    {
        #region Constructors

        public EditableNPC()
        {
            Moves = new int[4];
            Drops = new EditableNpcDrop[MaxInfo.MAX_NPC_DROPS];
        }

        #endregion Constructors

        #region Properties

        public string Name
        {
            get;
            set;
        }

        public string AttackSay
        {
            get;
            set;
        }

        public string AttackSay2 { get; set; }
        public string AttackSay3 { get; set; }

        public int Form
        {
            get;
            set;
        }

        public int Species
        {
            get;
            set;
        }

        public bool SpawnsAtDawn { get; set; }
        public bool SpawnsAtDay { get; set; }
        public bool SpawnsAtDusk { get; set; }
        public bool SpawnsAtNight { get; set; }

        public int ShinyChance { get; set; }
        public Enums.NpcBehavior Behavior { get; set; }
        public int RecruitRate { get; set; }

        public int[] Moves { get; set; }
        public EditableNpcDrop[] Drops { get; set; }

        public string AIScript { get; set; }

        public Enums.Direction SpawnDirection { get; set; }
        public Enums.Weather SpawnWeather { get; set; }

        public int Story { get; set; }
        public int Shop { get; set; }
        public int DeathStory { get; set; }
        public int Costume { get; set; }

        #endregion Properties
    }
}
