export const TOTAL_LENGTH = 4; // 전체 길이를 나타내는 4바이트
export const PACKET_TYPE_LENGTH = 1; // 패킷타입을 나타내는 1바이트 // 0 = 핑, 1 = 일반 패킷

export const PACKET_TYPE = {
  PING: 0,
  FIRST_CONNECTION:1,
  INITIAL_USER: 2,
  GAME_START: 3,
  LOCATION: 4,
  NORMAL: 5,
};
