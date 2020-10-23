If (Test-Path "test")
{
	Remove-Item -Recurse -Force "test"
}

New-Item "test" -ItemType Directory -Force 

Set-Location "test"

dotnet ../ugit.dll init

dotnet ../ugit.dll status

"Hello World" | Out-File "hello.txt" -Encoding UTF8

dotnet ../ugit.dll hash-object "hello.txt"

dotnet ../ugit.dll cat-file c7bd5be26845954f7a0e7b4a3fc77340b8d90c3c

New-Item "sub" -ItemType Directory -Force

"Hello Ugit" | Out-File "sub/ugit.txt"

dotnet ../ugit.dll write-tree

dotnet ../ugit.dll read-tree 56d892c161a818fb4163ee233bd5aa1fe99e9f9d

dotnet ../ugit.dll commit -m "Hello World"

dotnet ../ugit.dll status

"Hello Next" | Out-File "next.txt"

dotnet ../ugit.dll commit -m "Hello next"

dotnet ../ugit.dll log

dotnet ../ugit.dll tag v1.0

dotnet ../ugit.dll log refs/tags/v1.0

dotnet ../ugit.dll checkout 5ef1ea5b6f1342f6d79d9109216598af8cf04621

dotnet ../ugit.dll checkout refs/tags/v1.0

dotnet ../ugit.dll k

dotnet ../ugit.dll branch dev

dotnet ../ugit.dll branch

dotnet ../ugit.dll reset 5ef1ea5b6f1342f6d79d9109216598af8cf04621

Set-Location "../"

Write-Host "Done"