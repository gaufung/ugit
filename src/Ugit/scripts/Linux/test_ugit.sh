#! /bin/bash

if [[ -d remote ]]
then
    rm -rf remote
fi

mkdir remote
cd remote

directoryPath="$(pwd)"
directoryPath="$(dirname "$directoryPath")"
directoryPath="$(dirname "$directoryPath")"
directoryPath="$(dirname "$directoryPath")"

cd "$directoryPath"
chmod 744 ugit
export PATH=$PATH:"$directoryPath"
cd -

ugit init
cp ../../data/remote.md ./
ugit add "remote.md"
ugit commit -m "zero remote commit"
remotePath="$(pwd)"

cd ../


if [[ -d test ]]
then
    rm -rf test
fi

mkdir test
cd test

ugit init

echo "$remotePath"
ugit fetch "$remotePath"
ugit merge remote/master

cp ../../data/hello.txt ./

ugit add "hello.txt"
ugit commit -m "first commit"
ugit log
ugit branch

mkdir sub
cp ../../data/ugit.txt ./sub/
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

cp ../../data/dev.md ./
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

ugit push "$remotePath" master