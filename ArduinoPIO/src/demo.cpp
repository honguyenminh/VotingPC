//
//  Demo code for FPC1020 Fingerprint Sensor
//  Created by Deray on 2015-10-07.
//
#include <debugMode.h>

#ifdef DEMO
#include <Arduino.h>
#include <EEPROM.h>
#include <SoftwareSerial.h>
#include "FPC1020.h"

extern unsigned char l_ucFPID;  // Current user ID
extern unsigned char rBuf[192]; //Receive return data
#define sw_serial_rx_pin 2      //  Connect this pin to TX on the FPC1020
#define sw_serial_tx_pin 3      //  Connect this pin to RX on the FPC1020

SoftwareSerial swSerial(sw_serial_rx_pin, sw_serial_tx_pin); // Fingerprint serial (RX, TX)
FPC1020 finger(&swSerial);

unsigned int ReadNumberSerial()
{
    // Read all serial data available, as fast as possible
    byte index = 0;
    static char inData[80];
    inData[index] = '\0';
    while (true)
    {
        while (Serial.available() == 0) {}
        char inChar = Serial.read();

        if (inChar == '\n') break;
        else
        {
            if (index < 79)
            {
                inData[index] = inChar;
                Serial.print(inChar);
                index++;
                inData[index] = '\0';
            }
        }
    }
    Serial.println();
    return atoi(inData);
}

void setup()
{
    Serial.begin(115200);
}

void loop()
{
    unsigned int User_ID = 0;
    unsigned char rtf;

    while (1)
    {
        Serial.println("============== Menu ================");
        Serial.println("Add a New User ----------------- 1");
        Serial.println("Fingerprint Matching --------------- 2");
        Serial.println("Get User Number and Print All User ID ------ 3 ");
        Serial.println("Delete Assigned User --------- 4");
        Serial.println("Delete All User ---------- 5");
        Serial.println("============== End =================");

        unsigned char MODE = 0;

        while (Serial.available() <= 0)
        {
        }

        MODE = Serial.read() - '0'; // Basically toInt()

        switch (MODE)
        {
        case 0: // Null
            break;

        case 1: // Fingerprint Input and Add a New User
            MODE = 0;
            User_ID = 0;

            Serial.println("Please input the new user ID (0 ~ 99).");
            User_ID = ReadNumberSerial();
            Serial.println(User_ID);

            Serial.println("Add Fingerprint, please put your finger on the Fingerprint Sensor.");
            rtf = finger.Enroll(User_ID);

            if (rtf == TRUE)
            {
                Serial.print("Success, your User ID is: ");
                Serial.println(User_ID, DEC);
            }
            else if (rtf == FALSE)
            {
                Serial.println("Failed, please try again.");
            }
            else if (rtf == ACK_USER_OCCUPIED)
            {
                Serial.println("Failed, this User ID already exists.");
            }
            else if (rtf == ACK_USER_EXIST)
            {
                Serial.println("Failed, this fingerprint already exists.");
            }
            delay(2000);
            break;

        case 2: // Fingerprint Matching
            MODE = 0;
            Serial.println("Match Fingerprint, please put your finger on the Sensor.");

            if (finger.Search())
            {
                Serial.print("Success, your User ID is: ");
                Serial.println(l_ucFPID);
            }
            else
            {
                Serial.println("Failed, please try again.");
            }
            delay(1000);
            break;

        case 3: // Print all user ID
            MODE = 0;
            if (finger.TryGetUserId())
            {
                Serial.print("Number of saved fingerprints:");
                unsigned char fingerprintCount = finger.GetSavedFingerprintCount();
                Serial.println(fingerprintCount);

                Serial.println("Print all the User ID:");
                for (char i = 0; i < fingerprintCount; i++)
                {
                    Serial.println(rBuf[12 + i * 3]);
                }
            }
            else
            {
                Serial.println("Print User ID Fail!");
            }
            delay(1000);
            break;

        case 4: // Delete Assigned User ID
            MODE = 0;
            User_ID = 0;
            Serial.println("Please input the user ID(0 ~ 99) you want to delecte.");
            User_ID = ReadNumberSerial();

            if (finger.Delete(User_ID))
            {
                Serial.println("Delete Fingerprint User Success!");
            }
            else
            {
                Serial.println("Delete Fingerprint User Fail!");
            }
            delay(1000);
            break;

        case 5: // Delete All User ID

            MODE = 0;
            unsigned char DeleteFlag = 0;

            Serial.println("DELETE ALL fingerprints? [y/N]: ");

            for (unsigned char i = 200; i > 0; i--) //wait response info
            {
                delay(20);
                if (Serial.available() > 0)
                {
                    DeleteFlag = Serial.read();
                    break;
                }
            }

            if (DeleteFlag == 'Y' || DeleteFlag == 'y')
            {
                if (finger.Clear())
                {
                    uint16_t newValue = 0;
                    EEPROM.put(0, newValue);
                    Serial.println("Delete All Fingerprint User Success!");
                }
                else
                {
                    Serial.println("Delete All Fingerprint User Fail!");
                }
            }
            delay(500);
            break;
        }
    }
}
#endif