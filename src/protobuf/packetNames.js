export const packetNames = {
  mainHub: {
    UserData: 'mainHub.UserData',
    ConnectedUser: 'mainHub.ConnectedUser',
    ConnectedUsersData: 'mainHub.ConnectedUsersData',
    Room: 'mainHub.Room',
    RoomsData: 'mainHub.RoomsData',
    InitialUserPacket:'mainHub.InitialUserPacket',
    ResponseInitialUserPacket:'mainHub.ResponseInitialUserPacket',
    InitialRoomPacket:'mainHub.InitialRoomPacket',
    RoomJoinPacket:'mainHub.RoomJoinPacket',
    LobbyChatPacket:'mainHub.LobbyChatPacket',
    ResponseRoomInfoPacket:'mainHub.ResponseRoomInfoPacket',
    ResponseConnectedUserPacket:'mainHub.ResponseConnectedUserPacket',
    ResponseLobbyChatPacket:'mainHub.ResponseLobbyChatPacket',
    ResponseRoomStartAckPacket:'mainHub.ResponseRoomStartAckPacket',
    RoomPing:'mainHub.RoomPing',
  },
  gameHub: {
    Ping: 'gameHub.Ping',
  }
  // 프로토 파일이 추가되면 여기에 추가.
};
