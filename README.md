# VotingPC
[English](#english)

Bộ ứng dụng bầu cử điện tử VotingPC để tích hợp với Adafruit Fingerprint Reader.
Ứng dụng được xây dựng đặc biệt để chạy với một mạch custom dựa trên cấu trúc Arduino (ATMega328P).

Nói ngắn gọn là, ông cần một con mạch như thế để xài app :)

# Hướng dẫn cài đặt
- Qua tab release bên phía tay phải trên trang này
- Tải phiên bản mới nhất về, file cài có tên `VotingPC-vX.X.X.exe`
- Mở file đã tải về và làm theo hướng dẫn cài đặt trên màn hình
- Sau khi cài xong, biểu tượng 2 ứng dụng sẽ xuất hiện trên Desktop


# Lưu ý
Với board Arduino Uno R3 chính hãng sẽ không chạy ổn định, nhất là trên Windows 8.1. Để tránh lỗi vui lòng không rút dây USB trong quá trình ứng dụng chạy. Chỉ rút sau khi đã hoàn tất công việc trên ứng dụng.
Board clone sử dụng CH340G sẽ ổn định hơn, nhưng yêu cầu cài driver.
Trường hợp sử dụng clone CH340G vui lòng chọn cài `CH34X Driver` khi cài ứng dụng, rồi ấn nút Install trong ô cửa sổ hiện lên.

Nếu sử dụng Arduino chính hãng nhưng máy không nhận, vui lòng chọn cài `Arduino Driver`, và làm theo hướng dẫn trong ô cửa sổ hiện lên.

## Cách tạo board Arduino
### Nối mạch
- Nối dây đỏ và đen từ cảm biến Adafruit R307 vào +5V và GND của Arduino.
- Nối dây 3, 4 (thường là vàng, trắng) vào cổng 2, 3 của Arduino.
- Sau khi nối, sẽ có mạch tương tự hình
![Schema](https://github.com/honguyenminh/VotingPC/raw/master/Images/schema.png)
### Upload sketch
- GitHub repo này có chứa đầy đủ tất cả code, trong đó có sketch để Arduino làm việc
- Cài thư viện Adafruit Fingerprint Sensor tại [đây](https://github.com/adafruit/Adafruit-Fingerprint-Sensor-Library/releases)
- Tải thư mục Arduino/production trong GitHub repo này về máy. Có thể clone repo về hoặc tìm file thẳng trên web
- Mở file `production.ino` bằng Arduino IDE, rồi upload Sketch vào board

 

# English
Voting PC App bundle to integrate with Arduino Fingerprint Reader
This is a specialized app, made just to work with a custom Arduino-based hardware.

TL;DR: You need one like that to open this app :)

**Warning**, the UI is in Vietnamese, don't bother reading this repo if you don't know anything about that language.
## Install guide
- Go to `release` tab on the right of this page
- Download latest version, named `VotingPC-vX.X.X.exe` from there
- Open downloaded file, then follow the instructions on the installer
- After the installation has completed, app icons should appear on your Desktop
## Other stuff
No one's gonna read this english section anyway, if you need to, copy the vietnamese section to Google Translate. Or contact me on Discord (`Someone#9554`). I'm willing to help.
