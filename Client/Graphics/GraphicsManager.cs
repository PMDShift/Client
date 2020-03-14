using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using SdlDotNet.Graphics;
using Client.Logic.Windows;
using PMDCP.Core;
using System.IO;
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


namespace Client.Logic.Graphics
{
    /// <summary>
    /// Manages the game graphics.
    /// </summary>
    class GraphicsManager
    {
        #region Fields

        internal const int MAX_TILES = 12;
        internal const int PIC_X = 32;
        internal const int PIC_Y = 32;

        static MultiNameLRUCache<string, Mugshot> mugshotCache;
        static MultiNameLRUCache<string, SpriteSheet> spriteCache;
        static Cache<string, SpellSheet> spellCache;
        static TileCache tileCache;

        static readonly int spriteCacheSize = 200; // Cache 200 sprites
        static readonly int spellCacheSize = 1024 * 1024 * 5; // Total of 5 MB
        static readonly int mugshotCacheSize = 1024 * 100; // Total of 100kb cache
        static readonly int tileSetCacheSize = 500; /* This cache uses item amount as the limit. 
                                                     * Each tile is 4096 bytes * 100 = 819.2 KB
                                                     * There are 10 tilesets, 819.2 KB * 10 = 8192 KB = 8.192 MB
                                                     */

        #endregion Fields

        #region Properties

        internal static Surface FadeSurface
        {
            get;
            set;
        }

        internal static Surface Items
        {
            get;
            set;
        }

        internal static TileCache Tiles
        {
            get { return tileCache; }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Inits this instance.
        /// </summary>
        public static void Initialize()
        {
            FadeSurface = new SdlDotNet.Graphics.Surface(SdlDotNet.Graphics.Video.Screen.Size);

            spriteCache = new MultiNameLRUCache<string, SpriteSheet>(spriteCacheSize);
            spellCache = new Cache<string, SpellSheet>(spellCacheSize);
            mugshotCache = new MultiNameLRUCache<string, Mugshot>(mugshotCacheSize);
            tileCache = new TileCache(MAX_TILES);
        }

        /// <summary>
        /// Loads a tilesheet.
        /// </summary>
        /// <param name="index">The index.</param>
        public static void LoadTilesheet(int index)
        {
            Tileset tileSet = new Tileset(index, tileSetCacheSize);
            tileSet.Load(IO.Paths.GfxPath + "Tiles\\Tiles" + index.ToString() + ".tile");
            tileCache.AddTileset(tileSet);
        }

        public static int GetAnimDirInt(Enums.Direction dir)
        {
            switch (dir)
            {
                case Enums.Direction.Down:
                    return 0;
                case Enums.Direction.Left:
                    return 1;
                case Enums.Direction.Up:
                    return 2;
                case Enums.Direction.Right:
                    return 3;
                case Enums.Direction.DownLeft:
                    return 4;
                case Enums.Direction.UpLeft:
                    return 5;
                case Enums.Direction.UpRight:
                    return 6;
                case Enums.Direction.DownRight:
                    return 7;
                default:
                    return 0;
            }
        }

        public static Enums.Direction GetAnimIntDir(int dir)
        {
            switch (dir)
            {
                case 0:
                    return Enums.Direction.Down;
                case 1:
                    return Enums.Direction.Left;
                case 2:
                    return Enums.Direction.Up;
                case 3:
                    return Enums.Direction.Right;
                case 4:
                    return Enums.Direction.DownLeft;
                case 5:
                    return Enums.Direction.UpLeft;
                case 6:
                    return Enums.Direction.UpRight;
                case 7:
                    return Enums.Direction.DownRight;
                default:
                    return Enums.Direction.Down;
            }
        }

        public static SpriteSheet GetSpriteSheet(int num)
        {
            return GetSpriteSheet(num, -1, -1, -1);
        }

        public static SpriteSheet GetSpriteSheet(int num, int form, int shiny, int gender)
        {
            //System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            //watch.Start();
            //System.Diagnostics.Debug.WriteLine("PkMn #"+num+"("+form+"/"+shiny+"/"+gender+") requested");
            string formString = SpriteXLoader.GetSpriteFormString(form, shiny, gender);

            SpriteSheet sheet = null;
            lock (spriteCache)
            {
                sheet = spriteCache.Get(num + formString);
            }
            if (sheet != null)
            {
                //watch.Stop();
                //System.Diagnostics.Debug.WriteLine("PkMn #"+num+"("+form+"/"+shiny+"/"+gender+") retrieved in cache in "+watch.ElapsedMilliseconds);
                return sheet;
            }
            // If we are still here, that means the sprite wasn't in the cache
            if (System.IO.File.Exists(IO.Paths.GfxPath + "Sprites/Sprite" + num + ".zip"))
            {
                sheet = new SpriteSheet(num, formString);
                string changedFormString = formString;

                SpriteXLoader spriteXLoader = new SpriteXLoader(IO.Paths.GfxPath + "Sprites/Sprite" + num + ".zip", true);

                List<string> forms = spriteXLoader.LoadForms();

                while (true)
                {
                    if (spriteCache.ContainsKey(num + changedFormString))
                    {//this point will be hit if the first fallback data to be found is already in the cache
                        //the cache needs to be updated for aliases, but that's it.  No need to load any new data.

                        sheet = spriteCache.Get(num + changedFormString);
                        break;
                    }
                    else if (forms.Contains(changedFormString) || changedFormString == "r")
                    {//we've found a spritesheet in the file, so load it.
                        sheet.LoadSpriteX(spriteXLoader, changedFormString);

                        spriteCache.Add(num + changedFormString, sheet);

                        break;
                    }

                    // If the form specified wasn't found, continually revert to the backup until only "r" is reached
                    changedFormString = changedFormString.Substring(0, changedFormString.LastIndexOf('-'));
                }

                //continually add aliases
                string aliasString = formString;
                while (aliasString != changedFormString)
                {
                    //add aliases here
                    spriteCache.AddAlias(num + aliasString, num + changedFormString);
                    // If the form specified wasn't found, continually revert to the backup until only "r" is reached
                    aliasString = aliasString.Substring(0, aliasString.LastIndexOf('-'));
                }

                //string rootForm = spriteCache.GetOriginalKeyFromAlias(num + formString);
                //if (rootForm != num + formString)
                //{
                //    Logs.Logger.LogDebug("Could not load " + num + formString + ", loaded " + num + rootForm +" instead.");
                //}

                return sheet;
            }
            else
            {
                //watch.Stop();
                //System.Diagnostics.Debug.WriteLine("PkMn #"+num+"("+form+"/"+shiny+"/"+gender+") failed retrieval in "+watch.ElapsedMilliseconds);
                return null;
            }
        }

        public static SpellSheet GetSpellSheet(Enums.StationaryAnimType animType, int num, bool semiTransparent)
        {
            lock (spellCache)
            {
                if (spellCache.ContainsKey(animType.ToString() + "-" + num + "-" + semiTransparent.ToIntString()))
                {
                    return spellCache[animType.ToString() + "-" + num + "-" + semiTransparent.ToIntString()];
                }
                else
                {
                    if (System.IO.File.Exists(IO.Paths.GfxPath + "Spells/" + animType.ToString() + "-" + num + ".png"))
                    {
                        Surface surf;


                        if (semiTransparent)
                        {
                            surf = SurfaceManager.LoadSurface(IO.Paths.GfxPath + "Spells/" + animType.ToString() + "-" + num + ".png", false, true);
                            surf.Transparent = true;
                            surf.TransparentColor = Color.Black;
                            surf.AlphaBlending = true;
                            surf.Alpha = 128;
                        }
                        else
                        {
                            surf = SurfaceManager.LoadSurface(IO.Paths.GfxPath + "Spells/" + animType.ToString() + "-" + num + ".png");
                            surf.Transparent = true;
                        }
                        int bytesUsed = surf.Width * surf.Height * surf.BitsPerPixel / 8;
                        SpellSheet sheet = new SpellSheet(surf, bytesUsed);
                        spellCache.Add(animType.ToString() + "-" + num + "-" + semiTransparent.ToIntString(), sheet);
                        return sheet;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        public static SpellSheet GetEmoteSheet(int num)
        {
            lock (spellCache)
            {
                if (spellCache.ContainsKey("Status-" + num + "-0"))
                {
                    return spellCache["Status-" + num + "-0"];
                }
                else
                {
                    if (System.IO.File.Exists(IO.Paths.GfxPath + "Status/Status-" + num + ".png"))
                    {
                        Surface surf = SurfaceManager.LoadSurface(IO.Paths.GfxPath + "Status/Status-" + num + ".png");
                        surf.Transparent = true;

                        int bytesUsed = surf.Width * surf.Height * surf.BitsPerPixel / 8;
                        SpellSheet sheet = new SpellSheet(surf, bytesUsed);
                        spellCache.Add("Status-" + num + "-0", sheet);
                        return sheet;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        public static Mugshot GetMugshot(int num, int form, int shiny, int gender)
        {
            string formString = SpriteXLoader.GetSpriteFormString(form, shiny, gender);

            Mugshot sheet = null;
            lock (mugshotCache)
            {
                sheet = mugshotCache.Get(num + formString);
            }
            if (sheet != null)
            {
                return sheet;
            }
            // If we are still here, that means the sprite wasn't in the cache
            if (System.IO.File.Exists(IO.Paths.GfxPath + "Mugshots/Portrait" + num + ".zip"))
            {
                sheet = new Mugshot(num, formString);
                string changedFormString = formString;

                SpriteXLoader spriteXLoader = new SpriteXLoader(IO.Paths.GfxPath + "Mugshots/Portrait" + num + ".zip", false);
                List<string> forms = spriteXLoader.LoadForms();

                using (FileStream fileStream = File.OpenRead(IO.Paths.GfxPath + "Mugshots/Portrait" + num + ".zip"))
                {
                    while (true)
                    {
                        if (mugshotCache.ContainsKey(num + changedFormString))
                        {//this point will be hit if the first fallback data to be found is already in the cache
                            //the cache needs to be updated for aliases, but that's it.  No need to load any new data.

                            sheet = mugshotCache.Get(num + changedFormString);
                            break;
                        }
                        else if (forms.Contains(changedFormString) || changedFormString == "r")
                        {//we've found a spritesheet in the file, so load it.
                            spriteXLoader.LoadMugshot(sheet, changedFormString);

                            mugshotCache.Add(num + changedFormString, sheet);

                            break;
                        }

                        // If the form specified wasn't found, continually revert to the backup until only "r" is reached
                        changedFormString = changedFormString.Substring(0, changedFormString.LastIndexOf('-'));
                    }
                }

                //continually add aliases
                string aliasString = formString;
                while (aliasString != changedFormString)
                {
                    //add aliases here
                    mugshotCache.AddAlias(num + aliasString, num + changedFormString);
                    // If the form specified wasn't found, continually revert to the backup until only "r" is reached
                    aliasString = aliasString.Substring(0, aliasString.LastIndexOf('-'));
                }

                return sheet;
            }
            else
            {
                return null;
            }
        }

        #endregion Methods
    }
}
