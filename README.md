# VotingPC - DEPRECATED
*So, hello there.*  
It's me, *honguyenminh*. Or as my nickname goes, you may know me as "Someone".

As of 20:30 19/01/2022 +07 UTC, **I am deprecating this project**, and also **archiving this repo**.
 
I feel like I've accomplished what I wanted to achieve with this, and finished what I set out to do originally.
I learned how a database works, how to do data bindings, validation, JSON config parsing, error handling,...  
And overall, I got to know C# a little bit better. As you can guess, it was a very nice learning experience.

Can this project be applied and used IRL? Nah. But does it matters to me? Totally, of course.  
I believe every step on this journey is important, and this is truly a step forward. I do not regret any moment doing this at all, and now I can go there, saying proudly that we made a voting system that works with an Arduino, and made it public for everyone to see and use.

**Thank you, to my team @lehoangtrong and @tvqkcsnbk, and also to everyone who worked with me on this.**
From debugging, testing to design advices, etc. I can't do everything by myself. You guys made this possible, and I cannot express my gratitude enough.

To everyone else who just happened upon this niche little project of us, I hope you enjoy your stay here.

Til the day we meet again. さよなら。  
           *- honguyenminh -*

[Pictures](#project-images)

# OLD Readme
[English](##english)

Bộ ứng dụng bầu cử điện tử VotingPC để tích hợp với một mạch máy quét vân tay.
Ứng dụng được xây dựng đặc biệt để chạy với một mạch custom dựa trên cấu trúc Arduino.

Nói ngắn gọn là, ông cần một con mạch như thế để xài app :)

## Hướng dẫn cài đặt
- Qua tab release bên phía tay phải trên trang này
- Tải phiên bản mới nhất về, file cài có tên `VotingPC-vX.X.X.exe`
- Mở file đã tải về và làm theo hướng dẫn cài đặt trên màn hình
- Sau khi cài xong, biểu tượng các ứng dụng sẽ xuất hiện trên Desktop


## Lưu ý
Với board Arduino Uno R3 chính hãng sẽ không chạy ổn định, nhất là trên Windows 8.1 (nghe vô lý không?). Để tránh lỗi vui lòng không rút dây USB trong quá trình ứng dụng chạy. Chỉ rút sau khi đã hoàn tất công việc trên ứng dụng. Tất nhiên rồi.
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

 

## English
Voting PC App bundle to integrate with Arduino Fingerprint Reader
This is a specialized app, made just to work with a custom Arduino-based hardware.

TL;DR: You need one like that to open this app :)

**Warning**, the UI is in Vietnamese, don't bother reading this repo if you don't know anything about that language.
### Install guide
- Go to `release` tab on the right of this page
- Download latest version, named `VotingPC-vX.X.X.exe` from there
- Open downloaded file, then follow the instructions on the installer
- After the installation has completed, app icons should appear on your Desktop
### Other stuff
No one's gonna read this english section anyway, if you need to, copy the vietnamese section to Google Translate. Or contact me on Discord (`Someone#9954`). I'm willing to help.


# Project images
- VotingPC
![MainUI with dialog](https://github.com/honguyenminh/VotingPC/raw/master/Images/main%20ui.png)
![Page 1 - Welcome](https://github.com/honguyenminh/VotingPC/raw/master/Images/votingpc.png)
![Page 2 - Vote](https://github.com/honguyenminh/VotingPC/raw/master/Images/votingpc2.png)
- DbMaker
![DbMaker](https://github.com/honguyenminh/VotingPC/raw/master/Images/dbmaker.jpg)
- VoteCounter
![Page 1 - Overall](https://github.com/honguyenminh/VotingPC/raw/master/Images/counter1.png)
![Page 2 - Details](https://github.com/honguyenminh/VotingPC/raw/master/Images/counter2.png)
- Fingerprint Scanner
![Scanner](https://github.com/honguyenminh/VotingPC/raw/master/Images/scanner.jpg)
