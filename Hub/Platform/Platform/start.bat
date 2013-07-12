:: check for administrator rights
net session >nul 2>&1
if NOT %errorLevel% == 0 ( 
   echo You must run with adminstrator privileges
   exit /B 1
)

:: what config directory are we working with?
::set configDir=%1
set configDir=..\..\Configs\Config

:: set path to the Hub output folder
set outputPath=%~dp0

:: stop WatchDog service if it is running
net stop "HomeOS Hub Watchdog"

:: kill homeos if it is already running
taskkill /F /IM HomeOS.Hub.Platform.exe

:: remove current firewall settings for HomeOS.Hub.Platform.exe
netsh advfirewall firewall delete rule name="Hub Platform" dir=in program=%outputPath%HomeOS.Hub.Platform.exe profile=private,public localip=any remoteip=any protocol=TCP localport=any remoteport=any
netsh advfirewall firewall delete rule name="Hub Platform" dir=in program=%outputPath%HomeOS.Hub.Platform.exe profile=private,public localip=any remoteip=any protocol=UDP localport=any remoteport=any

:: create firewall settings for HomeOS.Hub.Platform.exe
netsh advfirewall firewall add rule name="Hub Platform" dir=in program=%outputPath%HomeOS.Hub.Platform.exe action=allow enable=yes profile=private,public localip=any remoteip=any protocol=TCP localport=any remoteport=any
netsh advfirewall firewall add rule name="Hub Platform" dir=in program=%outputPath%HomeOS.Hub.Platform.exe action=allow enable=yes profile=private,public localip=any remoteip=any protocol=UDP localport=any remoteport=any 

:: start the platform
HomeOS.Hub.Platform.exe -c %configDir% -l :stdout