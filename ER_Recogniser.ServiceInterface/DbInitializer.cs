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

namespace ER_Recogniser.ServiceInterface
{
    /// <summary>
    /// Database initializer class
    /// </summary>
    public class DbInitializer
    {
        /// <summary>
        /// Initializes the database.
        /// </summary>
        /// <param name="dbConnectionFactory">The database connection factory.</param>
        public static void InitializeDb(IDbConnectionFactory dbConnectionFactory)
        {
            //var tblTeams = new List<tblTeam>()
            //{
            //    new tblTeam(){Name="Team1",UID=Guid.NewGuid()},
            //    new tblTeam(){Name="Team2",UID=Guid.NewGuid()},
            //};

            //var tblTeamMembers = new List<tblTeamMember>()
            //{
            //    new tblTeamMember(){Name="TeamMember1_1",UID=Guid.NewGuid(), TeamID=1},
            //    new tblTeamMember(){Name="TeamMember1_2",UID=Guid.NewGuid(), TeamID=1},
            //    new tblTeamMember(){Name="TeamMember2_1",UID=Guid.NewGuid(), TeamID=2},
            //    new tblTeamMember(){Name="TeamMember2_2",UID=Guid.NewGuid(), TeamID=2},
            //    new tblTeamMember(){Name="TeamMember2_3",UID=Guid.NewGuid(), TeamID=2},
            //};

            using (var db = dbConnectionFactory.OpenDbConnection())
            {
                if (!db.TableExists("tblApi"))
                {
                    db.CreateTable<tblApi>();
                    //db.InsertAll<tblTeam>(tblTeams);
                }

                if (!db.TableExists("tblTeam"))
                {
                    db.CreateTable<tblTeam>();
                    //db.InsertAll<tblTeam>(tblTeams);
                }

                if (!db.TableExists("tblTeamMember"))
                {
                    db.CreateTable<tblTeamMember>();
                    //db.InsertAll<tblTeamMember>(tblTeamMembers);
                }
                if (!db.TableExists("tblFaceImage"))
                {
                    db.CreateTable<tblFaceImage>();
                }

                //test
                //int lastteammemberid = 1;
                //try
                //{
                //    Guid ez = Guid.Parse("D29CC514-8AA8-4B43-9155-2E166FE13BEB");

                //    lastteammemberid = db.Scalar<int>(db.From<tblTeamMember>().Where(x => x.TeamID == ez).Select(x => Sql.Max(x.TeamMemberID)));
                //    ++lastteammemberid;
                //    tblTeamMember m = new tblTeamMember() { Name="kaksi", TeamID = ez, TeamMemberID = lastteammemberid };
                //    db.Insert<tblTeamMember>(m);
                //    lastteammemberid = db.Scalar<int>(db.From<tblTeamMember>().Where(x => x.TeamID == ez).Select(x => Sql.Max(x.TeamMemberID)));
                //    ++lastteammemberid;
                //    m = new tblTeamMember() { Name = "kaksi", TeamID = ez, TeamMemberID = lastteammemberid };
                //    db.Insert<tblTeamMember>(m);
                //}
                //catch
                //{

                //}

            }
        }
    }
}
