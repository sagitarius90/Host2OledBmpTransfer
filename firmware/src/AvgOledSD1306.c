
/********************************************************************
* @file       : AvgOledSSD1306.c
* @author     : Glushanenko Alexander
* @version    : V1.0.0 (spec)
* @date       : 28.11.2025
* @brief      : OLED SSD1306 display initialization and rendering.
* Unused functions have been removed
********************************************************************/

#include "AvgOledSD1306.h"
#include "AvgI2C.h"
#include "math.h"

static uint8_t oledbuf[OLED_BUFFER_SIZE];

void AvgOledInit(uint8_t include_i2c_init, uint8_t inverse,	uint8_t contrast, uint8_t precharge){
	
	uint8_t init_sequence[] = {
		0xAE,			// display OFF
		0xC8,			// COM scan dir (C0 - normal; C8 - remap)
		0x00,			// [low column addr]
		0x10,			// high column addr
		0x00,			// [start line addr]
		0x81, contrast,		// contrast control
		0xA1,			// SEG remap 0-127
		inverse ? 0xA7 : 0xA6,			// A6 - normal; A7 - inverse
		0xA8, 0x3F,			// MUX ratio
		0xA4,			// Entire display ON
		0xD3, 0x00,			// display offset
		0xD5, 0x50,			// display clock divide
		0xD9, precharge,			// precharge period
		0xDA, 0x12,			// COM pin HW-config
		0xDB, 0x20,			// set Vcomh
		0x8D,			// DC-DC enable
		0x14,
		0xAF			// display ON
	};
	
	if(include_i2c_init > 0)
		AvgI2C_Init();
	
	AvgI2C_Write(OLED_I2C_ADDRESS, OLED_I2C_COMMAND, init_sequence, sizeof(init_sequence));
	AvgOledClear();
	AvgOledUpdate();
}

void AvgOledUpdate(){
	uint8_t pagebuf[OLED_WIDTH];
	for(uint8_t page = 0; page < OLED_HEIGHT / 8; page++){
		for(uint32_t i = 0; i < OLED_WIDTH; i++)
			pagebuf[i] = oledbuf[page * OLED_WIDTH + i];
		AvgI2C_Write(OLED_I2C_ADDRESS, OLED_I2C_COMMAND, (uint8_t[]){0x21, 0, OLED_WIDTH - 1}, 3);	// Set start COLUMN (0)
		AvgI2C_Write(OLED_I2C_ADDRESS, OLED_I2C_COMMAND, (uint8_t[]){0x22, page, 7}, 3);						// Set current PAGE
		AvgI2C_Write(OLED_I2C_ADDRESS, OLED_I2C_DATA, pagebuf, sizeof(pagebuf));
	}
}

void AvgOledClear(){
	for(uint32_t i = 0; i < OLED_BUFFER_SIZE; i++)
		oledbuf[i] = 0;
}

void AvgOledWriteBuffer(uint8_t *bitmap, uint32_t bitmapSize){
	for(uint32_t i = 0; i < OLED_BUFFER_SIZE; i++)
		oledbuf[i] = bitmap[i];
}

