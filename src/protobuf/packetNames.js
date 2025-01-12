export const packetNames = {
  mainHub: {
    UserData: 'mainHub.UserData',
    ConnectedUser: 'mainHub.ConnectedUser',
    ConnectedUsersData: 'mainHub.ConnectedUsersData',
    Participant: 'mainHub.Participant',
    Room: 'mainHub.Room',
    RoomsData: 'mainHub.RoomsData',
    InitialUserPacket:'mainHub.InitialUserPacket',
    ResponseInitialUserPacket:'mainHub.ResponseInitialUserPacket',
    InitialRoomPacket:'mainHub.InitialRoomPacket',
    RoomJoinPacket:'mainHub.RoomJoinPacket',
    LobbyChatPacket:'mainHub.LobbyChatPacket',
    ResponseRoomJoinPacket:'mainHub.ResponseRoomJoinPacket',
    ResponseRoomInfoPacket:'mainHub.ResponseRoomInfoPacket',
    ResponseConnectedUserPacket:'mainHub.ResponseConnectedUserPacket',
    ResponseLobbyChatPacket:'mainHub.ResponseLobbyChatPacket',
    RoomReadyPacket:'mainHub.RoomReadyPacket',
    RoomUnreadyPacket:'mainHub.RoomUnreadyPacket',
    RoomChatPacket:'mainHub.RoomChatPacket',
    RoomExitPacket:'mainHub.RoomExitPacket',
    ResponseRoomStatusPacket:'mainHub.ResponseRoomStatusPacket',
    ResponseRoomStartAckPacket:'mainHub.ResponseRoomStartAckPacket',
    ResponseRoomChatPacket:'mainHub.ResponseRoomChatPacket',
    RoomPing:'mainHub.RoomPing',
  },
  gameHub: {
    Ping: 'gameHub.Ping',
  }
  // 프로토 파일이 추가되면 여기에 추가.
};
