#include <Adafruit_Sensor.h>
#include <IRremote.hpp>
#include <Wire.h>
#include <TM1637Display.h>    // Include TM1637 display library
#include "CRC8.h"


CRC8 crc;


const int IR_RECEIVE_PIN = 3; // IR Sensor (Receiver)
#define VIBRATION_PIN 5       // Vibration Motor
#define BUZZER_PIN A3         // Buzzer
#define DISPLAY_CLK 4         // TM1637 CLK pin (D4)
#define DISPLAY_DIO 2         // TM1637 DIO pin (D2)




int health = 100;             // Initial health
TM1637Display display(DISPLAY_CLK, DISPLAY_DIO);  // Initialize TM1637 Display




///////////////////////////////////////////////////////////////////////////
struct handshakeData {
  int8_t packetType;
  int8_t checksum;
};


struct vestData
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
struct vestData vest_data;
int new_health = 0;




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


void sendVest()
{
  crc.restart();
  vest_data.packetType = 'V';
  Serial.write(vest_data.packetType);
  crc.add(vest_data.packetType);


  Serial.write(vest_data.seqnum);
  crc.add(vest_data.seqnum);


  if (vest_data.seqnum < 5) {
    vest_data.seqnum += 1;
  } else {
    vest_data.seqnum = 1;
  }


  Serial.write(1);
  crc.add(1);


  makePadding(16);
  vest_data.checksum = crc.getCRC();
  Serial.write(vest_data.checksum);
  Serial.flush();
}




///////////////////////////////////////////////////////////////////////////




void vibrate() {
  digitalWrite(VIBRATION_PIN, HIGH);
  delay(500);
  digitalWrite(VIBRATION_PIN, LOW);
  delay(500);
}


void buzz() {
  tone(BUZZER_PIN, 1000, 200); // 1000Hz tone for 200ms
}


void updateDisplay() {
  display.showNumberDec(health, false); // Show health value on display
}






void setup() {
  pinMode(VIBRATION_PIN, OUTPUT);
  pinMode(BUZZER_PIN, OUTPUT);


  Serial.begin(115200);
  while (!Serial) delay(10);


  // IR Receiver setup
  IrReceiver.begin(IR_RECEIVE_PIN, ENABLE_LED_FEEDBACK);
  //Serial.println("Start test");


  // Initialize TM1637 Display
  display.setBrightness(7);  // Max brightness
  updateDisplay();           // Show initial health (100)
}




void loop() {


  int static handshake_start = 0;
  int static handshake_finish = 0;
  byte packetType = buffer[0];


  //reads incoming packts
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


  if (packetType == 'U' && handshake_finish == 1) {
    new_health = buffer[1];
    health = new_health;
    updateDisplay();
  }




  if (IrReceiver.decode() && handshake_finish == 1) {
    sendVest();
    //Serial.println("IR Signal Detected!");
    //Serial.print("Command: 0x");
    //Serial.println(IrReceiver.decodedIRData.command, HEX);


    vibrate();   // Trigger vibration on hit
    buzz();      // Trigger buzzer on hit


    // Reduce health by 5 per hit
    // if (health > 0) {
    //   health -= 5;
    //   updateDisplay();
    // }


    // if (health <= 0) {
    //   Serial.println("Health depleted! Game over.");
    // }


    IrReceiver.resume();  // Prepare for next reception
  }


  delay(10);
}





