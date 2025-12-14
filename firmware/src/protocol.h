
/********************************************************************
* @file       : protocol.h
* @author     : Glushanenko Alexander
* @version    : V1.0.1
* @date       : 14.12.2025
* @brief      : Host <> MCU protocol definition
********************************************************************/

#include <stdint.h>

#define CMD_ECHO           0x01
#define CMD_BMP_TRANSFER   0x02
#define REPLY_OK           0x03
#define REPLY_CHK_ERROR    0x04
#define REPLY_UNSUP_FORMAT 0x05
#define REPLY_UNSUP_CMD    0x06

#pragma pack(push, 1) // struct data align 1 byte !!!!!!

typedef struct{
	uint8_t width;
	uint8_t height;
	uint32_t bufsize;
	uint32_t checksum;
} dataInfo;

#pragma pack(pop)
