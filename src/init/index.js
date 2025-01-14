// 서버 초기화 작업
import { testAllConnections } from '../utils/db/testConnection.js';
import pools from '../db/mysql/mysql.database.js';
import { loadProtos } from './loadProtos.js';
import  RedisManager  from '../db/redis/redisManager.js'
import UserManager from '../classes/managers/user.manager.js';
import RoomManager from '../classes/managers/room.manager.js';

const initServer = async () => {
  try {
    await loadProtos();
    await testAllConnections(pools);

    // Redis 매니저 초기화.
    await RedisManager.getInstance().deleteAllData();
    await RedisManager.getInstance().getAllDataFromAllKeys();
    
    // 매니저 초기화
    UserManager.getInstance();
    RoomManager.getInstance();

    // 다음 작업
  } catch (e) {
    console.error(e);
    process.exit(1); // 오류 발생 시 프로세스 종료
  }
};

export default initServer;
