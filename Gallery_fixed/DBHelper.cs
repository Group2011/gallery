using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Gallery
{
    public class DBHelper
    {
        public static SqlConnection GetConnection()
        {
            SqlConnection connSql = new SqlConnection(ConfigurationManager.ConnectionStrings["mycs"].ConnectionString);
            connSql.Open();
            return connSql;
        }

        /// <summary>
        /// Метод проверяет существует ли уже в базе тег 
        /// </summary>
        /// <param name="tag">Имя тега</param>
        /// <returns>-1 если тег не существует или id тега</returns>
        public static int CheckTag(string tag)
        {
            int id = -1;
            SqlConnection connSql = GetConnection();
            SqlCommand commSQl = new SqlCommand();
            commSQl.CommandText = "SELECT id FROM Tags WHERE name = '" + tag + "';";
            commSQl.Connection = connSql;
            SqlDataReader reader = null;
            try
            {
                reader = commSQl.ExecuteReader();
                if (reader.Read())
                    id = Convert.ToInt32(reader[0]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при попытке прочитать из базы тег\n" + ex.Message + "\n" + ex.StackTrace);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                if (connSql != null)
                    connSql.Close();
            }
            return id;
        }

        /// <summary>
        /// Добавляе тег в базу
        /// </summary>
        /// <param name="tag">ИмЯ тега</param>
        /// <returns>true если тег успешно добавлен, иначе false</returns>
        public static bool AddTag(string tag)
        {
            SqlConnection connSql = GetConnection();
            SqlCommand commSQl = new SqlCommand();
            commSQl.CommandText = "INSERT INTO Tags VALUES('" + tag + "');";
            commSQl.Connection = connSql;
            bool result;

            try
            {
                commSQl.ExecuteNonQuery();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                MessageBox.Show("Произошла ошибка при попытке добавить в базу тег\n" + ex.Message + "\n" + ex.StackTrace);
            }
            finally
            {
                if (connSql != null)
                    connSql.Close();
            }
            return result;
        }

        /// <summary>
        /// Добавляет строку в таблицу Images БД
        /// </summary>
        /// <param name="name">Имя</param>
        /// <param name="path">Путь</param>
        /// <param name="user_id">Id пользователя</param>
        /// <param name="tag">Имя тега</param>
        /// <returns>true если строка успешно добавлена, иначе false</returns>
        public static bool AddImage(string name, string path, int user_id, string tag)
        {
            SqlConnection connSql = GetConnection();
            SqlCommand commSQl = new SqlCommand();

            commSQl.CommandText = "INSERT INTO Images VALUES(@p1, @p2, @p3, @p4, @p5);";
            commSQl.Parameters.Add("@p1", SqlDbType.NVarChar, 200).Value = name;
            commSQl.Parameters.Add("@p2", SqlDbType.NVarChar, 200).Value = path;
            commSQl.Parameters.Add("@p3", SqlDbType.Int).Value = 0;
            commSQl.Parameters.Add("@p4", SqlDbType.Int).Value = user_id;
            int idtag = CheckTag(tag);
            bool result;

            try
            {
                if (idtag == -1)
                {
                    if (AddTag(tag))
                        idtag = CheckTag(tag);
                    else
                    {
                        int defTagId = CheckTag("разное");
                        if (defTagId != -1)
                            idtag = defTagId;
                        else
                        {
                            AddTag("разное");
                            idtag = CheckTag("разное");
                        }
                    }
                }
                commSQl.Parameters.Add("@p5", SqlDbType.Int).Value = idtag;
                commSQl.Connection = connSql;
                commSQl.ExecuteNonQuery();

                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                MessageBox.Show("Произошла ошибка при попытке добавить в таблицу Images строку \n" + ex.Message + "\n" + ex.StackTrace);
            }
            finally
            {
                if (connSql != null)
                    connSql.Close();
            }
            return result;
        }

        public static List<string> GetTags()
        {
            try
            {
                SqlConnection connSql = GetConnection();
                SqlCommand commSQl = new SqlCommand();
                SqlDataReader reader = null;
                List<string> tags = new List<string>();
                commSQl.Connection = connSql;
                commSQl.CommandText = "select name from tags";
                DataTable dt = new DataTable();

                reader = commSQl.ExecuteReader();
                int line = 0;
                while (reader.Read())
                {
                    if (line == 0)
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            dt.Columns.Add(reader.GetName(i));
                        }
                        line++;

                    }
                    DataRow row = dt.NewRow();
                    for (int j = 0; j < line; j++)
                    {
                        row[j] = reader[j];
                        tags.Add(row[j].ToString());
                    }
                    dt.Rows.Add(row);
                }
                return tags;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
                return null;
            }
        }
    }
}
