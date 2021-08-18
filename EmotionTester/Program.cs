using EmotionApi_Keras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmotionTester
{
    class Program
    {
        static void Main(string[] args)
        {
            EmotionDetector.OnDebugMessage += EmotionDetector_OnDebugMessage;

            EmotionDetector.Instance.Init(@"c:\Projects\Embeddingpython\Resource\fer2013_emotions.h5");

            byte[] buff = new byte[48 * 48];

            string error;

            int emot = EmotionDetector.Instance.Predict(buff, out error);
            Console.WriteLine("RESULT: " + emot + ", ERROR: " + error);


        }

        private static void EmotionDetector_OnDebugMessage(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
