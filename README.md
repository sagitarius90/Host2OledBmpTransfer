# Host2OledBmpTransfer
Software for sending image files from the host via UART to the MCU + firmware for receiving and rendering to SSD1306-based OLED display.<br>

![Screenshot](using.gif)<br><br>
![Screenshot](screenshot.png)

## Compiled with
MCU firmware: Keil uVision v5.29.0.0<br>
Host side: MS Visual Studio 2019<br>

## Hardware
MCU: STM32F103C8 (BluePill board)<br>
Display: SSD1306-based OLED 128x64<br>
Host<>MCU: UART link (CP2102 or similar)<br>
Prorgammer: ST-Link v2<br>
