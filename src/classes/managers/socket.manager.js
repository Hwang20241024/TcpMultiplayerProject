export const connectedSockets = [];

// 소켓 추가.
export const addSocket = (socket) => {
  connectedSockets.push(socket);
};

// 소켓 삭제.
export const removeSocket = (socket) => {
  const index = connectedSockets.indexOf(socket);
  if (index !== -1) {
    connectedSockets.splice(index, 1);
  }
};

// 전체 메세지 보내기
export const sendMessageToAllSockets = (message) => {
  connectedSockets.forEach((socket) => {
    socket.write(message);
  });
};
