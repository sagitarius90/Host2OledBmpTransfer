
/********************************************************************
* @file       : AvgUart.h
* @author     : Glushanenko Alexander
* @version    : V1.0.1
* @date       : 14.12.2025
* @brief      : UART RX/TX functions
********************************************************************/

#include <stdint.h>

void     AvgUartInit(void);
void     AvgUartSendByte(uint8_t byte);
void     AvgUartSendData(uint8_t *data, uint32_t len);
void     AvgUartRecvData(uint8_t *data, uint32_t len);
uint8_t  AvgUartRecvByte(void);
