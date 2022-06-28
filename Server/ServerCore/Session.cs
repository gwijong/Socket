using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
//열람모드: 컨트롤 시프트 스페이스
namespace ServerCore
{
    class Session
    {
        /// <summary> 버클리 소켓 인터페이스를 구현합니다. </summary>
        Socket _socket; 
        int _disconnected = 0;

        /// <summary> 락용 오브젝트 </summary>
        object _lock = new object();
        /// <summary> 부호 없는 8비트 정수(255) 큐 </summary>
        Queue<byte[]> _sendQueue = new Queue<byte[]>();
        /// <summary> 보류중 </summary>
        bool _pending = false;
        /// <summary> 비동기 소켓처리를 지원하기 위한 클래스 </summary>
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs(); 

        public void Start(Socket socket)
        {
            _socket = socket;
            SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs(); //비동기 소켓처리를 지원하기 위한 클래스
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted); //비동기 작업을 완료하는데 사용되는 이벤트
            recvArgs.SetBuffer(new byte[1024], 0, 1024); //비동기 소켓 메서드와 함께 사용할 데이터 버퍼를 설정합니다.

            //비동기 작업을 완료하는데 사용되는 이벤트 += 이벤트가 데이터를 제공할 때 이벤트를 처리할 메서드를 나타냅니다.
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted); 

            RegisterRecv(recvArgs);
        }

        /// <summary> 보내기 </summary>
        public void Send(byte[] sendBuff)//부호없는 정수(255) 배열 매개변수
        {
            //lock 문은 지정된 개체에 대한 상호 배제 잠금을 획득하여 명령문 블록을 실행한 다음, 잠금을 해제합니다.
            //잠금이 유지되는 동안 잠금을 보유하는 스레드는 잠금을 다시 획득하고 해제할 수 있습니다.
            //다른 스레드는 잠금을 획득할 수 없도록 차단되며 잠금이 해제될 때까지 대기합니다.
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuff); //큐의 끝에 객체를 추가
                if (_pending == false) //보류중이지 않으면
                {
                    RegisterSend();//레지스터 보내기
                }
            }
        }

        /// <summary> 연결 끊기 </summary>
        public void Disconnect()
        {
            //여러 스레드가 공유하는 변수에 대한 원자적 연산을 제공합니다.
            if(Interlocked.Exchange(ref _disconnected, 1) == 1) //32비트 부호 있는 정수를 지정된 값으로 설정하고 원자성 연산으로 원래 값을 반환합니다.
            {
                return;
            }
            _socket.Shutdown(SocketShutdown.Both); //소켓에서 보내기 및 받기 비활성화
            _socket.Close(); //소켓 연결을 닫고 관련된 모든 리소스를 해제합니다.
        }

        #region 네트워크 통신

        /// <summary> 레지스터 보내기</summary>
        void RegisterSend()
        {
            _pending = true; //보류중
            byte[] buff = _sendQueue.Dequeue(); //큐의 시작 부분에 있는 객체를 제거하고 반환
            _sendArgs.SetBuffer(buff, 0, buff.Length); //비동기 소켓 메서드와 함께 사용할 데이터 버퍼를 설정합니다.

            bool pending = _socket.SendAsync(_sendArgs); //연결된 소켓 객체에 비동기적으로 데이터를 보냅니다.
            if(pending == false) //보류중이지 않으면
            {
                OnSendCompleted(null, _sendArgs);
            }
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                //소켓 작업에서 전송한 바이트 수를 포함하는 정수가 0보다 크고
                //비동기 소켓 작업의 결과를 가져오거나 설정한 값이 == 소켓 작업 성공이면
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        if (_sendQueue.Count > 0)//큐가 0보다 크면
                        {
                            RegisterSend(); //레지스터 보내기
                        }
                        else // 큐가 0보다 작으면
                        {
                            _pending = false; //보류중이지 않음
                        }
                        
                    }
                    catch (Exception e) //예외가 발생하면
                    {
                        Console.WriteLine($"OnSendCompleted Failed {e}"); //콘솔에 예외 출력
                    }
                }
                else //소켓 작업에서 전송한 바이트가 0보다 같거나 작거나 소켓 작업이 실패한 경우
                {
                    Disconnect(); //연결 끊기
                }
            }
        }

        /// <summary> 레지스터 받기</summary>
        void RegisterRecv(SocketAsyncEventArgs args) 
        {
            bool pending = _socket.ReceiveAsync(args); //연결된 소켓 객체로부터 데이터를 받기 위한 비동기 요청을 시작합니다.
            if(pending == false) //보류중이지 않으면
            {
                OnRecvCompleted(null, args);//받기 완료
            }
        }

        /// <summary> 받기 완료</summary>
        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            //바이트 전송이 0보다 크고 소켓작업이 성공하면
            if(args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    //GetString 파생 클레스에서 재정의 되면 지정된 바이트 배열의 바이트 시퀀스를 문자열로 디코딩합니다.
                    //Buffer 비동기 소켓 메서드와 함께 사용할 데이터 버퍼를 가져옵니다.
                    //Offset SocketAsyncEventArg.Buffer 속성이 참조하는 데이터 버퍼에 오프셋을 바이트 단위로 가져옵니다.
                    //BytesTransferred 소켓 작업에서 전송된 바이트 수를 가져옵니다
                    string recvData = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
                    Console.WriteLine($"[From Clint]{recvData}");
                    RegisterRecv(args);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Failed {e}");
                }

            }
            else //바이트 전송이 0보다 같거나 작거나 소켓작업이 실패하면
            {
                Disconnect(); //연결 끊기
            }
        }
        #endregion
    }
}
