# ./protoc-22.2-osx-aarch_64/bin/protoc addressbook.proto --csharp_out .
FILENAME="Protocol"
./protoc-3.12.3-osx-x86_64/bin/protoc $FILENAME.proto --csharp_out .

/bin/cp -f $FILENAME.cs ../Server/$FILENAME.cs
/bin/cp -f $FILENAME.cs ../Client/$FILENAME.cs

GENERATOR_PATH="../PacketGenerator/bin"
$GENERATOR_PATH/PacketGenerator
/bin/cp -f $GENERATOR_PATH/ClientPacketManager.cs ../../UnityClient2D/Assets/Scripts/Packet/ClientPacketManager.cs
 /bin/cp -f $GENERATOR_PATH/ServerPacketManager.cs ../../Server/ServerPacketManager.cs
 