using System;
using System.Collections.Generic;
using System.Text;

using PMDCP.Core;
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


namespace Client.Logic.Items
{
    class ItemHelper
    {
        #region Fields

        private static int dataLoadPercent = 0;
        private static ItemCollection mItems;

        #endregion Fields

        #region Properties

        public static int DataLoadPercent
        {
            get { return dataLoadPercent; }
        }

        public static ItemCollection Items
        {
            get { return mItems; }
        }

        #endregion Properties

        #region Methods

        public static void InitItemCollection()
        {
            mItems = new ItemCollection(MaxInfo.MaxItems);
        }

        public static void LoadItemsFromPacket(string[] parse)
        {
            try
            {
                int n, temp;
                n = 1;
                for (int i = 0; i < MaxInfo.MaxItems; i++)
                {
                    temp = Logic.MathFunctions.CalculatePercent(i, MaxInfo.MaxItems);
                    if (temp >= 100)
                    {
                        temp = 99;
                    }
                    dataLoadPercent = temp;
                    Item item = new Item();
                    if (parse[n + 1] != "0")
                    {
                        // Update the item
                        item.Name = parse[n + 1];
                        item.Desc = parse[n + 2];
                        item.Pic = parse[n + 3].ToInt();
                        item.Type = (Enums.ItemType)parse[n + 4].ToInt();
                        item.Data1 = parse[n + 5].ToInt();
                        item.Data2 = parse[n + 6].ToInt();
                        item.Data3 = parse[n + 7].ToInt();
                        item.Price = parse[n + 8].ToInt();
                        item.StackCap = parse[n + 9].ToInt();
                        item.Bound = parse[n + 10].ToBool();
                        item.Loseable = parse[n + 11].ToBool();
                        item.Rarity = parse[n + 12].ToInt();
                        item.AttackReq = parse[n + 13].ToInt();
                        item.DefenseReq = parse[n + 14].ToInt();
                        item.SpAtkReq = parse[n + 15].ToInt();
                        item.SpDefReq = parse[n + 16].ToInt();
                        item.SpeedReq = parse[n + 17].ToInt();
                        item.ScriptedReq = parse[n + 18].ToInt();
                        item.AddHP = parse[n + 19].ToInt();
                        item.AddPP = parse[n + 20].ToInt();
                        item.AddAttack = parse[n + 21].ToInt();
                        item.AddDefense = parse[n + 22].ToInt();
                        item.AddSpAtk = parse[n + 23].ToInt();
                        item.AddSpDef = parse[n + 24].ToInt();
                        item.AddSpeed = parse[n + 25].ToInt();
                        item.AddEXP = parse[n + 26].ToInt();
                        item.AttackSpeed = parse[n + 27].ToInt();
                        item.RecruitBonus = parse[n + 28].ToInt();
                        //29
                        n += 30;
                    }
                    else
                    {
                        n += 2;
                    }

                    mItems.AddItem(i, item);
                    ((Windows.winLoading)Windows.WindowSwitcher.FindWindow("winLoading")).UpdateLoadText("Recieving Data... " + DataManager.AverageLoadPercent().ToString() + "%");
                }
                dataLoadPercent = 100;
            }
            catch (Exception ex)
            {
                Exceptions.ExceptionHandler.OnException(ex);
            }
        }

        #endregion Methods
    }
}
