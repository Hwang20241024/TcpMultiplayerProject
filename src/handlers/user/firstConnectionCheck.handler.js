import { getProtoMessages } from '../../init/loadProtos.js';
import { config } from '../../config/config.js';
import { PACKET_TYPE } from '../../constants/header.js';

const firstConnectionCheckHandler = async (socket) => {
  const protoMessages = getProtoMessages();
  const Response = protoMessages.main.FirstConnectionCheck;

  const responsePayload = {
    timestamp: Date.now(),
  };

  const buffer = Response.encode(responsePayload).finish();

  // 패킷 길이 정보를 포함한 버퍼 생성.
  const bufferLength = buffer.length;
  const packetTotalLength = config.packet.totalLength;
  const packetTypeLength = config.packet.typeLength;

  const packetLength = Buffer.alloc(config.packet.totalLength);  
  packetLength.writeUInt32BE(bufferLength + packetTotalLength + packetTypeLength, 0);


  // 패킷 타입 정보를 포함한 버퍼 생성
  const packetType = Buffer.alloc(packetTypeLength);
  packetType.writeUInt8(PACKET_TYPE.FIRST_CONNECTION, 0);

  // 길이 정보와 메시지를 함께 전송
  const messages = Buffer.concat([packetLength, packetType, buffer]);
  socket.write(messages);

};


export default firstConnectionCheckHandler;