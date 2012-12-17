#!/bin/sh

rm -rf release
mkdir release
cd release
cp ../AaltoWindrawModel/bin/Release/*.dll .
cp ../AaltoWindrawView/bin/Release/AaltoWindrawView.exe ../AaltoWindrawView/bin/Release/*.dll .
cp ../AaltoWindrawServer/bin/Release/AaltoWindrawServer.exe ../AaltoWindrawServer/bin/Release/*.dll .

mkdir Database
cd Database/
cp ../../own_resources/mongodb/*.exe .