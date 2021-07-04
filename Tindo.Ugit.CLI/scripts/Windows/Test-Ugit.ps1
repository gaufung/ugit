if (Test-Path "test")
{
    Remove-Item -Recurse -Force "test"
}
New-Item "test" -ItemType Directory -Force
$executeDirectory = (Get-Item (Get-Location).Path).Parent.Parent.FullName
$env:Path += ";$executeDirectory"
Set-Location "test"

ugit init
ugit status

Copy-Item "../../data/hello.txt" ./

ugit add "hello.txt"
ugit commit -m "first commit"
ugit log
ugit branch

New-Item "sub" -ItemType Directory -Force
Copy-Item "../../data/ugit.txt" "sub/"

ugit status
ugit add "sub"
ugit commit -m "second commit"
ugit status
ugit log

ugit tag v1.0 
ugit log


ugit branch dev
ugit checkout dev
ugit branch

Copy-Item "../../data/dev.md" "./"
ugit status
ugit add "dev.md"
ugit commit -m "this is dev commit"
ugit status
ugit log

ugit checkout master
ugit merge dev
ugit log
ugit tag v2.0
ugit tag

Write-Host "Done"
Set-Location "../"