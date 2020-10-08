If (Test-Path "test")
{
	Remove-Item -Recurse -Force "test"
}

New-Item "test" -ItemType Directory -Force 

Set-Location "test"

dotnet ../ugit.dll init

"Hello World" | Out-File "hello.txt" -Encoding UTF8

dotnet ../ugit.dll hash-object "hello.txt"

dotnet ../ugit.dll cat-file 640eca3c6afadf19c3fe7c8595075d99a218ee61

Write-Host "Done"

Set-Location "../"