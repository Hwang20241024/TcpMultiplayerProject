import { HANDLER_IDS, RESPONSE_SUCCESS_CODE } from '../../constants/handlerIds.js';
import { createResponse } from '../../utils/response/createResponse.js';
import { handleError } from '../../utils/error/errorHandler.js';
import { createUser, findUserByDeviceID, updateUserLogin } from '../../db/mysql/user/user.db.js';
import UserManager from '../../classes/managers/user.manager.js';
import RedisManager from '../../db/redis/redisManager.js';

const initialUserHandler = async (socket, payload) => {
  try {
    const { deviceId } = payload;
    // 접속할려는 유저를 DB에서 검색.
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

    // 데이터
    let key = `user`;
    let exists  = await RedisManager.getInstance().getDataByPrefixAndSearchTerm(key, deviceId);

    let responseCode = 0;
    if (!exists) {
      console.log('성공');
      responseCode = RESPONSE_SUCCESS_CODE.Success;

      // 유저매니저에 추가. (나중에보자.)
      await UserManager.getInstance().addSocket(socket, user, deviceId);
      
    } else {
      console.log('실패');
      responseCode = RESPONSE_SUCCESS_CODE.Failure;
    }

    // 데이터를 읽어오자 
    key = `user:${socket.remoteAddress}:${socket.remotePort}`;
    const data = await RedisManager.getInstance().getAllData(key); 

    // 클라이언트에 전송할 패킷 생성.
    const initialResponse = createResponse(socket, HANDLER_IDS.INITIAL_USER, responseCode, data);

    // 소켓을 통해 클라이언트에게 응답 메시지 전송
    socket.write(initialResponse);
  } catch (error) {
    handleError(socket, error);
  }
};

export default initialUserHandler;
