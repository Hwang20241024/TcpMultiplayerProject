import { getProtoMessages } from '../../init/loadProtos.js';
import { PACKET_TYPE } from '../../constants/header.js';
import { config } from '../../config/config.js';

const makeNotification = (message, type) => {
  // 패킷 길이 정보를 포함한 버퍼 생성.
  const bufferLength = buffer.length;
  const packetTotalLength = config.packet.totalLength;
  const packetTypeLength = config.packet.typeLength;
  
  const packetLength = Buffer.alloc(config.packet.totalLength);  
  packetLength.writeUInt32BE(bufferLength + packetTotalLength + packetTypeLength, 0);


  // 패킷 타입 정보를 포함한 버퍼 생성
  const packetType = Buffer.alloc(packetTypeLength);
  packetType.writeUInt8(type, 0);
  
  // 길이 정보와 메시지를 함께 전송
  return Buffer.concat([packetLength, packetType, message]);
};

export const createPingPacket = (timestamp) => {
  const protoMessages = getProtoMessages();
  const ping = protoMessages.common.Ping;

  const payload = { timestamp };
  const message = ping.create(payload);
  const pingPacket = ping.encode(message).finish();
  return makeNotification(pingPacket, PACKET_TYPE.PING);
};
