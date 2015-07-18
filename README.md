# OhMyDanmaku
A Desktop Danmaku System based on WPF

### Document
##### What's this
This is a program for showing danmaku on desktop directly.

Based on WPF.And only support Windows.

##### How to Send Danmaku

OhMyDanmaku will listen on `localhost:8585` using `UDP` protocol by default.

Just send string data in UTF-8 to listener, and the danmaku will show up

Also, you can use reverse proxy to handle the communication, there is a sample in /src/WebProxySample/index.php

##### Custom Parts
You can custom the danmaku style(color,size,time,etc..) and communication port in setting menu very easily.

Or just modify the code, lots of parameters reserved in order to make code modify easier.

##### If any idea come out
Welcome Pull requests and Issues.

##### License
GPL v2
