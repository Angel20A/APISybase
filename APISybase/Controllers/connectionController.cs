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
        public string _Connstring = @"data source=C:\Users\HP\Documents\Angel\SDG\Sybase\Data;ServerType=local;CharType=ansi;TableType=ADT;";

        [HttpPost]
        [Route("GetUsers")]
        public async Task<string> GetUsers([FromBody] Query query)
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

        [HttpPost]
        [Route("PostUser")]
        public async Task<string> PostUser([FromBody] User user)
        {
            try
            {
                //string connStr = _Connstring;
                string localquery = $@"INSERT INTO Card (PkData, CardNumberFormatted, UserName, Email, Picture) 
                    VALUES (?, ?, ?, ?, ?)";

                using (AdsConnection conn = new AdsConnection(_Connstring))
                {
                    await conn.OpenAsync();
                    //string sql = localquery;
                    AdsCommand cmd = new AdsCommand(localquery, conn);
                    cmd.Parameters.Add(new AdsParameter { DbType = DbType.Int32, Value = user.PkData });
                    cmd.Parameters.Add(new AdsParameter { DbType = DbType.String, Value = user.CardNumberFormatted });
                    cmd.Parameters.Add(new AdsParameter { DbType = DbType.String, Value = user.Username });
                    cmd.Parameters.Add(new AdsParameter { DbType = DbType.String, Value = user.Email });

                    byte[] pictureByte = Convert.FromBase64String(user.Picture);
                    /*if (!(user.Picture == null || user.Picture == ""))
                    {
                        pictureByte = Convert.FromBase64String(user.Picture);
                    }*/
                    cmd.Parameters.Add(new AdsParameter { DbType = DbType.Binary, Value = pictureByte });

                    Console.WriteLine($"{user.PkData}, {user.CardNumberFormatted}, {user.Username}, {user.Email}, {user.Picture.Length}");

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

        public class Query
        {
            public string Connstring { get; set; }
            public string Localquery { get; set; }
        }

        public class User
        {
            public int PkData { get; set; }
            public string CardNumberFormatted { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string Picture { get; set; }
        }
    }
}
