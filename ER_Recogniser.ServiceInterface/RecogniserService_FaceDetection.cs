using ServiceStack;
using ServiceStack.Logging;
using ER_Recogniser.ServiceModel;
using System;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.SqlServer;
using ServiceStack.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using FaceDetection;
using VideoApi;
using EmotionApi_PyProc;

namespace ER_Recogniser.ServiceInterface
{
    /// <summary>
    /// Face detection methods for handling web requests.
    /// </summary>
    /// <seealso cref="ServiceStack.Service" />
    public partial class RecogniserService : Service
    {

        /// <summary>
        /// Detect faces and eyes in an image.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public object Any(ER_Recogniser.ServiceModel.FaceDetection request)
        {

            if (request == null || request.ImageData == null || request.ImageData.Length < 64)
            {
                Log.DebugFormat("Detection image type: {0}, length: {1}", request.MimeType, request.ImageData.Length);
                return new FaceDetectionResponse() { Status = "Error: Empty request" };
            }
            else
            {
                //Log.DebugFormat("Initializing settings with object: {0}", request.Dump());
                Log.DebugFormat("Detection image type: {0}, length: {1}", request.MimeType, request.ImageData.Length);

                /*
                //saving image to temp file from byte array
                string tempImagePath = System.IO.Path.Combine(Common.ImageDir, Guid.NewGuid().ToString() + "." + request.MimeType);
                System.IO.File.WriteAllBytes(tempImagePath, request.ImageData);
                return DetectFace(tempImagePath);
                //*/

                return DetectFace(request.ImageData,request.PredictEmotion);
            }
        }

        /// <summary>
        /// Detect faces and eyes in an image.
        /// </summary>
        /// <param name="tempImagePath">The temporary image path.</param>
        /// <returns></returns>
        FaceDetectionResponse DetectFace(string tempImagePath, bool predictemotion)
        {
            byte[] img = System.IO.File.ReadAllBytes(tempImagePath);
            return DetectFace(img, predictemotion);
        }
        /// <summary>
        /// Detect faces and eyes in an image.
        /// </summary>
        /// <param name="img">The img.</param>
        /// <returns></returns>
        FaceDetectionResponse DetectFace(byte[] img,bool predictemotion)
        {
            FaceDetectionResponse response = new FaceDetectionResponse { Status = "OK" };
            try
            {
                List<DetectedFace_PyProc> faces_pp = new List<DetectedFace_PyProc>();

                if(EmotionDetector_PyProc.Instance.DetectFaces(img,faces_pp))
                {
                    response.Eyes = new List<FaceDetectionResponse.ImageRect>();
                    response.Faces = new List<FaceDetectionResponse.ImageRect>();

                    if (faces_pp.Count > 0)
                    {
                        foreach (var face in faces_pp)
                        {
                            int emot = -1;
                            if (predictemotion)
                            {
                                emot = (int)EmotionParser.GetEmotion(face.E);
                            }
                            response.Faces.Add(new FaceDetectionResponse.ImageRect() { X = face.X, Y = face.Y, W = face.W, H = face.H, E = emot });
                            response.Eyes.Add(new FaceDetectionResponse.ImageRect() { X = face.LEX - 2, Y = face.LEY - 2 , W = face.LEX + 2, H = face.LEY + 2 });
                            response.Eyes.Add(new FaceDetectionResponse.ImageRect() { X = face.REX - 2, Y = face.REY - 2, W = face.REX + 2, H = face.REY + 2 });
                        }
                    }
                    Log.Debug("FaceDetection result: faces: " + response.Faces.Count.ToString() + ", eyes: " + response.Eyes.Count.ToString());

                }
                else
                {
                    Log.Debug("FaceDetection result: Error: detection failed.");
                    response.Status = "Error: detection failed.";
                }

                /*
                List<DetectedFace> faces = new List<DetectedFace>();
                string facecascadefile = "Res/haarcascade_frontalcatface_alt.xml";
                if (FaceApi_OpenCV.FaceApi.DetectFaces(img, faces, facecascadefile))
                {
                    response.Eyes = new List<FaceDetectionResponse.ImageRect>();
                    response.Faces = new List<FaceDetectionResponse.ImageRect>();

                    if (faces.Count > 0)
                    {
                        foreach (var face in faces)
                        {
                            int emot = -1;
                            if (predictemotion)
                            {
                                System.Drawing.Rectangle facerect = new System.Drawing.Rectangle(face.face.X, face.face.Y, face.face.Width, face.face.Height);
                                byte[] pic = CropImage.CopyRect(img, facerect, true, 48, 48);
                                string emotion;
                                EmotionDetector_PyProc.Instance.PredictEmotion(pic, out emotion);
                                emot = (int)EmotionParser.GetEmotion(emotion);
                            }
                            response.Faces.Add(new FaceDetectionResponse.ImageRect() { X = face.face.X, Y = face.face.Y, W = face.face.Width, H = face.face.Height, E = emot });
                            foreach (var rect in face.eyes)
                            {
                                response.Eyes.Add(new FaceDetectionResponse.ImageRect() { X = rect.X, Y = rect.Y, W = rect.Width, H = rect.Height });
                            }
                        }
                    }
                    Log.Debug("FaceDetection result: faces: " + response.Faces.Count.ToString() + ", eyes: " + response.Eyes.Count.ToString());
                }
                else
                {
                    Log.Debug("FaceDetection result: Error: detection failed.");
                    response.Status = "Error: detection failed.";
                }
                */
            }
            catch (Exception ex)
            {
                response.Status = "Error:" + ex;
                Log.Error("FaceDetection result: Error: " + ex);
            }
            return response;
        }
    }
}
