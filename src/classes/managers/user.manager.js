import RedisManager from '../../db/redis/redisManager.js';

export default class UserManager {
  static instance = null;

  constructor() {
    if (UserManager.instance) {
      throw new Error('UserManager는 싱글턴 클래스입니다. getInstance()를 사용하세요.');
    }
    this.connectedSockets = {};
    UserManager.instance = this; // 인스턴스를 static 변수에 저장
  }

  static getInstance() {
    if (!UserManager.instance) {
      UserManager.instance = new UserManager();
    }

    return UserManager.instance;
  }

  // 소켓 추가.
  async addSocket(socket, user, deviceId) {
    // 현재 연결된 소켓추가
    const userkey = `user:${socket.remoteAddress}:${socket.remotePort}`;
    this.connectedSockets[userkey] = socket;

    // 유저 정보 세팅.
    let info = {
      uuid: user.id,
      userId: deviceId,
      socketId: socket.remoteAddress,
      socketPort: socket.remotePort,
      sequence: 0,
      isGame: false,
      score: 0,
      bestScore: user.high_score,
      roomId: '',
      x: 0,
      y: 0,
      inputKey: 'NULL',
      keyPressedTimestamp: 0,
      isJump: false,
    };

    // 점프 정보를 추가하자.

    // 유저 키값생성 후 Redis에 추가
    // 키값은 별다른 검색없이 소캣만가지고 할 수 있게끔 구현.
    await RedisManager.getInstance().createData(userkey, info);
    //const test = await RedisManager.getInstance().getDataByPrefix('user');
  }

  // updateIsGame
  async updateIsGame(socket, data) {
    if (typeof data === 'boolean') {
      const userKey = `user:${socket.remoteAddress}:${socket.remotePort}`;
      await RedisManager.getInstance().updateData(userKey, 'isGame', data);
    } else {
      console.log('데이터타입이 boolean이 아닙니다.');
    }
  }

  //
  async updateRoomId(socket, data) {
    if (typeof data === 'string') {
      const userKey = `user:${socket.remoteAddress}:${socket.remotePort}`;
      await RedisManager.getInstance().updateData(userKey, 'roomId', data);
    } else {
      console.log('데이터타입이 string가 아닙니다.');
    }
  }

  async updateInputKey(socket, data) {
    if (typeof data === 'string') {
      const userKey = `user:${socket.remoteAddress}:${socket.remotePort}`;
      await RedisManager.getInstance().updateData(userKey, 'inputKey', data);
    } else {
      console.log('데이터타입이 string가 아닙니다.');
    }
  }
  
  async updatePressedTimestamp(socket, data) {
    if (typeof data === 'number') {
      const userKey = `user:${socket.remoteAddress}:${socket.remotePort}`;
      await RedisManager.getInstance().updateData(userKey, 'keyPressedTimestamp', data);
    } else {
      console.log('데이터가 number가 아닙니다.');
    }
  }

  async updateIsJump(userKey, data) {
    if (typeof data === 'boolean') {
      await RedisManager.getInstance().updateData(userKey, 'isJump', data);
    } else {
      console.log('데이터타입이 boolean이 아닙니다.');
    }
  }


  async updateX(userKey, data) {
    if (typeof data === 'number') {
      await RedisManager.getInstance().updateData(userKey, 'x', data);
    } else {
      console.log('데이터타입이 number가 아닙니다.');
    }
  }

  async updateY(userKey, data) {
    if (typeof data === 'number') {
      await RedisManager.getInstance().updateData(userKey, 'y', data);
    } else {
      console.log('데이터타입이 number가 아닙니다.');
    }
  }



  // 소켓 삭제.
  async removeSocket(socket) {
    const userKey = `user:${socket.remoteAddress}:${socket.remotePort}`;

    if (this.connectedSockets[userKey]) {
      await RedisManager.getInstance().deleteKey(userKey);
      delete this.connectedSockets[userKey];
    } else {
      console.log('삭제 소켓이 없음.');
    }

    const test = await RedisManager.getInstance().getDataByPrefix('user');
    console.log(test);
  }

  // 시퀀스
  async getNextSequence(socket) {
    const userKey = `user:${socket.remoteAddress}:${socket.remotePort}`;

    let sequence = await RedisManager.getInstance().getData(userKey, 'sequence');

    if (sequence !== null) {
      await RedisManager.getInstance().updateData(userKey, 'sequence', sequence + 1);
      return sequence + 1;
    }

    return 0;
  }

  getConnectedSockets() {
    return this.connectedSockets;
  }

  getConnectedSocket(userKey) {
    return this.connectedSockets[userKey];
  }

  async getUserData(userKey) {
    return await RedisManager.getInstance().getAllData(userKey);
  }

  // 로직추가~
}
