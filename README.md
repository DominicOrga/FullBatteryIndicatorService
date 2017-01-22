# FullBatteryIndicatorService
A windows desktop service with the purpose of displaying a toast notification at the desktop, informing the user that their device's battery level is at more than 90%.

Problems: This service works without any issues during debug mode. However, at release mode the toast notification cannot be displayed.
The problem is difficult to fix, since as it turns out, Windows does not allow services to launch toast notification to be displayed at the desktop due to security purposes.  
