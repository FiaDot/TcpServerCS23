using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSimulation : MonoBehaviour
{
    CharController characterController;
    Queue<ClientInputState> clientInputs;

    void Start()
    {
        // TODO : register(OnClientInputStateReceived);
    }

    void FixedUpdate()
    {
        Queue<ClientInputState> queue = clientInputs;
        ClientInputState inputState = null;

        while (queue.count > 0 && (inputState = queue.Dequeue()) != null)
        {
            characterController.ProcessInputs(inputState);
            SimulationState state = characterController.CurrentSimulationState(inputState);

            // TODO : send client(state);            
            characterController.OnServerSimulationStateReceive(0, state);
        }

    }

    public void OnClientInputStateReceived(int clientId, ClientInputState message)
    {
        // TODO : clientId 구분은 일단 패스

        // TODO : lock
        clientInputs.Add(message);
    }
}
