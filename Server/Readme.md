# 개발 순서

## git pull 직후 

1. PacketGenerator 빌드

2. Protocol에 protoc-3.12.3-osx-x86_64/bin에 protoc 있는지 체크
(없다면 protoc-3.12.3-osx-x86_64.zip 압축 해제)

## 반복

1. Protocol/Protocol.proto 
MsgId 및 구조체 추가

2. Protocol에서 build-apple-silicon.sh 실행

3. Server 에서 구현

4. Client 에서 구현


