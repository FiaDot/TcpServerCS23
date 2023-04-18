# ./protoc-22.2-osx-aarch_64/bin/protoc addressbook.proto --csharp_out .
FILENAME="Protocol"
GENERATOR_PATH="../PacketGenerator/bin"
CLIENT_PATH="../UnityClient2D/Assets/Scripts/Packet"

./protoc-3.12.3-osx-x86_64/bin/protoc $FILENAME.proto --csharp_out .
/bin/cp -f $FILENAME.cs ../Server/Packet/$FILENAME.cs
/bin/cp -f $FILENAME.cs $CLIENT_PATH/$FILENAME.cs

$GENERATOR_PATH/PacketGenerator
#pwd 
# /Users/newtrocode/Project/TcpServerCS23/Protocol

/bin/cp -f ClientPacketManager.cs $CLIENT_PATH/ClientPacketManager.cs
/bin/cp -f ServerPacketManager.cs ../Server/Packet/ServerPacketManager.cs
 