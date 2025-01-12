import RedisManager from '../../db/redis/redisManager.js';

export default class UserManager {

  static instance = null;

  constructor() {
    if(UserManager.instance){
      throw new Error("UserManager는 싱글턴 클래스입니다. getInstance()를 사용하세요.");
    }
    this.connectedSockets = {};
    UserManager.instance = this;  // 인스턴스를 static 변수에 저장
  }

  static getInstance() {
    if(!UserManager.instance) {
      UserManager.instance = new UserManager();
    }

    return UserManager.instance;
  }

  // 소켓 추가.
  async addSocket(socket, userId, deviceId) {
    // 현재 연결된 소켓추가
    const key = `${socket.remoteAddress}:${socket.remotePort}`;
    this.connectedSockets[key] = socket;

    // 유저 정보 세팅.
    let info = {
      uuid: userId,
      userId: deviceId,
      socketId: socket.remoteAddress,
      socketPort: socket.remotePort,
      sequence: 0,
      isGame: false,
      score: 0,
      bestScore: 0,
      roomId:"",
      x: 0,
      y: 0,
    };

    // 유저 키값생성 후 Redis에 추가
    // 키값은 별다른 검색없이 소캣만가지고 할 수 있게끔 구현.
    const userKey = `user:${key}`;
    await RedisManager.getInstance().createData(userKey, info);
    //const test = await RedisManager.getInstance().getDataByPrefix('user');
  }

  // 소켓 삭제.
  async removeSocket(socket) {
    const key = `${socket.remoteAddress}:${socket.remotePort}`;

    if (this.connectedSockets[key]) {
      const userKey = `user:${key}`;
      await RedisManager.getInstance().deleteKey(userKey);
      delete this.connectedSockets[key];;
    } else {
      console.log("삭제 소켓이 없음.");
    }

    const test = await RedisManager.getInstance().getDataByPrefix('user');
    console.log(test);
  }

  // 시퀀스
  async getNextSequence(socket) {
    const key = `${socket.remoteAddress}:${socket.remotePort}`;
    const userKey = `user:${key}`;

    let sequence = await RedisManager.getInstance().getData(userKey, 'sequence');
    
    if(sequence !== null) {
      await RedisManager.getInstance().updateData(userKey, 'sequence', sequence + 1 );
      return sequence + 1;
    }

    return 0;
  }

  // 로직추가~ 
  

}



