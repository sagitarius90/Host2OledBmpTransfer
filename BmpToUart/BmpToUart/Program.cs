
/*
 * Project: BMP to OLED via UART
 * Author:  Alexander Glushanenko (sagitarius_0x07C6)
 * Date:    14.12.2025
 */

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Ports;

namespace BmpToUart
{
    internal enum ErrorCode
    {
        NONE,
        COM_OPEN_ERROR,
        INCORRECT_INPUT,
        FILE_NOT_EXIST,
        BITMAP_INVALID_PIXEL_FORMAT,
        BITMAP_TOO_SMALL_WIDTH,
        BITMAP_INCORRECT_HEIGHT,
        UART_RW_EXCEPTION,
        REPLY_UNSUPPORTED_FORMAT,
        UART_TRANSFER_ERROR
    }

    internal enum Mode
    {
        SINGLE,
        SERIAL
    }
    class Program
    {
        private static ErrorCode BitmapConvertAndSend(SerialPort port, string bmpfilename)
        {
            ErrorCode errorCode = ErrorCode.NONE;

            Bitmap bitmap = new Bitmap(bmpfilename);
            if (bitmap.PixelFormat != PixelFormat.Format1bppIndexed)
                errorCode = ErrorCode.BITMAP_INVALID_PIXEL_FORMAT;
            else
            {
                if (bitmap.Width < 1)
                    errorCode = ErrorCode.BITMAP_TOO_SMALL_WIDTH;
                else
                {
                    if (bitmap.Height < 8 || bitmap.Height % 8 != 0)
                        errorCode = ErrorCode.BITMAP_INCORRECT_HEIGHT;
                    else
                    {
                        int pages = bitmap.Height / 8;
                        int columns = bitmap.Width;
                        int bufsize = pages * columns;
                        byte[] buf = new byte[bufsize];
                        int bufcnt = 0;
                        int exp_checksum = 0;

                        for (int page = 0; page < pages; page++)
                        {
                            for (int pixel_x = 0; pixel_x < columns; pixel_x++)
                            {
                                int pixel_y = page * 8;

                                byte curbyte = 0;
                                byte curbit = 0;
                                for (int i = 0; i < 8; i++)
                                {
                                    curbit = (bitmap.GetPixel(pixel_x, pixel_y + i).ToArgb() & 0xFFFFFF) == 0 ? (byte)0x01 : (byte)0x00;
                                    curbyte |= (byte)(curbit << i);
                                }

                                buf[bufcnt] = curbyte;
                                bufcnt++;

                                exp_checksum += curbyte;
                            }
                        }

                        if (port.IsOpen)
                        {
                            if (port.BytesToRead > 0) port.DiscardInBuffer();
                            if (port.BytesToWrite > 0) port.DiscardOutBuffer();

                            Byte[] bytebuf = new Byte[1];

                            bytebuf[0] = (Byte)Protocol.Command.CMD_BMP_TRANSFER;

                            try
                            {
                                port.Write(bytebuf, 0, 1);
                                port.Read(bytebuf, 0, 1);
                            }
                            catch (Exception) { errorCode = ErrorCode.UART_RW_EXCEPTION; }

                            if(errorCode == ErrorCode.NONE)
                            {
                                if (bytebuf[0] == (Byte)Protocol.Reply.REPLY_OK)
                                {
                                    Protocol.dataInfo dataInfo = new Protocol.dataInfo((Byte)bitmap.Width, (Byte)bitmap.Height, (UInt32)bufsize, (UInt32)exp_checksum);
                                    Byte[] infobuf = new byte[Protocol.dataInfo.DATASIZE];
                                    dataInfo.ToBuffer(infobuf);
                                    try
                                    {
                                        port.Write(infobuf, 0, Protocol.dataInfo.DATASIZE);
                                        port.Read(bytebuf, 0, 1);
                                    }
                                    catch (Exception) { errorCode = ErrorCode.UART_RW_EXCEPTION; }
                                    if (errorCode == ErrorCode.NONE)
                                    {
                                        if (bytebuf[0] == (Byte)Protocol.Reply.REPLY_OK)
                                        {
                                            try
                                            {
                                                port.Write(buf, 0, bufsize);
                                                port.Read(bytebuf, 0, 1);
                                            }
                                            catch (Exception) { errorCode = ErrorCode.UART_RW_EXCEPTION; }

                                            if(errorCode == ErrorCode.NONE)
                                            {
                                                if (bytebuf[0] != (Byte)Protocol.Reply.REPLY_OK)
                                                    errorCode = ErrorCode.UART_TRANSFER_ERROR;
                                            }
                                        }
                                        else if (bytebuf[0] == (Byte)Protocol.Reply.REPLY_UNSUP_FORMAT)
                                            errorCode = ErrorCode.REPLY_UNSUPPORTED_FORMAT;
                                        else errorCode = ErrorCode.UART_TRANSFER_ERROR;
                                    }
                                }
                                else errorCode = ErrorCode.UART_TRANSFER_ERROR;
                            }
                        }
                    }
                }
            }
            return errorCode;
        }
        static void Main(string[] args)
        {
            ConsoleColor fgColorDefault = Console.ForegroundColor;
            ConsoleColor fgColorError = ConsoleColor.Red;
            ConsoleColor fgColorGood = ConsoleColor.Green;

            Console.Write("BMP to OLED via UART v1.2 (14.12.2025)\n");
            Console.Write("(C) Alexander Glushanenko, 2025\n\n");

            ErrorCode errorCode = ErrorCode.NONE;
            DateTime timeStart;
            DateTime timeEnd;
            TimeSpan timeSpan;

            // 1    Select COM port

            SerialPort serialPort = null;
            Console.Write("COM port number: ");
            string comnum = Console.ReadLine();

            Console.Write("Select baudrate (1 - 57600 bit/s; 2 - 115200 bit/s; 3 - 256000 bit/s): ");
            string brsel = Console.ReadLine();
            int baudrate = 0;
            if (brsel.Equals("1")) baudrate = 57600;
            else if (brsel.Equals("2")) baudrate = 115200;
            else if (brsel.Equals("3")) baudrate = 256000;
            else errorCode = ErrorCode.INCORRECT_INPUT;

            if(errorCode == ErrorCode.NONE)
            {
                try
                {
                    serialPort = new SerialPort($"COM{comnum}", baudrate, Parity.None, 8, StopBits.Two)
                    {
                        ReadTimeout = 10000,
                        WriteTimeout = 10000,
                    };
                    serialPort.Open();
                }
                catch (IOException) { };

                if (serialPort != null && !serialPort.IsOpen)
                    errorCode = ErrorCode.COM_OPEN_ERROR;
                else
                    Console.Write($"COM{comnum} (Baudrate = {baudrate}; Parity = None; StopBits = Two) is opened\n");
            }

            // 2    Select mode (1 - single send; 2 - serial files sending)

            Mode mode = Mode.SINGLE;
            if (errorCode == ErrorCode.NONE)
            {
                Console.Write("Select mode (1 - single send; 2 - serial files sending): ");
                string strmode = Console.ReadLine();
                if (strmode.Equals("1")) mode = Mode.SINGLE;
                else if (strmode.Equals("2")) mode = Mode.SERIAL;
                else errorCode = ErrorCode.INCORRECT_INPUT;
            }

            if(errorCode == ErrorCode.NONE)
            {
                // 3    Single:
                if(mode == Mode.SINGLE)
                {
                    Console.Write("Single send mode:\n");

                    Console.Write("BMP file name ([name].bmp): ");
                    string filename = Console.ReadLine() + ".bmp";
                    if (!File.Exists(filename)) errorCode = ErrorCode.FILE_NOT_EXIST;
                    else
                    {
                        Console.Write($"File \"{filename}\" sending...");
                        timeStart = DateTime.Now;
                        errorCode = BitmapConvertAndSend(serialPort, filename);
                        timeEnd = DateTime.Now;
                        if (errorCode == ErrorCode.NONE)
                        {
                            Console.ForegroundColor = fgColorGood;
                            Console.Write(" OK");
                            Console.ForegroundColor = fgColorDefault;
                            Console.Write($" ({(timeEnd - timeStart).TotalMilliseconds} ms)\n");
                        }
                        else
                        {
                            Console.ForegroundColor = fgColorError;
                            Console.Write(" error\n");
                            Console.ForegroundColor = fgColorDefault;
                        }
                    }
                }

                // 3    Serial:
                if (mode == Mode.SERIAL)
                {
                    Console.Write("Serial send mode:\n");

                    Console.Write("BMP file prefix (file name - [prefix]_[index:000-999].bmp): ");
                    string prefix = Console.ReadLine();
                    Console.Write("Files amount: ");
                    string strfiles = Console.ReadLine();
                    int files = 0;
                    if (!Int32.TryParse(strfiles, out files))
                        errorCode = ErrorCode.INCORRECT_INPUT;
                    else
                    {
                        if(errorCode == ErrorCode.NONE)
                        {
                            Console.Write("Reps amount: ");
                            string strreps = Console.ReadLine();
                            int reps = 0;
                            if (!Int32.TryParse(strreps, out reps))
                                errorCode = ErrorCode.INCORRECT_INPUT;
                            else
                            {
                                for (int repcnt = 0; repcnt < reps && errorCode == ErrorCode.NONE; repcnt++)
                                {
                                    Console.Write($"Cycle {repcnt + 1}:\n");
                                    for (int filecnt = 0; filecnt < files && errorCode == ErrorCode.NONE; filecnt++)
                                    {
                                        string filename = string.Format("{0}_{1:D3}.bmp", prefix, filecnt);
                                        if (!File.Exists(filename)) errorCode = ErrorCode.FILE_NOT_EXIST;
                                        else
                                        {
                                            Console.Write($"  File \"{filename}\" sending...");
                                            timeStart = DateTime.Now;
                                            errorCode = BitmapConvertAndSend(serialPort, filename);
                                            timeEnd = DateTime.Now;
                                            if (errorCode == ErrorCode.NONE)
                                            {
                                                Console.ForegroundColor = fgColorGood;
                                                Console.Write(" OK");
                                                Console.ForegroundColor = fgColorDefault;
                                                Console.Write($" ({(timeEnd - timeStart).TotalMilliseconds} ms)\n");
                                            }
                                            else
                                            {
                                                Console.ForegroundColor = fgColorError;
                                                Console.Write(" error\n");
                                                Console.ForegroundColor = fgColorDefault;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // 4    Print status (errors)

            String errorMessage = null;
            switch (errorCode)
            {
                case ErrorCode.COM_OPEN_ERROR:
                    errorMessage = "COM port open error";
                    break;
                case ErrorCode.INCORRECT_INPUT:
                    errorMessage = "Incorrect input";
                    break;
                case ErrorCode.FILE_NOT_EXIST:
                    errorMessage = "File not exist";
                    break;
                case ErrorCode.BITMAP_INVALID_PIXEL_FORMAT:
                    errorMessage = "Invalid pixel format";
                    break;
                case ErrorCode.BITMAP_TOO_SMALL_WIDTH:
                    errorMessage = "BMP too small width";
                    break;
                case ErrorCode.BITMAP_INCORRECT_HEIGHT:
                    errorMessage = "BMP height must be multiple of 8";
                    break;
                case ErrorCode.UART_RW_EXCEPTION:
                    errorMessage = "UART read/write exception";
                    break;
                case ErrorCode.UART_TRANSFER_ERROR:
                    errorMessage = "UART transfer error";
                    break;
            }
            if (errorCode != ErrorCode.NONE && errorMessage != null)
            {
                Console.Write("Error: ");
                Console.ForegroundColor = fgColorError;
                Console.Write(errorMessage + "\n");
                Console.ForegroundColor = fgColorDefault;
            }

            // 5    Exit

            if (serialPort.IsOpen) serialPort.Close();
            Console.Write("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
