
namespace TFCiclo.Data.Models
{
    namespace TFCiclo.Data.Models
    {
        public class ModelRefreshToken
        {
            #region Campos privados
            private int _id = 0;
            private int _user_id = 0;
            private string _token_hash = string.Empty;
            private DateTime _created_at = DateTime.MinValue;
            private DateTime _expires_at = DateTime.MinValue;
            private DateTime _revoked_at = DateTime.MinValue;
            private string _replaced_by_token_hash = string.Empty;
            private string _created_by_ip = string.Empty;
            private string _revoked_by_ip = string.Empty;
            #endregion

            #region Campos públicas
            public int id { get => _id; set => _id = value; }
            public int user_id { get => _user_id; set => _user_id = value; }
            public string token_hash { get => _token_hash; set => _token_hash = value; }
            public DateTime created_at { get => _created_at; set => _created_at = value; }
            public DateTime expires_at { get => _expires_at; set => _expires_at = value; }
            public DateTime revoked_at { get => _revoked_at; set => _revoked_at = value; }
            public string replaced_by_token_hash { get => _replaced_by_token_hash; set => _replaced_by_token_hash = value; }
            public string created_by_ip { get => _created_by_ip; set => _created_by_ip = value; }
            public string revoked_by_ip { get => _revoked_by_ip; set => _revoked_by_ip = value; }
            #endregion
        }
    }
}
