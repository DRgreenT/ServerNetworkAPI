# Table of Contents

- [Requirements](#requirements)
- [Dependencies](#dependencies)

- [Download & Installation](#download--installation)
  - [Required Packages](#required-packages)
  - [Sudo Rights for Headless Mode](#sudo-rights-for-headless-mode)
  - [Open the Web API Port](#open-the-web-api-port)
  - [Autostart with systemd (Headless Mode)](#autostart-with-systemd-headless-mode)
  - [Device Notifications via Webhook](#device-notifications-via-webhook)
  - [Application Configuration](#application-configuration)
  - [Available CLI parameters](#available-cli-parameters)
  - [WebUI](#webui)
	
- [Build Instructions](#build-instructions-requires-net-90-sdk)
- [Building and Deploying Tools](#building-and-deploying-tools)

- [Notes](#notes)


# Requirements

- root or sudo privileges (for `arp-scan` and `nmap` commands)
- Linux x64 system
- Web browser (for WebUI)

## Dependencies:

use: 
- [Dependencies installer](../../BuildAndDeployTools/setup_dependencies.sh)
- ```chmod +x setup-dependencies.sh```
- ```./setup-dependencies.sh```

or manual:
- [.NET 9.0 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) (only if building yourself)
- [.NET NuGet]```dotnet add package Microsoft.Extensions.Hosting``` (only if building yourself)
- [.NET NuGet]```dotnet add package Microsoft.AspNetCore.Mvc.Core``` (only if building yourself)
- [`nmap`](https://nmap.org/) (optional but recommended)
- [`arp-scan`](https://linux.die.net/man/1/arp-scan) – must be installed (`sudo apt install arp-scan`)

---

# Download & Installation

You can either:

- **Download the prebuilt binaries**:  
  [➡️ Latest Linux build (ZIP)](https://github.com/DRgreenT/ServerNetworkAPI/blob/main/publish/linux.zip)

- **Or build from source**:  
  Clone the repository and compile it yourself.

Once downloaded or built, make the binary executable and run it:

```bash
chmod +x ServerNetworkAPI
./ServerNetworkAPI
```

---

### Required Packages

To run the program, **`nmap`** and **`arp-scan`** must be installed:

```bash
sudo apt install nmap arp-scan     # Debian, Ubuntu, Raspberry Pi OS
sudo pacman -S nmap arp-scan       # Arch Linux, Manjaro
sudo dnf install nmap arp-scan     # Fedora
sudo zypper install nmap arp-scan  # openSUSE
```

---

### Sudo Rights for Headless Mode

By default, running `nmap` and `arp-scan` as a non-root user requires `sudo` and a password.  
This makes headless execution impossible, since there is no terminal available to enter the password.  

To solve this, grant **passwordless sudo rights** for these commands:  

```bash
sudo visudo -f /etc/sudoers.d/servernetworkapi
```

Add the following line (replace `thomas` with your username):

```
thomas ALL=(ALL) NOPASSWD: /usr/sbin/arp-scan, /usr/bin/nmap
```

Then secure the file:

```bash
sudo chmod 0440 /etc/sudoers.d/servernetworkapi
```

---

### Open the Web API Port

The Web API listens on port **5050** by default. Ensure it is open in your firewall:

```bash
sudo ufw allow 5050                                                        # Debian/Ubuntu with UFW
sudo firewall-cmd --add-port=5050/tcp --permanent && sudo firewall-cmd --reload  # Fedora/CentOS
sudo iptables -A INPUT -p tcp --dport 5050 -j ACCEPT                        # Generic fallback
```

---

### Autostart with systemd (Headless Mode)

To automatically run ServerNetworkAPI in headless mode after a reboot, create a systemd service file:

```bash
sudo nano /etc/systemd/system/servernetworkapi.service
```

Example configuration:

```ini
[Unit]
Description=ServerNetworkAPI Service
After=network.target

[Service]
Type=simple

#Replace the following with your data!
User=thomas
WorkingDirectory=/home/thomas/ServerNetworkAPI
ExecStart=/home/thomas/ServerNetworkAPI/ServerNetworkAPI --headless --nmap

Restart=no
# Set to "on-failure" if you want restart after app crashed
# Set to "always" if you want instant restart after killing the process

RestartSec=5

[Install]
WantedBy=multi-user.target
```

Then apply the changes:

```bash
# Reload systemd
sudo systemctl daemon-reload 

# Start the service
sudo systemctl start servernetworkapi

# Enable autostart on boot
sudo systemctl enable servernetworkapi
```


With this setup:  
- The program runs with required privileges in headless mode.  
- The Web API is accessible on port **5050**.  
- The service automatically starts after reboot.  

---

### Device Notifications via Webhook

Starting with version `v0.2.1b`, you can enable push-style notifications when a **new device** is detected on the network.

This is done using simple **webhooks**, such as:

- Discord Webhooks (custom channel alerts)
- [IFTTT Webhooks](https://ifttt.com/maker_webhooks) (e.g. push, email, SMS)
- Any URL that accepts JSON POST

#### Notifications config:

```
./Configs/NotificationConfig.json
```

```json
{
  "WebhookUrl": "https://discord.com/api/webhooks/your_webhook_here",
  "EnableNotifications": true,
  "NotificationLevel": "All"
}
```

> This file is created automatically on first run.

From now on, every time a **new device is detected**, a notification will be sent automatically - depending on your DHCP setup

#### Discord Notes

If you're using Discord and want push notifications on mobile:

- Use `@everyone` in your webhook message content
- Enable notifications for **all messages** in that channel (tap & hold → Notifications → All messages)
- Make sure push notifications are enabled in both **iOS settings** and **Discord app settings**

---

### Application Configuration

The runtime settings (like port, controller, timeout, etc.) are stored in:

```
./Configs/AppConfig.json
```

```json
{
  "FallbackIpMask": "192.168.178.",
  "ScanIntervalSeconds": 15,
  "IsNmapEnabled": true,
  "WebApiPort": 5050,
  "MaxIPv4AddressWithoutWarning": 190
}
```
```MaxIPv4AddressWithoutWarning``` This is the trigger value for the alert. If the IP count exceeds this threshold, it will be logged and, if configured, a notification will be sent.

### CLI arguments (e.g. `--t`, `--nmap`, etc.) override these values at runtime without modifying the file.

---

### Available CLI parameters:

```bash
--help / -help           Show help and usage info
--t {int}                Scan interval in seconds (default: 5, min: 1, max: 3600)
--p {int}                Web API port (default: 5050)
--nmap                   Enable nmap scanning (optional)
--fip {string}           Fallback IP mask (default: 192.168.178.)
```

---
### WebUI

Since this program includes a built-in Kestrel web server that serves both the API and the WebUI, you don’t need to install an additional (HTTP) web server to access all features.

The WebUI is accessible via the Web API at the IP and port of the local host where the tool is running: e.g.`http:\\192.168.178.10:5050` via web browser.

In a standard Visual Studio 2022 build, the WebUI is not included by default.  
You can either manually copy the files from the **`dev\WebUI`** folder into **`publish\wwwRoot`**,  
or use the **`copyWebUI.bat`** script located in **`BuildAndDeployTools`** (see *Building and Deploying Tools* below).  


---

## Build Instructions (Requires .NET 9.0 SDK)

```bash
git clone https://github.com/DRgreenT/ServerNetworkAPI.git
cd ServerNetworkAPI
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
```


# Building and Deploying Tools

The repository includes a folder named **`BuildAndDeployTools`**, which is primarily useful if you plan to work on this project in a Windows environment.  
It contains the following files:  

- **`build_linux.bat`** → Deletes the old build files (including the WebUI) and rebuilds the project into the `publish` folder of the repository *without* the WebUI. Afterward, it starts **`FTP_Deploy_Client.exe`** to update and restart the program on your server.  
- **`copyWebUI.bat`** → Copies the current content of the WebUI folder from `dev/WebUI` into `publish/wwwRoot`.  
- **`build_linux_publish.bat`** → Same as `build_linux.bat`, but also copies the WebUI and creates a `.zip` archive in the `publish/zip` folder.  
- **`FTP_Deploy_Client.exe`** → [More Info](https://github.com/DRgreenT/FTP_Deploy_Client)

You can also deploy the project to your server by using FileZilla or a similar SFTP/FTP client.  
Simply upload the contents of the **`ServerNetworkAPI\publish\linux`** folder into the `ServerNetworkAPI` directory on your server.  


---


# Notes:

## Important Notice for DNS Setup

If you are using a custom DNS server (e.g., AdGuard Home, Pi-hole, Unbound, etc.),
make sure that Reverse DNS (PTR) lookups for your local network are correctly configured.

Specifically:

Private Reverse DNS Resolver must be active.

The DNS server must be able to resolve PTR queries (reverse lookups) for local IP addresses.

Otherwise, no hostnames will be available — only IP addresses will be shown.

Example:
In AdGuard Home, enable the option "Private Reverse DNS Resolver" under DNS settings.