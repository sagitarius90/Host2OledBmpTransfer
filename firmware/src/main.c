
/********************************************************************
* @file       : main.c
* @author     : Glushanenko Alexander
* @version    : V1.0.0
* @date       : 14.12.2025
* @brief      : Image files receive via UART and rendering to OLED.
********************************************************************/

#include "stm32f10x.h"
#include "AvgUart.h"
#include "AvgOledSD1306.h"
#include "protocol.h"
#include <stdint.h>

static void delay_ms(uint32_t _delay);
static void ExecEcho(void);
static void ExecBmpReceive(void);

int main(){
	delay_ms(100);
	uint8_t command;
	AvgUartInit();
	AvgOledInit(1, 0, 0x25, 0x25);
	
	// echo test
	/*while(1){
		uint8_t buf[1];
		buf[0] = AvgUartRecvByte();
		AvgUartSendData((uint8_t*)&buf, 1);
	}*/
	
	while(1){
		
		command = AvgUartRecvByte();
		
		switch(command){
			case CMD_ECHO:
				AvgUartSendByte(REPLY_OK);
				ExecEcho();
				break;
			case CMD_BMP_TRANSFER:
				AvgUartSendByte(REPLY_OK);
				ExecBmpReceive();
				break;
			default:
				AvgUartSendByte(REPLY_UNSUP_CMD);
		}
	}
	
	return 0;
}

static void delay_ms(uint32_t _delay){
	for(uint32_t i = 0; i < _delay; i++){
		for(uint32_t j = 0; j < 8000; j++) __nop();
	}
}

static void ExecEcho(void){
	uint8_t byte = AvgUartRecvByte();
	AvgUartSendByte(byte);
}

static void ExecBmpReceive(void){
	dataInfo datainfo = {0};
	AvgUartRecvData((uint8_t*)&datainfo, 10);
	
	if(datainfo.width != OLED_WIDTH || datainfo.height != OLED_HEIGHT || datainfo.bufsize != OLED_BUFFER_SIZE)
		AvgUartSendByte(REPLY_UNSUP_FORMAT);
	else{
		uint8_t rcvbuf[OLED_BUFFER_SIZE];
		uint32_t checksum = 0;
		
		AvgUartSendByte(REPLY_OK);
		AvgUartRecvData((uint8_t*)&rcvbuf, OLED_BUFFER_SIZE);
		for(uint32_t i = 0; i < OLED_BUFFER_SIZE; i++)
			checksum += rcvbuf[i];
		
		if(checksum != datainfo.checksum)
			AvgUartSendByte(REPLY_CHK_ERROR);
		else{
			AvgOledWriteBuffer(rcvbuf, OLED_BUFFER_SIZE);
			AvgOledUpdate();
			AvgUartSendByte(REPLY_OK);
		}
	}
}
