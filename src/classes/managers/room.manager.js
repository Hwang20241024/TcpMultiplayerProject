import RedisManager from '../../db/redis/redisManager.js';

const MAX_PLAYER = 4;

export default class RoomManager {
  static instance = null;

  constructor() {
    if (RoomManager.instance) {
      throw new Error('RoomManager는 싱글턴 클래스입니다. getInstance()를 사용하세요.');
    }
    this.roomsId = [];
    RoomManager.instance = this; // 인스턴스를 static 변수에 저장
  }

  static getInstance() {
    if (!RoomManager.instance) {
      RoomManager.instance = new RoomManager();
    }
    return RoomManager.instance;
  }

  // 방 생성.
  async addRoom(socket, roomId) {
    
    const key = `room:${roomId}`;


    //console.log(await RedisManager.getInstance().getAllDataFromAllKeys());

    // 유저 정보 가져오자.
    const userKey = `user:${socket.remoteAddress}:${socket.remotePort}`;
    const deviceId = await RedisManager.getInstance().getData(userKey, 'userId'); 

    const players = [];
    players.push(userKey);

    const monsters = [];
    const bullets = [];

    // 방 정보 세팅.
    let roomInfo = {
      roomId: roomId,
      roomName: `${deviceId}님의 게임룸`,
      host: deviceId,
      players: players,
      monsters: monsters,
      bullets: bullets,
      currentPlayers: 1,
      maxPlayers: MAX_PLAYER,
    };

    // 방생성.
    await RedisManager.getInstance().createData(key, roomInfo);
    this.roomsId.push(key);
  }

  // 방 업데이트 (접속 중인 플레이어)
  async updateCurrentPlayers(roomId, data) {
    const key = `room:${roomId}`;
    const currentPlayers = await RedisManager.getInstance().getData(key, `currentPlayers`);
  
    if(currentPlayers !== data) {
      await RedisManager.getInstance().updateData(key, 'currentPlayers', data);
    }
  }

  
  // 방 엔티티 업데이트 (플레이어, 몬스터, 총알 등.. )
  async updateEntity(roomId, type, data) {
    const key = `room:${roomId}`;
    const entitys = await RedisManager.getInstance().getData(key, type);
    
    if(entitys.length !== 0) {
      const index = entitys.indexOf(data);
      if (index !== -1) {
        entitys.push(data);
        await RedisManager.getInstance().updateData(key, type, entitys);
      }
    }
  }

  // 방 엔티티 삭제 (플레이어, 몬스터, 총알 등..)
  async  removeEntity(roomId, type, data) {
    const key = `room:${roomId}`;
    const entitys = await RedisManager.getInstance().getData(key, type);

    if(entitys.length !== 0) {
      const index = entitys.indexOf(data);
      entitys.splice(index, 1);

      if (index !== -1) {
        await RedisManager.getInstance().updateData(key, type, entitys);
      }
    }
  }

  // 방 삭제.
  async removeRoom(roomId) {
    await RedisManager.getInstance().deleteKey(`room:${roomId}`);
  }

  // 방 가져오기. 
  async getRoom(roomId) {
    return await RedisManager.getInstance().getAllData(`room:${roomId}`);
  }

  async getRooms() {
    const rooms = [];

    for(let value of this.roomsId) {
      //console.log(value);
      rooms.push(await RedisManager.getInstance().getAllData(value));
    }

    return rooms;
  }
}
