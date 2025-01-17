import { config } from '../../config/config.js';
import { PACKET_TYPE } from '../../constants/header.js';
import { getHandlerById } from '../../handlers/index.js';
import CustomError from '../../utils/error/customError.js';
import { ErrorCodes } from '../../utils/error/errorCodes.js';
import { handleError } from '../../utils/error/errorHandler.js';
import { getProtoMessages } from '../../init/loadProtos.js';
import { HANDLER_IDS } from '../../constants/handlerIds.js';
import UserManager from '../../classes/managers/user.manager.js';
import RoomManager from '../../classes/managers/room.manager.js';
import RedisManager from '../../db/redis/redisManager.js';
import pingPongHandler from '../../handlers/gameHub/pingPong.handler.js';
import updatePositionHandler from '../../handlers/gameHub/updatePosition.handler.js';

export const onData = (socket) => async (data) => {
  
  // 핑퐁 실행
  await pingPongHandler();
  
  // 기존 버퍼에 새로 수신된 데이터를 추가
  socket.buffer = Buffer.concat([socket.buffer, data]);

  // 패킷의 총 헤더 길이 (패킷 길이 정보 + 타입 정보)
  const totalHeaderLength = config.packet.totalLength + config.packet.typeLength;

  while (socket.buffer.length >= totalHeaderLength) {
    // 1. 패킷 길이 정보 수신 (4바이트)
    const length = socket.buffer.readUInt32BE(0);

    // 2. 패킷 타입 정보 수신 (1바이트)
    const packetType = socket.buffer.readUInt8(config.packet.totalLength);
    // 3. 패킷 전체 길이 확인 후 데이터 수신
    if (socket.buffer.length >= length) {
      // 패킷 데이터를 자르고 버퍼에서 제거
      const packet = socket.buffer.slice(totalHeaderLength, length);
      socket.buffer = socket.buffer.slice(length);

      console.log(packetType);

      try {
        switch (packetType) {
          case PACKET_TYPE.PING: {
            const protoMessages = getProtoMessages();
            const ping = protoMessages.gameHub.Ping;
            const pingMessage = ping.decode(packet);
            const now = Date.now();
            const latency = (now - pingMessage.timestamp) / 2;
            
            // 레이턴시 유저에게 추가. (게임 진행할 경우)
            const userKey = `user:${socket.remoteAddress}:${socket.remotePort}`;
            const user = await UserManager.getInstance().getUserData(userKey);
            
            if(Object.keys(user).length !== 0){
              const room = await RoomManager.getInstance().getRoom(user.roomId);
              if(Object.keys(room).length !== 0){
                await RoomManager.getInstance().InstantiationLatency(user.roomId, userKey, latency);
                
                const test =   await RoomManager.getInstance().getRoom(user.roomId);
                console.log(test.latencys);
              }
            }
            break;
          }
          case PACKET_TYPE.INITIAL_USER: {
            const protoMessages = getProtoMessages();
            const temp = protoMessages.mainHub.InitialUserPacket;
            console.log(temp.decode(packet));

            // 서버의 발송 (유저 생성)
            const handler = getHandlerById(HANDLER_IDS.INITIAL_USER);
            await handler(socket, temp.decode(packet));

            // 방목록도 갱신하자.
            const roomInfoHandler = getHandlerById(HANDLER_IDS.CREATE_ROOM);
            await roomInfoHandler(socket, true);

            // 현재 접속중인유저
            const users = await UserManager.getInstance().getConnectedSockets();
            const connectedUserHandler = getHandlerById(HANDLER_IDS.CONNECTED_USERS);

            // 모든 소켓에 비동기적으로 메시지를 보내기
            await Promise.all(
              Object.values(users).map(async (element) => {
                await connectedUserHandler(element);
              }),
            );

            break;
          }
          case PACKET_TYPE.LOBBY_CHAT: {
            const protoMessages = getProtoMessages();
            const temp = protoMessages.mainHub.LobbyChatPacket;

            // 현재 접속중인유저
            const users = await UserManager.getInstance().getConnectedSockets();
            const lobbyChatHandler = getHandlerById(HANDLER_IDS.LOBBY_CHAT);

            // 모든 소켓에 비동기적으로 메시지를 보내기
            await Promise.all(
              Object.values(users).map(async (element) => {
                await lobbyChatHandler(element, temp.decode(packet));
              }),
            );
            break;
          }
          case PACKET_TYPE.CREATE_ROOM: {
            // 방생성 작성.
            const users = await UserManager.getInstance().getConnectedSockets();

            const roomInfoHandler = getHandlerById(HANDLER_IDS.CREATE_ROOM);

            // 모든 소켓에 비동기적으로 메시지를 보내기
            await Promise.all(
              Object.values(users).map(async (element) => {
                await roomInfoHandler(element);
              }),
            );

            const roomStartAckHandler = getHandlerById(HANDLER_IDS.START_ACK);
            await roomStartAckHandler(socket, true);

            // 게임(룸) 루프 시작
            await updatePositionHandler(socket);

            break;
          }
          case PACKET_TYPE.ROOM_JOIN: {
            const protoMessages = getProtoMessages();
            const temp = protoMessages.mainHub.RoomJoinPacket;
            const roomId = temp.decode(packet).roomId;

            const room = await RoomManager.getInstance().getRoom(roomId);

            if (Object.keys(room).length !== 0) {
              // 최대 접속자수가 넘었다면.
              if (room.currentPlayers < room.maxPlayers) {
                // 접속자를 추가합니다.
                const userKey = `${socket.remoteAddress}:${socket.remotePort}`;
                await RoomManager.getInstance().InstantiationEntity(roomId, 'players', userKey);

                const currentPlayers = room.currentPlayers;
                await RoomManager.getInstance().updateCurrentPlayers(roomId, currentPlayers + 1);

                // 방생성 작성.
                const users = await UserManager.getInstance().getConnectedSockets();
                const roomInfoHandler = getHandlerById(HANDLER_IDS.CREATE_ROOM);

                // 모든 소켓에 비동기적으로 메시지를 보내기
                await Promise.all(
                  Object.values(users).map(async (element) => {
                    await roomInfoHandler(element, true);
                  }),
                );
              }

              // 유저 업데이트 (방접속, 게임시작.)
              await UserManager.getInstance().updateIsGame(socket, true);
              await UserManager.getInstance().updateRoomId(socket, roomId);

              

              const roomStartAckHandler = getHandlerById(HANDLER_IDS.START_ACK);
              await roomStartAckHandler(socket, true);

              
            }

            break;
          }
          case PACKET_TYPE.SPAWN_USER: {
            const protoMessages = getProtoMessages();
            const temp = protoMessages.gameHub.SpawnUserRequest;
            const userKey = temp.decode(packet);

            
            // 검증
            const checkUser = await UserManager.getInstance().getUserData(userKey.userId);
            const checkRoom = await RoomManager.getInstance().getRoom(checkUser.roomId);

            if (Object.keys(checkUser).length !== 0 && Object.keys(checkRoom).length !== 0) {

              if(checkUser.isGame === true) {
                const userKey = `user:${checkUser.socketId}:${checkUser.socketPort}`;
                await RoomManager.getInstance().InstantiationEntity(checkUser.roomId, 'players', userKey );

                // 갱신된 룸데이터를 가져오자.
                const roomData = await RoomManager.getInstance().getRoom(checkUser.roomId);

                // 소캣을 가져오자. getConnectedSockets
                const roomUsers = roomData.players;
                const sockets  = [];
                const socket  = UserManager.getInstance().getConnectedSockets();

                for(let value of roomUsers) {
                  sockets.push(socket[value])
                }

                // 브로드 캐스트
                const userStartHandler = getHandlerById(HANDLER_IDS.USER_START);
                await Promise.all(
                  Object.values(sockets).map(async (element) => {
                    await userStartHandler(element);
                  }),
                );
              }       
            }
            break;
          }
          case PACKET_TYPE.KEY_INPUT: {
            const protoMessages = getProtoMessages();
            const temp = protoMessages.gameHub.KeyInput;
            const keyInput = temp.decode(packet);

            // 유저 키눌림 저장(갱신)
            await UserManager.getInstance().updateInputKey(socket, keyInput.keyName);
            await UserManager.getInstance().updatePressedTimestamp(socket, keyInput.timestamp.toNumber());
            

            // 로직 작성.
            break;
          }
          case PACKET_TYPE.ANIMATION_STATE: {
            // 로직 작성.
            break;
          }
          case PACKET_TYPE.COLLISION: {
            // 로직 작성.
            break;
          }
          case PACKET_TYPE.GAME_EXIT: {
            // 로직 작성.
            break;
          }
        }
      } catch (error) {
        handleError(socket, error);
      }
    } else {
      // 아직 전체 패킷이 도착하지 않음
      break;
    }
  }

  console.log('클라이언트가 데이터를 보냈습니다.');
};

// 프로파일러 확인. (병목)-> 코드최적화->

// 만들고 확인하고 나누자.

// 공부, 경험.필수도전 완료후 해보자.

// 1. 추가기능
// 2. 동기화 최적화
// 3. 분산서버.

//
