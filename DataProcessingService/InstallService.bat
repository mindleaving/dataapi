@echo OFF
echo Stopping old service version...
net stop "CH.RD.DataProcessingService"
echo Uninstalling old service version...
sc delete "CH.RD.DataProcessingService"

echo Installing service...
rem DO NOT remove the space after "binpath="!
sc create "CH.RD.DataProcessingService" binpath= "C:\DataProcessingService\DataProcessingService.exe" start= auto
sc description "CH.RD.DataProcessingService" "Data processing service for RD data objects"
echo Starting server complete
pause