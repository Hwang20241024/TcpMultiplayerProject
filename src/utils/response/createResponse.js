import { getProtoMessages } from '../../init/loadProtos.js';
import { config } from '../../config/config.js';
import { PACKET_TYPE } from '../../constants/header.js';
import { HANDLER_IDS } from '../../constants/handlerIds.js';
import UserManager from '../../classes/managers/user.manager.js';

export const createResponse =  (socket, handlerId, responseCode, data = null) => {
  // 프로토 생성
  const protoMessages = getProtoMessages();
  let response = protoMessages.mainHub.ResponseInitialUserPacket;
  let responseData = null;
  let bufferData = null;
  let bufferData2 = null;
  let dataKey = '';
  let sequenceKey = null;
  let PacketType = null;

  switch (handlerId) {
    case HANDLER_IDS.INITIAL_USER:
      response = protoMessages.mainHub.ResponseInitialUserPacket;
      responseData = protoMessages.mainHub.UserData;
      bufferData = responseData.encode(data).finish();
      bufferData2 = UserManager.getInstance().getNextSequence(socket);
      dataKey = 'userData';
      sequenceKey = 'sequence';
      PacketType = PACKET_TYPE.INITIAL_USER;
      break;
    case HANDLER_IDS.CONNECTED_USERS: {
      response = protoMessages.mainHub.ResponseConnectedUserPacket;
      responseData = protoMessages.mainHub.ConnectedUsersData;

      const connectedUsersData = responseData.create();
      data.forEach((user) => {
        const connectedUser = protoMessages.mainHub.ConnectedUser.create({
          deviceId: user.deviceId,
          score: user.score,
        });
        connectedUsersData.users.push(connectedUser); // 'users' 배열에 추가
      });

      bufferData = responseData.encode(connectedUsersData).finish();
      bufferData2 = UserManager.getInstance().getNextSequence(socket);
      dataKey = 'connectedUsersData';
      sequenceKey = 'sequence';
      PacketType = PACKET_TYPE.CONNECTED_USERS;
      break;
    }
    case HANDLER_IDS.LOBBY_CHAT:
      response = protoMessages.mainHub.ResponseLobbyChatPacket;
      bufferData = data.deviceId;
      bufferData2 = data.chatData;
      dataKey = 'deviceId';
      sequenceKey = 'chatData';
      PacketType = PACKET_TYPE.LOBBY_CHAT;

      break;
    case HANDLER_IDS.CREATE_ROOM: {
      response = protoMessages.mainHub.ResponseRoomInfoPacket;
      responseData = protoMessages.mainHub.RoomsData;

      const roomsDatas = responseData.create();
      data.forEach((roomInfo) => {
        const room = protoMessages.mainHub.Room.create({
          roomId: roomInfo.roomId,
          roomName: roomInfo.roomName,
          host: roomInfo.host,
          currentPlayers: roomInfo.currentPlayers,
          maxPlayers: roomInfo.maxPlayers,
        });
        roomsDatas.rooms.push(room); // 'users' 배열에 추가
      });

      bufferData = responseData.encode(roomsDatas).finish();     
      bufferData2 = UserManager.getInstance().getNextSequence(socket);
      dataKey = 'roomsData';
      sequenceKey = 'sequence';
      PacketType = PACKET_TYPE.CREATE_ROOM;

      break;
    }
    case HANDLER_IDS.START_ACK: {
      response = protoMessages.mainHub.ResponseRoomStartAckPacket;
      bufferData = data;
      bufferData2 = UserManager.getInstance().getNextSequence(socket);
      dataKey = 'isGame';
      sequenceKey = 'sequence';
      PacketType = PACKET_TYPE.START_ACK;

      break;
    }
    case HANDLER_IDS.USER_START: {
      response = protoMessages.gameHub.UserStartResponse;
      responseData = protoMessages.gameHub.UserData;
      bufferData = responseData.encode(data).finish();
      bufferData2 = UserManager.getInstance().getNextSequence(socket);
      dataKey = 'userData';
      sequenceKey = 'sequence';
      PacketType = PACKET_TYPE.USER_START;
      break;
    }
    case HANDLER_IDS.UPDATE_POSITION: {
      response = protoMessages.gameHub.UpdatePositionResponse;
      responseData = protoMessages.gameHub.UpdatePosition;
      bufferData = responseData.encode(data).finish();
      bufferData2 = UserManager.getInstance().getNextSequence(socket);
      dataKey = 'updatePosition';
      sequenceKey = 'sequence';
      PacketType = PACKET_TYPE.UPDATE_POSITION;
      break;
    }
    default:
      break;
  }

  // 테스트
// const temp = getProtoMessages();
// const temp2 = temp.mainHub.UserData;
// console.log(temp2.decode(bufferData));

  const responsePayload = {
    handlerId: handlerId,
    responseCode: responseCode,
    timestamp: Date.now(),
    [dataKey]: bufferData, // 데이터는 동적으로 설정.
    [sequenceKey]: bufferData2,
  };



  const buffer = response.encode(responsePayload).finish();

  // 패킷 길이 정보를 포함한 버퍼 생성
  const packetLength = Buffer.alloc(config.packet.totalLength);
  packetLength.writeUInt32BE(
    buffer.length + config.packet.totalLength + config.packet.typeLength,
    0,
  );

  // 패킷 타입 정보를 포함한 버퍼 생성
  const packetType = Buffer.alloc(config.packet.typeLength);
  packetType.writeUInt8(PacketType, 0);

  // 길이 정보와 메시지를 함께 전송
  return Buffer.concat([packetLength, packetType, buffer]);
};

/// 수정중 밥먹고하자
