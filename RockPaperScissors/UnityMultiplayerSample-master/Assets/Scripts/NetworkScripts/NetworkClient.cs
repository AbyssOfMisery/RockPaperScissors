﻿using UnityEngine;

using Unity.Collections;
using Unity.Networking.Transport;

public class NetworkClient : MonoBehaviour
{
    public UdpNetworkDriver m_Driver;
    public NetworkConnection m_Connection;
    public bool m_Done;

    void Start ()
    {
        m_Driver = new UdpNetworkDriver(new INetworkParameter[0]);
        m_Connection = default(NetworkConnection);

        //var endpoint = NetworkEndPoint.Parse("52.15.219.197",12345);
        var endpoint = NetworkEndPoint.LoopbackIpv4;
        endpoint.Port = 12345;
        m_Connection = m_Driver.Connect(endpoint);
    }

    public void OnDestroy()
    {
        m_Connection.Disconnect(m_Driver);
        m_Driver.Dispose();
    }

    void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        if (!m_Connection.IsCreated)
        {
            if (!m_Done)
                Debug.Log("Something went wrong during connect");
            return;
        }
        
        DataStreamReader stream;
        NetworkEvent.Type cmd;

        while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) !=
               NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {

                Debug.Log("We are now connected to the server");
                //Debug.Log("We are now connected to the server");

                //var value = 1;
                //using (var writer = new DataStreamWriter(4, Allocator.Temp))
                //{
                //    writer.Write(value);
                //    m_Connection.Send(m_Driver, writer);
                //}
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                var readerCtx = default(DataStreamReader.Context);
                uint value = stream.ReadUInt(ref readerCtx);
                Debug.Log("Got the value = " + value + " back from the server");
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnected from server");
                m_Connection = default(NetworkConnection);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            var value = 1;
            using (var writer = new DataStreamWriter(4, Allocator.Temp))
            {
                writer.Write(value);
                m_Connection.Send(m_Driver, writer);
            }
        }
    }
}