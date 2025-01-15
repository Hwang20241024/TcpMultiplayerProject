import { HANDLER_IDS, RESPONSE_SUCCESS_CODE } from '../../constants/handlerIds.js';
import { createResponse } from '../../utils/response/createResponse.js';
import { handleError } from '../../utils/error/errorHandler.js';

const roomStartAckHandler = async (socket, isGame) => {
  try {
    const initialResponse = createResponse(
      socket,
      HANDLER_IDS.START_ACK,
      RESPONSE_SUCCESS_CODE.Success,
      isGame,
    );

    socket.write(initialResponse);
  } catch (error) {
    handleError(socket, error);
  }
};

export default roomStartAckHandler;
