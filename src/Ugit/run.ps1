If (Test-Path "test")
{
	Remove-Item -Recurse -Force "test"
}

New-Item "test" -ItemType Directory -Force 

Set-Location "test"

dotnet ../ugit.dll init

"Hello World" | Out-File "hello.txt" -Encoding UTF8

dotnet ../ugit.dll hash-object "hello.txt"

dotnet ../ugit.dll cat-file c7bd5be26845954f7a0e7b4a3fc77340b8d90c3c

New-Item "sub" -ItemType Directory -Force

"Hello Ugit" | Out-File "sub/ugit.txt"

dotnet ../ugit.dll write-tree

Write-Host "Done"

Set-Location "../"