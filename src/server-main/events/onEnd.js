import UserManager from "../../classes/managers/user.manager.js";

export const onEnd = (socket) => () => {
  console.log('클라이언트 연결이 종료되었습니다.');
  UserManager.getInstance().removeSocket(socket);
};
