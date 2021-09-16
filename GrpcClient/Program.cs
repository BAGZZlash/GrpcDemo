using Google.Protobuf;
using Grpc.Net.Client;
using GrpcServer;
using OSGeo.GDAL;
using OSGeo.OGR;
using System;
using System.Threading.Tasks;

//using System.Text.Json;
//using System.Text.Json.Serialization;

using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;

namespace GrpcClient
{
    [Serializable]
    public class Shapefile : ISerializable
    {
        public Shapefile()
        {
            // Empty constructor required to compile.
        }

        public string sFilename;
        public int firstLayer = 0;
        public Boolean Loaded = false;
        public DataSource ds;
        public Layer Layer;

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

            Layer = ds.GetLayerByIndex(0);
            if (Layer == null)
            {
                Console.WriteLine("Getting the {0}th layer failed! \n", "0");
                return (false);
            }
            return (true);
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Use the AddValue method to specify serialized values.
            info.AddValue("Prop1", sFilename, typeof(string));
            info.AddValue("Prop2", firstLayer, typeof(int));
            info.AddValue("Prop3", Loaded, typeof(Boolean));
            info.AddValue("Prop4", ds, typeof(DataSource));
            //info.AddValue("Prop5", Layer, typeof(Layer));
        }

        // The special constructor is used to deserialize values.
        public Shapefile(SerializationInfo info, StreamingContext context)
        {
            // Reset the property value using the GetValue method.
            sFilename = (string)info.GetValue("Prop1", typeof(string));
            firstLayer = (int)info.GetValue("Prop2", typeof(int));
            Loaded = (Boolean)info.GetValue("Prop3", typeof(Boolean));
            ds = (DataSource)info.GetValue("Prop4", typeof(DataSource));
            //Layer =  (Layer)info.GetValue("Prop5", typeof(Layer));
        }
    }

    public class Program
    {
        static async Task Main(string[] args)
        {
            Shapefile TestShapeFile = new Shapefile();
            TestShapeFile.sFilename = "C:/Users/ccroo/ownCloud/WFLO/Vortex/Demo/Testdata/Point_4326.shp";
            Boolean LoadSuccess = TestShapeFile.LoadShapeFile(TestShapeFile.sFilename);

            Console.WriteLine("Shapefile loaded successfully: {0}", LoadSuccess);

            byte[] TestArray = { 17, 22, 19, 5, 93 };
            TestArray[0] = 18;
            ByteString MyBytes = ByteString.CopyFrom(TestArray);
            string Banane = MyBytes.ToStringUtf8();

            //byte[] jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(TestShapeFile); //Using-Zeug siehe oben!
            //MyBytes = ByteString.CopyFrom(jsonUtf8Bytes);
            //Banane = MyBytes.ToStringUtf8();

            //Banane = System.Runtime.Serialization(TestShapeFile);

            IFormatter formatter = new BinaryFormatter(); //Using-Zeug siehe oben! https://docs.microsoft.com/de-de/dotnet/api/system.runtime.serialization.serializationinfo?view=net-5.0
            MemoryStream memStream = new MemoryStream(100); //https://docs.microsoft.com/de-de/dotnet/api/system.io.memorystream?view=net-5.0
            formatter.Serialize(memStream, TestShapeFile);

            memStream.Seek(0, 0);
            TestShapeFile = (Shapefile)formatter.Deserialize(memStream);
            memStream.Close();
            Console.WriteLine("Filename: {0}", TestShapeFile.sFilename);

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
