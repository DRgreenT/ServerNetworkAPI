# 💻 ServerNetworkAPI (v0.1a)

A minimalistic Web API that scans your local IPv4 network and provides information like IP, OS, and open ports of all reachable devices.

Currently built with **.NET 9.0**.

---

## Features

- Automatic network scan (IPv4 local)
- Logfile
- RESTful Web API (JSON output)
- Device & Port scanning via `nmap`
- OS recognition (basic detection implemented)
- Persistent device data (loaded on startup)

---

## Preview

### Terminal Output (Linux)
<img src="./docs/networkAPI_1.png" alt="Linux Output" width="800"/>

### Web Output (HTML)
<img src="./docs/networkAPI_2.png" alt="Web Output" width="800"/>

---

## How It Works

The application continuously scans the local network using ICMP (ping) to detect active devices.  
Once a device responds, it runs a detailed `nmap` scan to gather more info such as:

- Open TCP ports
- Basic OS fingerprinting
- Hostname

At shutdown, all scan results are saved to disk and restored at the next start.

---

## Run
You can download the **latest prebuilt Linux release** here:  
[Download from GitHub](https://github.com/DRgreenT/ServerNetworkAPI/blob/main/publish/linux.zip)

Simply copy the binary to your Linux machine, unpack and execute it:

```bash
chmod +x ServerNetworkAPI
./ServerNetworkAPI
```
To run the program [nmap] is mandator, you can check with nmap --version in case its not installed than:
sudo apt install nmap    # Debian, Ubuntu, Raspberry Pi OS
sudo pacman -S nmap      # Arch Linux, Manjaro
sudo dnf install nmap    # Fedora
sudo zypper install nmap # openSUSE

---

## Build (Net9.0 SDK required)
git clone https://github.com/DRgreenT/ServerNetworkAPI.git
cd ServerNetworkAPI
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true

