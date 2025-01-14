export const TOTAL_LENGTH = 4; // 전체 길이를 나타내는 4바이트
export const PACKET_TYPE_LENGTH = 1; // 패킷타입을 나타내는 1바이트 // 0 = 핑, 1 = 일반 패킷

export const PACKET_TYPE = {
  PING: 0,
  INITIAL_USER:1,
  CONNECTED_USERS:2,
  LOBBY_CHAT: 3,
  CREATE_ROOM: 4,
  GAME_START: 5,
  LOCATION: 6,
  NORMAL: 7,
};
