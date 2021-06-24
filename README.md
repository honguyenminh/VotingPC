﻿# VotingPC
[English](#english)
Bộ ứng dụng bầu cử điện tử VotingPC để tích hợp với Adafruit Fingerprint Reader.
Ứng dụng được xây dựng đặc biệt để chạy với một mạch custom dựa trên cấu trúc Arduino (ATMega328P).

Nói ngắn gọn là, ông cần một con mạch như thế để xài app :)
## Install guide / Hướng dẫn cài đặt
- Qua tab release bên phải, tải phiên bản mới nhất về
- Giải nén file zip vừa tải
- Mở file `Setup.msi` lên và làm theo hướng dẫn cài đặt
- Nếu báo lỗi, cài Visual C++ trong thư mục `vcredist` trước
- Sau khi cài xong, biểu tượng 2 ứng dụng sẽ xuất hiện trên Desktop


## Lưu ý
Với board Arduino Uno R3 chính hãng sẽ không chạy ổn định, nhất là trên Windows 8.1. Để tránh lỗi vui lòng không rút dây USB trong quá trình ứng dụng chạy.
Board clone sử dụng CH340G sẽ ổn định hơn, nhưng yêu cầu cài driver.
Trường hợp sử dụng clone CH340G vui lòng cài driver `CH341SER.EXE` trong thư mục `Optional`.

Nếu sử dụng Arduino chính hãng nhưng máy không nhận, vui lòng chạy `dpinst-XXX.exe` với XXX tương ứng bitrate của hệ điều hành.
64-bit tương ứng với `amd64`, và 32-bit tương ứng với `x86`.

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
- Download latest version from there
- Extract downloaded file
- Open `Setup.msi`, then follow the instructions on the installer
- If errors were thrown, install Visual C++ inside `vcredist` folder first
- After the installation has completed, app icons should appear on your Desktop
## Other stuff
No one's gonna read this english section anyway, if you need to, copy the vietnamese section to Google Translate. Or contact me on Discord (`Someone#9554`). I'm willing to help.
