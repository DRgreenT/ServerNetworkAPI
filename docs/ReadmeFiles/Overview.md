# Why This Tool Exists

Since I´m learning programming C#, Linux handling and networking, I wanted to create a simple tool that helps me understand how devices communicate on a local network.
I also wanted to learn how to create a **Web API** and how to use **webhooks** for notifications.

So I developed this tool to monitor all active devices in my **private network**, specifically on a **headless Linux server**. 
I wanted an always-on **local Web API** that gives me live access to all devices on the network – including:

- IP & Mac addresses
- Hostnames
- OS guesses
- Open ports

In my setup, my DHCP server assigns IPs **from `192.168.178.190` upwards** to any **new/unregistered device**.

Whenever this tool detects a new IP in that range, 
it sends me a **webhook notification** (e.g. to Discord), allowing me to check immediately:

- Is this a device I expect?
- Or is someone unauthorized on my network?

This lightweight scanner gives me **peace of mind** and an easy way to monitor
who joins my WiFi or wired LAN – from any device via the built-in Web API or push alert.

I know this isn't a perfect security solution — but it’s definitely better than nothing :D
And for me, it’s a great learning project to grow my understanding of networks, APIs, and server monitoring.

---

## Current Features

- Automatic local network scan using `arp-scan`
- Optional detailed device scan via `nmap`
- Basic OS recognition
- CLI argument support
- RESTful Web API (JSON output)
- Build in HTML WebUI
- Persistent device data between restarts
- Color-coded terminal output
- Offline detection & live status
- Logfile creation and api with logs 
- Statically assigned IP detection (highlighted)
- **Webhook notifications** for new devices and service status changes (started/terminated/server restart)
- **External Config Files**:
  - `NotificationConfig.json` for webhooks
  - `AppConfig.json` for persistent settings (overridable by CLI)

## Upcoming Features

- Mac address recognition
- Device type recognition (e.g. phone, computer, etc.)
- Improved OS recognition
- WebUI overhaul
- more robust code
- and some other stuff...