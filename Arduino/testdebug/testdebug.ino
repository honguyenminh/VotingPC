// For debugging only
// Someone#9554
// v0.2.2

#define BUTTON_PIN 2

uint8_t id = 0;
bool isVote, fingerFound = false; // Current mode

void setup() {
    pinMode(BUTTON_PIN, INPUT);
    pinMode(LED_BUILTIN, OUTPUT);
    // Start serial at 115200 baud rate
    Serial.begin(115200);
}


void loop() {
    // Find mode
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
    
    // VotingPC mode
    while (isVote) {
        if (Serial.available() > 0) {
            char receivedChar = Serial.read();
            // Next fingerprint aka resume reading
            if (receivedChar == 'N') {
                // Emulate read fingerprint with button
                while (digitalRead(BUTTON_PIN) == HIGH) delay(100);
        
                Serial.print('D'); // Send this when finger found
                fingerFound = true;
            }
            // Delete fingerprint
            else if (receivedChar == 'X') {
                if (fingerFound) {
                    // Blink led to emulate delete fingerprint
                    digitalWrite(LED_BUILTIN, HIGH);
                    delay(200);
                    digitalWrite(LED_BUILTIN, LOW);
                    delay(200);
                    digitalWrite(LED_BUILTIN, HIGH);
                    delay(200);
                    digitalWrite(LED_BUILTIN, LOW);
                    delay(200);
                    digitalWrite(LED_BUILTIN, HIGH);
                    delay(200);
                    digitalWrite(LED_BUILTIN, LOW);
                    delay(200);
                    digitalWrite(LED_BUILTIN, HIGH);
                    delay(200);
                    digitalWrite(LED_BUILTIN, LOW);
                    fingerFound = false;
                }
            }
            // App closed
            else if (receivedChar == 'C') {
                fingerFound = false;
                return;
            }
            // Mode change (should not be run though, just here to be extra safe)
            else if (receivedChar == 'F') {
                Serial.println("Internal found");
                isVote = false;
                fingerFound = false;
                break;
            }
            else if (receivedChar == 'V') {
                Serial.print('K');
                isVote = true;
                fingerFound = false;
            }
        }
    }
    
    // FingerGet mode
    while (!isVote) {
        if (Serial.available() > 0) {
            char receivedChar = Serial.read();
            // Get fingerprint to memory
            if (receivedChar == 'S') {
                id++;
                // Replace this with generic waiting message if needed
                Serial.print("Đang chờ vân tay số "); Serial.println(id);
                // Emulate get fingerprint
                while (digitalRead(BUTTON_PIN) == HIGH) delay(100);
                // End session message
                Serial.println("Internal done");
            }
            // App closed
            else if (receivedChar == 'C') {
                break;
            }
            // Mode change (should not be run though, just here to be extra safe)
            else if (receivedChar == 'V') {
                Serial.print('K');
                isVote = true;
                break;
            }
            else if (receivedChar == 'F') {
                Serial.println("Internal found");
            }
        }
    }
}
