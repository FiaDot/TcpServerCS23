using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharController : MonoBehaviour
{
    // 입력 정보 없을 때 
    static ClientInputState defaultInputState = new ClientInputState();

    const int STATE_CACHE_SIZE = 1024;

    // 시뮬레이션한 상태 캐싱
    SimulationState[] simulationStateCache = new SimulationState[STATE_CACHE_SIZE];

    // 클라이언트 입력 캐싱
    ClientInputState[] inputStateCache = new ClientInputState[STATE_CACHE_SIZE];

    // 서버에서 받은 시뮬레이션 상태 (서버에서 받을 때 마다 갱신됨)
    SimulationState serverSimulationState;

    // 매 틱 마다 입력 체크
    ClientInputState inputState;

    // velocity
    float velX = 0f;

    // 현재 시뮬레이션 프레임
    int simulationFrame;

    // 서버에서 받아 마지막으로 재구성된 시뮬레이션 프레임
    int lastCorrectedFrame;

    [SerializeField]
    ServerSimulation server;

    void Start()
    {
        // register(OnServerSimulationStateReceive);
    }

    void Update()
    {
        // TODO : 서버는 skip

        inputState = new ClientInputState
        {
            left = Input.GetKey(KeyCode.A),
            right = Input.GetKey(KeyCode.A)
        };
    }

    void FixedUpdate()
    {
        // TODO : 서버는 skip

        // 물리 tick 단위로 업데이트        
        inputState.simulationFrame = simulationFrame;

        // 로컬 클라이언트의 입력 처리 
        ProcessInputs(inputState);

        // TODO : 서버로 보내기
        // server.Send(inputState);
        server.OnClientInputStateReceived(0, inputState);

        // 서버에서 받은 상태 정보가 있다면 재구성 체크 및 실행
        if (serverSimulationState != null)
            Reconciliate();

        // 현재 상태를 가져와서 저장 
        SimulationState simulationState = CurrentSimulationState(inputState);

        int cacheIndex = simulationFrame % STATE_CACHE_SIZE;
        simulationStateCache[cacheIndex] = simulationState;
        inputStateCache[cacheIndex] = inputState;

        // 다음 프레임으로 증가
        ++simulationFrame;
    }

    // Common
    void ProcessInputs(ClientInputState state)
    {
        if (state == null)
            state = defaultInputState;

        float vertical = 0f;

        bool moveLeft = state.left;
        bool moveRight = state.right;

        // 왼쪽이면 -1, 오른쪽이면 1, 아니면 0
        vertical = moveRight ? 1 : moveLeft ? -1 : 0;

        transform.Translate(new Vector3(0f, 0f, vertical));
    }

    // Common
    SimulationState CurrentSimulationState(ClientInputState inputState)
    {
        return new SimulationState
        {
            posX = transform.position.x,
            velX = velX,
            simulationFrame = inputState.simulationFrame
        };
    }


    void Reconciliate()
    {
        // 서버에서 받은 상태가 옛날 프레임이라면 스킵!
        if (serverSimulationState.simulationFrame <= lastCorrectedFrame)
            return;

        // 버퍼에서 위치 찾기
        int cacheIndex = serverSimulationState.simulationFrame % STATE_CACHE_SIZE;

        // 캐싱된 클라이언트 입력과 시뮬레이션된 상태를 가져옴
        ClientInputState cachedInputState = inputStateCache[cacheIndex];
        SimulationState cachedSimulationState = simulationStateCache[cacheIndex];

        // 입력이나 시뮬레이션 상태가 없다면 서버에서 받은 값으로 적용
        if (cachedInputState == null || cachedSimulationState == null)
        {
            Vector3 pos = transform.position;
            pos.x = serverSimulationState.posX;
            transform.position = pos;
            // transform.position = serverSimulationState.position;

            velX = serverSimulationState.velX;
            // velocity = serverSimulationState.velocity;

            // 서버에서 받은 프레임으로 저장
            lastCorrectedFrame = serverSimulationState.simulationFrame;
            return;
        }

        float differenceX = Mathf.Abs(cachedSimulationState.posX - serverSimulationState.posX);

        // 보정이 필요하기 전에 클라이언트의 예측이 서버의 위치에서 벗어날 수 있도록 
        // 허용할 거리(단위)입니다. 
        float tolerance = 0F;

        // 오차 수정이 필요한 경우라면...
        if (differenceX > tolerance) //  || differenceY > tolerance || differenceZ > tolerance)
        {
            // 서버에서 받은 시뮬레이션 정보로 플레이어 정보(위치, 속도) 변경 
            Vector3 pos = transform.position;
            pos.x = serverSimulationState.posX;
            transform.position = pos;
            //transform.position = serverSimulationState.position;

            velX = serverSimulationState.velX;
            // velocity = serverSimulationState.velocity;

            // Declare the rewindFrame as we're about to resimulate our cached inputs. 
            int rewindFrame = serverSimulationState.simulationFrame;

            // 서버 시뮬레이션 프레임부터 클라이언트에서 수행한 시뮬레이션 프레임까지
            // 시뮬레이션 다시 수행
            while (rewindFrame < simulationFrame)
            {
                // Determine the cache index 
                int rewindCacheIndex = rewindFrame % STATE_CACHE_SIZE;

                // Obtain the cached input and simulation states.
                ClientInputState rewindCachedInputState = inputStateCache[rewindCacheIndex];
                SimulationState rewindCachedSimulationState = simulationStateCache[rewindCacheIndex];

                // If there's no state to simulate, for whatever reason, 
                // increment the rewindFrame and continue.
                if (rewindCachedInputState == null || rewindCachedSimulationState == null)
                {
                    ++rewindFrame;
                    continue;
                }

                // Process the cached inputs. 
                ProcessInputs(rewindCachedInputState);

                // Replace the simulationStateCache index with the new value.
                SimulationState rewoundSimulationState = CurrentSimulationState(rewindCachedInputState);
                rewoundSimulationState.simulationFrame = rewindFrame;
                simulationStateCache[rewindCacheIndex] = rewoundSimulationState;

                // Increase the amount of frames that we've rewound.
                ++rewindFrame;
            }
        }

        // Once we're complete, update the lastCorrectedFrame to match.
        // NOTE: Set this even if there's no correction to be made. 
        lastCorrectedFrame = serverSimulationState.simulationFrame;
    }

    public void OnServerSimulationStateReceive(int clientId, SimulationState message)
    {
        // 현재 클라이언트 시뮬레이션 프레임보다 이전일 경우에만 업데이트 
        // 즉, 클라이언트보다 미리 계산된 내용은 있을 수 없음
        if (serverSimulationState?.simulationFrame < message.simulationFrame)
        {
            serverSimulationState = message;
        }
    }

}
