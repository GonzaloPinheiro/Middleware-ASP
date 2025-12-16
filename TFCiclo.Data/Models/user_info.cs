using Dapper.Contrib.Extensions;

namespace TFCiclo.Data.Models
{
    [Table("user_info")]
    public class user_info
    {
        #region Métodos privados
        private int _id = 0;
        private string _username = string.Empty;
        private string _user_password = string.Empty;
        private string _email = string.Empty;
        private DateTime _created_at = DateTime.MinValue;
        #endregion

        #region Métodos públicos
        [Key]
        public int id { get => _id; set => _id = value; }
        public string username { get => _username; set => _username = value; }
        public string email { get => _email; set => _email = value; }
        public string user_password { get => _user_password; set => _user_password = value; }
        public DateTime created_at { get => _created_at; set => _created_at = value; }

        #endregion
    }
}
