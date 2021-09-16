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

            return Task.FromResult(new HelloReply
            {
                Message = "You sent these bytes: " + TestArray[0].ToString() + ", " + TestArray[1].ToString() + ", " + TestArray[2].ToString() + ", " + 
                                                     TestArray[3].ToString() + ", " + TestArray[4].ToString() + "."
            });
        }
    }
}
