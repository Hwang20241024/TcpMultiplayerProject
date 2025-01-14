import { HANDLER_IDS, RESPONSE_SUCCESS_CODE } from '../../constants/handlerIds.js';
import { createResponse } from '../../utils/response/createResponse.js';
import { handleError } from '../../utils/error/errorHandler.js';
import RedisManager from '../../db/redis/redisManager.js';

const connectedUserHandler = async (socket) => {
  try {
    const key = 'user';
    const users = await RedisManager.getInstance().getDataByPrefix(key);
    let data = [];
    Object.values(users).forEach((element) => {
      data.push({ deviceId: element.userId, score: element.bestScore });
    });

    // 클라이언트에 전송할 패킷 생성.
    const initialResponse = createResponse(
      socket,
      HANDLER_IDS.CONNECTED_USERS,
      RESPONSE_SUCCESS_CODE.Success,
      data,
    );

    // 소켓을 통해 클라이언트에게 응답 메시지 전송
    socket.write(initialResponse);
  } catch (error) {
    handleError(socket, error);
  }
};

export default connectedUserHandler;
