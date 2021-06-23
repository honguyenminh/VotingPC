// For debugging only
// Someone#9554

bool isVote; // Current mode

void setup() {
    pinMode(2, INPUT);
    pinMode(LED_BUILTIN, OUTPUT);
    // Start serial at 115200 baud rate
    Serial.begin(115200);
    // Find mode first
    while (true) {
        if (Serial.available() > 0) {
            char receivedChar = Serial.read();
            // FingerGet signal
            if (receivedChar == 'F') {
                Serial.println("Internal found");
                isVote = false;
                break;
            }
            // VotingPC signal
            else if (receivedChar == 'V') {
                Serial.print('K');
                isVote = true;
                break;
            }
        }
    }
}


void loop() {
    // VotingPC mode
    if (isVote) {
        // Read fingerprint signal from button
        while (digitalRead(2) == HIGH) delay(50);
        Serial.print('D'); // Send this when finger found
        
        while (true) {
            if (Serial.available() > 0) {
                char receivedChar = Serial.read();
                if (receivedChar == 'X') {
                    // Delete fingerprint
                    // Blink led
                    digitalWrite(LED_BUILTIN, HIGH);
                    delay(500);
                    digitalWrite(LED_BUILTIN, LOW);
                    delay(500);
                    digitalWrite(LED_BUILTIN, HIGH);
                    delay(500);
                    digitalWrite(LED_BUILTIN, LOW);
                    delay(500);
                    digitalWrite(LED_BUILTIN, HIGH);
                    delay(500);
                    digitalWrite(LED_BUILTIN, LOW);
                    delay(500);
                    digitalWrite(LED_BUILTIN, HIGH);
                    delay(500);
                    digitalWrite(LED_BUILTIN, LOW);
                    break;
                }
                else if (receivedChar == 'F') {
                    Serial.println("Internal found");
                    isVote = false;
                    break;
                }
                else if (receivedChar == 'V') {
                    Serial.print('K');
                    isVote = true;
                    break;
                }
            }
        }
    }
    
    // FingerGet mode
    else {
        while (true) {
            if (Serial.available() > 0) {
                char receivedChar = Serial.read();
                if (receivedChar == 'F') {
                    Serial.println("Internal found");
                    break;
                }
                else if (receivedChar == 'V') {
                    Serial.print('K');
                    isVote = true;
                    break;
                }
                else if (receivedChar == 'S') {
                    // Demo code only
                    Serial.println("Đang chờ vân tay");
                    while (digitalRead(2) == HIGH) delay(50);
                    Serial.println("Xong!");
                    Serial.println("Internal done");
                }
            }
        }
    }

    // Find mode again, in case app is closed then reopened
    if (Serial.available() > 0) {
        char receivedChar = Serial.read();
        // FingerGet signal
        if (receivedChar == 'F') {
            Serial.println("Internal found");
            isVote = false;
        }
        // VotingPC signal
        else if (receivedChar == 'V') {
            Serial.print('K');
            isVote = true;
        }
    }
}
