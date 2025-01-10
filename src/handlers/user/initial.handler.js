import { HANDLER_IDS, RESPONSE_SUCCESS_CODE } from '../../constants/handlerIds.js';
import { createResponse } from '../../utils/response/createResponse.js';
import { handleError } from '../../utils/error/errorHandler.js';
import { createUser, findUserByDeviceID, updateUserLogin } from '../../db/mysql/user/user.db.js';
import UserManager from '../../classes/managers/user.manager.js';
// 디비추가해야함

const initialHandler = async (socket, payload) => {
  try {
    const { deviceId } = payload;

    let user = await findUserByDeviceID(deviceId);
    let message = '';

    // 디비 연결후 사용하자.
    if (!user) {
      user = await createUser(deviceId);
      message = '새로 생성하였습니다.';
      console.log(message);
    } else {
      await updateUserLogin(user.id);
      message = '로그인 하셨습니다.';
      console.log(message);
    }

    

    // 유저 정보 응답 생성
    const initialResponse = createResponse(
      HANDLER_IDS.INITIAL,
      RESPONSE_SUCCESS_CODE,
      { message: message },
      deviceId,
    );

    // 유저매니저에 추가.
    UserManager.getInstance().addSocket(socket, user.id, deviceId);
    
    // 소켓을 통해 클라이언트에게 응답 메시지 전송
    socket.write(initialResponse);
  } catch (error) {
    handleError(socket, error);
  }
};

export default initialHandler;
