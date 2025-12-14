
#include "stm32f10x.h"
#include <stdint.h>

#define AVGI2C_OWNADDRESS_7BIT   0x00000001U // Own address 1
#define AVGI2C_SPEED_FAST                    // Defined - 400kHz; Undefined - 100kHz

void AvgI2C_Init(void);
void AvgI2C_Write (uint8_t slave_addr, uint8_t first_byte, uint8_t *buf, uint32_t size);
void AvgI2C_AckTest(uint8_t slave_addr);
