# ./protoc-22.2-osx-aarch_64/bin/protoc addressbook.proto --csharp_out .
FILENAME="Protocol"
./protoc-3.12.3-osx-x86_64/bin/protoc $FILENAME.proto --csharp_out .

/bin/cp -f $FILENAME.cs ../Server/$FILENAME.cs