using Advantage.Data.Provider;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Dynamic;

namespace APISybase.Controllers
{
    public class connectionController : Controller
    {
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
                        user.id = !reader.IsDBNull(reader.GetOrdinal("PkData")) ? reader.GetInt32(reader.GetOrdinal("PkData")) : 0;
                        user.name = !reader.IsDBNull(reader.GetOrdinal("UserName")) ? reader.GetString(reader.GetOrdinal("UserName")) : "";
                        user.email = !reader.IsDBNull(reader.GetOrdinal("Email")) ? reader.GetString(reader.GetOrdinal("Email")) : "";
                        user.card = !reader.IsDBNull(reader.GetOrdinal("CardNumberFormatted")) ? reader.GetString(reader.GetOrdinal("CardNumberFormatted")) : "";
                        //user.image = reader.GetBytes(reader.GetOrdinal("ThumbNail"));
                        user.image = !reader.IsDBNull(reader.GetOrdinal("Picture")) ? reader.GetBytes(reader.GetOrdinal("Picture")) : null;
                        users.Add(user);
                    }
                }
            }
            return JsonConvert.SerializeObject(users);
        }

        public class Query
        {
            public string Connstring { get; set; }
            public string Localquery { get; set; }
        }
    }
}
