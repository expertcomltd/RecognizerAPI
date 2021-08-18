using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ER_Recogniser.ServiceModel
{
    public enum EmotionIDs
    {
        unknown = -1,
        angry = 0,
        disgust = 1,
        fear = 2,
        happy = 3,
        neutral = 4,
        sad = 5,
        surprise = 6
    }

    /// <summary>
    /// Helper class for parsing emotion enum from a string representation
    /// </summary>
    public class EmotionParser
    {
        public static EmotionIDs GetEmotion(string stremotion)
        {
            EmotionIDs emotionid;
            if (Enum.TryParse(stremotion.ToLower(), true, out emotionid))
            {
                return emotionid;
            }
            return EmotionIDs.unknown;
        }
    }

    [Api("EmotionDetection Service Description")]

    [Route("/emotiondetect", "GET", Summary = "GET Summary", Notes = "emotiondetect Notes")]
    public class EmotionDetection : IReturn<EmotionDetectionResponse>
    {
        [ApiMember(Name = "ApiKey", Description = "ApiKey Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string ApiKey { get; set; }

        [ApiMember(Name = "MimeType", Description = "MimeType Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string MimeType { get; set; }

        [ApiMember(Name = "ImageData", Description = "ImageData Description", ParameterType = "body", DataType = "byte[]", IsRequired = true)]
        public byte[] ImageData { get; set; }
    }

    [ApiResponse]
    public class EmotionDetectionResponse
    {
        [ApiResponse]
        public class ImageRect
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int W { get; set; }
            public int H { get; set; }
        }

        public string Status { get; set; }


        public List<ImageRect> Faces { get; set; }
        public List<ImageRect> Eyes { get; set; }
    }




}
