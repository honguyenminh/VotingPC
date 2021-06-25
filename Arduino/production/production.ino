// Refactoring the original code is pure pain
// Someone#9554

// TODO: add mechanism to stop receiving fingerprint until done voting

#include <Adafruit_Fingerprint.h>

SoftwareSerial softSerial(2, 3);
Adafruit_Fingerprint fingerReader = Adafruit_Fingerprint(&softSerial);

uint8_t id, tam;
bool isVote; // Current mode

void setup() {
    // Start serial at 115200 baud rate
    Serial.begin(115200);
    fingerReader.begin(57600);
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
        // Read fingerprint
        if (fingerReader.getImage() != FINGERPRINT_OK) return;
        if (fingerReader.image2Tz() != FINGERPRINT_OK) return;
        if (fingerReader.fingerFastSearch() != FINGERPRINT_OK) return;

        Serial.print('D'); // Send this when finger found

        while (true) {
            if (Serial.available() > 0) {
                char receivedChar = Serial.read();
                if (receivedChar == 'X') {
                    // Delete fingerprint
                    fingerReader.deleteModel(fingerReader.fingerID);
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
                    // Read fingerprint
                    fingerReader.getTemplateCount();
                    id = fingerReader.templateCount + 1;
                    if (id != tam)
                    {
                        tam = id;
                        Serial.println("Đang chờ xử lí");
                        delay(1000);
                    }
                    Serial.print("Đang chờ vân tay số "); Serial.println(id);
                    GetFingerprintEnroll();
                    Serial.println("Internal done");
                    break;
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
    Serial.print("ID "); Serial.println(id);
    
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
