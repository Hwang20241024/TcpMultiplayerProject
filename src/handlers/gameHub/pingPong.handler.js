import { handleError } from '../../utils/error/errorHandler.js';
import UserManager from '../../classes/managers/user.manager.js';
import { PACKET_TYPE } from '../../constants/header.js';
import { getProtoMessages } from '../../init/loadProtos.js';
import { config } from '../../config/config.js';

const INTERVAL_TIME = 3000;
let isIntervalRunning = false;

const pingPongHandler = async () => {
  try {
    // 중복 방지.
    if (isIntervalRunning) return; // 이미 실행 중이면 중단
    isIntervalRunning = true;

    // 일정 시간 간격 (예: 5초)마다 핑을 보냄
    setInterval(async () => {
      // 접속한 모든 유저에게 핑을 보내자.
      const sockets = await UserManager.getInstance().getConnectedSockets();
      if (Object.keys(sockets).length !== 0) {
        await Promise.all(
          Object.values(sockets).map(async (element) => {
            const now = Date.now();

            const protoMessages = getProtoMessages();
            const ping = protoMessages.gameHub.Ping;

            const payload = {
              timestamp: now,
            };
            const pingPacket = ping.encode(payload).finish();

            // 패킷 길이 정보를 포함한 버퍼 생성
            const packetLength = Buffer.alloc(config.packet.totalLength);
            packetLength.writeUInt32BE(
              pingPacket.length + config.packet.totalLength + config.packet.typeLength,
              0,
            );

            // 패킷 타입 정보를 포함한 버퍼 생성
            const packetType = Buffer.alloc(config.packet.typeLength);
            packetType.writeUInt8(PACKET_TYPE.PING, 0);

            // 길이 정보와 메시지를 함께 전송
            const initialResponse = Buffer.concat([packetLength, packetType, pingPacket]);

            element.write(initialResponse);
          }),
        );
      }
    }, INTERVAL_TIME); // 3000ms (3초)마다 핑을 보냄 setInterval
  } catch (error) {
    handleError(socket, error);
  }
};

export default pingPongHandler;
