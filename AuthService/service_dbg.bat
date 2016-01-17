SET NETROOT=%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319

%NETROOT%\installutil AuthService.exe

net start AuthService
pause
net stop AuthService

%NETROOT%\installutil /u AuthService.exe