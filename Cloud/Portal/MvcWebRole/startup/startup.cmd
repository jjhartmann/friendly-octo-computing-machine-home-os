%windir%\system32\inetsrv\AppCmd.exe unlock config -section:system.webServer/security/ipSecurity
%APPCMD% set config -section:system.webServer/httpErrors -errorMode:Detailed
