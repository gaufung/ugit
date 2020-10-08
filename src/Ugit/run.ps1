If (Test-Path "test")
{
	Remove-Item -Recurse -Force "test"
}

New-Item "test" -ItemType Directory -Force 

Set-Location "test"

dotnet ../ugit.dll init

"Hello" | Out-File "hello.txt"

dotnet ../ugit.dll hash-object "hello.txt"


Write-Host "Done"

Set-Location "../"