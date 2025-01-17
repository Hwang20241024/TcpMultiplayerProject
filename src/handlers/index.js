import { HANDLER_IDS } from '../constants/handlerIds.js';
import { ErrorCodes } from '../utils/error/errorCodes.js'
import CustomError from '../utils/error/customError.js'
import initialUserHandler from './mainHub/initialUser.handler.js';
import connectedUserHandler from './mainHub/connectedUser.handler.js';
import lobbyChatPacketHandler from './mainHub/lobbyChatPacket.handler.js';
import roomInfoHandler from './mainHub/roomInfo.handler.js';
import roomStartAckHandler from './mainHub/roomStartAck.handler.js';
import userStartHandler from './gameHub/userStart.handler.js'
import updatePositionHandler from './gameHub/updatePosition.handler.js';
import updateAnimationHandler from './gameHub/updateAnimation.handler.js';
import initialEntityHandler from './gameHub/initialEntity.handler.js';
import deleteEntityHandler from './gameHub/deleteEntity.handler.js';
import deleteUserHandler from './gameHub/deleteUser.handler.js';

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
  [HANDLER_IDS.START_ACK]: {
    handler: roomStartAckHandler,
    protoType: `mainHub.ResponseRoomStartAckPacket`,
  },
  [HANDLER_IDS.USER_START]: {
    handler: userStartHandler,
    protoType: `gameHub.UserStartResponse `,
  },
  [HANDLER_IDS.UPDATE_POSITION]: {
    handler: updatePositionHandler,
    protoType: `gameHub.UpdatePositionResponse `,
  },
  [HANDLER_IDS.UPDATE_ANIMATION]: {
    handler: updateAnimationHandler,
    protoType: `gameHub.UpdateAnimationResponse `,
  },
  [HANDLER_IDS.INITIAL_ENTITY]: {
    handler: initialEntityHandler,
    protoType: `gameHub.InitialEntityResponse `,
  },
  [HANDLER_IDS.DELETE_ENTITY]: {
    handler: deleteEntityHandler,
    protoType: `gameHub.DeleteEntityResponse `,
  },
  [HANDLER_IDS.DELETE_USER]: {
    handler: deleteUserHandler,
    protoType: `gameHub.DeleteUserResponse `,
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