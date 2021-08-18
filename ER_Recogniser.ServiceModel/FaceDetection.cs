using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//https://www.danesparza.net/2014/06/things-your-dad-never-told-you-about-nlog/
//http://www.faciletechnolab.com/Blog/2017/4/30/10-tips-to-kick-start-your-back-end-api-with-servicestack

//https://www.codeproject.com/Articles/1238546/Passing-Multiple-File-Using-Rest-Sharp-To-Service

namespace ER_Recogniser.ServiceModel
{

    [Api("FaceDetection Service Description")]

    [Route("/facedetect", "GET", Summary = "GET Summary", Notes = "facedetect Notes")]
    public class FaceDetection : IReturn<FaceDetectionResponse>
    {
        [ApiMember(Name = "ApiKey", Description = "ApiKey Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string ApiKey { get; set; }

        [ApiMember(Name = "MimeType", Description = "MimeType Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string MimeType { get; set; }

        [ApiMember(Name = "ImageData", Description = "ImageData Description", ParameterType = "body", DataType = "byte[]", IsRequired = true)]
        public byte[] ImageData { get; set; }

        [ApiMember(Name = "PredictEmotion", Description = "Request to predict emotions", ParameterType = "body", DataType = "boolean", IsRequired = true)]
        public bool PredictEmotion { get; set; }
    }

    [ApiResponse]
    public class FaceDetectionResponse
    {
        [ApiResponse]
        public class ImageRect
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int W { get; set; }
            public int H { get; set; }
            public int E { get; set; }
        }

        public string Status { get; set; }


        public List<ImageRect> Faces { get; set; }
        public List<ImageRect> Eyes { get; set; }
    }

}
