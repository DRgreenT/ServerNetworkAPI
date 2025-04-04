# 💻 ServerNetworkAPI (v0.1b)

A minimalistic Web API that scans your local IPv4 network and provides information like IP, OS, and open ports of all reachable devices.

Currently built with **.NET 9.0**.

---

## Features

- Automatic network scan (IPv4 local)
- CLI arguments
- Logfile
- RESTful Web API (JSON output)
- Device & Port scanning via `nmap`
- OS recognition (basic detection implemented)
- Persistent device data (loaded on startup)
- HTML GUI template

---

## Preview

### Terminal Output (Linux)
<img src="./dev/docs/networkAPI_1.png" alt="Linux Output" width="800"/>

### Web Output (HTML Raw)
<img src="./dev/docs/networkAPI_2.png" alt="Web Output(json)" width="800"/>

### Web Output (HTML frontend example)
<img src="./dev/docs/networkAPI_3_FrontEnd.png" alt="Web Output(html)" width="800"/>

---

## How It Works

The application continuously scans the local network using ICMP (ping) to detect active devices.  
Once a device responds, it runs a detailed `nmap` scan to gather more info such as:

- Open ports
- Very basic OS fingerprinting
- Hostname

At shutdown, all scan results are saved to disk and restored at the next start.

---
## 🔧 Requirements

- Linux x64 System
- [.NET 9.0 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) (only needed if you build it your self)
- [nmap](https://nmap.org/) installed and accessible via command line (typically requires sudo privileges to run correctly)

## Run
You can download the **latest prebuilt Linux release** here:  
[Download from GitHub](https://github.com/DRgreenT/ServerNetworkAPI/blob/main/publish/linux.zip)

Simply copy the binary to your Linux machine, unpack and execute it:

```bash
chmod +x ServerNetworkAPI
./ServerNetworkAPI
```
To run the program [nmap] is mandator, you can check with ```nmap --version``` in case its not installed than:<br>
```sudo apt install nmap```    # Debian, Ubuntu, Raspberry Pi OS<br>
```sudo pacman -S nmap```      # Arch Linux, Manjaro<br>
```sudo dnf install nmap```    # Fedora<br>
```sudo zypper install nmap``` # openSUSE<br>

To access the API from another device in your local network, make sure port 5050 is open:<br>
```sudo ufw allow 5050```                                                              # Debian/Ubuntu (with UFW enabled)<br>
```sudo firewall-cmd --add-port=5050/tcp --permanent && sudo firewall-cmd --reload```  # Fedora/CentOS<br>
```sudo iptables -A INPUT -p tcp --dport 5050 -j ACCEPT```                             # Fallback (legacy systems)<br>

If you want to use the included [Downlaod Html template](https://github.com/DRgreenT/ServerNetworkAPI/blob/main/publish/zip/frontEndTemplate.zip) for visualizing the API data,
please make sure to adjust the IP address to match the device where your API is hosted in the following line (line 1) in the `./JS/script.js` file:

```var localHostIP = "192.168.178.10";```


### Available parameters:
       
`--help/-help`           Shows this help message<br>
`--t {int}`              Delay between loops in seconds (default: 5 min: 1 max:3600)<br>
`--p {int}`              Web API port (default: 5050)<br>
`--c {string}`           Controller name (default: 'network')<br>
`--nmap`                 Set nmap scan active (default ping only)<br>
`--fip {string}`         Default fallback IP mask 192.168.178.<br> 

---

## Build (Net9.0 SDK required)

```git clone https://github.com/DRgreenT/ServerNetworkAPI.git```<br>
```cd ServerNetworkAPI```<br>
```dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true```<br>

---

## Disclaimer

This project is intended for **educational and local testing purposes only**.  
Please use `nmap` **only in networks where you have explicit authorization**.  
The developer takes **no responsibility** for any damage, malfunctions, or legal consequences resulting from improper use.


