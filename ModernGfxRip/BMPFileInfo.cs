using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Xps.Serialization;

namespace ModernGfxRip
{
    public class BMPFileInfo
    {
        /* BMP File Format Definition from http://www.ece.ualberta.ca/~elliott/ee552/studentAppNotes/2003_w/misc/bmp_file_format/bmp_file_format.htm */
        public enum BMPHeaderTypes
        {
            BM, // Windows family
            BA, // OS/2 struct bitmap array
            CI, // OS/2 struct color icon
            CP, // OS/2 const color pointer
            IC, // OS/2 struct icon
            PT, // OS/2 pointer
            Invalid, // if two bytes from loaded file don't equal to any values of all BMP possible formats
        };

        public class Header
        {
            public BMPHeaderTypes signature = BMPHeaderTypes.Invalid;
            public long fileSize = 0;               // File Size in Bytes
            public long reservedA = 0;              // Unused
            public long reservedB = 0;              // Unused
            public long dataOffset = 0;             // Offset from beginning of file to the beginning of the bitmap data

        }

        // DIB Info Header
        public class InfoHeader
        {
            public long headerSize = 40;            // Size of InfoHeader = 40
            public long width = 0;                  // Width of Bitmap
            public long height = 0;                 // Height of Bitmap
            public short planes = 1;                // Number of Planes (=1)
            public short bitsPerPixel = 8;          // Bits per Pixel used to store palette entry information.
                                                    // This also identifies in an indirect way the number of possible colors.
                                                    // Possible values are:
                                                    //      1 = monochrome palette.NumColors = 1  
                                                    //      4 = 4bit palletized. NumColors = 16  
                                                    //      8 = 8bit palletized. NumColors = 256 
                                                    //      16 = 16bit RGB. NumColors = 65536
                                                    //      24 = 24bit RGB. NumColors = 16M
            public long compression = 0;            // Type of Compression  
                                                    //      0 = BI_RGB no compression  
                                                    //      1 = BI_RLE8 8bit RLE encoding  
                                                    //      2 = BI_RLE4 4bit RLE encoding
            public long imageSize = 0;              // (compressed) Size of Image 
                                                    //      It is valid to set this =0 if Compression = 0
            public long xPixelsPerM = 0;            // horizontal resolution: Pixels/meter
            public long yPixelsPerM = 0;            // vertical resolution: Pixels/meter
            public long colorsUsed = 0;             // Number of actually used colors. For a 8-bit / pixel bitmap this will be 100h or 256.
            public long importantColors = 0;        // Number of important colors: 0 = all
        }

        public class ColorTable
        {
            public List<System.Windows.Media.Color> palette = new();
        }

        // Data Read from the File
        public Header? header;
        public InfoHeader? infoHeader;
        public ColorTable? colorTable;

        /*
         * Pixel Storage 
         * Bitmap pixels are stored as bits packed in rows where  the size of each row is rounded up to a multiple of 4 bytes (a 32-bit DWORD)
         * by padding. The total amount of bytes required to store the pixels of an image can not be directly calculated by just counting the
         * bits. Since there is padding involved, the effect of round up the size of each row to a multiple of 4 bytes is required. Padding
         * bytes (not necessarily 0) are to be appended to the end of the rows in order to bring up the length of the rows to a multiple of
         * four bytes. When the pixel array is loaded into memory, each row must begin at a memory address that is a multiple of 4.
         *
         * The image is actually described by the 32-bit DWORDs representation of the pixel array. Usually pixels are stored “bottom-up”,
         * starting in the lower left corner, going from left to right, and then row by row from the bottom to the top of the image. Pixel
         * formats and their implications are as listed below:
         *
         * * The 1-bit per pixel (1bpp) format supports 2 distinct colours, (for example: black and white).
         * * The 2-bit per pixel (2bpp) format supports 4 distinct colours and stores 4 pixels per 1 byte, the left-most pixel being in the two
         *     most significant bits. Each pixel value is a 2-bit index into a table of up to 4 colours.
         * * The 4-bit per pixel (4bpp) format supports 16 distinct colours and stores 2 pixels per 1 byte, the left-most pixel being in the more
         *     significant nibble. Each pixel value is a 4-bit index into a table of up to 16 colours.
         * * The 8-bit per pixel (8bpp) format supports 256 distinct colours and stores 1 pixel per 1 byte. Each byte is an index into a table of
         *     up to 256 colours.
         * * The 16-bit per pixel (16bpp) format supports 65536 distinct colours and stores 1 pixel per 2-byte WORD. Each WORD can define the
         *     alpha, red, green and blue samples of the pixel.
         * * The 24-bit pixel (24bpp) format supports 16,777,216 distinct colours and stores 1 pixel value per 3 bytes. Each pixel value defines
         *     the red, green and blue samples of the pixel (8.8.8.0.0 in RGBAX notation). Specifically, in the order: blue, green and red (8 bits
         *     per each sample).
         * * The 32-bit per pixel (32bpp) format supports 4,294,967,296 distinct colours and stores 1 pixel per 4-byte DWORD. Each DWORD can define
         *     the alpha, red, green and blue samples of the pixel.
         */
        public byte[]? pixelData;

        // How the Data is encoded
        public Encoding? fileEncoding;

        public BMPFileInfo(string fileName)
        {
            byte[]? fileData;

            // Read the BMP file into Memory
            try
            {
                FileStream fs = new(fileName, FileMode.Open);
                fileData = new byte[fs.Length];
                fs.Read(fileData, 0, fileData.Length);
                fs.Close();

                using var fileStreamReader = new StreamReader(fileName, true);
                Encoding currentEncoding = fileStreamReader.CurrentEncoding;
                switch (currentEncoding.BodyName)
                {
                    case "utf-8":
                        fileEncoding = Encoding.UTF8;
                        break;
                    case "us-ascii":
                        fileEncoding = Encoding.ASCII;
                        break;
                }

                // File was loaded
            }
            catch (IOException e)
            {
                throw e;
            }

            if (fileData != null)
            {
                // Read in Header
                header = ReadHeader(ref fileData);

                // Read in InfoHeader
                infoHeader = ReadInfoHeader(ref fileData);

                // Check to see if Palettized, if so create ColorTable
                if ((infoHeader.bitsPerPixel > 1) && (infoHeader.bitsPerPixel <= 8))
                {
                    colorTable = ReadColorTable(ref fileData, infoHeader.colorsUsed, infoHeader.headerSize);
                }

                // Copy Pixel Data from file data
                var pixelData = new byte[fileData.Length - header.dataOffset + 1];
                Array.Copy(fileData, header.dataOffset, pixelData, 0, fileData.Length - header.dataOffset);
            }
        }

        private static Header ReadHeader(ref byte[] fileData)
        {
            byte[] longData = new byte[4];
            byte[] shortData = new byte[2];

            Header bmpHeader = new()
            {
                signature = BMPCheckHeaderType(fileData[0], fileData[1])
            };

            Array.Copy(fileData, 0x02, longData, 0, 4);
            bmpHeader.fileSize = ConvertLong(longData);
            Array.Copy(fileData, 0x06, shortData, 0, 2);
            bmpHeader.reservedA = ConvertShort(shortData);
            Array.Copy(fileData, 0x08, shortData, 0, 2);
            bmpHeader.reservedB = ConvertShort(shortData);
            Array.Copy(fileData, 0x0A, longData, 0, 4);
            bmpHeader.dataOffset = ConvertLong(longData);

            return bmpHeader;
        }

        private static InfoHeader ReadInfoHeader(ref byte[] fileData)
        {
            byte[] longData = new byte[4];
            byte[] shortData = new byte[2];

            InfoHeader bmpInfoHeader = new();
            Array.Copy(fileData, 0x0E, longData, 0, 4);
            bmpInfoHeader.headerSize = ConvertLong(longData);
            Array.Copy(fileData, 0x12, longData, 0, 4);
            bmpInfoHeader.width = ConvertLong(longData);
            Array.Copy(fileData, 0x16, longData, 0, 4);
            bmpInfoHeader.height = ConvertLong(longData);
            Array.Copy(fileData, 0x1A, shortData, 0, 2);
            bmpInfoHeader.planes = ConvertShort(shortData);
            Array.Copy(fileData, 0x1C, shortData, 0, 2);
            bmpInfoHeader.bitsPerPixel = ConvertShort(shortData);
            Array.Copy(fileData, 0x1E, longData, 0, 4);
            bmpInfoHeader.compression = ConvertLong(longData);
            Array.Copy(fileData, 0x22, longData, 0, 4);
            bmpInfoHeader.imageSize = ConvertLong(longData);
            Array.Copy(fileData, 0x26, longData, 0, 4);
            bmpInfoHeader.xPixelsPerM = ConvertLong(longData);
            Array.Copy(fileData, 0x2A, longData, 0, 4);
            bmpInfoHeader.yPixelsPerM = ConvertLong(longData);
            Array.Copy(fileData, 0x2E, longData, 0, 4);
            bmpInfoHeader.colorsUsed = ConvertLong(longData);
            Array.Copy(fileData, 0x32, longData, 0, 4);
            bmpInfoHeader.importantColors = ConvertLong(longData);

            return bmpInfoHeader;
        }

        private static ColorTable ReadColorTable(ref byte[] fileData, long numColors, long headerSize)
        {
            byte red, green, blue;
            ColorTable bmpColorTable = new();

            long offset = 0x0E + headerSize;

            for (int loop = 0; loop < numColors; loop++)
            {
                blue = fileData[offset++];
                green = fileData[offset++];
                red = fileData[offset++];
                // Unused byte for Alpha
                offset++;

                bmpColorTable.palette.Add(System.Windows.Media.Color.FromRgb(red, green, blue));
            }

            return bmpColorTable;
        }

        private static BMPHeaderTypes BMPCheckHeaderType(byte firstValue, byte secondValue)
        {
            if (firstValue == 'B' && secondValue == 'M')
            {
                return BMPHeaderTypes.BM;
            }

            return BMPHeaderTypes.Invalid;
        }

        private static long ConvertLong(byte[] longData)
        {
            long longValue = (long)longData[3] << 24
                            | (long)longData[2] << 16
                            | (long)longData[1] << 8
                            | longData[0];

            return longValue;
        }

        private static short ConvertShort(byte[] shortData)
        {
            short shortValue = (short) (shortData[1] << 8 | shortData[0]);

            return shortValue;
        }
    }
}
