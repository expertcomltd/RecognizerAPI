using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ER_Recogniser.ServiceModel
{
    #region FaceIdentification

    [Route("/facerecognition", "GET", Summary = "GET Summary", Notes = "facerecognition Notes")]
    [Api("Description")]
    public class FaceRecognition : IReturn<FaceRecognitionResponse>
    {
        [ApiMember(Name = "ApiKey", Description = "ApiKey Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string ApiKey { get; set; }
        [ApiMember(Name = "TeamGroupID", Description = "TeamGroupID Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string TeamGroupID { get; set; }
        [ApiMember(Name = "MimeType", Description = "MimeType Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string MimeType { get; set; }
        [ApiMember(Name = "ImageData", Description = "ImageData Description", ParameterType = "body", DataType = "byte[]", IsRequired = true)]
        public byte[] ImageData { get; set; }
        [ApiMember(Name = "PredictEmotion", Description = "Request to predict emotions", ParameterType = "body", DataType = "boolean", IsRequired = true)]
        public bool PredictEmotion { get; set; }
    }

    [ApiResponse]
    public class FaceRecognitionResponse
    {
        public string Status { get; set; }
        public int TeamMemberID { get; set; }
        public TeamMember TeamMember { get; set; }
        public double Distance { get; set; }
        public int EmotionID { get; set; }
    }
    #endregion

    #region Teams


    /// <summary>
    /// TrainTeam
    /// </summary>
    [Route("/TrainTeam", "POST", Summary = "Train a teams", Notes = "Notes")]
    [Api("Description")]
    public class TrainTeam : IReturn<TrainTeamResponse>
    {
        [ApiMember(Name = "ApiKey", Description = "ApiKey Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string ApiKey { get; set; }
        [ApiMember(Name = "TeamGroupID", Description = "TeamGroupID Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string TeamGroupID { get; set; }
    }
    [ApiResponse]
    public class TrainTeamResponse
    {
        public string Status { get; set; }
    }

    /// <summary>
    /// GetTeams
    /// </summary>
    [Route("/Team", "GET", Summary = "Get all Teams", Notes = "Notes")]
    [Api("Description")]
    public class GetTeams : IReturn<GetTeamsResponse>
    {
        [ApiMember(Name = "ApiKey", Description = "ApiKey Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string ApiKey { get; set; }

    }
    [ApiResponse]
    public class GetTeamsResponse
    {
        public string Status { get; set; }
        public List<Team> Teams { get; set; }
    }

    /// <summary>
    /// CreateTeam
    /// </summary>
    [Route("/Team", "POST", Summary = "Create a Team", Notes = "Notes")]
    [Api("Description")]
    public class CreateTeam : IReturn<CreateTeamResponse>
    {
        [ApiMember(Name = "ApiKey", Description = "ApiKey Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string ApiKey { get; set; }
        public string Name { get; set; }
    }
    [ApiResponse]
    public class CreateTeamResponse
    {
        public Team Team { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// UpdateTeam
    /// </summary>
    [Route("/Team", "PUT", Summary = "Update a Team identified by TeamID", Notes = "Notes")]
    [Api("Description")]
    public class UpdateTeam : IReturn<UpdateTeamResponse>
    {
        [ApiMember(Name = "ApiKey", Description = "ApiKey Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string ApiKey { get; set; }
        public Team Team { get; set; }

    }
    [ApiResponse]
    public class UpdateTeamResponse
    {
        public string TeamID { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// DeleteTeam
    /// </summary>
    [Route("/Team", "DELETE", Summary = "Delete a Team identified by TeamID", Notes = "Notes")]
    [Api("Description")]
    public class DeleteTeam : IReturn<DeleteTeamResponse>
    {
        [ApiMember(Name = "ApiKey", Description = "ApiKey Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string ApiKey { get; set; }
        public Team Team { get; set; }

    }
    [ApiResponse]
    public class DeleteTeamResponse
    {
        public string TeamID { get; set; }
        public string Status { get; set; }
    }


    #endregion

    #region TeamMembers
    [Route("/TeamMember", "GET", Summary = "Delete a TeamMember identified by TeamMemberID", Notes = "Notes")]
    [Api("Description")]
    public class GetTeamMembers : IReturn<GetTeamMembersResponse>
    {
        [ApiMember(Name = "ApiKey", Description = "ApiKey Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string ApiKey { get; set; }
        public string TeamGroupID { get; set; }
    }
    [ApiResponse]
    public class GetTeamMembersResponse
    {
        public List<TeamMember> TeamMembers { get; set; }
        public string Status { get; set; }


    }

    [Route("/TeamMember", "POST", Summary = "Update a TeamMember identified by TeamMemberID", Notes = "Notes")]
    [Api("Description")]
    public class CreateTeamMember : IReturn<CreateTeamMemberResponse>
    {
        [ApiMember(Name = "ApiKey", Description = "ApiKey Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string ApiKey { get; set; }
        public string TeamGroupID { get; set; }
        public string Name { get; set; }
        
    }
    [ApiResponse]
    public class CreateTeamMemberResponse
    {
        public TeamMember TeamMember { get; set; }
        public string Status { get; set; }
    }

    [Route("/TeamMember", "DELETE", Summary = "Delete a TeamMember identified by TeamMemberID", Notes = "Notes")]
    [Api("Description")]
    public class DeleteTeamMember : IReturn<DeleteTeamMemberResponse>
    {
        [ApiMember(Name = "ApiKey", Description = "ApiKey Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string ApiKey { get; set; }
        public TeamMember TeamMember { get; set; }

    }
    [ApiResponse]
    public class DeleteTeamMemberResponse
    {
        public int TeamMemberID { get; set; }
        public string Status { get; set; }
    }


    [Route("/TeamMember", "PUT", Summary = "Create a TeamMember in a specific Team", Notes = "Notes")]
    [Api("Description")]
    public class UpdateTeamMember : IReturn<UpdateTeamMemberResponse>
    {
        [ApiMember(Name = "ApiKey", Description = "ApiKey Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string ApiKey { get; set; }
        public TeamMember TeamMember { get; set; }

    }

    [ApiResponse]
    public class UpdateTeamMemberResponse
    {
        public int TeamMemberID { get; set; }
        public string Status { get; set; }
    }
    #endregion


    #region FaceImages
    [Route("/FaceImage", "GET", Summary = "Delete a FaceImage identified by FaceImageID", Notes = "Notes")]
    [Api("Description")]
    public class GetFaceImages : IReturn<GetFaceImagesResponse>
    {
        [ApiMember(Name = "ApiKey", Description = "ApiKey Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string ApiKey { get; set; }
        public int TeamMemberID { get; set; }
    }
    [ApiResponse]
    public class GetFaceImagesResponse
    {
        public List<FaceImage> FaceImages { get; set; }
        public string Status { get; set; }


    }

    [Route("/FaceImage", "POST", Summary = "Update a FaceImage identified by FaceImageID", Notes = "Notes")]
    [Api("Description")]
    public class CreateFaceImage : IReturn<CreateFaceImageResponse>
    {
        [ApiMember(Name = "ApiKey", Description = "ApiKey Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string ApiKey { get; set; }
        [ApiMember(Name = "TeamGroupID", Description = "TeamGroupID Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string TeamGroupID { get; set; }
        [ApiMember(Name = "TeamMemberID", Description = "TeamMemberID Description", ParameterType = "path", DataType = "integer", IsRequired = true)]
        public int TeamMemberID { get; set; }
        [ApiMember(Name = "Comment", Description = "ApiKey Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string Comment { get; set; }
        [ApiMember(Name = "MimeType", Description = "MimeType Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string MimeType { get; set; }
        [ApiMember(Name = "ImageData", Description = "ImageData Description", ParameterType = "body", DataType = "byte[]", IsRequired = true)]
        public byte[] ImageData { get; set; }
        [ApiMember(Name = "TrainGroup", Description = "TrainGroup Description", ParameterType = "path", DataType = "boolean", IsRequired = true)]
        public bool TrainGroup { get; set; }
    }
    [ApiResponse]
    public class CreateFaceImageResponse
    {
        public FaceImage FaceImage { get; set; }
        public string Status { get; set; }
    }

    [Route("/FaceImage", "DELETE", Summary = "Delete a FaceImage identified by FaceImageID", Notes = "Notes")]
    [Api("Description")]
    public class DeleteFaceImage : IReturn<DeleteFaceImageResponse>
    {
        [ApiMember(Name = "ApiKey", Description = "ApiKey Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string ApiKey { get; set; }
        public FaceImage FaceImage { get; set; }

    }
    [ApiResponse]
    public class DeleteFaceImageResponse
    {
        public int FaceImageID { get; set; }
        public string Status { get; set; }
    }


    [Route("/FaceImage", "PUT", Summary = "Update a FaceImage for a specific TeamMember", Notes = "Notes")]
    [Api("Description")]
    public class UpdateFaceImage : IReturn<UpdateFaceImageResponse>
    {
        [ApiMember(Name = "ApiKey", Description = "ApiKey Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string ApiKey { get; set; }
        public FaceImage FaceImage { get; set; }

        [ApiMember(Name = "MimeType", Description = "MimeType Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string MimeType { get; set; }

        [ApiMember(Name = "ImageData", Description = "ImageData Description", ParameterType = "body", DataType = "byte[]", IsRequired = true)]
        public byte[] ImageData { get; set; }

    }

    [ApiResponse]
    public class UpdateFaceImageResponse
    {
        public int FaceImageID { get; set; }
        public string Status { get; set; }
    }


    [Route("/FaceImageContent", "GET", Summary = "Returns octet stream for a specific FaceImage", Notes = "Notes")]
    [Api("Description")]
    public class FaceImageContent : IReturn<FaceImageContentResponse>
    {
        [ApiMember(Name = "ApiKey", Description = "ApiKey Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string ApiKey { get; set; }
        [ApiMember(Name = "TeamGroupID", Description = "TeamGroupID Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string TeamGroupID { get; set; }
        [ApiMember(Name = "TeamMemberID", Description = "TeamMemberID Description", ParameterType = "path", DataType = "integer", IsRequired = true)]
        public int TeamMemberID { get; set; }
        [ApiMember(Name = "FaceImageID", Description = "FaceImageID Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string FaceImageID { get; set; }
    }

    [ApiResponse]
    public class FaceImageContentResponse
    {
        [ApiMember(Name = "TeamGroupID", Description = "TeamGroupID Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string TeamGroupID { get; set; }
        [ApiMember(Name = "FaceImageID", Description = "FaceImageID Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string FaceImageID { get; set; }
        [ApiMember(Name = "TeamMemberID", Description = "TeamMemberID Description", ParameterType = "path", DataType = "integer", IsRequired = true)]
        public int TeamMemberID { get; set; }
        [ApiMember(Name = "ImageData", Description = "ImageData Description", ParameterType = "body", DataType = "byte[]", IsRequired = true)]
        public byte[] ImageData { get; set; }
        [ApiMember(Name = "MimeType", Description = "MimeType Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string MimeType { get; set; }
        [ApiMember(Name = "Width", Description = "Width Description", ParameterType = "path", DataType = "integer", IsRequired = true)]
        public int Width { get; set; }
        [ApiMember(Name = "Height", Description = "Height Description", ParameterType = "path", DataType = "integer", IsRequired = true)]
        public int Height { get; set; }

        public string Status { get; set; }
    }

    #endregion

    #region VideoAnalizer

    [Route("/videoanalizercontext", "POST", Summary = "POST", Notes = "Notes")]
    [Api("Videoanalizer")]
    public class CreateAnalizer : IReturn<CreateAnalizerResponse>
    {
        [ApiMember(Name = "ApiKey", Description = "ApiKey Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string ApiKey { get; set; }
        [ApiMember(Name = "ThresHold", Description = "ThresHold Description", ParameterType = "path", DataType = "float", IsRequired = true)]
        public float ThresHold { get; set; }
        [ApiMember(Name = "Recognizer", Description = "Recognizer Description", ParameterType = "path", DataType = "integer", IsRequired = true)]
        public int Recognizer { get; set; }
        public CreateAnalizer() { ThresHold = -1f; Recognizer = 0; }
    }

    [ApiResponse]
    public class CreateAnalizerResponse
    {
        public string Status { get; set; }
        public string AnalizerID { get; set; }
    }

    [Route("/videoanalizercontext", "DELETE", Summary = "DELETE", Notes = "Notes")]
    [Api("Videoanalizer")]
    public class RemoveAnalizer : IReturn<RemoveAnalizerResponse>
    {
        [ApiMember(Name = "ApiKey", Description = "ApiKey Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string ApiKey { get; set; }
        [ApiMember(Name = "AnalizerID", Description = "AnalizerID Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string AnalizerID { get; set; }
    }

    [ApiResponse]
    public class RemoveAnalizerResponse
    {
        public string Status { get; set; }
    }

    [Route("/videoanalizer", "POST", Summary = "POST", Notes = "Notes")]
    [Api("Videoanalizer")]
    public class AnalizeFrame : IReturn<AnalizeFrameResponse>
    {
        [ApiMember(Name = "ApiKey", Description = "ApiKey Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string ApiKey { get; set; }
        [ApiMember(Name = "AnalizerID", Description = "AnalizerID Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string AnalizerID { get; set; }
        [ApiMember(Name = "MimeType", Description = "MimeType Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string MimeType { get; set; }
        [ApiMember(Name = "ImageData", Description = "ImageData Description", ParameterType = "body", DataType = "byte[]", IsRequired = true)]
        public byte[] ImageData { get; set; }
    }

    [ApiResponse]
    public class AnalizeFrameResponse
    {
        public string Status { get; set; }
        public List<VideoAnalizerResult> Result { get; set; }
    }
    #endregion

    #region DTO

    public class VideoAnalizerResult
    {
        public int Label { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; }
        public int H { get; set; }
    }

    public class Api
    {
        public Guid ApiKey { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return "" + ApiKey + " | " + Name;
        }
        public string GetApiKeyString() { return ApiKey.ToString("N"); }
    }

    public class Team
    {
        public Guid TeamID { get; set; }
        public Guid ApiKey { get; set; }
        public string Name { get; set; }
        public override string ToString()
        {
            return "ApiKey:" + ApiKey + "|TeamID:" + TeamID + "|Name:" + Name;
        }
        public string GetTeamIDString() { return TeamID.ToString("N"); }
        public string GetApiKeyString() { return ApiKey.ToString("N"); }
    }

    public class TeamMember
    {
        public long DBID { get; set; }
        public int TeamMemberID { get; set; }
        public Guid TeamID { get; set; }
        public string Name { get; set; }
        public string GetTeamIDString() { return TeamID.ToString("N"); }
        public override string ToString()
        {
            return "TeamID:" + TeamID + "|TeamMemberID:" + TeamMemberID + "|Name:" + Name;
        }
    }

    public class FaceImage
    {
        public Guid FaceImageID { get; set; }
        public Guid TeamID { get; set; }
        public int TeamMemberID { get; set; }
        public string Comment { get; set; }
        public string GetTeamIDString() { return TeamID.ToString("N"); }
        public string GetFaceImageIDString() { return FaceImageID.ToString("N"); }

        public override string ToString()
        {
            return "TeamID:" + TeamID + "|TeamMemberID:" + TeamMemberID + "|FaceImageID:" + FaceImageID;
        }

    }
    #endregion
}
