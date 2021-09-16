using Google.Protobuf;
using Grpc.Net.Client;
using GrpcServer;
using OSGeo.GDAL;
using OSGeo.OGR;
using System;
using System.Threading.Tasks;

namespace GrpcClient
{
    public class Shapefile
    {
        public string sFilename;
        public int firstLayer = 0;
        public Boolean Loaded = false;
        public DataSource ds;
        private Layer Layer;

        public Boolean LoadShapeFile(string sFilename)
        {            
            Shapefile MyShapeFile = new Shapefile();
            MyShapeFile.sFilename = sFilename;
            bool RetVal = MyShapeFile.InitLayer(MyShapeFile.sFilename);
            if (firstLayer == 0) firstLayer = 1;
            return (RetVal);
        }

        public Boolean InitLayer(string sFilename)
        {
            Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "NO");
            Gdal.SetConfigOption("SHAPE_ENCODING", "");
            Ogr.RegisterAll(); // Register all drivers
            ds = Ogr.Open(sFilename, 0); // 0 means read-only, 1 means modifiable

            if (ds == null)
            {
                Console.WriteLine("Failed to open file [{0}]!", sFilename);
                return (false);
            }

            //string Banane = ds.ToString();
            //int MySize = Banane.Length;

            Layer = ds.GetLayerByIndex(0);
            if (Layer == null)
            {
                Console.WriteLine("Getting the {0}th layer failed! \n", "0");
                return (false);
            }
            return (true);
        }
    }

    public class Program
    {
        static async Task Main(string[] args)
        {
            Shapefile TestShapeFile = new Shapefile();
            TestShapeFile.sFilename = "C:/Users/ccroo/ownCloud/WFLO/Vortex/Demo/Testdata/Point_4326.shp";
            Boolean LoadSuccess = TestShapeFile.LoadShapeFile(TestShapeFile.sFilename);

            byte[] TestArray = { 17, 22, 19, 5, 93 };
            TestArray[0] = 18;
            ByteString MyBytes = ByteString.CopyFrom(TestArray);
            string Banane = MyBytes.ToStringUtf8();

            Console.WriteLine("Shapefile loaded successfully: {0}", LoadSuccess);

            var input = new HelloRequest { Name = Banane };

            Console.WriteLine("\nOpening channel.");
            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Greeter.GreeterClient(channel);

            Console.WriteLine("Sending greeter request.");
            var reply = await client.SayHelloAsync(input);

            Console.WriteLine("\nServer replied:");
            Console.WriteLine(reply.Message);

            Console.ReadLine();
        }
    }
}
