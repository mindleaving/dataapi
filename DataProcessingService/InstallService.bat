@echo OFF
echo Stopping old service version...
net stop "DataProcessingService"
echo Uninstalling old service version...
sc delete "DataProcessingService"

echo Installing service...
rem DO NOT remove the space after "binpath="!
sc create "DataProcessingService" binpath= "C:\DataProcessingService\DataProcessingService.exe" start= auto
sc description "DataProcessingService" "Data processing service for DataAPI objects"
echo Starting server complete
pause