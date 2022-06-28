using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    class Listener
    {
        /// <summary> 버클리 소켓 인터페이스 </summary>
        Socket _listenSocket;
        //Action 단일 매개변수를 갖고 값을 리턴하지 않는 메소드를 캡슐화합니다.
        /// <summary> 수락 처리기 </summary>
        Action<Socket> _onAcceptHandler;

        /// <summary> 초기화 </summary>
        public void Init(IPEndPoint endPoint, Action<Socket> onAcceptHandler)
        {
            //AddressFamily 인터넷 프로토콜 주소 패밀리를 가져옵니다.
            //Stream 데이터 중복 및 경계 보존 없이 안정적인 양방향 연결 기반 바이트 스트림 지원
            //Tcp 전송 제어 프로토콜
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _onAcceptHandler += onAcceptHandler;

            //문지기 교육
            //Bind 소켓을 로컷 엔드포인트와 연결
            _listenSocket.Bind(endPoint);

            //영업 시작
            //Listen 소켓을 청취 상태로 둡니다.
            _listenSocket.Listen(10); //backlog 최대 대기 수

            //비동기 소켓처리를 지원하기 위한 클래스
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            RegisterAccept(args);

        }

        /// <summary> 등록 수락 </summary>
        void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;

            //   보류중                   비동기 수락
            bool pending = _listenSocket.AcceptAsync(args);//AcceptAsync 들어오는 연결 시도를 수락하기 위해 비동기 작업을 시작합니다.
            if (pending == false) //보류중이지 않으면
            {
                OnAcceptCompleted(null, args);
            }
        }

        /// <summary> 수락 완료 </summary>
        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            //SocketError 비동기 소켓 작업의 결과를 가져오거나 설정합니다.
            if (args.SocketError == SocketError.Success)
            {
                _onAcceptHandler.Invoke(args.AcceptSocket);
            }
            else
            {
                Console.WriteLine(args.SocketError.ToString());
            }

            RegisterAccept(args);
        }
    }
}
