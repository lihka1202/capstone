#include <Arduino.h>
#include <TinyIRSender.hpp>   // Include TinyIRSender library
#include <TM1637Display.h>    // Include TM1637 display library
#include "CRC8.h"




CRC8 crc;




#define BUTTON_PIN 5          // Button connected to D5
#define IR_SEND_PIN 4         // IR Emitter (Transmitter) on D4
#define DISPLAY_CLK 2         // TM1637 CLK pin to D2
#define DISPLAY_DIO 3         // TM1637 DIO pin to D3
#define BUZZER_PIN A0         // Buzzer set to A0 pin
#define VIBRATION_PIN A1      // Vibration set to A1 pin








const uint8_t sAddress = 0x02;
const uint8_t sCommand = 0x15;  // Updated to send 0x15 instead of 0x34
const uint8_t sRepeats = 0;








// Debounce variables
int buttonState = LOW;
int lastButtonState = LOW;
uint32_t lastDebounceTime = 0;
const uint32_t debounceDelay = 50;








// Gun variables
int bulletcount = 6;








// Flag to prevent multiple triggers per press
bool isTriggered = false;








// Initialize TM1637 Display
TM1637Display display(DISPLAY_CLK, DISPLAY_DIO);
















///////////////////////////////////////////////////////////////////////////
struct handshakeData {
  int8_t packetType;
  int8_t checksum;
};








struct gunData
{
  char packetType;
  int seqnum;
  int seqnum_old;
  char checksum;
};








//time and buffer variables for serial comms
unsigned static long currtime_ACK = 0;
unsigned static long lasttime_ACK = 0;
int static initFlag = 0;
const int BUFFER_SIZE = 20;
byte buffer[BUFFER_SIZE];
unsigned long previousMillis = 0;
byte twoByteBuf[2];
struct handshakeData handshake_data;
struct gunData gun_data;








void (* resetBeetle) (void) = 0;








void makePadding(int n) {
  for (int i = 0; i < n; i++) {
    Serial.write('0');
    crc.add('0');
  }
}








void writeIntToSerial(int16_t data) {
  twoByteBuf[1] = data & 255;
  twoByteBuf[0] = (data >> 8) & 255;
  Serial.write(twoByteBuf, sizeof(twoByteBuf));
  crc.add(twoByteBuf, sizeof(twoByteBuf));
}








void sendACK() {
  crc.restart();
  handshake_data.packetType = 'A';
  Serial.write(handshake_data.packetType);
  crc.add(handshake_data.packetType);




  makePadding(18);




  handshake_data.checksum = crc.getCRC();
  Serial.write(handshake_data.checksum);
  Serial.flush();
}








void sendGun()
{
  crc.restart();
  gun_data.packetType = 'G';
  Serial.write(gun_data.packetType);
  crc.add(gun_data.packetType);








  Serial.write(gun_data.seqnum);
  crc.add(gun_data.seqnum);
 
  if (gun_data.seqnum < 5) {
    gun_data.seqnum += 1;
  } else {
    gun_data.seqnum = 1;
  }




  Serial.write(1);
  crc.add(1);




  makePadding(16);
  gun_data.checksum = crc.getCRC();
  Serial.write(gun_data.checksum);
  Serial.flush();
}




///////////////////////////////////////////////////////////////////////////




void updateDisplay() {
  if (bulletcount > 0) {
    display.showNumberDec(bulletcount);  // Show single digit only
  } else {
    display.showNumberDec(0);  // Show 0 when out of bullets
  }
}








void vibrate() {
  digitalWrite(VIBRATION_PIN, HIGH); // Send signal to MEGA
  delay(500);
  digitalWrite(VIBRATION_PIN, LOW);  // Stop signal
  delay(500);
}








void setup() {
  pinMode(BUTTON_PIN, INPUT);
  pinMode(BUZZER_PIN, OUTPUT);
  pinMode(VIBRATION_PIN, OUTPUT);
  Serial.begin(115200);
  // Serial.println(F("IR Transmitter & Display Initialized"));








  // Initialize TM1637 Display
  display.setBrightness(7); // Max brightness
  updateDisplay(); // Show initial bullet count








  delay(100);
}








void loop() {








  int static handshake_start = 0;
  int static handshake_finish = 0;
  byte packetType = buffer[0];








  //reads incoming packets
  if (Serial.available() > 0) {
    int rlen = Serial.readBytes(buffer, BUFFER_SIZE);
  }








  if (packetType == 'R') {
    resetBeetle();
  }








  if (packetType == 'H') {
    currtime_ACK = millis();
    if(currtime_ACK-lasttime_ACK>350){
      sendACK();
      handshake_start = 1;
      handshake_finish = 0;
      lasttime_ACK = currtime_ACK;
    }
  }








  if (packetType == 'A' && handshake_start == 1) {
    handshake_start = 0;
    handshake_finish = 1;
  }








  if (handshake_finish == 1) {








  // Debounce button
  int reading = digitalRead(BUTTON_PIN);
  if (reading != lastButtonState) {
    lastDebounceTime = millis();
  }




  // read U packets for reload command
  if (packetType == 'U' && handshake_finish == 1) {
    bulletcount = buffer[1];
    updateDisplay();
    tone(BUZZER_PIN, 500, 500);
    delay(100);
  }




  if ((millis() - lastDebounceTime) > debounceDelay) {
    if (reading != buttonState) {
      buttonState = reading;
      if (buttonState == HIGH && !isTriggered) {
        if (bulletcount > 0) {
          sendGun();
          // Serial.println("IR Signal Sent");
          // Serial.print("Bullets left: ");
          sendNEC(IR_SEND_PIN, sAddress, sCommand, sRepeats); // Now sends 0x15
          tone(BUZZER_PIN, 1000, 100);  // 1000Hz tone for 100ms
          // bulletcount--;  // Reduce bullet count
          // Serial.println(bulletcount);
          // updateDisplay(); // Update display with new count
          vibrate();
        } else {
          // Serial.println("No bullets left");
          // Serial.println("Please reload");
          tone(BUZZER_PIN, 500, 500);  // 1000Hz tone for 100ms
          // display.showNumberDec(0, true); // Show 0 when out of bullets
          // delay(1000); // "fake reload lag"
          // bulletcount = 6;
          // display.showNumberDec(6, false); // Show 5 bullets after reload
        }








        isTriggered = true;
      }
    }
  }








  // Reset flag when button is released
  if (buttonState == LOW) {
    isTriggered = false;
  }








  lastButtonState = reading;








  delay(10);
}
}







