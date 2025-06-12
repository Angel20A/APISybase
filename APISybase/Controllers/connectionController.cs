using System;
using System.IO;
using Advantage.Data.Provider;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Dynamic;
using System.Drawing;

namespace APISybase.Controllers
{
    public class connectionController : Controller
    {
        public string _Connstring = @"data source=C:\\Users\\HP\\Documents\\Angel\\SDG\\Sybase\\Data;ServerType=local;CharType=general_vfp_ci_as_1252;TableType=ADT;";

        [HttpPost]
        [Route("GetUsers")]
        public async Task<string> GetUsers([FromBody] Query query)
        {
            try
            {
                string connStr = query.Connstring;
                string localquery = query.Localquery;
                var users = new List<dynamic>();
                using (AdsConnection conn = new AdsConnection(connStr))
                {
                    await conn.OpenAsync();
                    string sql = localquery;
                    using (AdsCommand cmd = new AdsCommand(sql, conn))
                    using (AdsDataReader reader = (AdsDataReader)await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int records = 0;
                            dynamic user = new ExpandoObject();

                            int id = !reader.IsDBNull(reader.GetOrdinal("PkData")) ? reader.GetInt32(reader.GetOrdinal("PkData")) : 0;
                            string name = !reader.IsDBNull(reader.GetOrdinal("UserName")) ? reader.GetString(reader.GetOrdinal("UserName")).ToString().TrimEnd() : "";
                            string email = !reader.IsDBNull(reader.GetOrdinal("Email")) ? reader.GetString(reader.GetOrdinal("Email")).ToString().TrimEnd() : "";
                            string card = !reader.IsDBNull(reader.GetOrdinal("CardNumberFormatted")) ? reader.GetString(reader.GetOrdinal("CardNumberFormatted")).ToString().TrimEnd() : "";
                            byte[]? picture = !reader.IsDBNull(reader.GetOrdinal("Picture")) ? reader.GetBytes(reader.GetOrdinal("Picture")) : null;

                            user.id = id;
                            user.name = name;
                            user.email = email;
                            user.card = card;
                            //user.image = reader.GetBytes(reader.GetOrdinal("ThumbNail"));
                            user.image = picture;
                            users.Add(user);
                        }
                    }
                }
                return JsonConvert.SerializeObject(users);
            }
            catch (Exception ex) 
            {
                return JsonConvert.SerializeObject(ex.Message);
            }
        }

        [HttpPost]
        [Route("PostUser")]
        public async Task<string> PostUser([FromBody] User user)
        {
            try
            {
                //string connStr = _Connstring;
                string localquery = $@"INSERT INTO Card (PkData, CardNumberFormatted, UserName, Email, Picture) 
                    VALUES ({user.PkData}, '{user.CardNumberFormatted}', '{user.Username}', '{user.Email}', ?)";

                using (AdsConnection conn = new AdsConnection(_Connstring))
                {
                    await conn.OpenAsync();
                    //string sql = localquery;
                    AdsCommand cmd = new AdsCommand(localquery, conn);
                    
                    /*AdsParameter param1 = new AdsParameter();
                    param1.DbType = DbType.Int32;
                    param1.Value = user.PkData;
                    cmd.Parameters.Add(param1);

                    AdsParameter param2 = new AdsParameter();
                    param2.DbType = DbType.String;
                    param2.Value = user.CardNumberFormatted;
                    cmd.Parameters.Add(param2);

                    AdsParameter param3 = new AdsParameter();
                    param3.DbType = DbType.String;
                    param3.Value = user.Username;
                    cmd.Parameters.Add(param3);

                    AdsParameter param4 = new AdsParameter();
                    param4.DbType = DbType.String;
                    param4.Value = user.Email;
                    cmd.Parameters.Add(param4);*/

                    byte[] pictureByte = Convert.FromBase64String(user.Picture);
                    //if (!(user.Picture == null || user.Picture == ""))
                    //{
                        //pictureByte = Convert.FromBase64String(user.Picture);
                    //}
                    AdsParameter param5 = new AdsParameter();
                    param5.DbType = DbType.Binary;
                    param5.Value = pictureByte;
                    cmd.Parameters.Add(param5);

                    //Console.WriteLine($"{cmd.Parameters[0].Value}, {cmd.Parameters[1].Value}, {cmd.Parameters[2].Value}, {cmd.Parameters[3].Value}, {cmd.Parameters[4].Value}");
                    int reader = await cmd.ExecuteNonQueryAsync();

                    string result;
                    if (reader > 0)
                    {
                        result = "Usuario creado correctamente. " + reader;
                    }
                    else
                    {
                        result = "No se pudo crear el usuario.";
                    }
                    return JsonConvert.SerializeObject(result);
                }
            }
            catch(Exception ex)
            {
                return JsonConvert.SerializeObject(ex.Message);
            }
        }

        [HttpPut]
        [Route("PutUser")]
        public async Task<string> PutUser([FromBody] User user)
        {
            try
            {
                string localquery = $@"UPDATE Card SET CardNumberFormatted='{user.CardNumberFormatted}', UserName='{user.Username}',
                    Email='{user.Email}', Picture=? WHERE PkData={user.PkData}";

                using (AdsConnection conn = new AdsConnection(_Connstring))
                {
                    await conn.OpenAsync();
                    AdsCommand cmd = new AdsCommand(localquery, conn);

                    byte[]? pictureByte = Convert.FromBase64String(user.Picture) ?? null;
                    AdsParameter param1 = new AdsParameter();
                    param1.DbType = DbType.Binary;
                    param1.Value = pictureByte;
                    cmd.Parameters.Add(param1);

                    Console.WriteLine(user.PkData + ", " + user.CardNumberFormatted + ", " + user.Username + ", " + user.Email + ", " + cmd.Parameters[0].Value);
                    int reader = await cmd.ExecuteNonQueryAsync();

                    string result;
                    if (reader > 0)
                    {
                        result = "Usuario actualizado correctamente. " + reader;
                    }
                    else
                    {
                        result = "No se pudo actualizar el usuario.";
                    }
                    return JsonConvert.SerializeObject(result);

                }
            }
            catch(Exception ex)
            {
                return JsonConvert.SerializeObject(ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteUser/{PkUser}")]
        public async Task<string> DeleteUser(int PkUser)
        {
            try
            {
                string localquery = $@"DELETE FROM Card WHERE PkData={PkUser}";
                using (AdsConnection conn = new AdsConnection(_Connstring))
                {
                    await conn.OpenAsync();
                    AdsCommand cmd = new AdsCommand(localquery, conn);
                    int reader = await cmd.ExecuteNonQueryAsync();

                    string result;
                    if (reader > 0)
                    {
                        result = "Usuario eliminado correctamente. " + reader;
                    }
                    else
                    {
                        result = "No se pudo eliminar el usuario.";
                    }
                    return JsonConvert.SerializeObject(result);

                }
            }
            catch(Exception ex)
            {
                return JsonConvert.SerializeObject(ex.Message);
            }
        }

        public int triggerInsert(Int32 ulConnectionID, Int32 hConnection, String strTriggerName, String strTableName,
                      Int32 ulEventType, Int32 ulTriggerType, Int32 ulRecNo)
        {
            AdsConnection conn = new AdsConnection("ConnectionHandle=" + hConnection.ToString());
            AdsCommand cmd;
            AdsCommand cmdNew;
            AdsExtendedReader extReader;
            AdsDataReader reader;

            Guid guid = Guid.NewGuid();
            string s = guid.ToString();
            try
            {
                conn.Open();
                cmd = conn.CreateCommand();
                cmdNew = conn.CreateCommand();

                cmd.CommandText = "SELECT * FROM __new";
                reader = cmd.ExecuteReader();

                cmdNew.CommandText = "select*from Card";
                extReader = cmdNew.ExecuteExtendedReader();

                //insert __net
                extReader.AppendRecord();
                reader.Read();
                if (reader.IsDBNull(0))
                {
                    extReader.SetString(0, s);
                }
                else
                {
                    extReader.SetString(0, reader.GetString(0));
                }

                for (int i = 1; i < reader.FieldCount - 1; i++)
                {
                    if (!reader.IsDBNull(i))
                    {
                        extReader.SetValue(i, reader.GetValue(i));
                    }
                }
                extReader.WriteRecord();
                extReader.Close();
                reader.Close();
            }
            catch (Exception ex) 
            {
                AdsCommand errCmd = conn.CreateCommand();
                errCmd.CommandText = "INSERT INTO __error VALUES( 0, '" + ex.Message + "' )";
                errCmd.ExecuteNonQuery();

            }

            //siempre retorna 0
            return 0;
        }


        public class Query
        {
            public string Connstring { get; set; }
            public string Localquery { get; set; }
        }

        public class User
        {
            public Int32 PkData { get; set; }
            public string CardNumberFormatted { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string Picture { get; set; }
        }
    }
}
