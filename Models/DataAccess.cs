using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace Quiz.Models
{
    /**
     * Class responsible for database connections, methods inside it are calling stored procedures
     */
    public class DataAccess
    {
        private SqlConnection con;

        /// <summary>
        /// Retrieves connection string from "appsettings.json" file
        /// </summary>
        /// <returns>SqlConnection object</returns>
        public SqlConnection GetCon()
        {
            var config = Configuration();
            con = new SqlConnection(config.GetSection("ConnectionStrings").GetSection("QuizContext").Value);
            //con = new SqlConnection(config.GetSection("ConnectionStrings").GetSection("LocalDB_Test").Value);

            return con;
        }

        /// <summary>
        /// Builds configuration object, based on "appsettings.json" file
        /// </summary>
        /// <returns>Configuration object</returns>
        public IConfigurationRoot Configuration()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            return builder.Build();
        }

        /// <summary>
        /// When parameter is bigger than 0, retrieves all training groups assigned to training campaign, else retrieves all groups possible        
        /// </summary>
        /// <param name="Id_Campaign">Optional parameter, indicates primary key in training campaign table</param>
        /// <returns>List of training groups objects</returns>
        public async Task<List<Training_Group>> GetAllGroups(int Id_Campaign = 0)
        {
            List<Training_Group> groups = new List<Training_Group>();

            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Get_All_Groups", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id_Campaign", Id_Campaign);

                await con.OpenAsync();
                SqlDataReader rdr = await cmd.ExecuteReaderAsync();

                while (await rdr.ReadAsync())
                {
                    Training_Group group = new Training_Group();

                    group.Id_Group = Convert.ToInt32(rdr["Id_Training_Group"]);
                    group.Name = rdr["Name"].ToString();

                    groups.Add(group);
                }
                con.Close();
            }
            return groups;
        }

        //OPTIMIZE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!! TOO MUCH DATA
        /// <summary>
        /// When parameter is bigger than 0, retrieves all trainings assigned to training campaign, else retrieves all trainings  
        /// </summary>
        /// <param name="Id_Campaign">Optional parameter, indicates primary key in training campaign table</param>
        /// <returns>List of presentations(trainings) objects</returns>
        public async Task<List<Presentations>> GetAllPresentations(int Id_Campaign = 0)
        {
            List<Presentations> PresentationsList = new List<Presentations>();

            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Get_All_Presentations", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id_Campaign", Id_Campaign);

                await con.OpenAsync();
                SqlDataReader rdr = await cmd.ExecuteReaderAsync();

                while (await rdr.ReadAsync())
                {
                    Presentations presentations = new Presentations();

                    presentations.Id = Convert.ToInt32(rdr["Id_Presentation"]);
                    presentations.Name = rdr["Presentation_Name"].ToString();
                    presentations.URL = rdr["Presentation_URL"].ToString();
                    presentations.Type = rdr["Type"].ToString();
                    presentations.Type_Name = rdr["Type_Name"].ToString();
                    presentations.Author_Mandant = Convert.ToInt32(rdr["Author_Mandant"]);
                    presentations.Question_Count = Convert.ToInt32(rdr["Question_Count"]);

                    PresentationsList.Add(presentations);
                }
                con.Close();
            }
            return PresentationsList;
        }

        /// <summary>
        /// Adds new training group to table and assigns users to it
        /// </summary>
        /// <param name="training_Group">Object containing list of users ids</param>
        /// <returns>Status message, info for user about executed procedure</returns>
        public string AddGroup(Training_Group training_Group)
        {
            string message = "";

            DataTable users = new DataTable();
            users.Columns.Add("Id_User", typeof(int));
            DataRow row;

            for (int i = 0; i < training_Group.Users.Count; i++)
            {
                row = users.NewRow();
                row["Id_User"] = training_Group.Users[i].Id_User;
                users.Rows.Add(row);
            }

            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Add_New_Group", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Name", training_Group.Name);
                cmd.Parameters.AddWithValue("@User_List", users);

                IDbDataParameter status = cmd.CreateParameter();
                status.ParameterName = "@Status";
                status.Direction = System.Data.ParameterDirection.Output;
                status.DbType = System.Data.DbType.String;
                status.Size = 200;
                cmd.Parameters.Add(status);

                con.Open();

                cmd.ExecuteNonQuery();

                message = status.Value.ToString();

                con.Close();

            }
            return message;
        }

        /// <summary>
        /// Adds new question to table for a specific training
        /// </summary>
        /// <param name="questions">Question object</param>
        /// <param name="id">Indicates primary key in presentation table</param>
        /// <returns>Status message, info for user about executed procedure</returns>
        public bool AddQuestion(Questions questions, int? id)
        {
            DataTable answers = new DataTable();
            answers.Columns.Add("Id_answer", typeof(int));
            answers.Columns.Add("answer", typeof(string));
            answers.Columns.Add("correct_Answer", typeof(bool));

            DataRow row;

            for (int i = 0; i < questions.Answers.Count; i++)
            {
                if (questions.Answers[i].Answer != null)
                {
                    row = answers.NewRow();
                    row["Id_answer"] = questions.Answers[i].Id_Answer + (i + 1);
                    row["answer"] = questions.Answers[i].Answer;
                    row["correct_Answer"] = questions.Answers[i].Correct_Answer;
                    //row["status"] = false;
                    answers.Rows.Add(row);
                }
            }

            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Add_New_Question", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id_Presentation", id);
                cmd.Parameters.AddWithValue("@Question", questions.Question);

                var param = new SqlParameter("@Answer_List", answers);
                param.SqlDbType = SqlDbType.Structured;
                cmd.Parameters.Add(param);

                IDbDataParameter status = cmd.CreateParameter();
                status.ParameterName = "@Status";
                status.Direction = System.Data.ParameterDirection.Output;
                status.DbType = System.Data.DbType.Boolean;
                status.Size = 1;
                cmd.Parameters.Add(status);

                con.Open();

                cmd.ExecuteNonQuery();

                bool message = Convert.ToBoolean(status.Value);

                con.Close();

                return message;
            }
        }

        /// <summary>
        /// Retrieves all questions for a specific training
        /// </summary>
        /// <param name="id">Indicates primary key in presentation table</param>
        /// <returns>List of questions objects</returns>
        public List<Questions> GetQuestions(int? id)
        {
            List<Questions> QuestionsList = new List<Questions>();

            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Get_All_Questions", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id_Presentation", id);

                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    Questions questions = new Questions();

                    questions.Id = Convert.ToInt32(rdr["Id_Question"]);
                    questions.Question = rdr["Question"].ToString();
                    questions.Presentation_Name = rdr["Presentation_Name"].ToString();


                    QuestionsList.Add(questions);
                }
                con.Close();
            }
            return QuestionsList;
        }

        /// <summary>
        /// Retrieves all answers for a specific question
        /// </summary>
        /// <param name="id">Indicates primary key in qiuz questions table</param>
        /// <returns>List of ansewrs objects</returns>
        public List<Answers> GetAnswers(int? id)
        {
            List<Answers> answersList = new List<Answers>();

            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Get_All_Answers", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id_Question", id);

                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    Answers answers = new Answers();

                    answers.Id_Answer = Convert.ToInt32(rdr["Id_Answer"]);
                    answers.Answer = rdr["Answer"].ToString();
                    answers.Correct_Answer = Convert.ToBoolean(rdr["Correct_Answer"]);

                    answersList.Add(answers);
                }
                con.Close();
            }
            return answersList;
        }

        /// <summary>
        /// Retrieves specific group by its id
        /// </summary>
        /// <param name="id">Indicates primary key in training group table</param>
        /// <returns>Training group object</returns>
        public Training_Group GetGroupById(int? id)
        {
            Training_Group group = new Training_Group();

            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Get_Group_By_Id", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id_Group", id);

                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    group.Name = rdr["Name"].ToString();
                    group.Id_Group = Convert.ToInt32(rdr["Id_Training_Group"]);
                }
                con.Close();
            }
            return group;
        }

        /// <summary>
        /// Retrieves specific campaign by its id
        /// </summary>
        /// <param name="id">Indicates primary key in training campaign table</param>
        /// <returns>Training campaign object</returns>
        public Training_Campaign GetCampaignById(int id)
        {
            Training_Campaign campaign = new Training_Campaign();

            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Get_Campaign_By_Id", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id_Campaign", id);

                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    //presentations.Id = Convert.ToInt32(rdr["Id_Presentation"]);
                    campaign.Name = rdr["Name"].ToString();

                    campaign.Author_Id = Convert.ToInt32(rdr["Author_Id"]);
                    campaign.Author_Name = rdr["Author_Name"].ToString();
                    campaign.Create_Date = rdr["Create_Date"].ToString();

                    if (rdr["Last_Modification_Date"].ToString() == null)
                    {
                        campaign.Last_Modification_Date = "";
                    }
                    else
                    {
                        campaign.Last_Modification_Date = rdr["Last_Modification_Date"].ToString();
                    }
                    campaign.Start_Date = rdr["Start_Date"].ToString();
                    campaign.End_Date = rdr["End_Date"].ToString();
                    campaign.Attempts = Convert.ToInt32(rdr["Attempts"]);
                }
                con.Close();
            }
            return campaign;
        }

        //remove some usless things here !!!!!!!!!!!!!!!
        /// <summary>
        /// Retrieves specific presetation(training) by its id
        /// </summary>
        /// <param name="id">Indicates primary key in presentations table</param>
        /// <returns>Presentation object</returns>
        public Presentations GetPresentationById(int id)
        {
            Presentations presentations = new Presentations();

            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Get_Presentation_By_Id", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id_Presentation", id);

                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    presentations.Id = id;
                    presentations.Name = rdr["Presentation_Name"].ToString();
                    presentations.URL = rdr["Presentation_URL"].ToString();
                    presentations.Type = rdr["Type"].ToString();
                    presentations.Type_Name = rdr["Type_Name"].ToString();

                    presentations.Author_Id = Convert.ToInt32(rdr["Author_Id"]);
                    presentations.Author_Name = rdr["Author_Name"].ToString();
                    presentations.Create_Date = rdr["Create_Date"].ToString();

                    if (rdr["Last_Modification_Date"].ToString() == null)
                    {
                        presentations.Last_Modification_Date = "";
                    }
                    else
                    {
                        presentations.Last_Modification_Date = rdr["Last_Modification_Date"].ToString();
                    }
                }
                con.Close();
            }
            return presentations;
        }

        //public Presentations GetPresentationByName(string Name)
        //{
        //    Presentations presentations = new Presentations();

        //    using (GetCon())
        //    {
        //        SqlCommand cmd = new SqlCommand("Get_Presentation_By_Name", con);
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        cmd.Parameters.AddWithValue("@Presentation_Name", Name);

        //        con.Open();
        //        SqlDataReader rdr = cmd.ExecuteReader();

        //        while (rdr.Read())
        //        {
        //            presentations.Id = Convert.ToInt32(rdr["Id_Presentation"]);
        //            presentations.Name = Name;
        //            presentations.URL = rdr["Presentation_URL"].ToString();
        //            presentations.Type = rdr["Type"].ToString();
        //            presentations.Type_Name = rdr["Type_Name"].ToString();
        //        }
        //        con.Close();
        //    }
        //    return presentations;
        //}

        /// <summary>
        /// Check training name availability
        /// </summary>
        /// <param name="name">Indicates name column in presentations table</param>
        /// <param name="id">Indicates primary key in presentations table</param>
        /// <returns>Boolean data type, true or false</returns>
        public bool CheckName(string name, int id)
        {
            string old_Name = null;

            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Check_Name", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Presentation_Name", name);
                cmd.Parameters.AddWithValue("@Presentation_Id", id);

                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    old_Name = rdr["Presentation_Name"].ToString();
                }
                con.Close();
            }

            if (old_Name != null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Update name of group
        /// </summary>
        /// <param name="group">Training group object</param>
        /// <returns>Status message, info for user about executed procedure</returns>
        public string UpdateGroupName(Training_Group group)
        {
            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Update_Group_Name", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id_group", group.Id_Group);
                cmd.Parameters.AddWithValue("@New_Name", group.Name);

                IDbDataParameter status = cmd.CreateParameter();
                status.ParameterName = "@Status";
                status.Direction = System.Data.ParameterDirection.Output;
                status.DbType = System.Data.DbType.String;
                status.Size = 550;
                cmd.Parameters.Add(status);

                con.Open();

                cmd.ExecuteNonQuery();

                string message = Convert.ToString(status.Value);

                con.Close();
                return message;
            }
        }

        /// <summary>
        /// Updates presentation
        /// </summary>
        /// <param name="presentations">Presentation object</param>
        /// <returns>Status message, info for user about executed procedure</returns>
        public string UpdatePresentation(Presentations presentations)
        {
            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Update_Presentation", con);
                cmd.CommandType = CommandType.StoredProcedure;

                //string[] dates = presentations.Date_Range.Split("-");
                //DateTime startDate = Convert.ToDateTime(dates[0]);
                //DateTime endDate = Convert.ToDateTime(dates[1]);

                cmd.Parameters.AddWithValue("@Presentation_Name", presentations.Name);
                cmd.Parameters.AddWithValue("@Presentation_Id", presentations.Id);
                cmd.Parameters.AddWithValue("@Type", presentations.Type);
                cmd.Parameters.AddWithValue("@Type_Name", presentations.Type_Name);
                cmd.Parameters.AddWithValue("@URL", presentations.URL);

                cmd.Parameters.AddWithValue("@Author_Id", presentations.Author_Id);
                //cmd.Parameters.AddWithValue("@Start_Date", startDate);
                //cmd.Parameters.AddWithValue("@End_Date", endDate);

                IDbDataParameter status = cmd.CreateParameter();
                status.ParameterName = "@Status";
                status.Direction = System.Data.ParameterDirection.Output;
                status.DbType = System.Data.DbType.String;
                status.Size = 550;
                cmd.Parameters.Add(status);

                con.Open();

                cmd.ExecuteNonQuery();

                string message = Convert.ToString(status.Value);

                con.Close();
                return message;
            }
        }

        /// <summary>
        /// Retrieves specific question by its id
        /// </summary>
        /// <param name="id">Indicates primary key in quiz question table</param>
        /// <returns>Question object</returns>
        public Questions GetQuestionById(int? id)
        {
            Questions questions = new Questions();

            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Get_Question_By_Id", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id_Question", id);

                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    questions.Id = Convert.ToInt32(rdr["Id_Question"]);
                    questions.Id_Presentation = Convert.ToInt32(rdr["Id_Presentation"]);
                    questions.Presentation_Name = rdr["Presentation_Name"].ToString();
                    questions.Question = rdr["Question"].ToString();

                }
                con.Close();
            }
            return questions;
        }

        /// <summary>
        /// Deletes row from training group table with a specific id
        /// </summary>
        /// <param name="id">Indicates primary key in training group table</param>
        /// <returns>Status message, info for user about executed procedure</returns>
        public string DeleteGroup(int? id)
        {
            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Delete_Group", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id_Group", id);

                IDbDataParameter status = cmd.CreateParameter();
                status.ParameterName = "@Status";
                status.Direction = System.Data.ParameterDirection.Output;
                status.DbType = System.Data.DbType.String;
                status.Size = 150;
                cmd.Parameters.Add(status);

                con.Open();
                cmd.ExecuteNonQuery();

                string message = Convert.ToString(status.Value);

                con.Close();

                return message;
            }
        }

        /// <summary>
        /// Deletes row from quiz question table with a specific id
        /// </summary>
        /// <param name="id">Indicates primary key in quiz question  table</param>
        /// <returns>Status message, info for user about executed procedure</returns>
        public string DeleteQuestion(int? id)
        {
            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Delete_Question", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id_Question", id);

                IDbDataParameter status = cmd.CreateParameter();
                status.ParameterName = "@Status";
                status.Direction = System.Data.ParameterDirection.Output;
                status.DbType = System.Data.DbType.String;
                status.Size = 150;
                cmd.Parameters.Add(status);

                con.Open();
                cmd.ExecuteNonQuery();

                string message = Convert.ToString(status.Value);

                con.Close();

                return message;
            }
        }

        /// <summary>
        /// Updates question and its answers if necessary, or deletes useless answers
        /// </summary>
        /// <param name="questions">Question object</param>
        /// <returns>Status message, info for user about executed procedure</returns>
        public string UpdateQuestion(Questions questions)
        {
            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Update_Question", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id_Question", questions.Id);
                cmd.Parameters.AddWithValue("@Question", questions.Question);

                IDbDataParameter status = cmd.CreateParameter();
                status.ParameterName = "@Status";
                status.Direction = System.Data.ParameterDirection.Output;
                status.DbType = System.Data.DbType.String;
                status.Size = 150;
                cmd.Parameters.Add(status);

                con.Open();
                cmd.ExecuteNonQuery();

                string message = Convert.ToString(status.Value);
                if (message.Contains("OK"))
                {
                    for (int i = 0; i < questions.Answers.Count; i++)
                    {
                        if ((questions.Answers[i].Answer != "") && (questions.Answers[i].Answer != null))
                        {
                            SqlCommand cmd2 = new SqlCommand("Update_Question_Answers", con);
                            cmd2.CommandType = CommandType.StoredProcedure;

                            cmd2.Parameters.AddWithValue("@Id_@Answer", questions.Answers[i].Id_Answer);
                            cmd2.Parameters.AddWithValue("@Answer", questions.Answers[i].Answer);
                            cmd2.Parameters.AddWithValue("@Correct", questions.Answers[i].Correct_Answer);
                            cmd2.Parameters.AddWithValue("@Id_Question", questions.Id);

                            // use the @Status parmam !!!!!!!!!!!!!!!
                            IDbDataParameter status2 = cmd2.CreateParameter();
                            status2.ParameterName = "@Status";
                            status2.Direction = System.Data.ParameterDirection.Output;
                            status2.DbType = System.Data.DbType.String;
                            status2.Size = 150;
                            cmd2.Parameters.Add(status2);

                            cmd2.ExecuteNonQuery();
                        }
                        else if ((questions.Answers[i].Answer == null) && (questions.Answers[i].Id_Answer != 0))
                        {
                            SqlCommand cmd3 = new SqlCommand("Delete_Question_Answers", con);
                            cmd3.CommandType = CommandType.StoredProcedure;

                            cmd3.Parameters.AddWithValue("@Id_@Answer", questions.Answers[i].Id_Answer);

                            cmd3.ExecuteNonQuery();
                        }
                    }
                }
                else
                {
                    con.Close();
                    return message;
                }

                con.Close();

                return message;
            }
        }

        /// <summary>
        /// Retrieves data from Format table
        /// </summary>
        /// <returns>List of format objects</returns>
        public List<Formats> GetFormatTypes()
        {
            List<Formats> TypeList = new List<Formats>();

            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Get_All_Formats", con);
                cmd.CommandType = CommandType.StoredProcedure;

                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    Formats formats = new Formats();

                    formats.Format = rdr["Format"].ToString();
                    formats.Name = rdr["Name"].ToString();

                    TypeList.Add(formats);
                }
                con.Close();
            }
            return TypeList;
        }

        /// <summary>
        /// Retrieves format name for a specific type
        /// </summary>
        /// <param name="type">Indicates specific type in format table</param>
        /// <returns>Format name as a string</returns>
        public string GetNameByType(string type)
        {
            string result = null;

            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Get_Name_By_Type", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Type", type);

                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    result = rdr["Name"].ToString();
                }
                con.Close();
            }
            return result;
        }

        /// <summary>
        /// Send all users answers to SQL server
        /// </summary>
        /// <param name="questions">List of question objects</param>
        /// <param name="Id_User">Indicates primary key in user table</param>
        /// <param name="Id_Campaign">Indicates primary key in training campaign table</param>
        /// <returns>Sum of points</returns>
        public string SendAnswers(List<Questions> questions, int? Id_User, int Id_Campaign)
        {
            DataTable answers = new DataTable();
            answers.Columns.Add("Id_Question", typeof(int));
            answers.Columns.Add("Id_answer", typeof(int));
            answers.Columns.Add("answer", typeof(string));
            answers.Columns.Add("correct_Answer", typeof(bool));

            DataRow row;

            for (var j = 0; j < questions.Count; j++)
            {
                for (int i = 0; i < questions[j].Answers.Count; i++)
                {
                    row = answers.NewRow();
                    row["Id_Question"] = questions[j].Id;
                    row["Id_answer"] = questions[j].Answers[i].Id_Answer;
                    row["answer"] = questions[j].Answers[i].Answer;
                    row["correct_Answer"] = questions[j].Answers[i].Correct_Answer;

                    answers.Rows.Add(row);
                }
            }

            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Check_Answers", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id_User", Id_User);
                cmd.Parameters.AddWithValue("@Id_Campaign", Id_Campaign);
                cmd.Parameters.AddWithValue("@Id_Presentation", questions.FirstOrDefault().Id_Presentation);

                var param = new SqlParameter("@User_Answer_List", answers);
                param.SqlDbType = SqlDbType.Structured;
                cmd.Parameters.Add(param);

                IDbDataParameter status = cmd.CreateParameter();
                status.ParameterName = "@Status";
                status.Direction = System.Data.ParameterDirection.Output;
                status.DbType = System.Data.DbType.String;
                status.Size = 150;
                cmd.Parameters.Add(status);

                IDbDataParameter points = cmd.CreateParameter();
                points.ParameterName = "@Points";
                points.Direction = System.Data.ParameterDirection.Output;
                points.DbType = System.Data.DbType.Int16;
                points.Size = 16;
                cmd.Parameters.Add(points);

                con.Open();

                cmd.ExecuteNonQuery();

                string message = Convert.ToString(status.Value);

                //if status is ok then ->
                int sum_Of_Points = Convert.ToInt16(points.Value);

                con.Close();

                return sum_Of_Points.ToString();
            }
        }

        /// <summary>
        /// Adds new presentation(training) to table
        /// </summary>
        /// <param name="presentations">Presentation object</param>
        /// <returns>Status message, info for user about executed procedure</returns>
        public string AddPresentation(Presentations presentations)
        {
            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Add_New_Presentation", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Presentation_Name", presentations.Name);
                cmd.Parameters.AddWithValue("@Type", presentations.Type);
                cmd.Parameters.AddWithValue("@Url", presentations.URL);
                cmd.Parameters.AddWithValue("@Type_Name", presentations.Type_Name);
                cmd.Parameters.AddWithValue("@Author_Id", presentations.Author_Id);
                cmd.Parameters.AddWithValue("@Id_Mandant", presentations.Author_Mandant);

                IDbDataParameter status = cmd.CreateParameter();
                status.ParameterName = "@Status";
                status.Direction = System.Data.ParameterDirection.Output;
                status.DbType = System.Data.DbType.String;
                status.Size = 150;
                cmd.Parameters.Add(status);

                con.Open();

                cmd.ExecuteNonQuery();

                string message = Convert.ToString(status.Value);

                con.Close();
                return message;
            }
        }

        /// <summary>
        /// Deletes presentation(training) from table with a specific id
        /// </summary>
        /// <param name="id">Indicates primary key in presenation table</param>
        /// <returns>Status message, info for user about executed procedure</returns>
        public string DeletePresentation(int? id)
        {
            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Delete_Presentation", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id_Presentation", id);

                IDbDataParameter status = cmd.CreateParameter();
                status.ParameterName = "@Status";
                status.Direction = System.Data.ParameterDirection.Output;
                status.DbType = System.Data.DbType.String;
                status.Size = 150;
                cmd.Parameters.Add(status);

                con.Open();
                cmd.ExecuteNonQuery();

                string message = Convert.ToString(status.Value);

                con.Close();

                return message;
            }
        }

        /// <summary>
        /// Allows to log into application
        /// </summary>
        /// <param name="login">Login object</param>
        /// <returns>User object</returns>
        public User LoginUserLocal(Login login)
        {
            User user = new User();

            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("LOG_IN", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Name", login.login);
                cmd.Parameters.AddWithValue("@Password", login.Password);

                //cmd.Parameters.AddWithValue("@ASPAPP", login.App_Name);
                //cmd.Parameters.AddWithValue("@HOSTNAME", login.Host_Name);
                //cmd.Parameters.AddWithValue("@HOSTIP", login.Ip_Address);
                //cmd.Parameters.AddWithValue("@Browser", login.Browser_Name);

                IDbDataParameter status = cmd.CreateParameter();
                status.ParameterName = "@INFO";
                status.Direction = System.Data.ParameterDirection.Output;
                status.DbType = System.Data.DbType.String;
                status.Size = 200;
                cmd.Parameters.Add(status);

                con.Open();

                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    user.Id_User = Convert.ToInt32(rdr["IDUSER"]);
                    user.Id_Mandant = Convert.ToInt32(rdr["IDMANDANT"]);
                    user.First_Name = rdr["FirstName"].ToString();
                    user.Last_Name = rdr["Lastname"].ToString();

                    user.Email = rdr["Email"].ToString();

                    //user.Administrator = Convert.ToBoolean(rdr["Administrator"]);
                    //user.Onsite_Tech = Convert.ToBoolean(rdr["OnsiteTechnician"]);
                    user.DB_Admin = Convert.ToBoolean(rdr["db_admin"]);
                }
                con.Close();
                user.Info_Login = "";
                user.Info_Login = Convert.ToString(status.Value);

                con.Close();

                return user;
            }
        }

        /// <summary>
        /// Allows to log into application
        /// </summary>
        /// <param name="login">Login object</param>
        /// <returns>User object</returns>
        public User LoginUser(Login login)
        {
            User user = new User();

            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("[10.1.2.8].OPAL.dbo.ASP_DBUSER_LOGIN", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Anmeldename", login.login);
                cmd.Parameters.AddWithValue("@Passwort", login.Password);

                cmd.Parameters.AddWithValue("@ASPAPP", login.App_Name);
                cmd.Parameters.AddWithValue("@HOSTNAME", login.Host_Name);
                cmd.Parameters.AddWithValue("@HOSTIP", login.Ip_Address);
                cmd.Parameters.AddWithValue("@Browser", login.Browser_Name);

                IDbDataParameter status = cmd.CreateParameter();
                status.ParameterName = "@INFO";
                status.Direction = System.Data.ParameterDirection.Output;
                status.DbType = System.Data.DbType.String;
                status.Size = 200;
                cmd.Parameters.Add(status);

                con.Open();

                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    user.Id_User = Convert.ToInt32(rdr["IDUSER"]);
                    user.Id_Mandant = Convert.ToInt32(rdr["IDMANDANT"]);
                    user.First_Name = rdr["FirstName"].ToString();
                    user.Last_Name = rdr["Lastname"].ToString();

                    user.Email = rdr["Email"].ToString();

                    user.Administrator = Convert.ToBoolean(rdr["Administrator"]);
                    user.Onsite_Tech = Convert.ToBoolean(rdr["OnsiteTechnician"]);
                    user.DB_Admin = Convert.ToBoolean(rdr["dbadmin"]);
                }
                con.Close();
                user.Info_Login = "";
                user.Info_Login = Convert.ToString(status.Value);

                con.Close();

                return user;
            }
        }

        /// <summary>
        /// Retrieves users points for a specific training in specific campaign
        /// </summary>
        /// <param name="Id_Campaign">Indicates primary key in training campaign table</param>
        /// <param name="Id_Presentation">Indicates primary key in presenation table</param>
        /// <param name="Id_User">Indicates primary key in user table</param>
        /// <returns>several different variables, stored in one tulpe object</returns>
        public Tuple<string, double, int, string, string> GetUserPoints(int Id_Campaign, int Id_Presentation, int? Id_User)
        {
            string User_points = "0";
            double Points_Percentage = 0.0;//maybe usless
            int Attempts_Used = 0;
            string Attempt_Date = "";
            string StatusResult = "";

            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Get_User_Points", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id_User", Id_User);
                cmd.Parameters.AddWithValue("@Id_Campaign", Id_Campaign);
                cmd.Parameters.AddWithValue("@Id_Presentation", Id_Presentation);

                IDbDataParameter points = cmd.CreateParameter();
                points.ParameterName = "@Points";
                points.Direction = System.Data.ParameterDirection.Output;
                points.DbType = System.Data.DbType.String;
                points.Size = 200;
                cmd.Parameters.Add(points);

                IDbDataParameter Points_Per = cmd.CreateParameter();
                Points_Per.ParameterName = "@Points_Percentage";
                Points_Per.Direction = System.Data.ParameterDirection.Output;
                Points_Per.DbType = System.Data.DbType.Double;
                Points_Per.Size = 32;
                cmd.Parameters.Add(Points_Per);

                IDbDataParameter Attempts = cmd.CreateParameter();
                Attempts.ParameterName = "@Attempts";
                Attempts.Direction = System.Data.ParameterDirection.Output;
                Attempts.DbType = System.Data.DbType.Int16;
                Attempts.Size = 16;
                cmd.Parameters.Add(Attempts);

                IDbDataParameter Date = cmd.CreateParameter();
                Date.ParameterName = "@Date";
                Date.Direction = System.Data.ParameterDirection.Output;
                Date.DbType = System.Data.DbType.String;
                Date.Size = 200;
                cmd.Parameters.Add(Date);

                IDbDataParameter Status = cmd.CreateParameter();
                Status.ParameterName = "@Status";
                Status.Direction = System.Data.ParameterDirection.Output;
                Status.DbType = System.Data.DbType.String;
                Status.Size = 200;
                cmd.Parameters.Add(Status);

                con.Open();
                cmd.ExecuteNonQuery();

                User_points = points.Value.ToString();
                Points_Percentage = Convert.ToDouble(Points_Per.Value);
                Attempts_Used = Convert.ToInt16(Attempts.Value);
                Attempt_Date = Date.Value.ToString();
                StatusResult = Status.Value.ToString();

                con.Close();
            }

            var result = Tuple.Create<string, double, int, string, string>(User_points, Points_Percentage, Attempts_Used, Attempt_Date, StatusResult);
            return result;
        }

        /// <summary>
        /// Retrieves users points and attempts dates of all trainings 
        /// </summary>
        /// <param name="Id_Campaign">Indicates primary key in training campaign table, optional</param>
        /// <param name="Id_Presentation">Indicates primary key in presenation table, optional</param>
        /// <param name="Id_User">Indicates primary key in user table, optional</param>
        /// <param name="Type">Int, number of attempt, optional</param>
        /// <returns></returns>
        public List<Presentations> GetAllResults(int Id_Campaign = 0, int Id_Presentation = 0, int Id_User = 0, int Type = 1)
        {
            List<Presentations> presentations = new List<Presentations>();

            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Get_All_Results", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id_User", Id_User);
                cmd.Parameters.AddWithValue("@Id_Campaign", Id_Campaign);
                cmd.Parameters.AddWithValue("@Id_Presentation", Id_Presentation);
                cmd.Parameters.AddWithValue("@Type", Type);

                IDbDataParameter Question_Qty = cmd.CreateParameter();
                Question_Qty.ParameterName = "@Question_Qty";
                Question_Qty.Direction = System.Data.ParameterDirection.Output;
                Question_Qty.DbType = System.Data.DbType.Int32;
                Question_Qty.Size = 32;
                cmd.Parameters.Add(Question_Qty);

                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                if (Id_User != 0)
                {
                    while (rdr.Read())
                    {
                        Presentations presentation = new Presentations();

                        presentation.Attempt_Date = rdr["Date"].ToString();
                        presentation.User_Points = rdr["Sum_of_Points"].ToString();

                        presentations.Add(presentation);
                    }
                }
                else
                {
                    Presentations presentation = new Presentations();
                    List<User> users = new List<User>();

                    while (rdr.Read())
                    {
                        User user = new User();

                        user.Id_User = Convert.ToInt32(rdr["Id_User"]);
                        user.Personal_Number = rdr["PersonalNumber"].ToString();
                        user.First_Name = rdr["UserName"].ToString();
                        user.Last_Name = rdr["FullName"].ToString();
                        user.Email = rdr["Result"].ToString();
                        user.Info_Group = rdr["Branch"].ToString();
                        user.Info_Login = rdr["Date"].ToString();
                        user.Info_Login = user.Info_Login.Substring(0, user.Info_Login.IndexOf(" "));
                        user.Sum_of_Points = Convert.ToInt32(rdr["Sum_of_Points"]);

                        users.Add(user);
                    }
                    rdr.Close();
                    presentation = GetPresentationById(Id_Presentation);
                    presentation.Users_Assigned = users;
                    presentations.Add(presentation);
                }
                con.Close();
            }
            return presentations;
        }

        /// <summary>
        /// Retrieves all trainig campaigns
        /// </summary>
        /// <returns>List of trainig campaigns</returns>
        public async Task<List<Training_Campaign>> GetAllCampaigns()
        {
            List<Training_Campaign> campaigns = new List<Training_Campaign>();

            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Get_All_Campaigns", con);
                cmd.CommandType = CommandType.StoredProcedure;

                await con.OpenAsync();
                SqlDataReader rdr = await cmd.ExecuteReaderAsync();

                while (await rdr.ReadAsync())
                {
                    Training_Campaign training_Campaign = new Training_Campaign();

                    training_Campaign.Id = Convert.ToInt32(rdr["Id_Campaign"]);
                    training_Campaign.Name = rdr["Name"].ToString();
                    //training_Campaign.Author_Name = rdr["Author_Name"].ToString();
                    //training_Campaign.Create_Date = rdr["Create_Date"].ToString();
                    training_Campaign.Start_Date = rdr["Start_Date"].ToString();
                    training_Campaign.End_Date = rdr["End_Date"].ToString();
                    // training_Campaign.Last_Modification_Date = rdr["Last_Modification_Date"].ToString();

                    campaigns.Add(training_Campaign);
                }
                con.Close();
            }
            return campaigns;
        }

        /// <summary>
        /// When Id_Campaign equals 0, retrieves users not assinged to any groups, else retrieves all users assigned to a specific campaign
        /// </summary>
        /// <param name="groups">Training group object</param>
        /// <param name="Id_Campaign">Optional parameter, indicates primary key in training campaign table</param>
        /// <returns>List of user objects</returns>
        public List<User> GetAllUsers(List<Training_Group> groups, int Id_Campaign = 0)
        {
            List<User> users = new List<User>();

            DataTable groups_Ids = new DataTable();
            groups_Ids.Columns.Add("Id_Group", typeof(int));

            DataRow row;

            for (var j = 0; j < groups.Count; j++)
            {
                row = groups_Ids.NewRow();
                row["Id_Group"] = groups[j].Id_Group;

                groups_Ids.Rows.Add(row);
            }

            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Get_All_Users", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Group_List", groups_Ids);
                cmd.Parameters.AddWithValue("@Id_Campaign", Id_Campaign);

                con.Open();

                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    User user = new User();

                    user.Id_User = Convert.ToInt32(rdr["IDUSER"]);
                    user.Personal_Number = rdr["PersonalNumber"].ToString();

                    user.First_Name = rdr["FirstName"].ToString();
                    user.Last_Name = rdr["Lastname"].ToString();
                    user.Info_Group = rdr["Branch"].ToString();

                    users.Add(user);
                }
                con.Close();
            }
            return users;
        }

        /// <summary>
        /// Method must have one of parameters(Group_Id,Id_Campaign) bigger than 0. When Group_Id is bigger than 0, retrieves users assinged to a specific group. 
        /// When Id_Campaign is bigger, retrieves users assinged to a specific campaign
        /// </summary>
        /// <param name="Group_Id">Optional parameter, indicates primary key in training group table</param>
        /// <param name="Id_Campaign">Optional parameter, indicates primary key in training campaign table</param>
        /// <param name="Additional">Optional parameter, when it's true, allows to retrieves additional users assigned to campaign</param>
        /// <returns></returns>
        public List<User> GetAllAssignedUsers(int Group_Id = 0, int Id_Campaign = 0, bool Additional = false)
        {
            List<User> users = new List<User>();

            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Get_All_Assigned_Users", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id_Group", Group_Id);
                cmd.Parameters.AddWithValue("@Id_Campaign", Id_Campaign);
                cmd.Parameters.AddWithValue("@Additional", Additional);

                con.Open();

                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    User user = new User();

                    user.Id_User = Convert.ToInt32(rdr["IDUSER"]);
                    user.Personal_Number = rdr["PersonalNumber"].ToString();

                    user.First_Name = rdr["FirstName"].ToString();
                    user.Last_Name = rdr["Lastname"].ToString();

                    if (Group_Id == 0)
                    {
                        user.Info_Group = rdr["Groups"].ToString();
                    }

                    users.Add(user);
                }
                con.Close();
            }
            return users;
        }

        // maybe some status ?!?!?!?!?!?!
        /// <summary>
        /// Deletes employee from training group table with a specific id
        /// </summary>
        /// <param name="group">Training group object</param>
        public void DeleteEmployee(Training_Group group)
        {
            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Delete_Employee", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id_Group", group.Id_Group);
                cmd.Parameters.AddWithValue("@Id_User", group.Users[0].Id_User);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
        }

        /// <summary>
        /// Deletes campaign from database with a specific id
        /// </summary>
        /// <param name="id">Indicates primary key in training campaign table</param>
        public void DeleteCampaign(int id)
        {
            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Delete_Campaign", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id_Campaign", id);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
        }

        /// <summary>
        /// Deletes employee from training campaign table with a specific id
        /// </summary>
        /// <param name="campaign">Training campaign object</param>
        public void DeleteEmployeeFromCampaign(Training_Campaign campaign)
        {
            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Delete_Employee_From_Campaign", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id_Campaign", campaign.Id);
                cmd.Parameters.AddWithValue("@Id_User", campaign.Users_Assigned[0].Id_User);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
        }

        /// <summary>
        /// Adds new employees to training campaign
        /// </summary>
        /// <param name="campaign">Training campaign object</param>
        public void AddNewEmployeeForCampaign(Training_Campaign campaign)
        {
            DataTable users = new DataTable();
            users.Columns.Add("Id_User", typeof(int));
            DataRow row;

            for (int i = 0; i < campaign.Users_Assigned.Count; i++)
            {
                row = users.NewRow();
                row["Id_User"] = campaign.Users_Assigned[i].Id_User;
                users.Rows.Add(row);
            }

            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Add_New_Employee_For_Campaign", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id_Campaign", campaign.Id);
                cmd.Parameters.AddWithValue("@User_List", users);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
        }

        // maybe some status ?!?!?!?!?!?!
        /// <summary>
        /// Adds new employees to training group
        /// </summary>
        /// <param name="group">Training group object</param>
        public void AddNewEmployee(Training_Group group)
        {
            DataTable users = new DataTable();
            users.Columns.Add("Id_User", typeof(int));
            DataRow row;

            for (int i = 0; i < group.Users.Count; i++)
            {
                row = users.NewRow();
                row["Id_User"] = group.Users[i].Id_User;
                users.Rows.Add(row);
            }

            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Add_New_Employee", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id_Group", group.Id_Group);
                cmd.Parameters.AddWithValue("@User_List", users);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
        }

        /// <summary>
        /// Adds new training campaign to database
        /// </summary>
        /// <param name="training_Campaign">Training campaign object</param>
        /// <returns>Status message, info for user about executed procedure</returns>
        public string AddNewCampaign(Training_Campaign training_Campaign)
        {
            DataTable groups_Ids = new DataTable();
            groups_Ids.Columns.Add("Id_Group", typeof(int));
            DataRow row_Groups;

            for (var j = 0; j < training_Campaign.Training_Groups.Count; j++)
            {
                row_Groups = groups_Ids.NewRow();
                row_Groups["Id_Group"] = training_Campaign.Training_Groups[j].Id_Group;

                groups_Ids.Rows.Add(row_Groups);
            }

            DataTable users = new DataTable();
            users.Columns.Add("Id_User", typeof(int));
            DataRow row_Users;

            if (training_Campaign.Users_Assigned.Count > 0)
            {
                for (int i = 0; i < training_Campaign.Users_Assigned.Count; i++)
                {
                    row_Users = users.NewRow();
                    row_Users["Id_User"] = training_Campaign.Users_Assigned[i].Id_User;
                    users.Rows.Add(row_Users);
                }
            }

            DataTable presentations = new DataTable();
            presentations.Columns.Add("Id_Presentation", typeof(int));
            DataRow row_Pres;

            for (int i = 0; i < training_Campaign.Presentations.Count; i++)
            {
                row_Pres = presentations.NewRow();
                row_Pres["Id_Presentation"] = training_Campaign.Presentations[i].Id;
                presentations.Rows.Add(row_Pres);
            }

            DateTime startDate = Convert.ToDateTime(training_Campaign.Start_Date);
            DateTime endDate = Convert.ToDateTime(training_Campaign.End_Date);

            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Add_New_Campaign", con);
                cmd.CommandType = CommandType.StoredProcedure;

                if (training_Campaign.Id > 0)
                {
                    cmd.Parameters.AddWithValue("@Id_Campaign", training_Campaign.Id);
                }

                cmd.Parameters.AddWithValue("@Name", training_Campaign.Name);
                cmd.Parameters.AddWithValue("@Author_Id", training_Campaign.Author_Id);
                cmd.Parameters.AddWithValue("@Start_Date", startDate);
                cmd.Parameters.AddWithValue("@End_Date", endDate);
                cmd.Parameters.AddWithValue("@Attempts", training_Campaign.Attempts);

                cmd.Parameters.AddWithValue("@Group_List", groups_Ids);
                cmd.Parameters.AddWithValue("@User_List", users);
                cmd.Parameters.AddWithValue("@Presentations_List", presentations);

                IDbDataParameter status = cmd.CreateParameter();
                status.ParameterName = "@Status";
                status.Direction = System.Data.ParameterDirection.Output;
                status.DbType = System.Data.DbType.String;
                status.Size = 150;
                cmd.Parameters.Add(status);

                con.Open();
                cmd.ExecuteNonQuery();

                string message = Convert.ToString(status.Value);

                con.Close();

                return message;
            }
        }

        /// <summary>
        /// Updates column "Shuffle_Questions" in presentation table, allows to shuffle questions during the test
        /// </summary>
        /// <param name="presentations">Presentation object</param>
        public void ShuffleQuestions(Presentations presentations)
        {
            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Shuffle_Questions", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Shuffle_Questions", presentations.Shuffle_Questions);
                cmd.Parameters.AddWithValue("@Id_Presentation", presentations.Id);
                con.Open();
                cmd.ExecuteNonQuery();

                con.Close();
            }
        }

        /// <summary>
        /// Updates column "Points_Required" in presentation table, this number of points will be required to pass the test
        /// </summary>
        /// <param name="presentations">Presentation object</param>
        public void SetRequirements(Presentations presentations)
        {
            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Set_Requirements", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Points_Required", presentations.Points_Required);
                cmd.Parameters.AddWithValue("@Id_Presentation", presentations.Id);
                con.Open();
                cmd.ExecuteNonQuery();

                con.Close();
            }
        }

        /// <summary>
        /// Checks points required to pass all trainings
        /// </summary>
        /// <param name="presentations"></param>
        /// <returns>Presentation object, trainings that have no pass requirements</returns>
        public List<Presentations> CheckRequirements(List<Presentations> presentations)
        {
            List<Presentations> Pres_Req = new List<Presentations>();

            DataTable pres = new DataTable();
            pres.Columns.Add("Id_Presentation", typeof(int));
            DataRow row_Pres;

            for (int i = 0; i < presentations.Count; i++)
            {
                row_Pres = pres.NewRow();
                row_Pres["Id_Presentation"] = presentations[i].Id;
                pres.Rows.Add(row_Pres);
            }

            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Check_Requirements", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Presentations_List", pres);
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    Presentations presentation = new Presentations();

                    presentation.Id = Convert.ToInt32(rdr["Id_Presentation"]);
                    presentation.Name = rdr["Presentation_Name"].ToString();
                    presentation.Points_Required = Convert.ToInt32(rdr["Points_Required"]);
                    presentation.Question_Count = Convert.ToInt32(rdr["Question_Count"]);

                    Pres_Req.Add(presentation);
                }

                con.Close();
            }
            return Pres_Req;
        }

        /// <summary>
        /// Ckeck if questions should be shuffled 
        /// </summary>
        /// <param name="id">Indicates primary key in presentation table</param>
        /// <returns>Status message, boolean true or false, converted to string</returns>
        public string CheckShuffle_Questions(int? id)
        {
            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("CheckShuffle_Questions", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id_Presentation", id);

                IDbDataParameter status = cmd.CreateParameter();
                status.ParameterName = "@Shuffle_Questions";
                status.Direction = System.Data.ParameterDirection.Output;
                status.DbType = System.Data.DbType.Boolean;
                status.Size = 150;
                cmd.Parameters.Add(status);

                con.Open();
                cmd.ExecuteNonQuery();

                string message = Convert.ToString(status.Value);

                con.Close();

                return message;
            }
        }

        /// <summary>
        /// Checks points required to pass a specific training
        /// </summary>
        /// <param name="id">Indicates primary key in presentation table</param>
        /// <returns>Status message, number of points, converted to string</returns>
        public string CheckPoints_Required(int id)
        {
            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("CheckPoints_Required", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id_Presentation", id);

                IDbDataParameter status = cmd.CreateParameter();
                status.ParameterName = "@Points_Required";
                status.Direction = System.Data.ParameterDirection.Output;
                status.DbType = System.Data.DbType.Int32;
                status.Size = 32;
                cmd.Parameters.Add(status);

                con.Open();
                cmd.ExecuteNonQuery();

                string message = Convert.ToString(status.Value);
                if (message == "" || message == null)
                {
                    message = "1";
                }

                con.Close();

                return message;
            }
        }

        /// <summary>
        /// Retrieves available campaigns with trainings for current logged in user
        /// </summary>
        /// <param name="Id_User">Indicates primary key in user table, key is mapped with other tables</param>
        /// <returns>List of training campaign objects</returns>
        public async Task<List<Training_Campaign>> AvailableTrainings(int Id_User)
        {
            List<Training_Campaign> campaigns = new List<Training_Campaign>();


            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Available_Trainings", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id_User", Id_User);

                await con.OpenAsync();
                SqlDataReader rdr = await cmd.ExecuteReaderAsync();

                while (await rdr.ReadAsync())
                {
                    Training_Campaign training_Campaign = new Training_Campaign();
                    List<Presentations> presentations = new List<Presentations>();
                    Presentations presentation = new Presentations();

                    training_Campaign.Id = Convert.ToInt32(rdr["Id_Campaign"]);
                    training_Campaign.Name = rdr["Name"].ToString();

                    presentation.Name = rdr["Presentation_Name"].ToString();
                    presentation.Id = Convert.ToInt32(rdr["Id_Presentation"]);

                    presentations.Add(presentation);
                    training_Campaign.Presentations = presentations;
                    //presentations.Clear();

                    training_Campaign.Start_Date = rdr["Start_Date"].ToString();
                    training_Campaign.End_Date = rdr["End_Date"].ToString();
                    training_Campaign.Attempts = Convert.ToInt32(rdr["Attempts Available"]);
                    // training_Campaign.Last_Modification_Date = rdr["Last_Modification_Date"].ToString();

                    campaigns.Add(training_Campaign);
                }
                con.Close();
            }
            return campaigns;
        }

        /// <summary>
        /// Retrieves menu links for users or admins
        /// </summary>
        /// <param name="DB_Admin">Boolean variable, indicates that users is an admin</param>
        /// <returns>List of parent menu objects</returns>
        public async Task<List<Parent_Menu>> GetMenu(bool DB_Admin)
        {
            List<Parent_Menu> parent_Menus = new List<Parent_Menu>();

            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Get_Parent_Menus", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@DB_Admin", DB_Admin);

                await con.OpenAsync();
                SqlDataReader rdr = await cmd.ExecuteReaderAsync();

                while (await rdr.ReadAsync())
                {
                    Parent_Menu parent = new Parent_Menu();

                    parent.Id_Parent = Convert.ToInt32(rdr["Id_Parent"]);
                    parent.Name = rdr["Name"].ToString();
                    parent.Css_Class = rdr["Css_Class"].ToString();
                    parent.Icon_Css_Class = rdr["Icon_Css_Class"].ToString();
                    parent.HTML_Events = rdr["HTML_Events"].ToString();
                    parent.JS_Function = rdr["JS_Function"].ToString();
                    parent.Additional_HTML = rdr.SafeGetString("Additional_HTML").ToString();
                    parent.Additional_Css = rdr.SafeGetString("Additional_Css").ToString();

                    parent_Menus.Add(parent);
                }

                rdr.Close();
                foreach (var parent in parent_Menus)
                {
                    List<Child_Menu> child_Menus = new List<Child_Menu>();

                    SqlCommand cmd_2 = new SqlCommand("Get_Child_Menus", con);
                    cmd_2.CommandType = CommandType.StoredProcedure;

                    cmd_2.Parameters.AddWithValue("@Id_Parent", parent.Id_Parent);
                    cmd_2.Parameters.AddWithValue("@DB_Admin", DB_Admin);

                    SqlDataReader rdr_2 = await cmd_2.ExecuteReaderAsync();

                    while (await rdr_2.ReadAsync())
                    {
                        Child_Menu child_Menu = new Child_Menu();

                        child_Menu.Id_Menu = Convert.ToInt32(rdr_2["Id_Menu"]);
                        child_Menu.Name = rdr_2["Name"].ToString();
                        child_Menu.Css_Class = rdr_2["Css_Class"].ToString();
                        child_Menu.Icon_Css_Class = rdr_2["Icon_Css_Class"].ToString();
                        child_Menu.Action_Name = rdr_2["Action_Name"].ToString();
                        child_Menu.Controller_Name = rdr_2["Controller_Name"].ToString();
                        child_Menu.Additional_HTML = rdr_2.SafeGetString("Additional_HTML").ToString();

                        child_Menus.Add(child_Menu);
                    }
                    rdr_2.Close();
                    parent.Child_Menus = child_Menus;
                }
                con.Close();
            }
            return parent_Menus;
        }

        public void ReduceAttempt(int? Id_User, int Id_Campaign, int Id_Presentation)
        {
            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("Reduce_Attempt", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id_User", Id_User);
                cmd.Parameters.AddWithValue("@Id_Campaign", Id_Campaign);
                cmd.Parameters.AddWithValue("@Id_Presentation", Id_Presentation);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<User> GetAllParticipants(int Id_Campaign, int Id_Presentation)
        {

            List<User> users = new List<User>();
            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("GetAllParticipants", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id_Campaign", Id_Campaign);
                cmd.Parameters.AddWithValue("@Id_Presentation", Id_Presentation);
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    User user = new User();

                    user.Id_User = Convert.ToInt32(rdr["IDUSER"]);
                    user.Personal_Number = rdr["PersonalNumber"].ToString();
                    user.First_Name = rdr["UserName"].ToString();
                    user.Last_Name = rdr["FullName"].ToString();
                    user.Email = rdr["Result"].ToString();
                    //user.Info_Group = rdr["Branch"].ToString();
                    user.Info_Login = rdr.SafeGetString("Date").ToString();

                    //user.Info_Login = rdr.IsDBNull(Convert.ToInt32(rdr["Date"])) ? null : (string)rdr["Date"];
                    //user.Info_Login = user.Info_Login.Substring(0, user.Info_Login.IndexOf(" "));
                    //user.Sum_of_Points = Convert.ToInt32(rdr["Sum_of_Points"]);

                    users.Add(user);
                }
                rdr.Close();
            }
            return users;
        }

        public bool IsAdmin(int Id)
        {
            using (GetCon())
            {
                SqlCommand cmd = new SqlCommand("IsAdmin", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", Id);  

                IDbDataParameter status = cmd.CreateParameter();
                status.ParameterName = "@Answer";
                status.Direction = System.Data.ParameterDirection.Output;
                status.DbType = System.Data.DbType.Boolean;
                status.Size = 200;
                cmd.Parameters.Add(status);

                con.Open();

                cmd.ExecuteNonQuery();
                con.Close();
                return Convert.ToBoolean(status.Value);
            }
        }
    }
}

// make use of this !!!!!!!!!!!!!!!!!!!
//public SqlDataReader ExecuteReader(string commandText, params SqlParameter[] parameters)
//{
//    using (SqlCommand cmd = new SqlCommand(commandText, con))
//    {
//        cmd.CommandType = CommandType.StoredProcedure;
//        cmd.CommandText = commandText;
//        cmd.Parameters.AddRange(parameters);

//        con.Open();

//        // When using CommandBehavior.CloseConnection, the connection will be closed when the IDataReader is closed.  
//        SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

//        return reader;
//    }
//}

