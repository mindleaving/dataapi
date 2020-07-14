$acl = Get-Acl "C:\DataProcessingService"
$aclRuleArgs = "{DOMAIN OR COMPUTER NAME\USER}", "Read,Write,ReadAndExecute", "ContainerInherit,ObjectInherit", "None", "Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule($aclRuleArgs)
$acl.SetAccessRule($accessRule)
$acl | Set-Acl "C:\DataProcessingService"

New-Service `
-Name DataProcessing `
-BinaryPathName C:\DataProcessingService\DataProcessingService.Web.dll `
-Credential {DOMAIN OR COMPUTER NAME\USER} `
-Description "Automated processing of DataAPI data and scheduled task runner" `
-DisplayName "DataProcessingService" `
-StartupType Automatic