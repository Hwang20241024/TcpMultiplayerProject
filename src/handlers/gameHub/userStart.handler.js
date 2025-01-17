import { HANDLER_IDS, RESPONSE_SUCCESS_CODE } from '../../constants/handlerIds.js';
import { createResponse } from '../../utils/response/createResponse.js';
import { handleError } from '../../utils/error/errorHandler.js';
import UserManager from '../../classes/managers/user.manager.js';

const userStartHandler = async (socket) => {
  try {
    const userKey = `user:${socket.remoteAddress}:${socket.remotePort}`;
    const userData = await UserManager.getInstance().getUserData(userKey);

    // 클라이언트에 전송할 패킷 생성.
    const initialResponse = createResponse(
      socket,
      HANDLER_IDS.USER_START,
      RESPONSE_SUCCESS_CODE.Success,
      userData,
    );

    // 소켓을 통해 클라이언트에게 응답 메시지 전송
    socket.write(initialResponse);
  } catch (error) {
    handleError(socket, error);
  }
};
export default userStartHandler;
