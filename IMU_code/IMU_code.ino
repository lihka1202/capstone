#include <Adafruit_MPU6050.h>
#include <Adafruit_Sensor.h>
#include <Wire.h>
#include "CRC8.h"


Adafruit_MPU6050 mpu;


/////////////////////////////////////////////////////


CRC8 crc;


float calibration[6] = {-0.80,0.83,9.70,-0.05,-0.02,-0.00};


struct acc {
  float acc_x;
  float acc_y;
  float acc_z;
};


struct gyro {
  float gyro_x;
  float gyro_y;
  float gyro_z;
};


struct acc currentAcc;
struct gyro currentGyro;


struct handshakeData {
  int8_t packetType;
  int8_t checksum;
};


struct motionData {
  int8_t packetType;
  int16_t  roll;
  int16_t  pitch;
  int16_t  yaw;
  int8_t checksum;
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
struct motionData motion_data;


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






void sendMotionData() {
  crc.restart();
  motion_data.packetType = 'M';
  Serial.write(motion_data.packetType);
  crc.add(motion_data.packetType);


  //readMotionData();
  writeIntToSerial(int16_t((currentAcc.acc_x-calibration[0])*100));
  writeIntToSerial(int16_t((currentAcc.acc_y-calibration[1])*100));
  writeIntToSerial(int16_t((currentAcc.acc_z-calibration[2])*100));
  writeIntToSerial(int16_t((currentGyro.gyro_x-calibration[3])*100));
  writeIntToSerial(int16_t((currentGyro.gyro_y-calibration[4])*100));
  writeIntToSerial(int16_t((currentGyro.gyro_z-calibration[5])*100));
  //Serial.println("Packet sent");


  makePadding(6);
  motion_data.checksum = crc.getCRC();
  Serial.write(motion_data.checksum);
  Serial.flush();
}


/////////////////////////////////////////////////////////////////////////////


void setup(void) {
  Serial.begin(115200);
  while (!Serial)
    delay(10); // will pause Zero, Leonardo, etc until serial console opens


  // Try to initialize!
  if (!mpu.begin()) {
    while (1) {
      delay(10);
    }
  }
  // Serial.println("MPU6050 Found!");


  //setupt motion detection
  mpu.setHighPassFilter(MPU6050_HIGHPASS_0_63_HZ);
  mpu.setMotionDetectionThreshold(50);
  mpu.setMotionDetectionDuration(50);
  mpu.setInterruptPinLatch(true); // Keep it latched.  Will turn off when reinitialized.
  mpu.setInterruptPinPolarity(true);
  mpu.setMotionInterrupt(true);
  delay(100);
}


int static handshake_finish = 0;
int static handshake_start = 0;


void loop() {


  //reads incoming packets
  if (Serial.available() > 0) {
    int rlen = Serial.readBytes(buffer, BUFFER_SIZE);
  }


  byte packetType = buffer[0];


  if (packetType == 'R') {
    resetBeetle();
  }


  if (packetType == 'H') {
    currtime_ACK = millis();
    if(currtime_ACK-lasttime_ACK>350){
      sendACK();
      handshake_start = 1;
    //  handshake_finish = 0;
      lasttime_ACK = currtime_ACK;
    }
  }


  if (packetType == 'A' && handshake_start == 1) {
    handshake_start = 0;
    handshake_finish = 1;
  }


  if(mpu.getMotionInterruptStatus() && handshake_finish == 1) {
    /* Get new sensor events with the readings */
    sensors_event_t a, g, temp;
    for (int i = 0; i < 10; i++) {
      mpu.getEvent(&a, &g, &temp);


      //adds acc and gyro data to currentAcc and currentGyro
      currentAcc.acc_x = a.acceleration.x;
      currentAcc.acc_y = a.acceleration.y;
      currentAcc.acc_z = a.acceleration.z;
      currentGyro.gyro_x = g.gyro.x;
      currentGyro.gyro_y = g.gyro.y;
      currentGyro.gyro_z = g.gyro.z;


      sendMotionData();
      delay(50);
    }
  }
}
