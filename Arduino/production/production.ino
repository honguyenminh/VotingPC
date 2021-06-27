// Refactoring the original code is pure pain
// Someone#9554
// v0.2.2

#include <Adafruit_Fingerprint.h>

SoftwareSerial softSerial(2, 3);
Adafruit_Fingerprint fingerReader = Adafruit_Fingerprint(&softSerial);

uint8_t id;
bool isVote, fingerFound = false; // Current mode

void setup() {
    // Start serial at 115200 baud rate
    Serial.begin(115200);
    fingerReader.begin(57600);
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
                // Read fingerprint
                if (fingerReader.getImage() != FINGERPRINT_OK) continue;
                if (fingerReader.image2Tz() != FINGERPRINT_OK) continue;
                if (fingerReader.fingerFastSearch() != FINGERPRINT_OK) continue;
        
                Serial.print('D'); // Send this when finger found
                fingerFound = true;
            }
            // Delete fingerprint
            else if (receivedChar == 'X') {
                if (fingerFound) {
                    fingerReader.deleteModel(fingerReader.fingerID);
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
                // Get total fingerprint already in memory
                // Remove if not needed to show
                fingerReader.getTemplateCount();
                id = fingerReader.templateCount + 1;
                // Replace this with generic waiting message if needed
                Serial.print("Đang chờ vân tay số "); Serial.println(id);
                // Get fingerprint
                GetFingerprintEnroll();
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


uint8_t GetFingerprintEnroll() {
    int p = -1;
    Serial.println("Đặt tay lên cảm biến.");
    while (p != FINGERPRINT_OK)
    {
        p = fingerReader.getImage();
    }
    
    p = fingerReader.image2Tz(1);
    Serial.println("Lấy tay ra khỏi cảm biến.");
    p = 0;
    while (p != FINGERPRINT_NOFINGER)
    {
        p = fingerReader.getImage();
    }
    // Serial.print("ID "); Serial.println(id);
    
    p = -1;
    Serial.println("Đặt tay lên cảm biến lần nữa.");
    while (p != FINGERPRINT_OK)
    {
        p = fingerReader.getImage();
    }
    
    p = fingerReader.image2Tz(2);
    Serial.print("Đang tạo mô hình #"); Serial.println(id);

    p = fingerReader.createModel();
    switch (p) {
        case FINGERPRINT_OK:
            Serial.println("Trùng khớp");
            break;
        case FINGERPRINT_PACKETRECIEVEERR:
            Serial.println("Lỗi nhận gói vân tay");
            return p;
        case FINGERPRINT_ENROLLMISMATCH:
            Serial.println("Không trùng khớp");
            return p;
        default:
            Serial.println("Không rõ lỗi");
            return p;
    }


    Serial.print("ID "); Serial.println(id);
    // Save model to memory
    p = fingerReader.storeModel(id);
    switch (p) {
        case FINGERPRINT_OK:
            Serial.println("Đã hoàn tất.");
            break;
        case FINGERPRINT_PACKETRECIEVEERR:
            Serial.println("Lỗi giao tiếp");
            return p;
        case FINGERPRINT_BADLOCATION:
            Serial.println("Không thể lưu trữ");
            return p;
        case FINGERPRINT_FLASHERR:
            Serial.println("Lỗi viết bộ nhớ");
            return p;
        default:
            Serial.println("Lỗi");
            return p;
    }
    return p;
}
