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


namespace Client.Logic.Moves
{
    class MoveHelper
    {
        #region Fields

        private static int dataLoadPercent = 0;
        private static MoveCollection mMoves;

        #endregion Fields

        #region Properties

        public static int DataLoadPercent
        {
            get { return dataLoadPercent; }
        }

        public static MoveCollection Moves
        {
            get { return mMoves; }
        }

        #endregion Properties

        #region Methods

        public static void InitMoveCollection()
        {
            mMoves = new MoveCollection(MaxInfo.MaxMoves);
        }

        public static void LoadMovesFromPacket(string[] parse)
        {
            try
            {
                int n = 1;
                for (int i = 1; i <= MaxInfo.MaxMoves; i++)
                {
                    dataLoadPercent = System.Math.Min(99, Logic.MathFunctions.CalculatePercent(i, MaxInfo.MaxMoves));
                    mMoves[i] = new Move();
                    mMoves[i].Name = parse[n + 1];
                    mMoves[i].RangeType = (Enums.MoveRange)parse[n + 2].ToInt();
                    mMoves[i].Range = parse[n + 3].ToInt();
                    mMoves[i].TargetType = (Enums.MoveTarget)parse[n + 4].ToInt();
                    mMoves[i].HitTime = parse[n + 5].ToInt();
                    mMoves[i].HitFreeze = parse[n + 6].ToBool();

                    n += 3;

                    n += 7;
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
