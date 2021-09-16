using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcServer
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            string TestStr = request.Name;
            ByteString MyBytes = ByteString.CopyFromUtf8(TestStr);
            byte[] TestArray = MyBytes.ToByteArray();

            string RetMsg = "You sent these bytes: ";
            int MaxIter = TestArray.Length;
            if (MaxIter > 5) MaxIter = 5;
            for (int i = 0; i < MaxIter; i++)
            {
                RetMsg = RetMsg + TestArray[i].ToString() + ", ";
            }
            RetMsg = RetMsg.Substring(0, RetMsg.Length - 2) + ".";

            return Task.FromResult(new HelloReply
            {
                Message = RetMsg
            });
        }
    }
}
