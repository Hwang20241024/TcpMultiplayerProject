import { handleError } from '../../utils/error/errorHandler.js';
import UserManager from '../../classes/managers/user.manager.js';
import RoomManager from '../../classes/managers/room.manager.js';
import { deadReckoning } from '../../utils/deadReckoning.js';
import { HANDLER_IDS, RESPONSE_SUCCESS_CODE } from '../../constants/handlerIds.js';
import { createResponse } from '../../utils/response/createResponse.js';

let INTERVAL_TIME = 1000; // 기본시간
const VELOCITY = 10; // 속도
const GRAVITY = 5; // 중력
const JUMP_SPEED = 20; // 점프 속도
let isIntervalRunning = false;

//테스트
const MIN_DELTA_TIME = 0.016; // 최소 60 FPS (1/60초)
const MAX_DELTA_TIME = 0.1;   // 최대 10 FPS (1/10초)

const updatePositionHandler = async (socket) => {
  try {
    // 중복 방지.
    if (isIntervalRunning) return; // 이미 실행 중이면 중단
    isIntervalRunning = true;

    const userkey = `user:${socket.remoteAddress}:${socket.remotePort}`;
    const user = await UserManager.getInstance().getUserData(userkey);
    const roomId = user.roomId;

    const intervalId = setInterval(async () => {
      // 방정보를 가져오자.
      const room = await RoomManager.getInstance().getRoom(roomId);

      // 방 인원수가 0이면 종료
      if (room.players.length === 0) {
        clearInterval(intervalId); // 인원수가 0이면 타이머 종료
        isIntervalRunning = false;
        return;
      }

      // 레이턴시 가장 높은것을 고르자.
      const latencys = room.latencys;
      const maxLatency = Math.max(...Object.values(latencys));
      INTERVAL_TIME = maxLatency;
      console.log(`레이턴시 : ${maxLatency}`)

      // 플레이어 로직.
      // 1. 유저들의 userkey를 가져오자.
      // 2. 유저들이 key를 눌렀는지 확인하자.
      // 3. key를 누른 유저들만 속아내서 좌표를 추측항법으로 적용시키자.
      // 4. 적용시킨 좌표를 브로드 캐스트로 보내자. (소캣중에서 방안에 있는 유저들에게만)

      const usersUpdatePosition = [];
      for (let value of room.players) {
        let user = await UserManager.getInstance().getUserData(value);

        const userKeyInput = user.inputKey;
        const userIsJump = user.isJump;

        // 방향 설정.
        let directionX = 0;
        let directionY = 0;

        if (userKeyInput !== 'defaultValue' && userKeyInput !== 'NULL') {
          if (userKeyInput === 'LeftArrow') {
            directionX = -1; // 왼쪽
          } else {
            directionX = 1; // 오른쪽
          }
        }

        if (userIsJump !== 'defaultValue') {
          if (userIsJump === 'Jump') {
            directionY = 1;
            await UserManager.getInstance().updateIsJump(value, true);
            user = await UserManager.getInstance().getUserData(value);
          }
        }

        // 추측항법 매개변수 선언 하기전 설정. (델타 타임)
        const keyPressedTimestamp = user.keyPressedTimestamp || 0;
        const currentTime = Date.now();
        console.log(`keyPressedTimestamp : ${keyPressedTimestamp}`);
        console.log(`currentTime : ${currentTime}`);

        if (keyPressedTimestamp !== 0) {
          // 추측항법 매개변수
          let deltaTime = (currentTime - keyPressedTimestamp) / 1000;  // 초 단위
          console.log(`deltaTime : ${deltaTime}`);
          

          const direction = { x: directionX, y: directionY }; // 방향.
          const position = { x: user.x, y: user.y }; // 좌표
          const velocity = VELOCITY; // 스피드
          const gravity = GRAVITY; // 중력
          const jumpSpeed = JUMP_SPEED; // 점프 스피드.
          const isJump = user.isJump;

          const newPosition = deadReckoning(
            0,
            position,
            velocity,
            direction,
            deltaTime,
            gravity,
            jumpSpeed,
            isJump,
          ); // 작성중

          const data = {
            roomId: roomId,
            entityType: 0,
            sourceId: user.uuid,
            sourceX: newPosition.x,
            sourceY: newPosition.y,
          };

          const socket = UserManager.getInstance().getConnectedSockets();
          const updatePosition = {
            socket: socket[value],
            data: data,
          };
          await UserManager.getInstance().updatePressedTimestamp(socket[value], currentTime);
          await UserManager.getInstance().updateX(value, newPosition.x);
          await UserManager.getInstance().updateY(value, newPosition.y);

          console.log(`x : ${newPosition.x} / y : ${newPosition.y}`);

          usersUpdatePosition.push(updatePosition);
        }
      }

      // 유저
      await Promise.all(
        Object.values(usersUpdatePosition).map(async (element) => {
          const initialResponse = createResponse(
            element.socket,
            HANDLER_IDS.UPDATE_POSITION,
            RESPONSE_SUCCESS_CODE.Success,
            element.data,
          );

          socket.write(initialResponse);
        }),
      );

      // 몬스터 로직.
      // 브로드 캐스트 - 메세지 보내기

      // 총알 로직.
      // 브로드 캐스트 - 메세지 보내기

      // 충돌 로직.
      // 브로드 캐스트 - 메세지 보내기
    }, INTERVAL_TIME);
  } catch (error) {
    handleError(socket, error);
  }
};
export default updatePositionHandler;
