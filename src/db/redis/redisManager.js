import Redis from 'ioredis';

// Redis 연결 설정
class RedisManager {
  // RedisManager의 유일한 인스턴스를 저장하는 정적 변수
  static instance = null;

  // 생성자
  constructor() {
    if (!RedisManager.instance) {
      // Redis 연결 설정
      this.redis = new Redis({
        host: 'localhost',
        port: 6379,
      });

      this.redis.on('connect', () => {
        console.log('Redis에 연결되었습니다!');
      });

      this.redis.on('error', (err) => {
        console.error('Redis 연결 오류:', err);
      });

      // RedisManager의 유일한 인스턴스를 저장
      RedisManager.instance = this;
    }
  }

  // 싱글턴
  static getInstance() {
    if (!RedisManager.instance) {
      RedisManager.instance = new RedisManager();
    }
    return RedisManager.instance;
  }

  // 테스트용
  async getAllDataFromAllKeys() {
    try {
      let cursor = '0';
      const allData = {}; // 모든 데이터를 저장할 객체

      // SCAN 명령어로 모든 키를 조회
      do {
        const [newCursor, keys] = await this.redis.scan(cursor);
        cursor = newCursor;

        for (const key of keys) {
          // 각 키에 대한 데이터 조회
          const data = await this.redis.hgetall(key); // HGETALL로 필드 조회
          if (data && Object.keys(data).length > 0) {
            allData[key] = data; // 해당 키의 데이터를 allData 객체에 저장
          }
        }
      } while (cursor !== '0'); // SCAN이 완료되면 cursor가 '0'이 됨

      console.log('전체 키와 데이터:', allData);
      return allData; // 모든 데이터를 반환
    } catch (error) {
      console.error('전체 키와 데이터 조회 실패:', error);
      throw error;
    }
  }

  // 데이터 생성 (HSET)
  async createData(key, userInfo) {
    try {
      const formattedData = {};
      // 각 필드를 저장할 때 객체는 JSON.stringify를 사용하여 문자열로 변환
      for (const field in userInfo) {
        if (userInfo.hasOwnProperty(field)) {
          formattedData[field] =
            typeof userInfo[field] === 'object'
              ? JSON.stringify(userInfo[field]) // 객체일 경우 문자열로 변환
              : userInfo[field]; // 객체가 아닌 값은 그대로 저장
        }
      }

      // hset을 사용하여 여러 필드를 한번에 저장
      const result = await this.redis.hset(key, formattedData);
      console.log('데이터 저장 완료:', result);
      return result;
    } catch (error) {
      console.error('데이터 저장 실패:', error);
      throw error;
    }
  }

  // 데이터 조회 (HGET)
async getData(key, field) {
  try {
    const result = await this.redis.hget(key, field);
    if (result) {
      try {
        return JSON.parse(result);  // JSON 형식이 맞으면 파싱
      } catch (error) {
        return result;  // JSON 형식이 아니면 그냥 반환
      }
    }
    return null;
  } catch (error) {
    console.error('데이터 조회 실패:', error);
    throw error;
  }
}

  // 데이터 업데이트 (HSET)
  async updateData(key, field, value) {
    try {
      const result = await this.redis.hset(key, field, JSON.stringify(value));
      console.log('데이터 업데이트 완료:', result);
      return result;
    } catch (error) {
      console.error('데이터 업데이트 실패:', error);
      throw error;
    }
  }

  // 데이터 삭제 (HDEL)
  async deleteData(key, field) {
    try {
      const result = await this.redis.hdel(key, field);
      console.log('데이터 삭제 완료:', result);
      return result;
    } catch (error) {
      console.error('데이터 삭제 실패:', error);
      throw error;
    }
  }

  // 전체 데이터 조회 (HGETALL)
  async getAllData(key) {
    try {
      const result = await this.redis.hgetall(key);
      const parsedResult = {};

      for (const field in result) {
        try {
          // JSON 파싱 시도
          parsedResult[field] = JSON.parse(result[field]);
        } catch (error) {
          // JSON 형식이 아니면 그대로 반환
          parsedResult[field] = result[field];
        }
      }
      // 키를 제외하고 값만 포함한 새로운 객체 생성
      return Object.keys(parsedResult).reduce((obj, field) => {
        obj[field] = parsedResult[field];
        return obj;
      }, {});
    } catch (error) {
      console.error('전체 데이터 조회 실패:', error);
      throw error;
    }
  }

  // 접두어로 데이터 조회 (HGETALL, HSCAN)
  async getDataByPrefix(prefix) {
    const resultData = {};
    try {
      // 접두어에 해당하는 모든 key들을 가져오기 위해 keys 사용
      const keys = await this.redis.keys(`${prefix}:*`); // 해당 접두어로 시작하는 모든 키를 찾기

      // 각 key에 대해서 데이터 조회
      for (const key of keys) {
        const data = await this.redis.hgetall(key); // 해당 key의 모든 데이터를 가져옴
        if (data) {
          // 필요한 경우 데이터를 파싱 (필드가 객체일 경우 JSON.parse 필요)
          const parsedData = {};
          for (const field in data) {
            if (data.hasOwnProperty(field)) {
              // 데이터가 JSON 형태면 파싱
              try {
                parsedData[field] = JSON.parse(data[field]); // JSON 문자열을 객체로 변환
              } catch (error) {
                parsedData[field] = data[field]; // 파싱 실패하면 원본 데이터 그대로 저장
              }
            }
          }
          resultData[key] = parsedData;
        }
      }
      return resultData;
    } catch (error) {
      console.error('접두어로 데이터 조회 실패:', error);
      throw error;
    }
  }

  // 접두어와 검색어를 기반으로 데이터 조회 후 존재 여부 반환
  async getDataByPrefixAndSearchTerm(prefix, searchTerm) {
    try {
      // 접두어에 해당하는 모든 key들을 가져오기 위해 keys 사용
      const keys = await this.redis.keys(`${prefix}:*`); // 해당 접두어로 시작하는 모든 키를 찾기

      // 각 key에 대해서 데이터 조회
      for (const key of keys) {
        const data = await this.redis.hgetall(key); // 해당 key의 모든 데이터를 가져옴
        if (data) {
          // 각 필드에서 검색어가 포함된 경우를 찾기
          for (const field in data) {
            if (data.hasOwnProperty(field)) {
              // 데이터가 JSON 형태면 파싱
              let parsedValue;
              try {
                parsedValue = JSON.parse(data[field]); // JSON 문자열을 객체로 변환
              } catch (error) {
                parsedValue = data[field]; // 파싱 실패하면 원본 데이터 그대로 저장
              }

              // 검색어가 포함된 필드를 찾으면 true 반환
              if (parsedValue?.toString().includes(searchTerm)) {
                return true; // 검색어가 포함된 데이터가 발견되면 즉시 true 반환
              }
            }
          }
        }
      }

      // 검색어에 맞는 데이터가 없으면 false 반환
      return false;
    } catch (error) {
      console.error('접두어와 검색어로 데이터 조회 실패:', error);
      throw error;
    }
  }

  // 키 삭제 (DEL)
  async deleteKey(key) {
    try {
      const result = await this.redis.del(key); // 특정 키 삭제
      if (result > 0) {
        console.log(`키 삭제 완료: ${key}`);
      } else {
        console.log(`삭제할 키가 없습니다: ${key}`);
      }
    } catch (error) {
      console.error('키 삭제 실패:', error);
      throw error;
    }
  }

  // 모든 키 삭제 (DEL)
  async deleteAllData() {
    try {
      const keys = await this.redis.keys('*'); // 모든 키 조회
      for (const key of keys) {
        await this.redis.del(key); // 모든 키 삭제
      }
      console.log('모든 데이터 삭제 완료');
    } catch (error) {
      console.error('모든 데이터 삭제 실패:', error);
      throw error;
    }
  }

  // Redis 연결 종료
  async closeConnection() {
    try {
      await this.redis.quit();
      console.log('Redis 연결 종료');
    } catch (error) {
      console.error('Redis 연결 종료 실패:', error);
    }
  }
}

export default RedisManager;
