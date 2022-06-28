using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Program
    {
        static Listener _listener = new Listener();

        static void OnAcceptHandler(Socket clientSocket)
        {
            try
            {
                Session session = new Session();
                session.Start(clientSocket);
                //GetBytes 파생 클래스에서 재정의 되면 지정된 문자열의 모든 문자를 바이트 시퀀스로 인코딩합니다.
                byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to MMORPG Server !");
                session.Send(sendBuff);
                //Thread 스레드를 만들고 제어하여 우선 순위를 설정하고 상태를 가져옵니다.
                //Sleep 지정된 밀리초 동안 현재 스레드를 일시 중단합니다.
                Thread.Sleep(1000);

                session.Disconnect();
                session.Disconnect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }      
        }
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

            _listener.Init(endPoint, OnAcceptHandler);
            Console.WriteLine("Listening...");

            while (true)//무한 반복
            {
                ;
            }
        }
    }
}