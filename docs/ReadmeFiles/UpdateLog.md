### v0.2.7c
(19.09.2025)

- **Documentation:**  
  - `Readme.md` has been split into multiple files for improved structure and readability.  
  - `Installation.md`: Added sections for *Sudo Rights for Headless Mode*, *Autostart with systemd (Headless Mode)*, and *Building and Deploying Tools*.  
  - `UpdateLog.md` has been added.  

- **Headless Mode:**  
  - Fixed an exception on startup caused by the logger trying to write to the terminal.  
  - The program no longer automatically switches to headless mode; it must now be explicitly enabled.  

- **Webhook Notifications:**  
  - A notification is sent when the server restarts (only if the program autostarts after boot).  

- **Security:**  
  - Removed the ability to pass passwords via CLI arguments.  
  - The password handler has been removed.  

- **Code Quality:**  
  - Removed repetitive code snippets and improved maintainability.  
