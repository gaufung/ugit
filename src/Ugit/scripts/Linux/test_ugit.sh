#! /bin/bash

if [[ -d test ]]
then
    rm -rf test
fi

mkdir test
cd test

directoryPath="$(pwd)"
directoryPath="$(dirname "$directoryPath")"
directoryPath="$(dirname "$directoryPath")"
directoryPath="$(dirname "$directoryPath")"

cd "$directoryPath"
chmod 744 ugit
export PATH=$PATH:"$directoryPath"
cd -

ugit init
ugit status

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