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
using System.Runtime.Serialization;
using System.Data;

namespace ER_Recogniser.ServiceInterface
{
    /*
    class UnderscoreSeparatedCompoundNamingStrategy : OrmLiteNamingStrategyBase
    {

        public override string GetTableName(string name)
        {
            return toUnderscoreSeparatedCompound(name);
        }

        public override string GetColumnName(string name)
        {
            return toUnderscoreSeparatedCompound(name);
        }

        string toUnderscoreSeparatedCompound(string name)
        {

            string r = char.ToLower(name[0]).ToString();

            for (int i = 1; i < name.Length; i++)
            {
                char c = name[i];
                if (char.IsUpper(name[i]))
                {
                    r += "_";
                    r += char.ToLower(name[i]);
                }
                else
                {
                    r += name[i];
                }
            }
            return r;
        }

    }//*/


    /// <summary>
    /// Class for managind team database records
    /// </summary>
    public class TeamDbRepository
    {
        /// <summary>
        /// The mutex
        /// </summary>
        private object Mutex = new object();
        /// <summary>
        /// Gets or sets the database connection factory.
        /// </summary>
        /// <value>
        /// The database connection factory.
        /// </value>
        private IDbConnectionFactory DbConnectionFactory { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamDbRepository"/> class.
        /// </summary>
        /// <param name="DbConnectionFactory">The database connection factory.</param>
        public TeamDbRepository(IDbConnectionFactory DbConnectionFactory)
        {
            this.DbConnectionFactory = DbConnectionFactory;

        }

        /// <summary>
        /// Creates an API record.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <param name="ApiKey">The API key.</param>
        /// <returns></returns>
        public bool CreateApi(string Name,string ApiKey)
        {
            try
            {
                Guid guidApiKey = Guid.Parse(ApiKey);
                tblApi api = new tblApi() { ApiKey = guidApiKey, Name = Name };
                using (var db = DbConnectionFactory.OpenDbConnection())
                {
                    db.Insert<tblApi>(api);
                }
                return true;
            }
            catch
            {

            }
            return false;
        }

        /// <summary>
        /// Gets an API record.
        /// </summary>
        /// <param name="ApiKey">The API key.</param>
        /// <returns></returns>
        public Api GetApi(string ApiKey)
        {
            try
            {
                Guid guidApiKey = Guid.Parse(ApiKey);
                using (var db = DbConnectionFactory.OpenDbConnection())
                {
                    var dbApi = db.Select<tblApi>(x => x.ApiKey == guidApiKey).FirstOrDefault();
                    if (dbApi != null)
                    {
                        return dbApi.ConvertTo<Api>();
                    }
                    return null;
                }
            }
            catch
            {

            }
            return null;
        }

        #region List operations

        /// <summary>
        /// Gets team records.
        /// </summary>
        /// <param name="ApiKey">The API key.</param>
        /// <returns></returns>
        public List<Team> GetTeams(string ApiKey)
        {
            List<Team> lst = new List<Team>();
            try
            {
                Guid guidApiKey = Guid.Parse(ApiKey);

                using (var db = DbConnectionFactory.OpenDbConnection())
                {
                    var dbApi = db.Select<tblApi>(x => x.ApiKey == guidApiKey).FirstOrDefault();
                    if (dbApi == null)
                    {
                        return lst;
                    }

                    var dbItems = db.Select<tblTeam>(x => x.ApiKey == dbApi.ApiKey);
                    if (dbItems == null)
                    {
                        return lst;
                    }
                    else
                    {
                        foreach (var dbItem in dbItems)
                        {
                            lst.Add(dbItem.ConvertTo<Team>());
                        }
                        return lst;
                    }
                }
            }
            catch
            {

            }
            return lst;
        }

        /// <summary>
        /// Gets member records by team.
        /// </summary>
        /// <param name="TeamGroupID">The team group identifier.</param>
        /// <returns></returns>
        public List<TeamMember> GetMembersByTeam(string TeamGroupID)
        {
            List<TeamMember> lst = new List<TeamMember>();
            try
            {
                Guid guidTeamGroupID = Guid.Parse(TeamGroupID);
                using (var db = DbConnectionFactory.OpenDbConnection())
                {
                    var dbItems = db.Select<tblTeamMember>(m => m.TeamID == guidTeamGroupID);
                    if (dbItems == null)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var dbItem in dbItems)
                        {
                            lst.Add(dbItem.ConvertTo<TeamMember>());
                        }
                        return lst;
                    }
                }
            }
            catch
            {

            }
            return lst;
        }

        /// <summary>
        /// Gets the face images of a team member.
        /// </summary>
        /// <param name="TeamMemberID">The team member identifier.</param>
        /// <returns></returns>
        public List<FaceImage> GetFaceImagesByTeamMember(int TeamMemberID)
        {
            using (var db = DbConnectionFactory.OpenDbConnection())
            {
                var dbItems = db.Select<tblFaceImage>(m => m.TeamMemberID == TeamMemberID);
                if (dbItems == null)
                {
                    return null;
                }
                else
                {
                    List<FaceImage> lst = new List<FaceImage>();
                    foreach (var dbItem in dbItems)
                    {
                        lst.Add(dbItem.ConvertTo<FaceImage>());
                    }
                    return lst;
                }
            }
        }

        #endregion

        #region TeamMember Crud
        /// <summary>
        /// Gets a team member record.
        /// </summary>
        /// <param name="TeamMemberID">The team member identifier.</param>
        /// <returns></returns>
        public TeamMember GetTeamMember(int TeamMemberID)
        {
            using (var db = DbConnectionFactory.OpenDbConnection())
            {
                var dbItem = db.Select<tblTeamMember>(m => m.TeamMemberID == TeamMemberID).FirstOrDefault();
                return dbItem == null ? null : dbItem.ConvertTo<TeamMember>();
            }
        }

        /// <summary>
        /// Creates a team member record.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public int CreateTeamMember(TeamMember item)
        {
            lock (Mutex)
            {
                try
                {
                    using (var db = DbConnectionFactory.OpenDbConnection())
                    {
                        int lastteammemberid = 1;
                        try
                        {
                            lastteammemberid = db.Scalar<int>(db.From<tblTeamMember>().Where(x => x.TeamID == item.TeamID).Select(x => Sql.Max(x.TeamMemberID)));
                            ++lastteammemberid;
                        }
                        catch
                        {

                        }

                        item.TeamMemberID = lastteammemberid;
                        var dbItem = item.ConvertTo<tblTeamMember>();
                        long dbid = db.Insert(dbItem, selectIdentity: true);
                        return item.TeamMemberID;
                    }
                }
                catch
                {

                }
                return 0;
            }
        }

        /// <summary>
        /// Updates a team member record.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public int UpdateTeamMember(TeamMember item)
        {
            lock (Mutex)
            {
                try
                {
                    using (var db = DbConnectionFactory.OpenDbConnection())
                    {
                        var dbItem = item.ConvertTo<tblTeamMember>();
                        return db.Update(dbItem);
                    }
                }
                catch
                {

                }
            }
            return 0;
        }

        /// <summary>
        /// Deletes a team member record.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public bool DeleteTeamMember(TeamMember item)
        {
            lock (Mutex)
            {
                try
                {
                    using (var db = DbConnectionFactory.OpenDbConnection())
                    {
                        using (IDbTransaction dbTrans = db.BeginTransaction())
                        {
                            using (var dbCmd = db.CreateCommand())
                            {
                                try
                                {
                                    Dictionary<string, object> sqlparams = new Dictionary<string, object>();
                                    sqlparams.Add("TeamID", item.TeamID);
                                    sqlparams.Add("TeamMemberID",item.TeamMemberID);
                                    dbCmd.Transaction = dbTrans;
                                    dbCmd.ExecNonQuery("DELETE FROM tblFaceImage WHERE TeamID = @TeamID AND TeamMemberID = @TeamMemberID;", sqlparams);
                                    dbCmd.ExecNonQuery("DELETE FROM tblTeamMember WHERE TeamID = @TeamID AND TeamMemberID = @TeamMemberID;", sqlparams);
                                    //db.Delete<tblFaceImage>(x => x.TeamID == item.TeamID && x.TeamMemberID == item.TeamMemberID);
                                    //db.Delete<tblTeamMember>(x => x.TeamID == item.TeamID && x.TeamMemberID == item.TeamMemberID);
                                    dbTrans.Commit();
                                    return true;
                                }
                                catch (Exception ex)
                                {
                                    dbTrans.Rollback();
                                }
                            }
                        }
                    }
                }
                catch
                {

                }
                return false;
            }
        }
        #endregion

        #region Team crud
        /// <summary>
        /// Gets a team record.
        /// </summary>
        /// <param name="ApiKey">The API key.</param>
        /// <param name="TeamGroupID">The team group identifier.</param>
        /// <returns></returns>
        public Team GetTeam(string ApiKey, string TeamGroupID)
        {
            lock (Mutex)
            {
                try
                {
                    using (var db = DbConnectionFactory.OpenDbConnection())
                    {
                        Guid guidApiKey = Guid.Parse(ApiKey);
                        Guid guidTeamGroupID = Guid.Parse(TeamGroupID);

                        var dbApi = db.Select<tblApi>(x => x.ApiKey == guidApiKey).FirstOrDefault();
                        if (dbApi == null)
                        {
                            return null;
                        }
                        var dbteam = db.Select<tblTeam>(x => x.ApiKey == dbApi.ApiKey && x.TeamID == guidTeamGroupID).FirstOrDefault();
                        if (dbteam != null)
                        {
                            return dbteam.ConvertTo<Team>();
                        }
                    }
                }
                catch
                {

                }
            }
            return null;
        }

        /// <summary>
        /// Creates a team record.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public bool CreateTeam(Team item)
        {
            lock (Mutex)
            {

                try
                {
                    using (var db = DbConnectionFactory.OpenDbConnection())
                    {
                        var dbApi = db.Select<tblApi>(x => x.ApiKey == item.ApiKey).FirstOrDefault();
                        if (dbApi == null)
                        {
                            return false;
                        }
                        var dbItem = item.ConvertTo<tblTeam>();
                        db.Insert(dbItem);
                        return true;
                    }

                }
                catch
                {

                }
            }
            return false;
        }

        /// <summary>
        /// Updates a team record.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public bool UpdateTeam(Team item)
        {
            lock (Mutex)
            {
                try
                {
                    using (var db = DbConnectionFactory.OpenDbConnection())
                    {
                        var dbItem = item.ConvertTo<tblTeam>();
                        db.Update(dbItem);
                        return true;
                    }
                }
                catch
                {

                }
                return false;
            }
        }

        /// <summary>
        /// Deletes a team record.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public bool DeleteTeam(Team item)
        {
            lock (Mutex)
            {
                try
                {
                    using (var db = DbConnectionFactory.OpenDbConnection())
                    {
                        using (IDbTransaction dbTrans = db.BeginTransaction())
                        {
                            using (var dbCmd = db.CreateCommand())
                            {
                                try
                                {
                                    Dictionary<string, object> sqlparams = new Dictionary<string, object>();
                                    sqlparams.Add("TeamID", item.TeamID);
                                    dbCmd.Transaction = dbTrans;
                                    dbCmd.ExecNonQuery("DELETE FROM tblFaceImage WHERE TeamID = @TeamID;", sqlparams);
                                    dbCmd.ExecNonQuery("DELETE FROM tblTeamMember WHERE TeamID = @TeamID;", sqlparams);
                                    dbCmd.ExecNonQuery("DELETE FROM tblTeam WHERE TeamID = @TeamID;", sqlparams);
                                    dbTrans.Commit();
                                    return true;
                                }
                                catch
                                {
                                    dbTrans.Rollback();
                                    return false;
                                }
                            }
                        }
                    }
                }
                catch
                {

                }
                return false;
            }
        }
        #endregion

        #region FaceImage Crud
        /// <summary>
        /// Gets a face image record.
        /// </summary>
        /// <param name="ApiKey">The API key.</param>
        /// <param name="TeamGroupID">The team group identifier.</param>
        /// <param name="FaceImageID">The face image identifier.</param>
        /// <returns></returns>
        public FaceImage GetFaceImage(string ApiKey, string TeamGroupID,string FaceImageID)
        {
            try
            {
                Guid guidApiKey = Guid.Parse(ApiKey);
                Guid guidTeamGroupID = Guid.Parse(TeamGroupID);
                Guid guidFaceImageID = Guid.Parse(FaceImageID);

                using (var db = DbConnectionFactory.OpenDbConnection())
                {
                    //check team
                    var dbTeam = db.Select<tblTeam>(x => x.ApiKey == guidApiKey && x.TeamID == guidTeamGroupID).FirstOrDefault();
                    if (dbTeam == null)
                    {
                        return null;
                    }

                    var dbItem = db.Select<tblFaceImage>(m => m.TeamID == dbTeam.TeamID && m.FaceImageID == guidFaceImageID).FirstOrDefault();
                    return dbItem == null ? null : dbItem.ConvertTo<FaceImage>();
                }
            }
            catch
            {

            }
            return null;
        }

        /// <summary>
        /// Creates a face image record.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public bool CreateFaceImage(FaceImage item)
        {
            try
            {
                using (var db = DbConnectionFactory.OpenDbConnection())
                {
                    //check team
                    var dbTeam = db.Select<tblTeam>(x => x.TeamID == item.TeamID).FirstOrDefault();
                    if (dbTeam == null)
                    {
                        return false;
                    }

                    var dbItem = item.ConvertTo<tblFaceImage>();
                    db.Insert(dbItem);
                    return true;
                }

            }
            catch
            {

            }
            return false;
        }

        /// <summary>
        /// Updates a face image record.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public int UpdateFaceImage(FaceImage item)
        {
            using (var db = DbConnectionFactory.OpenDbConnection())
            {
                var dbItem = item.ConvertTo<tblFaceImage>();
                return db.Update(dbItem);
            }
        }

        /// <summary>
        /// Deletes a face image record.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public bool DeleteFaceImage(FaceImage item)
        {
            try
            {
                using (var db = DbConnectionFactory.OpenDbConnection())
                {
                    var dbItem = item.ConvertTo<tblFaceImage>();
                    return db.Delete(dbItem) > 0;
                }
            }
            catch
            {

            }
            return false;
        }
        #endregion

    }

    #region Tables
    /// <summary>
    /// The Api database table
    /// </summary>
    public class tblApi
    {
        /// <summary>
        /// Gets or sets the API key.
        /// </summary>
        /// <value>
        /// The API key.
        /// </value>
        [PrimaryKey]
        public Guid ApiKey { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// Gets the API key string.
        /// </summary>
        /// <returns></returns>
        public string GetApiKeyString() { return ApiKey.ToString("N"); }
    }

    /// <summary>
    /// The Team database table
    /// </summary>
    public class tblTeam
    {
        /// <summary>
        /// Gets or sets the team identifier.
        /// </summary>
        /// <value>
        /// The team identifier.
        /// </value>
        [PrimaryKey]
        public Guid TeamID { get; set; }
        /// <summary>
        /// Gets or sets the API key.
        /// </summary>
        /// <value>
        /// The API key.
        /// </value>
        public Guid ApiKey { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// Gets the API key string.
        /// </summary>
        /// <returns></returns>
        public string GetApiKeyString() { return ApiKey.ToString("N"); }
        /// <summary>
        /// Gets the team identifier string.
        /// </summary>
        /// <returns></returns>
        public string GetTeamIDString() { return TeamID.ToString("N"); }
    }

    /// <summary>
    /// The TeamMember database table
    /// </summary>
    public class tblTeamMember
    {
        /// <summary>
        /// Gets or sets the dbid.
        /// </summary>
        /// <value>
        /// The dbid.
        /// </value>
        [AutoIncrement]
        public long DBID { get; set; }
        /// <summary>
        /// Gets or sets the team member identifier.
        /// </summary>
        /// <value>
        /// The team member identifier.
        /// </value>
        public int TeamMemberID { get; set; } //
        /// <summary>
        /// Gets or sets the team identifier.
        /// </summary>
        /// <value>
        /// The team identifier.
        /// </value>
        public Guid TeamID { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// Gets the team identifier string.
        /// </summary>
        /// <returns></returns>
        public string GetTeamIDString() { return TeamID.ToString("N"); }
    }

    /// <summary>
    /// The FaceImage database table
    /// </summary>
    public class tblFaceImage
    {
        /// <summary>
        /// Gets or sets the face image identifier.
        /// </summary>
        /// <value>
        /// The face image identifier.
        /// </value>
        [PrimaryKey]
        public Guid FaceImageID { get; set; }
        /// <summary>
        /// Gets or sets the team identifier.
        /// </summary>
        /// <value>
        /// The team identifier.
        /// </value>
        public Guid TeamID { get; set; }
        /// <summary>
        /// Gets or sets the team member identifier.
        /// </summary>
        /// <value>
        /// The team member identifier.
        /// </value>
        public int TeamMemberID { get; set; }
        /// <summary>
        /// Gets or sets the comment.
        /// </summary>
        /// <value>
        /// The comment.
        /// </value>
        public string Comment { get; set; }
        /// <summary>
        /// Gets the team identifier string.
        /// </summary>
        /// <returns></returns>
        public string GetTeamIDString() { return TeamID.ToString("N"); }
    }
    #endregion Tables
}
