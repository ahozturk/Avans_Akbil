#include <SPI.h>
#include <MFRC522.h>

MFRC522 rfid(10, 9);
      
void setup() {
  Serial.begin(9600);
  SPI.begin();
  rfid.PCD_Init();
}

void loop() {
  if (rfid.PICC_IsNewCardPresent() && rfid.PICC_ReadCardSerial()) {
    Serial.print("rfid:");
    Serial.print(rfid.uid.uidByte[0]);
    Serial.print(rfid.uid.uidByte[1]);
    Serial.print(rfid.uid.uidByte[2]);
    Serial.print(rfid.uid.uidByte[3]);
    Serial.println(rfid.uid.uidByte[4]);
  }
  rfid.PICC_HaltA();
}
