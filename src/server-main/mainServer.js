import net from 'net';
import { config } from '../config/config.js';
import initServer from '../init/index.js';
import { onConnection } from './events/onConnection.js';

const server = net.createServer(onConnection);

// 소켓을 저장할 배열
const connectedSockets = [];

initServer()
  .then(() => {
    server.listen(config.server.Port, config.server.host, () => {
      console.log(`[메인서버]가 ${config.server.host}:${config.server.Port}에서 실행 중입니다.`);
      console.log(server.address());
    });
  })
  .catch((error) => {
    console.error(error);
    process.exit(1); // 오류 발생 시 프로세스 종료
  });

