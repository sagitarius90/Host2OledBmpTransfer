
/********************************************************************
* @file       : AvgOledSSD1306.c
* @author     : Glushanenko Alexander
* @version    : V1.0.0 (spec)
* @date       : 28.11.2025
* @brief      : OLED SSD1306 display initialization and rendering.
* Unused functions have been removed
********************************************************************/

#include <stdint.h>


#define OLED_I2C_COMMAND      0
#define OLED_I2C_DATA         0x40
#define OLED_I2C_ADDRESS      0x3C
#define OLED_WIDTH            128
#define OLED_HEIGHT           64  // (OLED_HEIGHT % 8 = 0) !!!
#define OLED_BUFFER_SIZE			OLED_WIDTH * OLED_HEIGHT/8

void AvgOledInit(uint8_t include_i2c_init, uint8_t inverse,	uint8_t contrast, uint8_t precharge);
void AvgOledUpdate(void);
void AvgOledClear(void);
void AvgOledWriteBuffer(uint8_t *bitmap, uint32_t bitmapSize);
