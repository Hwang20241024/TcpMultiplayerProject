import { HANDLER_IDS, RESPONSE_SUCCESS_CODE } from '../../constants/handlerIds.js';
import { createResponse } from '../../utils/response/createResponse.js';
import { handleError } from '../../utils/error/errorHandler.js';



const lobbyChatPacketHandler = async (socket, payload) => {
  try {
    const { deviceId, chatData } = payload;

    const str = `${deviceId} : ${chatData}`;

    console.log(str);
    // 클라이언트에 전송할 패킷 생성.
    const initialResponse = createResponse(
      socket,
      HANDLER_IDS.LOBBY_CHAT,
      RESPONSE_SUCCESS_CODE.Success,
      {deviceId: deviceId, chatData: str},
    );

    ;

    // 소켓을 통해 클라이언트에게 응답 메시지 전송
    socket.write(initialResponse);
  } catch (error) {
    handleError(socket, error);
  }
};

export default lobbyChatPacketHandler;
