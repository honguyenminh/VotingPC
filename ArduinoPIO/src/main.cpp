#include <debugMode.h>

#if !defined(DEMO)
#include <Arduino.h>
#include <FPC1020.h>
#include <SoftwareSerial.h>
#include <SignalTable.h>
#include <EEPROM.h>

#define BAUD_RATE 115200
#define RX_PIN 2 // Connect this pin to TX on the FPC1020
#define TX_PIN 3 // Connect this pin to RX on the FPC1020

SoftwareSerial swSerial(RX_PIN, TX_PIN);
FPC1020 finger(&swSerial);

enum Mode
{
    None,
    VotingPC,
    FingerGet
};

unsigned int id;
bool fingerFound = false;
Mode mode = Mode::None;

void Enroll(unsigned int);

void setup()
{
    Serial.begin(BAUD_RATE);
}

void loop()
{
    // Find mode
    while (mode == Mode::None)
    {
        if (Serial.available() > 0)
        {
            char receivedChar = Serial.read();
            // FingerGet signal
            if (receivedChar == F_ACK)
            {
                Serial.println("Internal found");
                mode = Mode::FingerGet;
            }
            // VotingPC signal
            else if (receivedChar == V_ACK)
            {
                Serial.print(V_ACK);
                mode = Mode::VotingPC;
            }
        }
    }

    // VotingPC mode
    while (mode == Mode::VotingPC)
    {
        if (Serial.available() > 0)
        {
            char receivedChar = Serial.read();
            // Start scanning fingerprint
            if (receivedChar == START_SCAN)
            {
                // TODO: check V_TOUCH here
                // Read fingerprint and save id
                if (finger.Search())
                {
                    Serial.print(V_FOUND);
                    id = finger.GetId();
                    fingerFound = true;
                }
                else Serial.print(V_INVALID);
            }
            // Delete fingerprint
            else if (receivedChar == V_DELETE_FINGER)
            {
                if (fingerFound)
                {
                    finger.Delete(id);
                    fingerFound = false;
                }
            }

            // App closed
            else if (receivedChar == CLOSE)
            {
                mode = Mode::None;
                fingerFound = false;
            }
            // Mode change (should not be run though, just here to be extra safe)
            else if (receivedChar == F_ACK)
            {
                Serial.println("Internal found");
                mode = Mode::FingerGet;
                fingerFound = false;
            }
            else if (receivedChar == V_ACK)
            {
                Serial.print(V_ACK);
                fingerFound = false;
            }
        }
    }

    // FingerGet mode
    while (mode == Mode::FingerGet)
    {
        if (Serial.available() > 0)
        {
            char receivedChar = Serial.read();
            // Get fingerprint to memory
            if (receivedChar == START_SCAN)
            {
                // Get known empty fingerprint ID from var in memory
                // Remove if not needed to show
                uint16_t emptyId;
                EEPROM.get(0, emptyId);

                //* NOTE: Replace this with generic waiting message if needed
                Serial.print("Đang chờ vân tay ID #");
                Serial.print(emptyId);
                Serial.println("...");
                // Get fingerprint
                unsigned char result = finger.Enroll(emptyId);

                if (result == TRUE)
                {
                    Serial.print("[OK] Hoàn tất, đã thêm vân tay số #");
                    Serial.println(emptyId);
                    EEPROM.put(0, ++emptyId);
                }
                else if (result == FALSE)
                {
                    Serial.println("[LỖI] Vui lòng thử lại");
                }
                else if (result == ACK_USER_OCCUPIED)
                {
                    Serial.print("[LỖI] ID #");
                    Serial.print(emptyId);
                    Serial.println(" đã đăng ký vân tay. Vui lòng thử lại.");
                    EEPROM.put(0, ++emptyId);
                }
                else if (result == ACK_USER_EXIST)
                {
                    Serial.println("[LỖI] Vân tay đã tồn tại.");
                }
                // TODO: use V_TOUCH here

                // End session message
                Serial.println("Internal done");
            }
            // App closed
            else if (receivedChar == CLOSE)
            {
                mode = Mode::None;
                fingerFound = false;
            }
            // Mode change (should not be run though, just here to be extra safe)
            else if (receivedChar == V_ACK)
            {
                Serial.print(V_ACK);
                mode = Mode::VotingPC;
                fingerFound = false;
            }
            else if (receivedChar == 'F')
            {
                Serial.println("Internal found");
                fingerFound = false;
            }
        }
    }
}
#endif // DEMO