import { HANDLER_IDS } from '../constants/handlerIds.js';
import { ErrorCodes } from '../utils/error/errorCodes.js'
import CustomError from '../utils/error/customError.js'
import initialUserHandler from './mainHub/initialUser.handler.js';
import connectedUserHandler from './mainHub/connectedUser.handler.js';
import lobbyChatPacketHandler from './mainHub/lobbyChatPacket.handler.js';
import roomInfoHandler from './mainHub/roomInfo.handler.js';


const handlers = {
  [HANDLER_IDS.INITIAL_USER]: {
    handler: initialUserHandler,
    protoType: 'mainHub.ResponseInitialUserPacket',
  },
  [HANDLER_IDS.CONNECTED_USERS]: {
    handler: connectedUserHandler,
    protoType: 'mainHub.ResponseConnectedUserPacket',
  },
  [HANDLER_IDS.LOBBY_CHAT]: {
    handler: lobbyChatPacketHandler,
    protoType: `mainHub.ResponseLobbyChatPacket`,
  },
  [HANDLER_IDS.CREATE_ROOM]: {
    handler: roomInfoHandler,
    protoType: `mainHub.ResponseRoomInfoPacket`,
  },
  // 다른 핸들러들을 추가
};

export const getHandlerById = (handlerId) => {
  if (!handlers[handlerId]) {
    throw new CustomError(
      ErrorCodes.UNKNOWN_HANDLER_ID,
      `핸들러를 찾을 수 없습니다: ID ${handlerId}`,
    );
  }
  return handlers[handlerId].handler;
};

export const getProtoTypeNameByHandlerId = (handlerId) => {
  if (!handlers[handlerId]) {
    // packetParser 체크하고 있지만 그냥 추가합니다.
    throw new CustomError(
      ErrorCodes.UNKNOWN_HANDLER_ID,
      `핸들러를 찾을 수 없습니다: ID ${handlerId}`,
    );
  }
  return handlers[handlerId].protoType;
};