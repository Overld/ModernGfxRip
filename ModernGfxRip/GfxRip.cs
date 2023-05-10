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
using System.Windows.Media;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Printing.IndexedProperties;
using System.Data.Common;
using System.Windows.Media.Media3D;
using System.Numerics;

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
            public bool Zoom { get; set; } = false;
            public sbyte[] BplOrder { get; set; } = new sbyte[8] { 0, 1, 2, 3, 4, 5, 6, 7 };
            public List<System.Windows.Media.Color> Colors { get; set; }


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
                Zoom = config.Zoom;
                BplOrder = config.BplOrder;
                // Perform a Deep Copy
                Colors = config.Colors.ConvertAll(color => color);
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
                Zoom = false;
                BplOrder = new sbyte[8] { 0,1,2,3,4,5,6,7 };
                Colors = GeneratePalette();
            }

            public List<System.Windows.Media.Color> GeneratePalette()
            {
                List<System.Windows.Media.Color> palette = new();
                Random rnd = new Random();
                byte[] randomBytes = new byte[3];

                for (int loop = 0; loop < 256; loop++)
                {
                    rnd.NextBytes(randomBytes);
                    palette.Add(System.Windows.Media.Color.FromRgb(randomBytes[0], randomBytes[1], randomBytes[2]));
                }

                palette[1] = System.Windows.Media.Color.FromRgb(0, 0, 0);
                palette[0] = System.Windows.Media.Color.FromRgb(255, 255, 255);
                palette[254] = System.Windows.Media.Color.FromRgb(0, 0, 0);
                palette[253] = System.Windows.Media.Color.FromRgb(203, 203, 215);

                return palette;
            }

            public string DisplayValues()
            {
                // Calculate Number of Screen Zones there are
                NumX = (ScreenWidth - 1) / (BlXSize * 8 + Hole);
                NumY = (ScreenHeight - 1) / (BlYSize + Hole);
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
        public bool isDirty = false;

        // Image that Writable Bitmap is copied into
        private Image? graphicBuffer;

        // Width of the Image
        private int graphicWidth;

        // Height of the Image
        private int graphicHeight;

        // Store the Binary Data read from the file
        public byte[]? BinaryData { get; set; }

        // Store the size of the Binary Data that was read
        public long FileSize { get; set; } = 0;

        // Temporary index used to search for Palettes
        public long TempPalSearch { get; set; } = 0;

        public BitmapPalette? BitmapPalette { get; set; }

        public byte[]? Pixels { get; set; }

        public WriteableBitmap? WBitMap { get; set; }

        // Current Configuration Settings
        public GfxRipConfig Config { get; set; }

        public GfxRip()
        {
            Config = new GfxRipConfig();
            BitmapPalette = new BitmapPalette(Config.Colors);
        }

        public void SetupDrawingBitmap(Image graphics, int width, int height)
        {
            graphicBuffer = graphics;
            graphicWidth = width;
            graphicHeight = height;

            SetupBitmapVariables();

            ClearToColor(253);

            if (WBitMap != null)
            {
                WBitMap.WritePixels(new Int32Rect(0, 0, WBitMap.PixelWidth, WBitMap.PixelHeight), Pixels,
                                  WBitMap.PixelWidth * WBitMap.Format.BitsPerPixel / 8, 0);
            }
        }

        private void SetupBitmapVariables()
        {
            WBitMap = new WriteableBitmap(graphicWidth, graphicHeight, 96, 96, PixelFormats.Indexed8, BitmapPalette);
            if (graphicBuffer != null)
            {
                graphicBuffer.Source = WBitMap;
            }

            Pixels = new byte[WBitMap.PixelHeight * WBitMap.PixelWidth * WBitMap.Format.BitsPerPixel / 8];
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

            // Redraw the screen
            Refresh();

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

            // Redraw the screen
            Refresh();
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

                // Redraw the screen
                Refresh();
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

        public void GetPaletteInfo(ref BMPFileInfo bmpDataInfo)
        {
            if (bmpDataInfo.infoHeader != null)
            {
                // Check to see if it is a BMP Palette
                if ((bmpDataInfo.infoHeader.bitsPerPixel > 1) && (bmpDataInfo.infoHeader.bitsPerPixel <= 8) && (bmpDataInfo.colorTable != null))
                {
                    Config.Colors = bmpDataInfo.colorTable.palette.ConvertAll(color => color);
                    BitmapPalette = new BitmapPalette(Config.Colors);

                    // Recreate Writeable Bitmap with new color Palette
                    SetupBitmapVariables();

                    isDirty = true;
                }
                else
                {
                    MessageBox.Show("Error! BMP File has no color palette. The bits per pixel are " + bmpDataInfo.infoHeader.bitsPerPixel + "!");
                }
            }
        }

        void Grab_UAE()
        {
            // Simpler method to find palettes!
            bool found = false;
            while (!found && (TempPalSearch < FileSize - 64) && (BinaryData != null))
            {
                if (BinaryData[TempPalSearch] == 0x01 && BinaryData[TempPalSearch + 1] == 0x80 && BinaryData[TempPalSearch + 4] == 0x01 && BinaryData[TempPalSearch + 5] == 0x82)
                {
                    found = true;
                    Config.PaletteFound = TempPalSearch;
                    short c1, c2;
                    byte red, green, blue;

                    // Generate the normal 32 colors
                    for (int i = 0; i < 32; i++)
                    {
                        c1 = BinaryData[TempPalSearch + 2 + i * 4];
                        c2 = BinaryData[TempPalSearch + 3 + i * 4];

                        // Convert 4 Bit Color range to 8 Bit Color.  i.e. 0 => 00, 1 => 11, 3 => 33, 9 => 99, B => BB, F => FF
                        red = (byte) (((c1 & 0x0F) << 4) & 0xFF);
                        green = (byte) (c2 & 0xF0);
                        blue  = (byte) (((c2 & 0x0F) << 4) & 0xFF);

                        red   |= (byte) ((red >> 4) & 0x0F);
                        green |= (byte) ((green >> 4) & 0x0F);
                        blue  |= (byte) ((blue >> 4) & 0x0F);

                        Config.Colors[i] = System.Windows.Media.Color.FromRgb(red, green, blue);
                    }

                    // Generate the Half Palette 32 colors
                    for (int i = 0; i < 32; i++)
                    {
                        c1 = BinaryData[TempPalSearch + 2 + i * 4];
                        c2 = BinaryData[TempPalSearch + 3 + i * 4];

                        // Convert 4 Bit Color range to 8 Bit Color.  i.e. 0 => 00, 1 => 11, 3 => 33, 9 => 99, B => BB, F => FF
                        // Convert colors to Half Colors by shifting right by 1
                        red = (byte)((((c1 & 0x0F) >> 1) << 4) & 0xF0);
                        green = (byte)((c2 >> 1) & 0xF0);
                        blue = (byte)((((c2 & 0x0F) >> 1) << 4) & 0xF0);

                        red |= (byte)((red >> 4) & 0x0F);
                        green |= (byte)((green >> 4) & 0x0F);
                        blue |= (byte)((blue >> 4) & 0x0F);

                        Config.Colors[32 + i] = System.Windows.Media.Color.FromRgb(red, green, blue);
                    }

                    BitmapPalette = new BitmapPalette(Config.Colors);

                    // Recreate Writeable Bitmap with new color Palette
                    SetupBitmapVariables();

                    isDirty = true;
                }
                TempPalSearch++;
            }

            if (!found)
            {
                TempPalSearch = 0;
            }
        }

        void Grab_UAE2()
        {
            // More advanced method to find palettes!
            bool[] colfound = new bool[32];
            bool nomorecols = false;
            int foundcolors = 0;

            for (int i = 0; i < 32; i++)
            {
                colfound[i] = false;
            }
            bool found = false;
            while (!found && (TempPalSearch < FileSize - 64) && (BinaryData != null))
            {
                if (BinaryData[TempPalSearch + 0] == 0x01 && BinaryData[TempPalSearch + 1] >= 0x80 && BinaryData[TempPalSearch + 1] <= 0x9f &&
                    BinaryData[TempPalSearch + 4] == 0x01 && BinaryData[TempPalSearch + 5] >= 0x80 && BinaryData[TempPalSearch + 5] <= 0x9f &&
                    BinaryData[TempPalSearch + 8] == 0x01 && BinaryData[TempPalSearch + 9] >= 0x80 && BinaryData[TempPalSearch + 9] <= 0x9f &&
                    BinaryData[TempPalSearch + 12] == 0x01 && BinaryData[TempPalSearch + 13] >= 0x80 && BinaryData[TempPalSearch + 13] <= 0x9f)
                {
                    found = true;
                    Config.PaletteFound = TempPalSearch;
                    short c1, c2;
                    byte red, green, blue;
                    int colind;

                    int i = 0;
                    bool ended = false;

                    while (TempPalSearch + (i * 4) < FileSize - 2 && !ended)
                    {
                        if (BinaryData[TempPalSearch + (i * 4) + 0] == 0xff && BinaryData[TempPalSearch + (i * 4) + 1] == 0xff)
                        {
                            ended = true;
                        }
                        else
                        {
                            if (BinaryData[TempPalSearch + 1 + (i * 4)] >= 0x80 && BinaryData[TempPalSearch + 1 + (i * 4)] <= 0xbf)
                            {
                                colind = (BinaryData[TempPalSearch + 1 + (i * 4)] - 0x80) >> 1;
                                if (!colfound[colind])
                                {
                                    colfound[colind] = true;
                                    c1 = BinaryData[TempPalSearch + 2 + (i * 4)];
                                    c2 = BinaryData[TempPalSearch + 3 + (i * 4)];

                                    // Convert 4 Bit Color range to 8 Bit Color.  i.e. 0 => 00, 1 => 11, 3 => 33, 9 => 99, B => BB, F => FF
                                    red = (byte)(((c1 & 0x0F) << 4) & 0xFF);
                                    green = (byte)(c2 & 0xF0);
                                    blue = (byte)(((c2 & 0x0F) << 4) & 0xFF);

                                    red |= (byte)((red >> 4) & 0x0F);
                                    green |= (byte)((green >> 4) & 0x0F);
                                    blue |= (byte)((blue >> 4) & 0x0F);

                                    Config.Colors[colind] = System.Windows.Media.Color.FromRgb(red, green, blue);
                                    if (!nomorecols)
                                    {
                                        foundcolors += 4;
                                    }
                                }
                                else
                                {
                                    nomorecols = true;
                                }
                            }
                            else
                            {
                                nomorecols = true;
                            }
                        }
                        i++;
                    }
                    BitmapPalette = new BitmapPalette(Config.Colors);

                    // Recreate Writeable Bitmap with new color Palette
                    SetupBitmapVariables();

                    isDirty = true;
                }
                if (foundcolors > 0)
                {
                    TempPalSearch += foundcolors;
                }
                else
                {
                    TempPalSearch++;
                }
            }
            if (!found)
            {
                TempPalSearch = 0;
            }
        }

        static int Search(byte[] src, byte[] pattern)
        {
            int maxFirstCharSlot = src.Length - pattern.Length + 1;
            for (int i = 0; i < maxFirstCharSlot; i++)
            {
                if (src[i] != pattern[0]) // compare only first byte
                    continue;

                // found a match on first byte, now try to match rest of the pattern
                for (int j = pattern.Length - 1; j >= 1; j--)
                {
                    if (src[i + j] != pattern[j]) break;
                    if (j == 1) return i;
                }
            }
            return -1;
        }

        int FindChunk(string chunkName)
        {
            if (BinaryData != null)
            {
                byte[] pattern = Encoding.ASCII.GetBytes(chunkName);

                return Search(BinaryData, pattern);
            }

            return -1;
        }

        void Grab_UAE3()
        {
            // Grab the palette from the snapshot
            int pos = FindChunk("CHIP");
            if ((pos != -1) && (BinaryData != null))
            {
                short c1, c2;
                byte red, green, blue;

                Config.PaletteFound = pos + 4 + 8 + 4 + 0x0180 - (64 + 12 * 8);
                for (int i = 0; i < 63; i++)
                {
                    c1 = BinaryData[Config.PaletteFound + (i * 2)];
                    c2 = BinaryData[Config.PaletteFound + (i * 2) + 1];

                    // Convert 4 Bit Color range to 8 Bit Color.  i.e. 0 => 00, 1 => 11, 3 => 33, 9 => 99, B => BB, F => FF
                    red = (byte)(((c1 & 0x0F) << 4) & 0xFF);
                    green = (byte)(c2 & 0xF0);
                    blue = (byte)(((c2 & 0x0F) << 4) & 0xFF);

                    red |= (byte)((red >> 4) & 0x0F);
                    green |= (byte)((green >> 4) & 0x0F);
                    blue |= (byte)((blue >> 4) & 0x0F);

                    Config.Colors[i] = System.Windows.Media.Color.FromRgb(red, green, blue);
                }
                BitmapPalette = new BitmapPalette(Config.Colors);

                // Recreate Writeable Bitmap with new color Palette
                SetupBitmapVariables();

                isDirty = true;
            }
        }

        void Grab_UAE3AGA()
        {
            // Grab the AGA palette from the snapshot
            int pos = FindChunk("AGAC");
            if ((pos != -1) && (BinaryData != null))
            {
                byte red, green, blue;

                Config.PaletteFound = pos + 8 + 4;
                for (int i = 0; i < 256; i++)
                {
                    red   = BinaryData[Config.PaletteFound + (i * 4) + 1];
                    green = BinaryData[Config.PaletteFound + (i * 4) + 2];
                    blue  = BinaryData[Config.PaletteFound + (i * 4) + 3];

                    Config.Colors[i] = System.Windows.Media.Color.FromRgb(red, green, blue);
                }
                BitmapPalette = new BitmapPalette(Config.Colors);

                // Recreate Writeable Bitmap with new color Palette
                SetupBitmapVariables();

                isDirty = true;
            }
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
                case "+Bit":
                    if (Config.Bits < 8)
                    {
                        Config.Bits++;
                    }
                    break;
                case "-Bit":
                    if (Config.Bits > 1)
                    {
                        Config.Bits--;
                    }
                    break;
                case "+Skip1":
                    Config.Skip++;
                    break;
                case "-Skip1":
                    if (Config.Skip > 0)
                    {
                        Config.Skip--;
                    }
                    break;
                case "+Skip8":
                    Config.Skip += 8;
                    break;
                case "-Skip8":
                    if (Config.Skip > 7)
                    {
                        Config.Skip -= 8;
                    }
                    break;
                default:
                    // Unknown parameter passed so just abort
                    return;
            }

            // Parameters were changed so update bitmap
            isDirty = true;

            // Redraw the screen
            Refresh();
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

            // Redraw the screen
            Refresh();
        }

        public void ModifyPalettes(string? command)
        {
            switch (command)
            {
                case "Skip":
                    if (Config.SkipMode == 0)
                    {
                        Config.SkipMode = 1;
                    }
                    else
                    {
                        Config.SkipMode = 0;
                    }
                    break;
                case "Pal":
                    Config.PalSearchMode += 1;
                    if (Config.PalSearchMode == PaletteSearchMode.LAST_MODE)
                    {
                        Config.PalSearchMode = 0;
                    }
                    break;
                case "Trans":
                    // Set Color 0 (Transparent Color) to Magenta
                    Config.Colors[0] = System.Windows.Media.Color.FromRgb(255, 0, 255);
                    BitmapPalette = new BitmapPalette(Config.Colors);

                    // Recreate Writeable Bitmap with new color Palette
                    SetupBitmapVariables();
                    break;
                case "Search":
                    switch (Config.PalSearchMode)
                    {
                        case PaletteSearchMode.SIMPLE:
                            Grab_UAE();
                            break;
                        case PaletteSearchMode.ADVANCED:
                            Grab_UAE2();
                            break;
                        case PaletteSearchMode.ECS_UAE_SAVESTATE:
                            Grab_UAE3();
                            break;
                        case PaletteSearchMode.AGA_UAE_SAVESTATE:
                            Grab_UAE3AGA();
                            break;
                    }
                    break;
                case "Standard":
                    // Reset Palette, and set it up
                    Config.Colors = Config.GeneratePalette();
                    BitmapPalette = new BitmapPalette(Config.Colors);

                    // Recreate Writeable Bitmap with new color Palette
                    SetupBitmapVariables();
                    break;
                case "Amiga":
                    TempPalSearch = Config.PaletteFound;
                    switch (Config.PalSearchMode)
                    {
                        case PaletteSearchMode.SIMPLE:
                            Grab_UAE();
                            break;
                        case PaletteSearchMode.ADVANCED:
                            Grab_UAE2();
                            break;
                        case PaletteSearchMode.ECS_UAE_SAVESTATE:
                            Grab_UAE3();
                            break;
                        case PaletteSearchMode.AGA_UAE_SAVESTATE:
                            Grab_UAE3AGA();
                            break;
                    }
                    break;
                case "Apple":
                    /* Apple II Colors
                     * Black  = #040204
                     * Green  = #04fe04
                     * Purple = #fc02fc
                     * Orange = #fc8204
                     * Blue   = #0402fc
                     * White  = #fcfefc
                     */
                    // Reset Palette, and set it up
                    Config.Colors = Config.GeneratePalette();
                    // Set Color 0 to Black
                    Config.Colors[0] = System.Windows.Media.Color.FromRgb(0x04, 0x02, 0x04);
                    // Set Color 1 to Green
                    Config.Colors[1] = System.Windows.Media.Color.FromRgb(0x04, 0xFE, 0x04);
                    // Set Color 2 to Violet
                    Config.Colors[2] = System.Windows.Media.Color.FromRgb(0xFC, 0x02, 0xFC);
                    // Set Color 3 to Orange
                    Config.Colors[3] = System.Windows.Media.Color.FromRgb(0xFC, 0x82, 0x04);
                    // Set Color 4 to Blue
                    Config.Colors[4] = System.Windows.Media.Color.FromRgb(0x04, 0x02, 0xFC);
                    // Set Color 5 to White
                    Config.Colors[5] = System.Windows.Media.Color.FromRgb(0xFC, 0xFE, 0xFC);
                    BitmapPalette = new BitmapPalette(Config.Colors);

                    // Recreate Writeable Bitmap with new color Palette
                    SetupBitmapVariables();
                    break;
                default:
                    // Unknown parameter passed so just abort
                    return;
            }

            // Parameters were changed so update bitmap
            isDirty = true;

            // Redraw the screen
            Refresh();
        }


        public void ModifyBitplanes(string? command)
        {
            switch (command)
            {
                case "Normal":
                    Config.Reverse = !Config.Reverse;
                    break;
                case "Mode":
                    Config.Mode += 1;
                    if (Config.Mode == BitplaneModeType.LAST_MODE)
                    {
                        Config.Mode = 0;
                    }
                    break;
                case "+BP0":
                    Config.BplOrder[0]++;
                    if (Config.BplOrder[0] > 7)
                    {
                        Config.BplOrder[0] = 0;
                    }
                    break;
                case "+BP1":
                    Config.BplOrder[1]++;
                    if (Config.BplOrder[1] > 7)
                    {
                        Config.BplOrder[1] = 0;
                    }
                    break;
                case "+BP2":
                    Config.BplOrder[2]++;
                    if (Config.BplOrder[2] > 7)
                    {
                        Config.BplOrder[2] = 0;
                    }
                    break;
                case "+BP3":
                    Config.BplOrder[3]++;
                    if (Config.BplOrder[3] > 7)
                    {
                        Config.BplOrder[3] = 0;
                    }
                    break;
                case "+BP4":
                    Config.BplOrder[4]++;
                    if (Config.BplOrder[4] > 7)
                    {
                        Config.BplOrder[4] = 0;
                    }
                    break;
                case "+BP5":
                    Config.BplOrder[5]++;
                    if (Config.BplOrder[5] > 7)
                    {
                        Config.BplOrder[5] = 0;
                    }
                    break;
                case "+BP6":
                    Config.BplOrder[6]++;
                    if (Config.BplOrder[6] > 7)
                    {
                        Config.BplOrder[6] = 0;
                    }
                    break;
                case "+BP7":
                    Config.BplOrder[7]++;
                    if (Config.BplOrder[7] > 7)
                    {
                        Config.BplOrder[7] = 0;
                    }
                    break;
                case "-BP0":
                    Config.BplOrder[0]--;
                    if (Config.BplOrder[0] < 0)
                    {
                        Config.BplOrder[0] = 7;
                    }
                    break;
                case "-BP1":
                    Config.BplOrder[1]--;
                    if (Config.BplOrder[1] < 0)
                    {
                        Config.BplOrder[1] = 7;
                    }
                    break;
                case "-BP2":
                    Config.BplOrder[2]--;
                    if (Config.BplOrder[2] < 0)
                    {
                        Config.BplOrder[2] = 7;
                    }
                    break;
                case "-BP3":
                    Config.BplOrder[3]--;
                    if (Config.BplOrder[3] < 0)
                    {
                        Config.BplOrder[3] = 7;
                    }
                    break;
                case "-BP4":
                    Config.BplOrder[4]--;
                    if (Config.BplOrder[4] < 0)
                    {
                        Config.BplOrder[4] = 7;
                    }
                    break;
                case "-BP5":
                    Config.BplOrder[5]--;
                    if (Config.BplOrder[5] < 0)
                    {
                        Config.BplOrder[5] = 7;
                    }
                    break;
                case "-BP6":
                    Config.BplOrder[6]--;
                    if (Config.BplOrder[6] < 0)
                    {
                        Config.BplOrder[6] = 7;
                    }
                    break;
                case "-BP7":
                    Config.BplOrder[7]--;
                    if (Config.BplOrder[7] < 0)
                    {
                        Config.BplOrder[7] = 7;
                    }
                    break;
                default:
                    // Unknown parameter passed so just abort
                    return;
            }

            // Parameters were changed so update bitmap
            isDirty = true;

            // Redraw the screen
            Refresh();
        }
        
        void PutPixel(int x, int y, byte pixelColor)
        {
            if ((WBitMap != null) && (Pixels != null))
            {
                int stride = WBitMap.PixelWidth * WBitMap.Format.BitsPerPixel / 8;
                int offset = (y * stride) + x;

                Pixels[offset] = pixelColor;
            }
        }

        void StretchBlit(int sourceX, int sourceY, int sourceWidth, int sourceHeight,
                         int destX, int destY, int destWidth, int destHeight)
        {
            if ((WBitMap != null) && (Pixels != null))
            {
                int stride = WBitMap.PixelWidth * WBitMap.Format.BitsPerPixel / 8;
                int height = sourceHeight;
                int srcStartOffset = (stride * sourceY) + sourceX;
                int dstStartOffset = (stride * destY) + destX;
                byte pixelColor = 0;

                while (height > 0)
                {
                    int width = sourceWidth;
                    int srcOffset = srcStartOffset;
                    int dstOffset = dstStartOffset;
                    while (width > 0)
                    {
                        // Read Source Color
                        pixelColor = Pixels[srcOffset++];

                        // Write Destination Pixels
                        Pixels[dstOffset] = pixelColor;
                        Pixels[dstOffset+1] = pixelColor;
                        Pixels[dstOffset + stride] = pixelColor;
                        Pixels[dstOffset + stride + 1] = pixelColor;
                        dstOffset += 2;
                        --width;
                    }
                    srcStartOffset += stride;
                    dstStartOffset += (stride * 2);
                    --height;
                }
            }
        }

        void DrawRect(int left, int top, int right, int bottom, byte pixelColor)
        {
            if ((WBitMap != null) && (Pixels != null))
            {
                int stride = WBitMap.PixelWidth * WBitMap.Format.BitsPerPixel / 8;

                int width = (right - left) + 1;
                int height = (bottom - top) + 1 - 2;    // Subtract the Top and Bottom Rectangles

                int topRow = top * stride + left;
                int bottomRow = bottom * stride + left;
                int leftColumn = (top + 1) * stride + left;
                int rightColumn = (top + 1) * stride + right;

                // Do the Top and Bottom Row
                for (int loop = 0; loop < width; loop++)
                {
                    Pixels[topRow + loop] = pixelColor;
                    Pixels[bottomRow + loop] = pixelColor;
                }

                // Do the Left and Right Column
                for (int loop = 0; loop < height; loop++)
                {
                    Pixels[leftColumn + (loop * stride)] = pixelColor;
                    Pixels[rightColumn + (loop * stride)] = pixelColor;
                }
            }
        }

        byte GetPixelColor(long pos, int x, int y)
        {
            bool[] bit = new bool[8];
            long p;
            int byte1;
            int bit1;
            int nn;
            int bitn;

            int i;

            for (i = 0; i < 8; i++)
            {
                bit[i] = false;
            }

            // Make sure there is data to analyze
            if (BinaryData == null)
            {
                return 0;
            }

            for (i = 0; i < Config.Bits; i++)
            {
                switch (Config.Mode)
                {
                    case BitplaneModeType.AMIGA_STANDARD:
                    default:
                        // Amiga type bitplanes
                        p = pos + (Config.BlXSize * Config.BlYSize) * (i);
                        nn = x + (y * (Config.BlXSize * 8));
                        break;
                    case BitplaneModeType.ATARI_ST_STANDARD:
                        // ST Type bitplanes
                        p = pos + Config.BlXSize * (i);
                        nn = x + (Config.Bits * y * (Config.BlXSize * 8));
                        break;
                    case BitplaneModeType.AMIGA_SPRITE:
                        // mode == 2	// Amiga Sprite !!!
                        if (i < 2)
                        {
                            p = pos + Config.BlXSize * (i);
                            nn = x + ((2 * y) * (Config.BlXSize * 8));
                        }
                        else
                        {
                            p = pos + Config.BlXSize * (i - 2) + (Config.BlYSize * Config.BlXSize) * 2;
                            nn = x + ((2 * y) * (Config.BlXSize * 8));
                        }
                        break;
                    case BitplaneModeType.CPC_DOUBLE_STANDARD:
                        // mode == 3 // CPC gfx for Bat-Man / HoH
                        p = pos + (x / 8) + i;
                        nn = x + ((2 * y) * (Config.BlXSize * 8));
                        break;
                    case BitplaneModeType.CPC_SINGLE_STANDARD:
                        //  mode ==4 // CPC gfx for Ultimate games
                        p = pos + (x / 4) + (Config.BlXSize * y * 2);
                        nn = (i * 4) + x % 4;
                        break;
                    case BitplaneModeType.APPLE_II_STANDARD:
                        // Apple II Colors
                        p = pos;
                        nn = 0;
                        break;
                }
                if (Config.SkipMode == 1)
                {
                    p += Config.Skip * i;
                }
                byte1 = nn / 8;
                bit1 = 7 - (nn % 8);
                if (Config.Reverse)
                {
                    bitn = (Config.Bits - i) - 1;
                }
                else
                {
                    bitn = i;
                }

                // Check to see if at end of Memory to view.
                if (p + byte1 < FileSize)
                {
                    if ((BinaryData[p + byte1] & (1 << bit1)) != 0)
                    {
                        bit[bitn] = true;
                    }
                    else
                    {
                        bit[bitn] = false;
                    }
                }
                else
                {
                    bit[bitn] = false;
                }
            }

            byte col = 0;

            for (i = 0; i < Config.Bits; i++)
            {
                if (bit[Config.BplOrder[i]])
                {
                    col += (byte) ((1 << i) & 0xFF);
                }
            }

            return col;
        }

        void DrawCustomBitmap(long pos, int xx, int yy)
        {
            for (int x = 0; x < Config.BlXSize * 8; x++)
            {
                for (int y = 0; y < Config.BlYSize; y++)
                {
                    PutPixel(xx + x, yy + y, GetPixelColor(pos, x, y));
                }
            }
        }

        void DrawGraphics()
        {
            // Draw the contents to the bitmap
            if (WBitMap != null)
            {
                int xx, yy;

                long pos = Config.Offset;

                for (int y = 0; y < Config.NumY; y++)
                {
                    for (int x = 0; x < Config.NumX; x++)
                    {
                        xx = x * (Config.BlXSize * 8 + GfxRipConfig.Hole);
                        yy = y * (Config.BlYSize + GfxRipConfig.Hole);

                        DrawCustomBitmap(pos, xx, yy);
                        if (Config.SkipMode == 0)
                        {
                            pos += (Config.BlXSize * Config.BlYSize * Config.Bits) + Config.Skip;
                        }
                        else
                        {
                            pos += (Config.BlXSize * Config.BlYSize + Config.Skip) * Config.Bits;
                        }
                    }
                }

                WBitMap.WritePixels(new Int32Rect(0, 0, WBitMap.PixelWidth, WBitMap.PixelHeight), Pixels,
                                  WBitMap.PixelWidth * WBitMap.Format.BitsPerPixel / 8, 0);
            }
        }

        void Draw_Zoom()
        {
            if ((WBitMap != null) && (Pixels != null))
            {
                int offset;
                int xx, yy;
                int zoomWidth, zoomHeight;
                int zoomBlXSize, zoomBlYSize;


                if ((Config.BlXSize * 8 * 2 + 4) > GfxRipConfig.ScreenWidth / 2) 
                {
                    zoomBlXSize = ((GfxRipConfig.ScreenWidth / 2) - 4) / (8 * 2);
                }
                else
                {
                    zoomBlXSize = Config.BlXSize;
                }

                if (((Config.BlYSize * 2) + 4) > GfxRipConfig.ScreenHeight / 2)
                {
                    zoomBlYSize = ((GfxRipConfig.ScreenHeight / 2) - 4) / 2;
                }
                else
                {
                    zoomBlYSize = Config.BlYSize;
                }

                zoomWidth = (zoomBlXSize * 8) * 2 + 4;
                zoomHeight = (zoomBlYSize * 2) + 4;

                xx = GfxRipConfig.ScreenWidth - zoomWidth;
                yy = GfxRipConfig.ScreenHeight - zoomHeight;

                // Draw Zoomed In Graphics of Picture located in first area
                StretchBlit(0, 0, zoomBlXSize * 8, zoomBlYSize, xx + 2, yy + 2, zoomBlXSize * 8 * 2, zoomBlYSize * 2);

                // Draw a Box around the Zoom window in Black
                DrawRect(xx, yy, GfxRipConfig.ScreenWidth - 1, GfxRipConfig.ScreenHeight - 1, 1);

                // Draw a Box around the Zoom window inset by 1 with default background color
                DrawRect(xx + 1, yy + 1, GfxRipConfig.ScreenWidth - 2, GfxRipConfig.ScreenHeight - 2, 253);

                // Determine the offset into the pixel data buffer
                offset = ((WBitMap.PixelWidth * WBitMap.Format.BitsPerPixel / 8) * yy) + xx;

                // Copy the Zoom Window Data
                WBitMap.WritePixels(new Int32Rect(xx, yy, zoomWidth, zoomHeight), Pixels,
                              WBitMap.PixelWidth * WBitMap.Format.BitsPerPixel / 8, offset);
            }
        }

        void ClearToColor(byte pixelColor)
        {
            if (Pixels != null) 
            {
                for (int loop = 0; loop < Pixels.Length; loop++)
                {
                    Pixels[loop] = pixelColor;
                }
            }
        }

        public void Refresh()
        {
            // Only draw graphics to screen when there is binary data to load
            if (BinaryData != null)
            {
                // Check to see if need to update bitmap
                if (isDirty)
                {
                    // Clear the bitmap
                    ClearToColor(253);

                    // Draw all the graphics
                    DrawGraphics();

                    // Draw the Zoom if enabled
                    if (Config.Zoom)
                    {
                        Draw_Zoom();
                    }

                    isDirty = false;
                }
            }
        }
    }
}
