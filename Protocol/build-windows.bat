REM 배치파일 위치로 현재경로 변경
pushd %~dp0

REM 프로토콜 컴파일(C#)
protoc.exe -I=./ --csharp_out=./ ./Protocol.proto

REM 핸들러 생성기 실행 (C#)
../PacketGenerator/bin/PacketGenerator.exe ./Protocol.proto

XCOPY /Y Protocol.cs ../UnityClient2D/Assets/Scripts/Packet
XCOPY /Y ClientPacketManager.cs ../UnityClient2D/Assets/Scripts/Packet
XCOPY /Y ServerPacketManager.cs ../Server/Packet/
 
REM 프로토콜 버퍼 임시 결과물 삭제 (C#)
REM DEL /Q /F Protocol.cs
REM DEL /Q /F ClientPacketManager.cs
REM DEL /Q /F ServerPacketManager.cs
