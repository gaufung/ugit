if [ -d "test" ]
then
    rm -rf "test"
fi

mkdir "test"
cd "test"

dotnet "../ugit.dll" init

echo "Hello" > hello.txt
dotnet "../ugit.dll" hash-object hello.txt
# echo "${oid}"
#dotnet "../ugit.dll" cat-file ${oid}
echo "done"