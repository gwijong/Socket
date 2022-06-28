using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DummyClient
{
    class Program
    {
        static void Main(string[] args)
        {
            //DMS (Domain Name System)
            //Dns 간단한 도메인 이름 확인 기능 제공
            //GetHostName 로컬 컴퓨터의 호스트 이름을 가져옵니다
            string host = Dns.GetHostName();
            //IPHostEntry 인터넷 호스트 주소 정보에 대한 컨테이너 클래스 제공
            //GetHostEntry IPHostEntry 인스턴스에 대한 호스트 이름 또는 IP 주소를 확인합니다.
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            //IPAddress 인터넷 프로토콜 주소 제공
            //AddressList 호스트와 연결된 IP 주소 목록을 가져오거나 설정합니다.
            IPAddress ipAddr = ipHost.AddressList[0];
            //IPEndPoint 네트워크 끝점을 IP 주소 및 포트 번호로 나타냅니다.
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            while (true)
            {
                //휴대폰 설정
                //AddressFamily 인터넷 프로토콜 주소 패밀리를 얻습니다.
                //Stream 데이터 중복 및 경계 보존 없이 안정적인 양방향 연결 기반 바이트 스트림을 지원합니다.
                //Tcp 전송 제어 프로토콜
                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    //문지기한테 입장 문의
                    //Connect 원격 호스트에 대한 연결을 설정합니다.
                    socket.Connect(endPoint);
                    //Console 콘솔 응용 프로그램에 대한 표준 입력 출력 및 오류 스트림을 나타냅니다. 이 클래스는 상속될 수 없습니다.
                    //WriteLine 표준 출력 스트림에 현재 줄 종결자가 뒤따르는 지정된 문자열 값을 씁니다.
                    Console.WriteLine($"Connected To{socket.RemoteEndPoint.ToString()}");

                    //보낸다
                    for (int i = 0; i < 5; i++)
                    {
                        //GetBytes 파생 클래스에서 재정이 될 때 지정된 문자열의 모든 문자를 바이트 시퀀스로 인코딩합니다.
                        byte[] sendBuff = Encoding.UTF8.GetBytes($"Hello World!{i}");
                        //Send 연결된 소켓에 데이터를 보냅니다.
                        int sendBytes = socket.Send(sendBuff);
                    }



                    //받는다
                    byte[] recvBuff = new byte[1024];
                    //Receive 바인딩된 소켓에서 수신 버퍼로 데이터를 수신합니다.
                    int resvBytes = socket.Receive(recvBuff);
                    //GetString 파생 클레스에서 재정의 되면 지정된 바이트 배열의 바이트 시퀀스를 문자열로 디코딩합니다.
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, resvBytes);
                    Console.WriteLine($"[From Server] {recvData}");

                    //나간다
                    //Shutdown 소켓에서 보내기 및 받기 비활성하
                    socket.Shutdown(SocketShutdown.Both);
                    //소켓 연결을 닫고 관련된 모든 리소스를 해제합니다.
                    socket.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                //Thread 스레드를 만들고 제어하여 우선 순위를 설정하고 상태를 가져옵니다.
                //Sleep 지정된 밀리초 동안 현재 스레드를 일시 중단합니다.
                Thread.Sleep(100);
            }
        }
    }
}
