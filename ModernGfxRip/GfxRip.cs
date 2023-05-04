using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography.X509Certificates;
using System.Security.RightsManagement;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Windows;

namespace ModernGfxRip
{
    class GfxRip
    {
        public enum BitplaneModeType
        {
            // Amiga standard (AM): Each bitplane is in its own picture
            AMIGA_STANDARD = 0,

            // Atari ST Standard (ST) : Same picture holds all bitplanes - each other line
            //                          is a new bitplane - some tiles are in this format
            ATARI_ST_STANDARD,

            // Amiga Sprite (SP) : This is a special mode to rip the AMIGA format sprites in
            //                     16 colours (4 bitplanes). The sprites are always 16 pixels
            //                     wide(X Size) and can be any Y size.  You can spot them by
            //                     seeing two bitmaps with 2 pictures in each bitmap (if looking in AM mode).
            AMIGA_SPRITE,

            // CPC Double (C+) : CPC Standard Double Byte mode. One Character is one bitplane - the Bytes follow
            //                   each other for Plane 0,1. (Head Over Heels, Batman)
            CPC_DOUBLE_STANDARD,

            // CPC Single (C-): CPC Standard Single Byte mode. One Character holds BOTH bitplanes (4 pixels per Byte).
            //                  This is used in many games.Sometimes Bitplanes are mirrored. (Amaurote, Ultimate games, etc.)
            CPC_SINGLE_STANDARD,

            // Apple II Standard (AP): Each Pixel is a color based on the pixel next to it,
            //                         and various other bits
            APPLE_II_STANDARD,

            // Used to determine if at end of Enum List
            LAST_MODE
        }

        /// <summary>
        /// Palette Search:
        /// The file is searched for a possible copperlist.
        /// By default the Mode E is selected which basically extracts the ECS palette
        /// from the UAE SaveState snapshot.  If the palette was not found in this mode
        /// then you will have to use one of the other three modes.
        /// </summary>
        public enum PaletteSearchMode
        {
            // Simple - Doesn't find so many palettes, and it is fast, searches whole memory for possible
            // palette entries (2 possible entries, always in sequence).
            SIMPLE = 0,

            // Advanced - The copperlist is searched in a way that when 4 possible colour entries (not
            // needed to be in sequence) are found it is reported as a valid palette.
            ADVANCED,

            // ECS UAE SaveState - The DEFAULT mode: the palette is extracted from the UAE SaveState
            // snapshot (if found). ECS (up to 64 colors) only!
            // Note: Make sure you save the Snapshots (SaveStates) as UNCOMPRESSED in WinUAE!!!
            ECS_UAE_SAVESTATE,

            // AGA UAE SaveState - the palette is extracted from the UAE SaveState snapshot (if found).
            // AGA colors (up to 256) are extracted!
            // Note: Make sure you save the Snapshots (SaveStates) as UNCOMPRESSED in WinUAE!!!
            AGA_UAE_SAVESTATE,

            // Used to determine if at end of Enum List
            LAST_MODE
        }

        /* Apple II Colors
         * Black  = #040204
         * Orange = #fc8204
         * Blue   = #0402fc
         * Purple = #fc02fc
         * Green  = #04fe04
         * White  = #fcfefc
         */

        /*
         * int offset = 0;
int blXSize = 320/8;
int blYSize = 200;

int contin_save = 1;

int bits = 1;

int skip = 0;
int skipmode = 0; // 0 = skip bytes after picture , 1 = skip bytes after each bitplane
char skipmoder;

bool reverse = false;
int mode = 0;
int palsearchmode = 2;

char pmoder[4] = { 'S', 'X', 'E', 'A' };

BITMAP * bitmap;

RGB pal[256];

int numX;
int numY;

bool zoom = false;

char goto_num[255];

int ttt=0;
int palfound = 0;

int bplorder[8] = {0,1,2,3,4,5,6,7};
        */

        public class GfxRipConfig
        {
            // Constants used in code
            public const int ScreenWidth = 800;
            public const int ScreenHeight = 600;
            public const int Hole = 2;

            public BitplaneModeType Mode { get; set; } = BitplaneModeType.AMIGA_STANDARD;
            public long Offset { get; set; } = 0;
            public int BlXSize { get; set; } = 320 / 8;
            public int BlYSize { get; set; } = 200;
            public int Bits { get; set; } = 1;
            public int NumX { get; set; } = 1;
            public int NumY { get; set; } = 1;
            public PaletteSearchMode PalSearchMode { get; set; } = PaletteSearchMode.ECS_UAE_SAVESTATE;
            public long PaletteFound { get; set; } = 0;
            public long Skip { get; set; } = 0;
            // 0 = skip bytes after picture, 1 = skip bytes after each bitplane
            public int SkipMode { get; set; } = 0;
            public bool Reverse { get; set; } = false;
            public byte[] BplOrder { get; set; } = new byte[8] { 0, 1, 2, 3, 4, 5, 6, 7 };

            /// <summary>
            /// Copy Constructor
            /// </summary>
            /// <param name="config">Passed in Configuration to Copy</param>
            public GfxRipConfig(GfxRipConfig config)
            {
                Mode = config.Mode;
                Offset = config.Offset;
                BlXSize = config.BlXSize;
                BlYSize = config.BlYSize;
                Bits = config.Bits;
                NumX = config.NumX;
                NumY = config.NumY;
                PalSearchMode = config.PalSearchMode;
                PaletteFound = config.PaletteFound;
                Skip = config.Skip;
                SkipMode = config.SkipMode;
                Reverse = config.Reverse;
                BplOrder = config.BplOrder;
            }

            /// <summary>
            /// Default Constructor (Initialize all values to defaults)
            /// </summary>
            public GfxRipConfig()
            {
                Mode = BitplaneModeType.AMIGA_STANDARD;
                Offset = 0;
                BlXSize = 320 / 8;
                BlYSize = 200;
                Bits = 1;
                NumX = 1;
                NumY = 1;
                PalSearchMode = PaletteSearchMode.ECS_UAE_SAVESTATE;
                PaletteFound = 0;
                Skip = 0;
                SkipMode = 0;
                Reverse = false;
                BplOrder = new byte[8] { 0,1,2,3,4,5,6,7 };
            }

            public string DisplayValues()
            {
                // Calculate Number of Screen Zones there are
                NumX = ScreenWidth / (BlXSize * 8 + Hole);
                NumY = ScreenHeight / (BlYSize + Hole);
                if (NumX <= 0) NumX = 1;
                if (NumY <= 0) NumY = 1;

                string result = string.Format("Off:{0,7:D} Siz: X:{1,4:D} Y:{2,4:D} Bit:{3:D} Vis.X:{4,2:D} Y:{5,2:D} Pal{6}:{7,7:D} Mode:{8,2} Skip-{9}:{10,4:D} Order:{11} {12:D}{13:D}{14:D}{15:D}{16:D}{17:D}{18:D}{19:D}",
                    Offset, BlXSize * 8, BlYSize, Bits, NumX, NumY, ConvertPaletteModeEnum(PalSearchMode), PaletteFound, ConvertModeEnum(Mode), (SkipMode == 0) ? "P" : "B", Skip, (Reverse) ? "R" : "N",
                    BplOrder[7], BplOrder[6], BplOrder[5], BplOrder[4], BplOrder[3], BplOrder[2], BplOrder[1], BplOrder[0]);

                return result;
            }

            /// <summary>
            /// Convert Bitplane Mode Type Enum into a Two Letter String for Display
            /// </summary>
            /// <param name="mode">Enum Value to Convert</param>
            /// <returns>Two Letter String for Display Purposes</returns>
            private static string ConvertModeEnum(BitplaneModeType mode)
            {
                string result;

                switch (mode)
                {
                    case BitplaneModeType.AMIGA_STANDARD:
                    default:
                        result = "AM";
                        break;
                    case BitplaneModeType.ATARI_ST_STANDARD:
                        result = "ST";
                        break;
                    case BitplaneModeType.AMIGA_SPRITE:
                        result = "SP";
                        break;
                    case BitplaneModeType.CPC_DOUBLE_STANDARD:
                        result = "C+";
                        break;
                    case BitplaneModeType.CPC_SINGLE_STANDARD:
                        result = "C-";
                        break;
                    case BitplaneModeType.APPLE_II_STANDARD:
                        result = "AP";
                        break;
                }

                return result;
            }
        }

        private static string ConvertPaletteModeEnum(PaletteSearchMode searchMode)
        {
            string result;

            switch (searchMode)
            {
                case PaletteSearchMode.SIMPLE:
                    result = "S";
                    break;
                case PaletteSearchMode.ADVANCED:
                    result = "X";
                    break;
                case PaletteSearchMode.ECS_UAE_SAVESTATE:
                default:
                    result = "E";
                    break;
                case PaletteSearchMode.AGA_UAE_SAVESTATE:
                    result = "A";
                    break;
            }

            return result;
        }

        // True if Configution has been saved
        public bool IsSaved { get; set; } = false;

        // True if settings have changed and screen needs to be refreshed
        private bool isDirty = false;

        // Store the Binary Data read from the file
        public byte[]? BinaryData { get; set; }

        // Store the size of the Binary Data that was read
        public long FileSize { get; set; } = 0;

        // Current Configuration Settings
        public GfxRipConfig Config { get; set; }

        public GfxRip()
        {
            Config = new GfxRipConfig();
        }

        public bool ReadBinaryData(string binaryFileName)
        {
            if (binaryFileName == null)
                return false;

            try
            {
                // File.ReadAllBytes opens a filestream and then ensures it is closed
                BinaryData = File.ReadAllBytes(binaryFileName);
            }
            catch (IOException e)
            {
                throw e;
            }

            // Initialize Variables based on new data loaded
            FileSize = BinaryData.Length;
            isDirty = true;

            return true;
        }

        /// <summary>
        /// Create a new GfxRipConfig configuration.  Resets all data back to defaults.
        /// </summary>
        public void NewConfiguration()
        {
            Config = new GfxRipConfig();

            // New Configuration so it is unsaved
            IsSaved = false;

            // Configuration changed, so will need to re-render the screen
            isDirty = true;
        }

        /// <summary>
        /// Load GfxRipConfig from a JSON File
        /// </summary>
        /// <param name="configFileName">Path to the JSON file to load</param>
        /// <returns>True if successful</returns>
        public bool LoadConfiguration(string configFileName)
        {
            bool result;
            try
            {
                string jsonString = File.ReadAllText(configFileName);
                GfxRipConfig loadConfig = JsonSerializer.Deserialize<GfxRipConfig>(jsonString)!;

                // Run the Copy Constructor to load all the settings
                Config = new GfxRipConfig(loadConfig);

                // Loaded Configuration so it is saved
                IsSaved = true;

                // Configuration changed, so will need to re-render the screen
                isDirty = true;

                result = true;
            }
            catch (IOException e)
            {
                throw e;
            }

            return result;
        }

        /// <summary>
        /// Save GfxRipConfig object as a serialized JSON File
        /// </summary>
        /// <param name="configFileName">Path to where to save the JSON file</param>
        /// <returns>True if successful</returns>
        public bool SaveConfiguration(string configFileName)
        {
            bool result;
            try
            {
                // Convert the Config File into JSON
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(Config, options);
                File.WriteAllText(configFileName, jsonString);

                // Configuration was saved so set flag accordingly
                IsSaved = true;

                result = true;
            }
            catch (IOException e)
            {
                throw e;
            }

            return result;
        }

        public void AdjustImageSize(string? command)
        {
            switch (command)
            {
                case "+X8":
                    Config.BlXSize++;
                    break;
                case "-X8":
                    if (Config.BlXSize > 1)
                    {
                        Config.BlXSize--;
                    }
                    break;
                case "+Y1":
                    Config.BlYSize++;
                    break;
                case "-Y1":
                    if (Config.BlYSize > 1)
                    {
                        Config.BlYSize--;
                    }
                    break;
                case "+Y8":
                    Config.BlYSize += 8;
                    break;
                case "-Y8":
                    if (Config.BlYSize > 9)
                    {
                        Config.BlYSize -= 8;
                    }
                    break;
                default:
                    // Unknown parameter passed so just abort
                    return;
            }

            // Parameters were changed so update bitmap
            isDirty = true;
        }

        public void AdjustPictureSize(string? command)
        {
            int modeskip = 1;
            if (Config.Mode == BitplaneModeType.ATARI_ST_STANDARD)
            {
                modeskip = Config.Bits;
            }

            switch (command)
            {
                case "-X8":
                    if (Config.Offset > 0)
                    {
                        Config.Offset--;
                    }
                    break;
                case "+X8":
                    if (Config.Offset < FileSize)
                    {
                        Config.Offset++;
                    }
                    break;
                case "-Pic":
                    if (Config.Offset >= (Config.BlXSize * Config.BlYSize * Config.Bits) + Config.Skip)
                    {
                        Config.Offset -= (Config.BlXSize * Config.BlYSize * Config.Bits) + Config.Skip;
                    }
                    break;
                case "+Pic":
                    if (Config.Offset < FileSize - ((Config.BlXSize * Config.BlYSize * Config.Bits) + Config.Skip))
                    {
                        Config.Offset += (Config.BlXSize * Config.BlYSize * Config.Bits) + Config.Skip;
                    }
                    break;
                case "-Y1":
                    if (Config.Offset >= Config.BlXSize * modeskip)
                    {
                        Config.Offset -= Config.BlXSize * modeskip;
                    }
                    break;
                case "+Y1":
                    if (Config.Offset < FileSize - (Config.BlXSize * modeskip))
                    {
                        Config.Offset += Config.BlXSize * modeskip;
                    }
                    break;
                case "-Y8":
                    if (Config.Offset >= (Config.BlXSize * 8 * modeskip))
                    {
                        Config.Offset -= Config.BlXSize * 8 * modeskip;
                    }
                    break;
                case "+Y8":
                    if (Config.Offset < (FileSize - (Config.BlXSize * 8 * modeskip)))
                    {
                        Config.Offset += Config.BlXSize * 8 * modeskip;
                    }
                    break;
                default:
                    // Unknown parameter passed so just abort
                    return;
            }

            // Parameters were changed so update bitmap
            isDirty = true;
        }

        public void Refresh()
        {
            // Check to see if need to update bitmap
            if (isDirty)
            {
                isDirty = false;
            }
        }

    }
}
