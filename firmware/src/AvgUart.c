
/********************************************************************
* @file       : AvgUart.c
* @author     : Glushanenko Alexander
* @version    : V1.0.1
* @date       : 14.12.2025
* @brief      : UART RX/TX functions
********************************************************************/

#include "stm32f10x.h"
#include "AvgUart.h"

void AvgUartInit(void){
	RCC->APB2ENR |= RCC_APB2ENR_IOPAEN | RCC_APB2ENR_USART1EN;
	uint32_t tmp = RCC->APB2ENR;
	
	// A9 - alternate function out with Push-Pull
	GPIOA->CRH |= GPIO_CRH_MODE9_0;
	GPIOA->CRH |= GPIO_CRH_MODE9_1;
	GPIOA->CRH &= ~GPIO_CRH_CNF9_0;
	GPIOA->CRH |= GPIO_CRH_CNF9_1;
	
	// A10 - floating input
	GPIOA->CRH &= ~GPIO_CRH_MODE10_0;
	GPIOA->CRH &= ~GPIO_CRH_MODE10_1;
	GPIOA->CRH |= GPIO_CRH_CNF10_0;
	GPIOA->CRH &= ~GPIO_CRH_CNF10_1;
	
	//USART1->BRR = 1250; // (72000000/57600 = 1250)
	USART1->BRR = 625; // (72000000/115200 = 625)
	//USART1->BRR = 281; // (72000000/256000 = 281,5)
	USART1->CR1 |= USART_CR1_UE;
}

void AvgUartSendByte(uint8_t byte){
	USART1->CR1 &= ~USART_CR1_RE;
	USART1->CR1 |= USART_CR1_TE;
	USART1->DR = byte;
	while(!(USART1->SR & USART_SR_TC));
}

void AvgUartSendData(uint8_t *data, uint32_t len){
	USART1->CR1 &= ~USART_CR1_RE;
	USART1->CR1 |= USART_CR1_TE;
	for(uint32_t i = 0; i != len; i++)
	{
		USART1->DR = data[i];
		while(!(USART1->SR & USART_SR_TC));
	}
}

uint8_t AvgUartRecvByte(void){
	USART1->CR1 &= ~USART_CR1_TE;
	USART1->CR1 |= USART_CR1_RE;
	while(!(USART1->SR & USART_SR_RXNE)) __nop();
	return USART1->DR;
}

void AvgUartRecvData(uint8_t *data, uint32_t len){
	USART1->CR1 &= ~USART_CR1_TE;
	USART1->CR1 |= USART_CR1_RE;
	for(uint32_t i = 0; i < len; i++){
		while(!(USART1->SR & USART_SR_RXNE)) __nop();
		data[i] = USART1->DR;
	}
}
