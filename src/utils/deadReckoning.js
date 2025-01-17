const EntityType = {
  PLAYER: 0,
  MONSTER: 1,
  BULLET: 2,
};

export const deadReckoning = (
  type,
  position,
  velocity,
  direction,
  deltaTime,
  gravity,
  jumpSpeed,
  isJumping
) => {

  let newPosition = {};

  switch (type) {
    case EntityType.PLAYER:
    case EntityType.MONSTER: {
      //const distanceX = velocity * deltaTime * direction.x;
      
      //const distanceX = velocity * 0.016 * direction.x;
      const distanceX = velocity * 0.016 * direction.x;
      console.log(deltaTime);

      // Y축 이동 처리 (점프 및 중력)
      let distanceY = 0;
      if (isJumping) {
        distanceY = jumpSpeed * deltaTime - 0.5 * gravity * Math.pow(deltaTime, 2);
      } else {
        // 중력 적용
        //distanceY = -gravity * deltaTime;
      }

      // 새로운 위치 계산
      newPosition = {
        x: position.x + distanceX,
        y: position.y + distanceY, // Y축은 점프와 중력에 따라 변화
      };

      console.log(`Calculated Distance: X=${distanceX}, Y=${distanceY}`);
      console.log(`New Position: X=${newPosition.x}, Y=${newPosition.y}`);

      break;
    }
    case EntityType.BULLET: {
      const distanceX = velocity * deltaTime * direction.x;
      const distanceY = velocity * deltaTime * direction.y;

      // 새로운 위치 계산
      newPosition = {
        x: position.x + distanceX,
        y: position.y + distanceY, // Y축은 점프와 중력에 따라 변화
      };

      break;
    }
    default:
      break;
  }

  return newPosition;
};
