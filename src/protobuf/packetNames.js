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
    UserData: 'gameHub.UserData',
    EntityType : 'gameHub.EntityType',
    UpdatePosition : 'gameHub.UpdatePosition',
    UpdateAnimation : 'gameHub.UpdateAnimation',
    InitialEntity : 'gameHub.InitialEntity',
    DeleteEntity : 'gameHub.DeleteEntity',
    DeleteUser : 'gameHub.DeleteUser',
    SpawnUserRequest: 'gameHub.SpawnUserRequest',
    KeyInput : 'gameHub.KeyInput',
    AnimationState : 'gameHub.AnimationState',
    GameExit : 'gameHub.GameExit',
    Collision  : 'gameHub.Collision',
    UserStartResponse : 'gameHub.UserStartResponse',
    UpdatePositionResponse : 'gameHub.UpdatePositionResponse',
    UpdateAnimationResponse : 'gameHub.UpdateAnimationResponse',
    InitialEntityResponse  : 'gameHub.InitialEntityResponse',
    DeleteEntityResponse  : 'gameHub.DeleteEntityResponse',
    DeleteUserResponse  : 'gameHub.DeleteUserResponse',

  }
  // 프로토 파일이 추가되면 여기에 추가.
};


// 여기작성하자.