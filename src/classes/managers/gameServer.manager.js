export const connectedGameServers = [];

// 서버 추가.
export const addGameServer = (socket) => {
  connectedGameServers.push(socket);
};

// 서버 삭제.
export const removeGameServer = (socket) => {
  const index = connectedGameServers.indexOf(socket);
  if (index !== -1) {
    connectedGameServers.splice(index, 1);
  }
};
