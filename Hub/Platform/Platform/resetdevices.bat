:: check for administrator rights
net session >nul 2>&1
if NOT %errorLevel% == 0 ( 
   echo You must run with adminstrator privileges
   exit /B 1
)

:: what config directory are we working with?
set configDir=%1

:: stop WatchDog service if it is running
net stop "HomeOS Hub Watchdog"

:: kill homeos if it is already running
taskkill /F /IM HomeOS.Hub.Platform.exe

:: save the settings file
move ..\\..\\Configs\\%configDir%\\Settings.xml Settings.xml.orig

:: nuke the current config
del /Q ..\\..\\Configs\\%configDir%\\*

:: stop hosted network
:: This line seems to cause delay when the network is restarting
::netsh wlan stop hostednetwork

:: copy over the fresh config
copy ..\\..\\..\\Platform\\Configs\\%configDir%\\* ..\\..\\Configs\\%configDir%

:: rejuvenate the settings file
move Settings.xml.orig ..\\..\\Configs\\%configDir%\\Settings.xml

:: make it writeable
:: attrib -r ..\\..\\Configs\\%configDir%\\*