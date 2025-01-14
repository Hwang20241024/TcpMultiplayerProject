import { HANDLER_IDS, RESPONSE_SUCCESS_CODE } from '../../constants/handlerIds.js';
import { createResponse } from '../../utils/response/createResponse.js';
import { handleError } from '../../utils/error/errorHandler.js';
import RoomManager from '../../classes/managers/room.manager.js';
import { v4 as uuidv4 } from 'uuid';

const roomInfoHandler = async (socket) => {
  try {
    // 룸 생성.
    const roomId = uuidv4();
    await RoomManager.getInstance().addRoom(socket, roomId);

    // 룸 검증.
    // 레디스 매니저로 배열로 받자.

    const roomsInfo = await RoomManager.getInstance().getRooms();

    if (roomsInfo.length !== 0) {
      const initialResponse = createResponse(
        socket,
        HANDLER_IDS.CREATE_ROOM,
        RESPONSE_SUCCESS_CODE.Success,
        roomsInfo,
      );

      socket.write(initialResponse);
    }
  } catch (error) {
    handleError(socket, error);
  }
};

export default roomInfoHandler;
