If (Test-Path "test")
{
	Remove-Item -Recurse -Force "test"
}

New-Item "test" -ItemType Directory -Force 

Set-Location "test"

dotnet ../ugit.dll init

dotnet ../ugit.dll status

Copy-Item "../data/hello.txt" "./"

dotnet ../ugit.dll hash-object "hello.txt"

dotnet ../ugit.dll cat-file 0a6649a0077da1bf5a8b3b5dd3ea733ea6a81938



New-Item "sub" -ItemType Directory -Force

Copy-Item "../data/sub/ugit.txt" "./sub/"

dotnet ../ugit.dll write-tree

dotnet ../ugit.dll read-tree 62b3608c7fe2f7dfe03c86b70b02946ac9042550

dotnet ../ugit.dll commit -m "Hello first"

dotnet ../ugit.dll status

Copy-Item "../data/next.txt" "./"

dotnet ../ugit.dll commit -m "Hello second"

dotnet ../ugit.dll log

dotnet ../ugit.dll tag v1.0

dotnet ../ugit.dll log refs/tags/v1.0

dotnet ../ugit.dll checkout fcf448a43cb07899ad1db21ba3b3e31a0386b2ef

dotnet ../ugit.dll checkout refs/tags/v1.0

dotnet ../ugit.dll k

dotnet ../ugit.dll branch dev

dotnet ../ugit.dll branch

dotnet ../ugit.dll reset fcf448a43cb07899ad1db21ba3b3e31a0386b2ef

dotnet ../ugit.dll show

dotnet ../ugit.dll diff

Set-Location "../"

Write-Host "Done"
