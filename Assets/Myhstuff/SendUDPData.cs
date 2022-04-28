using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using System.Numerics;
using Entities.Connection;
using Entities.Connection.DataTransport;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace StreamingClient
{
    public class SendUDPData : MonoBehaviour
    {
      [SerializeField] private int outboundPortNumber = 14049;
      // [SerializeField] private TMP_Text fpsText;

          public IPAddress ipAddress = IPAddress.Parse("192.168.0.239");
          private static IPEndPoint targetPoint = null;
          private UdpTransport udpTransport = null;
          // MessagePackDataSerializer serializer = new MessagePackDataSerializer();
          // private MediaPipeFrameFormatter _formatter = new MediaPipeFrameFormatter();
          private int count = 0;
          private int secondCount = 0;
          private int time = 0;

          private void Awake()
          {
            SetEndpoint();
            udpTransport = new UdpTransport();
            SessionReceive.Joint3DBroadcastAll += SendThing;
          }

          public void SetEndpoint()
          {
            targetPoint = new IPEndPoint(ipAddress, outboundPortNumber);
          }
          
          public void SetIPAddress(string newIPAddress)
          {
            ipAddress = IPAddress.Parse(newIPAddress);
            SetEndpoint();
          }

          public async void SendThing((bool[], UnityEngine.Vector3[], UnityEngine.Quaternion[]) valueTuple)
          {
            // if (landmark == null || landmark.Count == 0)
            // {
            //   return;
            // }


            bool[] poseDetections =  valueTuple.Item1;
            Vector3[] posePositions =  landmarksToVector3(valueTuple.Item2);
            Vector4[] poseRotations =  landmarksToVector4(valueTuple.Item3);
        
            // var stringPacket = JsonConvert.SerializeObject(new PacketClass(){landmarkPose = posePositions, landmarkLeftHand = leftHandPositions, landmarkRightHand = rightHandPositions, landmarkFace = facePositions}, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            byte[] packet = CustomSerializer(new ARKitBodyTrackingFrame()
            {
              poseDetections = poseDetections,
              PosePositions = posePositions, 
              PoseRotations = poseRotations,
              TimeStamp =  GetSystemTime(),
              UserProductID = "ARKitStreamer"
            });
            // count++;
            await udpTransport.Send(packet, targetPoint);
            // var bytePacket = Encoding.ASCII.GetBytes(stringPacket);

            // await _udpClient.SendAsync(bytePacket, bytePacket.Length);
          }
          private float GetSystemTime()
          {
            double timeInSecs = (double)Stopwatch.GetTimestamp() / Stopwatch.Frequency;
            return (float)timeInSecs;
          }
          
          private void Update()
          {
            // SendThing();
          }

          private void OnDestroy()
          {
            // _udpClient.Close();
            // _udpClient.Dispose();
          }
          
          private Vector3[] landmarksToVector3(UnityEngine.Vector3[] landmarks)
          {
            Vector3[] vector3s = new Vector3[landmarks.Length]; 
            for(int i = 0; i < vector3s.Length; i++)
            {
              vector3s[i] = new Vector3(landmarks[i].x, landmarks[i].y, landmarks[i].z);
            }

            return vector3s;
          }
          
          private Vector4[] landmarksToVector4(UnityEngine.Quaternion[] landmarks)
          {
            Vector4[] vector4s = new Vector4[landmarks.Length]; 
            for(int i = 0; i < vector4s.Length; i++)
            {
              vector4s[i] = new Vector4(landmarks[i].x, landmarks[i].y, landmarks[i].z, landmarks[i].w);
            }

            return vector4s;
          }
      

          public byte[] CustomSerializer(ARKitBodyTrackingFrame arKitBodyTrackingFrame)
          {
            byte[] detections = GetByteForModule(arKitBodyTrackingFrame.poseDetections);
            byte[] positions = GetByteForModule(arKitBodyTrackingFrame.PosePositions);
            byte[] rotations = GetByteForModule(arKitBodyTrackingFrame.PoseRotations);
            byte[] timeStamp = BitConverter.GetBytes(arKitBodyTrackingFrame.TimeStamp);
            var bytePacket = Encoding.ASCII.GetBytes(arKitBodyTrackingFrame.UserProductID);
            
            byte[] all = detections.Concat(positions).Concat(rotations).Concat(timeStamp).Concat(bytePacket).ToArray();
            
            // ARKitBodyTrackingFrame arKitBodyTrackingFrame2 = CustomSerializer(all);

            return all;
          }
          
          private byte[] GetByteForModule(bool[] moduleInfo)
          {
            byte[] byteArray = new byte[moduleInfo.Length];
            for (int i = 0; i < moduleInfo.Length; i++)
            {
              byteArray[i] = BitConverter.GetBytes(moduleInfo[i])[0];
            }

            return byteArray;
          }

          private byte[] GetByteForModule(Vector3[] moduleInfo)
          {
            byte[] byteArray = new byte[moduleInfo.Length * 12];
            for (int i = 0; i < moduleInfo.Length; i++)
            {
              var xBytes = BitConverter.GetBytes(moduleInfo[i].X);
              var yBytes = BitConverter.GetBytes(moduleInfo[i].Y);
              var zBytes = BitConverter.GetBytes(moduleInfo[i].Z);
              
              byteArray[(i * 12)] =     xBytes[0];
              byteArray[(i * 12) + 1] = xBytes[1];
              byteArray[(i * 12) + 2] = xBytes[2];
              byteArray[(i * 12) + 3] = xBytes[3];
              byteArray[(i * 12) + 4] = yBytes[0];
              byteArray[(i * 12) + 5] = yBytes[1];
              byteArray[(i * 12) + 6] = yBytes[2];
              byteArray[(i * 12) + 7] = yBytes[3];
              byteArray[(i * 12) + 8] = zBytes[0];
              byteArray[(i * 12) + 9] = zBytes[1];
              byteArray[(i * 12) + 10] = zBytes[2];
              byteArray[(i * 12) + 11] = zBytes[3];
            }

            return byteArray;
          }
          
          private byte[] GetByteForModule(Vector4[] moduleInfo)
          {
            byte[] byteArray = new byte[moduleInfo.Length * 16];
            for (int i = 0; i < moduleInfo.Length; i++)
            {
              var xBytes = BitConverter.GetBytes(moduleInfo[i].X);
              var yBytes = BitConverter.GetBytes(moduleInfo[i].Y);
              var zBytes = BitConverter.GetBytes(moduleInfo[i].Z);
              var wBytes = BitConverter.GetBytes(moduleInfo[i].W);
              
              byteArray[(i * 16)] =     xBytes[0];
              byteArray[(i * 16) + 1] = xBytes[1];
              byteArray[(i * 16) + 2] = xBytes[2];
              byteArray[(i * 16) + 3] = xBytes[3];
              byteArray[(i * 16) + 4] = yBytes[0];
              byteArray[(i * 16) + 5] = yBytes[1];
              byteArray[(i * 16) + 6] = yBytes[2];
              byteArray[(i * 16) + 7] = yBytes[3];
              byteArray[(i * 16) + 8] = zBytes[0];
              byteArray[(i * 16) + 9] = zBytes[1];
              byteArray[(i * 16) + 10] = zBytes[2];
              byteArray[(i * 16) + 11] = zBytes[3];
              byteArray[(i * 16) + 12] = wBytes[0];
              byteArray[(i * 16) + 13] = wBytes[1];
              byteArray[(i * 16) + 14] = wBytes[2];
              byteArray[(i * 16) + 15] = wBytes[3];
            }

            return byteArray;
          }
          
          public ARKitBodyTrackingFrame CustomSerializer(byte[] data)
          {
            int offset = 4;
            Vector3[] positions = GetModuleForData(data, 33, ref offset);
            // Time timeStamp = new Time(GetNextFloat(data, ref offset));
            // string productID = Encoding.ASCII.GetString(data, offset, data.Length - offset);

            return new ARKitBodyTrackingFrame()
            {
              PosePositions = positions,
              // TimeStamp = timeStamp,
              // UserProductID = productID
            };
          }

          private Vector3[] GetModuleForData(byte[] data, int count, ref int offset)
          {
            Vector3[] vector3s = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
              float x = GetNextFloat(data, ref offset);
              float y = GetNextFloat(data, ref offset);
              float z = GetNextFloat(data, ref offset);
              vector3s[i] = new Vector3(x, y, z);
            }

            return vector3s;
          }
          
          private static float GetNextFloat(byte[] data, ref int offset)
          {
            float result = BitConverter.ToSingle(data, offset);
            offset += 4;
            return result;
          }
          }
    
    
              
          

      
    
    
        // [MessagePackObject(true)]
        public class ARKitBodyTrackingFrame
        {
          public bool[] poseDetections;
          public Vector3[] PosePositions;
          public Vector4[] PoseRotations;
          public float TimeStamp;
          public string UserProductID;
        }

    public class SessionReceive
          {
            public static Action<(bool[], UnityEngine.Vector3[], UnityEngine.Quaternion[])> Joint3DBroadcastAll = null;
          }
}
