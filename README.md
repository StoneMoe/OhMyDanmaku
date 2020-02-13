# OhMyDanmaku
A Simple Desktop Danmaku System

# Get Started
### Clone
```
git clone https://github.com/StoneMoe/OhMyDanmaku.git
cd OhMyDanmaku
git submodule init
git submodule update
```

### Build
OhMyDanmaku is based on WPF, which means you need .NET Framework 3.5 or higher.

### How to Send Danmaku
OhMyDanmaku listen on `localhost:8585` with `UDP` by default.

Send string data in **UTF-8** to listener, then a danmaku will across screen like a bullet

Also, you can use reverse proxy to handle the communication, there is a sample in folder `web`

# Customize
Danmaku style(color,size,time,etc..) and listen port can change in setting menu very easily

Or just modify the code for more :)

# Welcome Pull requests and Issues.

# License
GPL v2
