using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSimulation : MonoBehaviour
{
    [SerializeField]
    CharController characterController;
    Queue<ClientInputState> clientInputs = new Queue<ClientInputState>();


    SimulationState state;

    uint server_simulated_frame;
    uint server_frame_acc;

    [SerializeField]
    uint server_snapshot_rate = 6; // 6 이면 10hz

    [SerializeField]
    Transform playerServer;

    void Start()
    {
        server_simulated_frame = 0;
        server_frame_acc = 0;
        // TODO : register(OnClientInputStateReceived);
    }

    void FixedUpdate()
    {
        Queue<ClientInputState> queue = clientInputs;
        ClientInputState inputState = null;

        while (queue.Count > 0 && (inputState = queue.Dequeue()) != null)
        {
            if (inputState.simulationFrame < server_simulated_frame)
            {
                //     Debug.Log($"[SERVER] Skip frame={inputState.simulationFrame} < {server_simulated_frame}");
                continue;
            }

            server_simulated_frame++;
            server_frame_acc++;

            characterController.ProcessInputs(inputState);
            state = characterController.CurrentSimulationState(inputState);

            // 서버 처리 주기
            if (server_frame_acc < server_snapshot_rate)
                continue;
            server_frame_acc = 0;

            // TODO : send client(state);
            characterController.OnServerSimulationStateReceive(0, state);

            if (null != state)
                MovePlayerServer(state.posX);
        }
    }

    void MovePlayerServer(float posX)
    {
        Vector3 pos = playerServer.position;
        pos.x = posX;
        playerServer.position = pos;
    }



    public void OnClientInputStateReceived(int clientId, ClientInputState message)
    {
        // TODO : clientId 구분은 일단 패스

        // TODO : lock
        clientInputs.Enqueue(message);
    }
}
