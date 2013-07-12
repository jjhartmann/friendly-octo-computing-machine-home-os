
ECHO did you change the HomeId?

taskkill /F /IM HomeService.exe

START .\HomeService.exe -c EmptyConfig

.\Platform.exe -c EmptyConfig -g -l homeos.log
