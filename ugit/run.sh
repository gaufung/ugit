if [ -d "test" ]
then
    rm -rf "test"
fi

mkdir "test"
cd "test"

echo "<----- init ---->"
dotnet "../ugit.dll" init

echo "<----- status ---->"
doent "../ugit.dll" status

echo "<----- hash object ---->"
echo "Hello" > hello.txt
dotnet "../ugit.dll" hash-object hello.txt

echo "<----- cat file ---->"
dotnet "../ugit.dll" cat-file "33c773c9fd51fbfc934a1d455166abb855e9b879"

mkdir sub
echo "World" > sub/world.txt 

echo "<----- write tree ---->"
dotnet "../ugit.dll" write-tree 

echo "<----- read tree ---->"
dotnet "../ugit.dll" read-tree "58ed9cdfca46878216b4527ff0d38def070d1e4c"

echo "<----- commit  ---->"
dotnet "../ugit.dll" commit -m "This is commit"
dotnet "../ugit.dll" cat-file "9a1931a6af4ca6d26b149f83920accc54532d5fc"

echo "<----- HEAD  ---->"
cat ".ugit/HEAD"
echo ""
echo "<----- second commit  ---->"
echo "ugit" > ugit.txt
dotnet "../ugit.dll" commit -m "This is second commit"
dotnet "../ugit.dll" cat-file "4d9e0706325e7235461a15f63c362dfb0f174455"

cat ".ugit/HEAD"
echo ""

echo "<----- log  ---->"
dotnet "../ugit.dll" log

echo "<----- previous log  ---->"
dotnet "../ugit.dll" log "9a1931a6af4ca6d26b149f83920accc54532d5fc"

echo "<----- checkout   ---->"
dotnet "../ugit.dll" checkout "9a1931a6af4ca6d26b149f83920accc54532d5fc"
cat ".ugit/HEAD"
echo ""

echo "<----- create tag   ---->"
dotnet "../ugit.dll" tag "myTag" "4d9e0706325e7235461a15f63c362dfb0f174455"
cat ".ugit/refs/tags/myTag"
echo ""

echo "<----- log by tag   ---->"
dotnet "../ugit.dll" log "refs/tags/myTag"

echo "<----- log by tag   ---->"
dotnet "../ugit.dll" log "myTag"

echo "<----- k   ---->"
dotnet "../ugit.dll" k

echo "<----- branch ---->"
dotnet "../ugit.dll" branch "myBranch" "4d9e0706325e7235461a15f63c362dfb0f174455"

echo "<----- check branch ---->"

dotnet "../ugit.dll" checkout "myBranch"


echo "<----- status ---->"
dotnet "../ugit.dll" status

echo "<----- branch ---->"
dotnet "../ugit.dll" branch

echo "<----- log  ---->"
dotnet "../ugit.dll" log

echo "<----- reset ---->"
dotnet "../ugit.dll" reset "4d9e0706325e7235461a15f63c362dfb0f174455"
echo "<----- log(reset)  ---->"
dotnet "../ugit.dll" log