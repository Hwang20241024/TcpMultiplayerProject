import { getProtoMessages } from '../../init/loadProtos.js';
import { config } from '../../config/config.js';
import { PACKET_TYPE } from '../../constants/header.js';
import { HANDLER_IDS } from '../../constants/handlerIds.js';
import UserManager from '../../classes/managers/user.manager.js';

export const createResponse = (socket, handlerId, responseCode, data = null) => {
  // 프로토 생성
  const protoMessages = getProtoMessages();
  let response = protoMessages.mainHub.ResponseInitialUserPacket;
  let responseData = null;
  let bufferData = null;
  let dataKey = '';

  switch (handlerId) {
    case HANDLER_IDS.INITIAL_USER:
      response = protoMessages.mainHub.ResponseInitialUserPacket;
      responseData = protoMessages.mainHub.UserData;
      bufferData = responseData.encode(data).finish();
      dataKey = 'userData';
      break;
    case 1: // 계속추가 
      break;

    default:
      break;
  }

  const responsePayload = {
    handlerId: handlerId,
    responseCode: responseCode,
    timestamp: Date.now(),
    [dataKey]: bufferData, // 데이터는 동적으로 설정.
    sequence: UserManager.getInstance().getNextSequence(socket),
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
  packetType.writeUInt8(PACKET_TYPE.NORMAL, 0);

  // 길이 정보와 메시지를 함께 전송
  return Buffer.concat([packetLength, packetType, buffer]);
};

/// 수정중 밥먹고하자
