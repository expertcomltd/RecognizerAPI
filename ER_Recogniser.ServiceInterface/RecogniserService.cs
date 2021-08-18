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
using FaceApi_OpenCV;

namespace ER_Recogniser.ServiceInterface
{
    /// <summary>
    /// Common class for initializing FaceApi
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// The log
        /// </summary>
        public static ILog Log = null;
        /// <summary>
        /// The resource folder
        /// </summary>
        public static string ResourceFolder = "";

        /// <summary>
        /// The team database repository
        /// </summary>
        public static TeamDbRepository TeamDbRepository = null;

        /// <summary>
        /// Initializes the face API.
        /// </summary>
        /// <param name="ResourceFolder">The resource folder.</param>
        /// <returns></returns>
        public static bool InitFaceAPI(string ResourceFolder)
        {
            if (!FaceApi.Init(ResourceFolder))
            {
                Log.Error("InitFaceAPI error - FaceApi.Init failed - LastError: " + FaceApi_OpenCV.FaceApi.LastError);
                return false;
            }
            //else
            //{
            //    if (!FaceApi_OpenCV.FaceApi.Train())
            //    {
            //        Log.Error("FaceApi.Train error - FaceApi.Train - LastError: " + FaceApi_OpenCV.FaceApi.LastError);
            //    } 
            //    return true;
            //}
            return true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ServiceStack.Service" />
    public partial class RecogniserService : Service
    {
        /// <summary>
        /// The log
        /// </summary>
        public static ILog Log = LogManager.GetLogger(typeof(RecogniserService));

    }
}