
/*
 * Project: BMP to OLED via UART
 * Author:  Alexander Glushanenko (sagitarius_0x07C6)
 * Date:    14.12.2025
 */

using System;

namespace BmpToUart
{
    namespace Protocol
    {
        public enum Command : Byte
        {
            CMD_ECHO = 0x01,
            CMD_BMP_TRANSFER = 0x02
        }

        public enum Reply : Byte
        {
            REPLY_OK = 0x03,
            REPLY_CHK_ERROR = 0x04,
            REPLY_UNSUP_FORMAT = 0x05,
            REPLY_UNSUP_CMD = 0x06
        }

        public sealed class dataInfo
        {
            public const int DATASIZE = 10;
            Byte Width;
            Byte Height;
            UInt32 Bufsize;
            UInt32 Checksum;

            public dataInfo(Byte width, Byte height, UInt32 bufsize, UInt32 checksum)
            {
                Width = width;
                Height = height;
                Bufsize = bufsize;
                Checksum = checksum;
            }

            public void ToBuffer(Byte[] buf)
            {
                buf[0] = Width;
                buf[1] = Height;
                buf[2] = (Byte)Bufsize;
                buf[3] = (Byte)(Bufsize >> 8);
                buf[4] = (Byte)(Bufsize >> 16);
                buf[5] = (Byte)(Bufsize >> 24);
                buf[6] = (Byte)Checksum;
                buf[7] = (Byte)(Checksum >> 8);
                buf[8] = (Byte)(Checksum >> 16);
                buf[9] = (Byte)(Checksum >> 24);
                return;
            }
        }
    }
}
