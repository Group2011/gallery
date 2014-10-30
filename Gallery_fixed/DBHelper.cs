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
        /// Добавление юзера в БД, если такого в ней нет
        /// </summary>
        /// <param name="uname">Имя юзера</param>
        /// <param name="upass">Пароль юзера</param>
        /// <returns></returns>
        public static bool AddUser(string uname, string upass)
        {
            try
            {
                int id = -1;
                SqlConnection connSql = GetConnection();
                SqlCommand commSQl = new SqlCommand();
                commSQl.CommandText = "SELECT id FROM Users WHERE uname = '" + uname + "';";
                commSQl.Connection = connSql;
                SqlDataReader reader = null;
                try
                {
                    reader = commSQl.ExecuteReader();
                    if (reader.Read())
                        id = Convert.ToInt32(reader[0]);
                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Произошла ошибка при попытке прочитать из базы имя пользователя\n" + ex.Message + "\n" + ex.StackTrace);
                }
                if (id > 0)
                {
                    connSql.Close();
                    return false;
                }
                else
                {
                    commSQl = new SqlCommand();
                    commSQl.CommandText = "INSERT INTO Users VALUES ('" + uname + "', " + "'" + upass + "');";
                    commSQl.Connection = connSql;
                    commSQl.ExecuteScalar();
                    connSql.Close();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static int GetUserID(string name)
        {
            SqlConnection connSql = GetConnection();
            SqlCommand commSQl = new SqlCommand();
            commSQl.CommandText = "SELECT id FROM Users WHERE uname = '" + name + "'";
            commSQl.Connection = connSql;
            SqlDataReader reader = null;
            int id = 0;
            try
            {
                reader = commSQl.ExecuteReader();
                if (reader.Read())
                    id = Convert.ToInt32(reader[0]);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при попытке прочитать из базы юзера\n" + ex.Message + "\n" + ex.StackTrace);
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

                
        public static bool CheckUser(string log, string pas = "")
        {
            SqlConnection connSql = GetConnection();
            SqlCommand commSQl = new SqlCommand();
            commSQl.CommandText = "SELECT COUNT(*) FROM Users WHERE uname = '" + log + "'";
            if (!pas.Equals(""))
                commSQl.CommandText += "AND upassword = '" + pas + "'";
            commSQl.Connection = connSql;
            SqlDataReader reader = null;
            int i = 0;
            try
            {
                reader = commSQl.ExecuteReader();
                if (reader.Read())
                    i = Convert.ToInt32(reader[0]);
                    
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при попытке прочитать из базы юзера\n" + ex.Message + "\n" + ex.StackTrace);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                if (connSql != null)
                    connSql.Close();
            }
            return i == 1;
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

        public static List<string> GetImagesPathes(string tag)
        {
            try
            {
                SqlConnection connSql = GetConnection();
                SqlCommand commSQl = new SqlCommand();
                SqlDataReader reader = null;
                List<string> images = new List<string>();
                commSQl.Connection = connSql;
                if (tag.Equals("случайные"))
                    commSQl.CommandText = "SELECT name FROM images";
                else
                    commSQl.CommandText = "SELECT I.name FROM images I JOIN tags T ON I.tag_id = T.id where T.name = '" + tag + "'";

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
                        images.Add(row[j].ToString());
                    }
                    dt.Rows.Add(row);
                }
                return images;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
                return null;
            }
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
