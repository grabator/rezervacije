# Dnevni bekap SQLite baze - lokalna/Windows verzija istog skripta kao backup-db.sh.
# Za automatsko pokretanje: Windows Task Scheduler -> Create Task -> Trigger: Daily ->
# Action: powershell.exe -File "C:\putanja\do\backup-db.ps1"
#
# Kopira rezervacije.db u backups\ sa datumom u imenu i briše bekape starije od 30 dana.

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$DbPath = Join-Path $ScriptDir "..\Rezervacije.Api\rezervacije.db"
$BackupDir = Join-Path $ScriptDir "..\backups"
$KeepDays = 30

if (-not (Test-Path $DbPath)) {
    Write-Error "Baza ne postoji: $DbPath"
    exit 1
}

if (-not (Test-Path $BackupDir)) {
    New-Item -ItemType Directory -Path $BackupDir | Out-Null
}

$Timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$Dest = Join-Path $BackupDir "rezervacije-$Timestamp.db"

Copy-Item -Path $DbPath -Destination $Dest
Write-Output "Bekap sacuvan: $Dest"

Get-ChildItem -Path $BackupDir -Filter "rezervacije-*.db" |
    Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-$KeepDays) } |
    Remove-Item -Force
