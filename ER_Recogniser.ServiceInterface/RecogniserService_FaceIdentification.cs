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
using System.IO;
using FaceApi_OpenCV;
using VideoApi;
using EmotionApi_PyProc;

namespace ER_Recogniser.ServiceInterface
{
    /// <summary>
    /// Service methods for handling FaceApi web requests
    /// </summary>
    /// <seealso cref="ServiceStack.Service" />
    public partial class RecogniserService : Service
    {
        /// <summary>
        /// Reconises a team memeber from a given group in an image
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public object Any(FaceRecognition request)
        {
            //Log.DebugFormat("Initializing settings with object: {0}", request.Dump());
            var resp = new FaceRecognitionResponse { Status = "OK",EmotionID = -1 };
            try
            {
                //todo: check db
                int TeamMemberID = 0;
                double distance;
                if (FaceApi.Predict(request.ApiKey, request.TeamGroupID, request.ImageData, out TeamMemberID, out distance))
                {
                    resp.TeamMemberID = TeamMemberID;
                    resp.Distance = distance;
                    Log.Debug(string.Format("FaceIdentification prediciton result label:{0}, distance:{1}, Found:{2}", TeamMemberID, distance, TeamMemberID >= 0));
                    if (TeamMemberID > 0)
                    {
                        resp.TeamMember = Common.TeamDbRepository.GetTeamMember(TeamMemberID);
                    }
                    else
                    {
                        resp.TeamMember = null;
                    }

                }
                else
                {
                    Log.Error("FaceIdentification error - FaceApi.LastError: " + FaceApi.LastError);
                    resp.Status = "Error: " + FaceApi.LastError;
                }

                int emot = -1;
                if (request.PredictEmotion)
                {
                    //System.Drawing.Rectangle facerect = new System.Drawing.Rectangle(0, 0, 0, 0);
                    //byte[] pic = CropImage.CopyRect(request.ImageData, facerect, true, 48, 48);
                    //EmotionDetector



                    string emotion;
                    EmotionDetector_PyProc.Instance.PredictEmotion(request.ImageData, out emotion);
                    emot = (int)EmotionParser.GetEmotion(emotion);
                }
                resp.EmotionID = emot;

            }
            catch (Exception ex)
            {
                Log.Error("FaceIdentification error:" + ex);
                resp.Status = "Error: " + ex;
            }
            return resp;
        }


        #region Teams


        /// <summary>
        /// Trains a team group
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public object Any(TrainTeam request)
        {
            var resp = new TrainTeamResponse() { Status = "OK" };
            try
            {
                if (!FaceApi.TrainGroup(request.ApiKey, request.TeamGroupID))
                {
                    resp.Status = "Error: " + FaceApi.LastError;
                    Log.Error("DeleteTeam error:" + FaceApi.LastError);
                }
                return resp;
            }
            catch (Exception ex)
            {
                Log.Error("TrainTeam error:" + ex);
                resp.Status = "Error: " + ex;
            }
            return resp;
        }

        /// <summary>
        /// Gets team data from database
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public object Any(GetTeams request)
        {
            var resp = new GetTeamsResponse() { Status = "OK" };
            try
            {
                resp.Teams = Common.TeamDbRepository.GetTeams(request.ApiKey);
                //log teams
                if (resp.Teams != null)
                {
                    foreach (var team in resp.Teams)
                    {
                        Log.Debug(team.ToString());
                    }
                }
                return resp;
            }
            catch (Exception ex)
            {
                Log.Error("GetTeams error:" + ex);
                resp.Status = "Error: " + ex;
            }
            return resp;
        }

        /// <summary>
        /// Creates a team in the database and in the filesystem
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public object Any(CreateTeam request)
        {
            var resp = new CreateTeamResponse() { Status = "OK" };
            try
            {
                var team = new Team()
                {
                    Name = request.Name,
                    TeamID = Guid.NewGuid(),
                    ApiKey = Guid.Parse(request.ApiKey),
                };

                Log.Debug("CreateTeam" + team.ToString());

                if (Common.TeamDbRepository.CreateTeam(team))
                {
                    if (!FaceApi.CreateTeamGroup(request.ApiKey, team.GetTeamIDString()))
                    {
                        resp.Status = "Error: " + FaceApi.LastError;
                        Log.Error("CreateTeam error:" + FaceApi.LastError);
                    }
                }
                else
                {
                    resp.Status = "Error: DB insert error";
                    Log.Error("CreateTeam error: Error: DB insert error");
                }
                return resp;
            }
            catch (Exception ex)
            {
                Log.Error("CreateTeam error:" + ex);
                resp.Status = "Error: " + ex;
            }
            return resp;
        }

        /// <summary>
        /// Deletes a team from the database and the filesystem
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public object Any(DeleteTeam request)
        {
            var resp = new DeleteTeamResponse() { Status = "OK" };
            try
            {
                if (Common.TeamDbRepository.DeleteTeam(request.Team))
                {
                    if (!FaceApi.DeleteTeamGroup(request.ApiKey, request.Team.GetTeamIDString()))
                    {
                        resp.Status = "Error: " + FaceApi.LastError;
                        Log.Error("DeleteTeam error:" + FaceApi.LastError);
                    }
                }
                else
                {
                    resp.Status = "Error: Failed to delete from DB";
                }
                Log.Debug("Team deleted:" + request.Team.ToString());
                return resp;
            }
            catch (Exception ex)
            {
                Log.Error("DeleteTeamResponse error:" + ex);
                resp.Status = "Error: " + ex;
            }
            return resp;
        }

        /// <summary>
        /// Changes team parameters in the database.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public object Any(UpdateTeam request)
        {
            var resp = new UpdateTeamResponse() { Status = "OK" };
            try
            {
                if (!Common.TeamDbRepository.UpdateTeam(request.Team))
                {
                    resp.Status = "Error";
                }
                Log.Debug("Team updated:" + request.Team.ToString());
                return resp;
            }
            catch (Exception ex)
            {
                Log.Error("UpdateTeamResponse error:" + ex);
                resp.Status = "Error: " + ex;
            }
            return resp;
        }
        #endregion

        #region TeamMembers
        /// <summary>
        /// Gets team members list from the database.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public object Any(GetTeamMembers request)
        {
            var resp = new GetTeamMembersResponse() { Status = "OK" };
            try
            {
                resp.TeamMembers = Common.TeamDbRepository.GetMembersByTeam(request.TeamGroupID);
                //log TeamMembers
                if (resp.TeamMembers != null)
                {
                    foreach (var TeamMember in resp.TeamMembers)
                    {
                        Log.Debug(TeamMember.ToString());
                    }
                }
                return resp;
            }
            catch (Exception ex)
            {
                Log.Error("GetTeamMembers error:" + ex);
                resp.Status = "Error: " + ex;
            }
            return resp;
        }

        /// <summary>
        /// Creates a team member in the databse and in the filesystem.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public object Any(CreateTeamMember request)
        {
            var resp = new CreateTeamMemberResponse() { Status = "OK" };
            try
            {
                Guid guidTeamGroupID = Guid.Parse(request.TeamGroupID);

                var teammember = new TeamMember()
                {
                    Name = request.Name,
                    TeamID = guidTeamGroupID,
                };
                int TeamMemberID = Common.TeamDbRepository.CreateTeamMember(teammember);
                resp.TeamMember = teammember;
                if (TeamMemberID > 0)
                {
                    if (!FaceApi.CreateMember(request.ApiKey, request.TeamGroupID, TeamMemberID))
                    {
                        Log.Error("CreateTeamMember error - FaceApi.AddImageToUser LastError: " + FaceApi.LastError);
                        resp.Status = "Error: " + FaceApi.LastError;
                    }
                }
                return resp;
            }
            catch (Exception ex)
            {
                Log.Error("CreateTeamMember error:" + ex);
                resp.Status = "Error: " + ex;
            }
            return resp;
        }

        /// <summary>
        /// Deletes a team member from the database and filesystem.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public object Any(DeleteTeamMember request)
        {
            var resp = new DeleteTeamMemberResponse() { Status = "OK" };
            try
            {
                if (Common.TeamDbRepository.DeleteTeamMember(request.TeamMember))
                {
                    if (!FaceApi_OpenCV.FaceApi.DeleteMember(request.ApiKey, request.TeamMember.GetTeamIDString(), request.TeamMember.TeamMemberID))
                    {
                        Log.Error("DeleteTeamMember error - FaceApi.AddImageToUser LastError: " + FaceApi_OpenCV.FaceApi.LastError);
                        resp.Status = "Error: " + FaceApi_OpenCV.FaceApi.LastError;
                    }
                    Log.Debug("TeamMember deleted:" + request.TeamMember.ToString());
                }
                return resp;
            }
            catch (Exception ex)
            {
                Log.Error("DeleteTeamMemberResponse error:" + ex);
                resp.Status = "Error: " + ex;
            }
            return resp;
        }

        /// <summary>
        /// Updates a team member's paramters in the database.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public object Any(UpdateTeamMember request)
        {
            var resp = new UpdateTeamMemberResponse() { Status = "OK" };
            try
            {
                resp.TeamMemberID = Common.TeamDbRepository.UpdateTeamMember(request.TeamMember);
                Log.Debug("TeamMember updated:" + request.TeamMember.ToString());
                return resp;
            }
            catch (Exception ex)
            {
                Log.Error("UpdateTeamMemberResponse error:" + ex);
                resp.Status = "Error: " + ex;
            }
            return resp;
        }
        #endregion

        #region FaceImages
        /// <summary>
        /// Gets the list of image descriptors of a team member from the database.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public object Any(GetFaceImages request)
        {
            var resp = new GetFaceImagesResponse() { Status = "OK" };
            try
            {
                resp.FaceImages = Common.TeamDbRepository.GetFaceImagesByTeamMember(request.TeamMemberID);
                //log FaceImages
                if (resp.FaceImages != null)
                {
                    foreach (var FaceImage in resp.FaceImages)
                    {
                        Log.Debug(FaceImage.ToString());
                    }
                }
                return resp;
            }
            catch (Exception ex)
            {
                Log.Error("GetFaceImages error:" + ex);
                resp.Status = "Error: " + ex;
            }
            return resp;
        }

        /// <summary>
        /// Adds a face image to a team member's collection (databse,filesystem).
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public object Any(CreateFaceImage request)
        {
            var resp = new CreateFaceImageResponse() { Status = "OK" };
            try
            {
                FaceImage faceimage = new FaceImage()
                {
                    Comment = request.Comment,
                    TeamID = Guid.Parse(request.TeamGroupID),
                    TeamMemberID = request.TeamMemberID,
                };

                string ImageID;

                if (FaceApi.AddImageToMember(request.ApiKey, request.TeamGroupID, request.TeamMemberID, request.ImageData, out ImageID))
                {
                    faceimage.FaceImageID = Guid.Parse(ImageID);
                    if (!Common.TeamDbRepository.CreateFaceImage(faceimage))
                    {
                        resp.Status = "Error: Failed to insert record to db";
                    }
                    else
                    {
                        resp.FaceImage = faceimage;
                        Log.Debug(resp.FaceImage.ToString());
                    }
                }
                else
                {
                    Log.Error("CreateFaceImage error - FaceApi.AddImageToUser LastError: " + FaceApi_OpenCV.FaceApi.LastError);
                    resp.Status = "Error: " + FaceApi_OpenCV.FaceApi.LastError;
                }

                if (request.TrainGroup)
                {
                    if (!FaceApi.TrainGroup(request.ApiKey, request.TeamGroupID))
                    {
                        Log.Error("CreateFaceImage error - FaceApi.Train - LastError: " + FaceApi_OpenCV.FaceApi.LastError);
                        resp.Status = "Error: " + FaceApi_OpenCV.FaceApi.LastError;
                    }
                }

                return resp;
            }
            catch (Exception ex)
            {
                Log.Error("CreateFaceImage error:" + ex);
                resp.Status = "Error: " + ex;
            }
            return resp;
        }

        /// <summary>
        /// Delete a face image of a team member fromt the database and filesystem.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public object Any(DeleteFaceImage request)
        {
            var resp = new DeleteFaceImageResponse() { Status = "OK" };
            try
            {
                if (Common.TeamDbRepository.DeleteFaceImage(request.FaceImage))
                {
                    if (!FaceApi.DeleteImageFromMember(request.ApiKey, request.FaceImage.GetTeamIDString(), request.FaceImage.TeamMemberID, request.FaceImage.GetFaceImageIDString()))
                    {
                        resp.Status = "Error: " + FaceApi.LastError;
                    }
                    else
                    {
                        Log.Debug("FaceImage deleted:" + request.FaceImage.ToString());
                    }
                }
                else
                {
                    resp.Status = "Error: Failed to delete faceimage from db";
                }


                return resp;
            }
            catch (Exception ex)
            {
                Log.Error("DeleteFaceImageResponse error:" + ex);
                resp.Status = "Error: " + ex;
            }
            return resp;
        }

        /// <summary>
        /// Updates a face image of a team member
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public object Any(UpdateFaceImage request)
        {
            var resp = new UpdateFaceImageResponse() { Status = "OK" };
            try
            {
                //resp.FaceImageID = Common.TeamDbRepository.UpdateFaceImage(request.FaceImage);

                //string ImageDir = Path.Combine(Common.ImageDir + "\\" + request.FaceImage.TeamID + "_" + request.FaceImage.TeamMemberID);
                //if (!Directory.Exists(ImageDir))
                //{
                //    Directory.CreateDirectory(ImageDir);
                //}
                //string filename = ImageDir + "\\" + request.FaceImage.FaceImageID.ToString() + ".png";
                //File.WriteAllBytes(filename, request.ImageData);

                //if (!FaceApi_OpenCV.FaceApi.AddImageToUser(request.FaceImage.TeamMemberID, request.ImageData))
                //{
                //    Log.Error("UpdateFaceImage error - FaceApi.AddImageToUser LastError: " + FaceApi_OpenCV.FaceApi.LastError);
                //    resp.Status = "Error: " + FaceApi_OpenCV.FaceApi.LastError;
                //}

                //if (!FaceApi_OpenCV.FaceApi.Train())
                //{
                //    Log.Error("UpdateFaceImage error - FaceApi.Train - LastError: " + FaceApi_OpenCV.FaceApi.LastError);
                //    resp.Status = "Error: " + FaceApi_OpenCV.FaceApi.LastError;
                //}

                Log.Debug("FaceImage updated:" + request.FaceImage.ToString());
                return resp;
            }
            catch (Exception ex)
            {
                Log.Error("UpdateFaceImageResponse error:" + ex);
                resp.Status = "Error: " + ex;
            }
            return resp;
        }

        /// <summary>
        /// Gets a face image content of a team member.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public object Any(FaceImageContent request)
        {
            var resp = new FaceImageContentResponse() { Status = "OK" };
            try
            {
                var FaceImage = Common.TeamDbRepository.GetFaceImage(request.ApiKey, request.TeamGroupID, request.FaceImageID);
                if (FaceImage != null)
                {
                    FaceApiImage image;
                    if (FaceApi.RetrieveImageFromMember(request.ApiKey, request.TeamGroupID, request.TeamMemberID, request.FaceImageID, out image))
                    {
                        resp.TeamGroupID = request.TeamGroupID;
                        resp.TeamMemberID = request.TeamMemberID;
                        resp.FaceImageID = request.FaceImageID;
                        resp.ImageData = image.data;
                        resp.MimeType = image.mimetype;
                        resp.Width = image.width;
                        resp.Height = image.height;
                    }
                    else
                    {
                        resp.Status = "Error: " + FaceApi.LastError;
                    }
                }
                else
                {
                    resp.Status = "Error: FaceImage db record not found";
                }

            }
            catch (Exception ex)
            {
                Log.Error("UpdateFaceImageResponse error:" + ex);
                resp.Status = "Error: " + ex.Message;
            }
            return resp;
        }

        #endregion

        #region Video Analizer

        /// <summary>
        /// Creates an analizer instance.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public object Any(CreateAnalizer request)
        {
            var resp = new CreateAnalizerResponse() { Status = "OK" };
            try
            {
                double threshold = request.ThresHold > 0f ? request.ThresHold : double.MaxValue;
                string groupid = Guid.NewGuid().ToString("N");
                Log.Debug("CreateAnalizer: " + request.ApiKey + "|" + groupid + "|" + threshold);
                RecognizerType recognizer = RecognizerType.LBPHFaceRecognizer;
                if (request.Recognizer>=0 && request.Recognizer<=2)
                {
                    recognizer = (RecognizerType)request.Recognizer;
                }
                if(FaceApi.CreateSelfTrainedGroup(request.ApiKey, groupid, recognizer, threshold))
                {
                    resp.AnalizerID = groupid;
                }
                else
                {
                    resp.Status = "Error: " + FaceApi.LastError;
                    Log.Error("CreateAnalizer error:" + FaceApi.LastError);
                }
            }
            catch (Exception ex)
            {
                Log.Error("CreateAnalizer error:" + ex);
                resp.Status = "Error: " + ex;

            }
            return resp;
        }

        /// <summary>
        /// Deletes an analizer instance.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public object Any(RemoveAnalizer request)
        {
            var resp = new RemoveAnalizerResponse() { Status = "OK" };
            try
            {
                Log.Debug("RemoveAnalizer: " + request.ApiKey + "|" + request.AnalizerID);
                if (!FaceApi.DeleteSelfTrainedGroup(request.ApiKey,request.AnalizerID))
                {
                    resp.Status = "Error: " + FaceApi.LastError;
                    Log.Error("CreateAnalizer error:" + FaceApi.LastError);
                }
            }
            catch (Exception ex)
            {
                Log.Error("RemoveAnalizer error:" + ex);
                resp.Status = "Error: " + ex;
            }
            return resp;
        }

        /// <summary>
        /// Analizes a video frame.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public object Any(AnalizeFrame request)
        {
            var resp = new AnalizeFrameResponse() { Status = "OK" };
            try
            {
                List<SelfTrainedGroupMember> members = new List<SelfTrainedGroupMember>();
                if(FaceApi.AnalizeFrame(request.ApiKey,request.AnalizerID,request.ImageData, members))
                {
                    resp.Result = new List<VideoAnalizerResult>();
                    foreach(var member in members)
                    {
                        resp.Result.Add(new VideoAnalizerResult()
                        {
                            Label = member.label,
                            X = member.rectangle.X,
                            Y = member.rectangle.Y,
                            H = member.rectangle.Height,
                            W = member.rectangle.Width,
                        });
                    }
                }
                else
                {
                    resp.Status = "Error: " + FaceApi.LastError;
                    Log.Error("AnalizeFrame error:" + FaceApi.LastError);
                }
            }
            catch (Exception ex)
            {
                Log.Error("AnalizeFrame error:" + ex);
                resp.Status = "Error: " + ex;

            }
            return resp;
        }


        #endregion
    }
}
